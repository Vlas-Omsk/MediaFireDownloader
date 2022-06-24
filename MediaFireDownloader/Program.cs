using MediaFireDownloader.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WebLib;

namespace MediaFireDownloader
{
    public sealed class Program
    {
        private readonly ConsoleLogger _logger = new ConsoleLogger();
        private int _skipped;
        private int _failed;
        private int _downloaded;
        private readonly Task[] _tasks;

        static void Main(string[] args)
        {
            string folderKey;
            string destination;
            int threadsCount;

            switch (args.Length)
            {
                default:
                    Console.WriteLine("Use MediaFireDownloader <folder key> [destination path] [threads count]");
                    return;
                case 1:
                    folderKey = args[0];
                    destination = ".";
                    threadsCount = 20;
                    break;
                case 2:
                    folderKey = args[0];
                    destination = args[1];
                    threadsCount = 20;
                    break;
                case 3:
                    folderKey = args[0];
                    destination = args[1];
                    if (!int.TryParse(args[2], out threadsCount) || threadsCount <= 0)
                    {
                        Console.WriteLine("The number of threads must be a positive number greater than zero");
                        return;
                    }
                    break;
            }

            var program = new Program(threadsCount);
            program.Start(folderKey, destination);
        }

        private Program(int threadsCount)
        {
            _tasks = new Task[threadsCount];
        }

        private void Start(string folderKey, string destination)
        {
            FolderEntry rootFolder;
            try
            {
                rootFolder = Mediafire.GetFolder(folderKey);
            }
            catch (Exception ex)
            {
                _logger.Exception("FolderKey: " + folderKey, ex);
                return;
            }

            _logger.Info("Folder info:");
            _logger.Padding("Name: " + rootFolder.Name);
            _logger.Padding("Destination: " + destination);

            DownloadFolder(rootFolder, destination);

            Task.WaitAll(_tasks.Where(x => x != null).ToArray());

            _logger.Info("Done");
            _logger.Padding("Downloaded: " + _downloaded);
            _logger.Padding("Skipped: " + _skipped);
            _logger.Padding("Failed: " + _failed);
        }

        private void DownloadFolder(FolderEntry folder, string destination)
        {
            destination = Path.Combine(destination, folder.Name);

            Directory.CreateDirectory(destination);

            try
            {
                var files = Mediafire.GetFiles(folder);

                for (var i = 0; i < files.Length; i++)
                {
                    var freeTaskIndex = Array.FindIndex(_tasks, x => x?.IsCompleted ?? true);

                    _tasks[freeTaskIndex] = DownloadFile(files[i], destination);

                    if (_tasks.All(x => x != null))
                        Task.WaitAny(_tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("Folder: " + destination, ex);
                _failed++;
            }

            try
            {
                var folders = Mediafire.GetFolders(folder);

                foreach (var folderToDownload in folders)
                    DownloadFolder(folderToDownload, destination);
            }
            catch (Exception ex)
            {
                _logger.Exception("Folder: " + destination, ex);
                _failed++;
            }
        }

        private async Task DownloadFile(FileEntry file, string destination)
        {
            await Task.Run(() =>
            {
                destination = Path.Combine(destination, file.Name);

                if (File.Exists(destination))
                {
                    string fileHash;
                    using (var stream = File.OpenRead(destination))
                    using (var sha256 = SHA256.Create())
                        fileHash = string.Concat(sha256.ComputeHash(stream).Select(b => b.ToString("x2")));

                    if (file.Hash == fileHash)
                    {
                        _skipped++;
                        _logger.Skipped(destination);
                        return;
                    }
                }

                string downloadLink;
                try
                {
                    downloadLink = Mediafire.GetDownloadLink(file);
                }
                catch (Exception ex)
                {
                    _logger.Exception("File: " + destination, ex);
                    _failed++;
                    return;
                }

                _logger.Download(destination);
                _logger.Padding("Source: " + downloadLink);

                var request = HttpRequest.GET;
                request.Url = downloadLink;

                using (var response = request.GetResponse())
                {
                    if (response.HaveErrors)
                    {
                        _logger.Error(downloadLink + " " + response.WebException.Status);
                        _failed++;
                        return;
                    }

                    using (var stream = response.GetStream())
                    using (var fileStream = File.Open(destination, FileMode.Create))
                    {
                        var position = 0L;
                        var buffer = new byte[8 * 1024];
                        int bytesRead;

                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            position += bytesRead;
                        }
                    }
                }

                _logger.End(destination);
                _downloaded++;
            });
        }
    }
}
