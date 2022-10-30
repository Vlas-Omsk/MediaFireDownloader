using MediaFireDownloader.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaFireDownloader
{
    internal sealed class Downloader
    {
        private const int _chunkSize = 100;
        private const int _bufferSize = 8 * 1024;
        private readonly ConsoleLogger _logger;
        private readonly Task[] _tasks;
        private readonly byte[][] _buffers;
        private readonly StringBuilder[] _links;
        private readonly HttpClient _httpClient;
        private readonly Mediafire _mediafire;
        private RecursiveFileReader _fileReader;
        private int _skipped;
        private int _failed;
        private int _downloaded;
        private ConcurrentBag<(string destination, Exception reason)> _fails = new ConcurrentBag<(string destination, Exception reason)>();

        public Downloader(ConsoleLogger logger, int threadsCount, HttpClient httpClient, bool useSsl)
        {
            _logger = logger;
            _tasks = new Task[threadsCount];
            _buffers = new byte[threadsCount][];
            _links = new StringBuilder[threadsCount];
            for (var i = 0; i < threadsCount; i++)
            {
                _buffers[i] = new byte[_bufferSize];
                _links[i] = new StringBuilder(64);
            }
            _httpClient = httpClient;
            _mediafire = new Mediafire(_httpClient, useSsl);
        }

        public async Task Start(string folderKey, string destination)
        {
            FolderEntry rootFolder;
            try
            {
                rootFolder = await _mediafire.GetInfo(folderKey);
            }
            catch (Exception ex)
            {
                _logger.Exception("FolderKey: " + folderKey, ex);
                return;
            }

            _logger.Info("Folder info:");
            _logger.Padding("Name: " + rootFolder.Name);
            _logger.Padding("Destination: " + destination);

            await DownloadFiles(rootFolder, destination);

            _logger.Info("Done");
            _logger.Padding("Downloaded: " + _downloaded);
            _logger.Padding("Skipped: " + _skipped);
            _logger.Padding("Failed: " + _failed);
            _logger.Info("Fails");
            foreach (var fail in _fails)
                _logger.Padding($"{fail.destination}: {fail.reason.Message}");
        }

        private async Task DownloadFiles(FolderEntry folder, string destination)
        {
            _fileReader = new RecursiveFileReader(_mediafire, folder, destination, _chunkSize);
            EntryWrapper<FileEntry> file;

            Directory.CreateDirectory(Path.Combine(destination, folder.Name));

            try
            {
                while ((file = await ReadNextFileOrFolder()) != null)
                {
                    var freeTaskIndex = Array.FindIndex(_tasks, x => x?.IsCompleted ?? true);

                    _tasks[freeTaskIndex] = DownloadFile(file, _buffers[freeTaskIndex], _links[freeTaskIndex]);

                    if (_tasks.All(x => x != null))
                    {
                        var task = _tasks[Task.WaitAny(_tasks)];

                        if (task.IsFaulted)
                            throw task.Exception;
                    }
                }
            }
            finally
            {
                Task.WaitAll(_tasks.Where(x => x != null).ToArray());
            }
        }

        private async Task<EntryWrapper<FileEntry>> ReadNextFileOrFolder()
        {
            var file = await _fileReader.ReadNextFile();

            while (file == null)
            {
                var folder = await _fileReader.ReadNextFolder();

                if (folder == null)
                    return null;

                Directory.CreateDirectory(Path.Combine(folder.Destination, folder.Entry.Name));

                file = await _fileReader.ReadNextFile();
            }

            return file;
        }

        private async Task DownloadFile(EntryWrapper<FileEntry> file, byte[] buffer, StringBuilder link)
        {
            var destination = Path.Combine(file.Destination, file.Entry.Name);

            if (File.Exists(destination))
            {
                string fileHash;
                using (var stream = File.OpenRead(destination))
                using (var sha256 = SHA256.Create())
                    fileHash = string.Concat(sha256.ComputeHash(stream).Select(b => b.ToString("x2")));

                if (file.Entry.Hash == fileHash)
                {
                    _skipped++;
                    _logger.Skipped(destination);
                    return;
                }
            }

            string downloadLink;
            try
            {
                link.Clear();
                await _mediafire.GetDownloadLink(file.Entry, link);
                downloadLink = link.ToString();
            }
            catch (Exception ex)
            {
                _logger.Exception("File: " + destination, ex);
                _failed++;
                _fails.Add((destination, ex));
                return;
            }

            _logger.Download(destination);
            _logger.Padding("Source: " + downloadLink);

            using var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(downloadLink),
                Version = HttpVersion.Version20
            };

            try
            {
                using var response = await _httpClient.SendAsync(request);
                using var resposeStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Open(destination, FileMode.Create);

                var position = 0L;
                int bytesRead;

                while ((bytesRead = resposeStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    position += bytesRead;
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("File: " + destination, ex);
                _failed++;
                _fails.Add((destination, ex));
                return;
            }

            _logger.End(destination);
            _downloaded++;
        }
    }
}
