using MediaFireDownloader.Models;
using MediaFireDownloader.Net;
using PinkJson2;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MediaFireDownloader
{
    internal sealed class Mediafire
    {
        private static readonly Uri _sslApiEndPoint = new Uri("https://www.mediafire.com/api/1.4/");
        private static readonly Uri _sslFileEndPoint = new Uri("https://www.mediafire.com/file/");
        private static readonly Uri _nonSslApiEndPoint = new Uri("http://www.mediafire.com/api/1.4/");
        private static readonly Uri _nonSslFileEndPoint = new Uri("http://www.mediafire.com/file/");
        private static readonly char[] _downloadSymbols = "://download".ToCharArray();
        private static readonly int _skipLength = "mediafire.com/".Length;
        private readonly Uri _apiEndPoint;
        private readonly Uri _fileEndPoint;
        private readonly bool _useSsl;
        private readonly HttpClient _httpClient;

        private enum RequestType
        {
            Get,
            Post
        }

        public Mediafire(HttpClient httpClient, bool useSsl)
        {
            _httpClient = httpClient;
            if (_useSsl = useSsl)
            {
                _apiEndPoint = _sslApiEndPoint;
                _fileEndPoint = _sslFileEndPoint;
            }
            else
            {
                _apiEndPoint = _nonSslApiEndPoint;
                _fileEndPoint = _nonSslFileEndPoint;
            }
        }

        public async Task GetDownloadLink(FileEntry file, StringBuilder link)
        {
            using var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_fileEndPoint, file.Key),
                Version = HttpVersion.Version20
            };

            using var response = await _httpClient.SendAsync(request);
            using var responseStream = await response.Content.ReadAsStreamAsync();

            int chInt;
            int dwi = 0;
            var compleated = false;
            var hasId = false;

            if (_useSsl)
                link.Append("https://download");
            else
                link.Append("http://download");

            while ((chInt = responseStream.ReadByte()) != -1)
            {
                if (dwi == _downloadSymbols.Length)
                {
                    if (chInt >= '0' && chInt <= '9')
                    {
                        hasId = true;
                        link.Append((char)chInt);
                    }
                    else if (hasId && chInt == '.')
                    {
                        responseStream.Seek(_skipLength, SeekOrigin.Current);
                        link.Append(".mediafire.com/");

                        for (var i = 0; i < 12; i++)
                            link.Append((char)responseStream.ReadByte());

                        compleated = true;
                        break;
                    }
                    else
                    {
                        dwi = 0;
                    }
                }
                else if (chInt == _downloadSymbols[dwi])
                {
                    dwi++;
                }
                else
                {
                    dwi = 0;
                }
            }

            link.Append('/');
            link.Append(file.Key);

            if (!compleated)
                throw new Exception("The file is blocked or not available");
        }

        public async Task<ChunkedResult<FolderEntry>> GetFolders(FolderEntry folder, int chunk, int chunkSize)
        {
            var responseJson = await GetContent(folder, "folders", chunk, chunkSize);

            return new ChunkedResult<FolderEntry>(
                responseJson["folder_content"]["folders"].Deserialize<FolderEntry[]>(),
                responseJson["folder_content"]["more_chunks"].Get<string>() == "yes"
            );
        }

        public async Task<ChunkedResult<FileEntry>> GetFiles(FolderEntry folder, int chunk, int chunkSize)
        {
            var responseJson = await GetContent(folder, "files", chunk, chunkSize);

            return new ChunkedResult<FileEntry>(
                responseJson["folder_content"]["files"].Deserialize<FileEntry[]>(),
                responseJson["folder_content"]["more_chunks"].Get<string>() == "yes"
            );
        }

        private async Task<IJson> GetContent(FolderEntry folder, string contentType, int chunk, int chunkSize)
        {
            var responseJson = await GetResponse(
                RequestType.Get,
                "folder/get_content.php",
                new FormData()
                {
                    { "content_type", contentType },
                    { "filter", "all" },
                    { "order_by", "name" },
                    { "order_direction", "asc" },
                    { "chunk", chunk.ToString() },
                    { "chunk_size", chunkSize.ToString() },
                    { "version", "1.5" },
                    { "folder_key", folder.Key }
                }
            );

            return responseJson;
        }

        public async Task<FolderEntry> GetInfo(string folderKey)
        {
            var responseJson = await GetResponse(
                RequestType.Post,
                "folder/get_info.php",
                new FormData()
                {
                    { "recursive", "yes" },
                    { "folder_key", folderKey }
                }
            );

            return new FolderEntry()
            {
                Key = folderKey,
                Name = responseJson["folder_info"]["name"].Get<string>()
            };
        }

        private async Task<IJson> GetResponse(RequestType type, string path, FormData data)
        {
            data["response_format"] = "json";

            using var request = new HttpRequestMessage()
            {
                Version = HttpVersion.Version20
            };

            switch (type)
            {
                case RequestType.Get:
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(_apiEndPoint, path + '?' + data);
                    break;
                case RequestType.Post:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(_apiEndPoint, path);
                    request.Content = new FormDataContent(data);
                    break;
            }

            using var response = await _httpClient.SendAsync(request);
            var responseData = await response.Content.ReadAsJsonAsync();
            var responseJson = responseData["response"];

            if (responseJson["result"].Get<string>().Equals("error", StringComparison.OrdinalIgnoreCase))
                throw new MediafireException(responseJson["error"].Get<int>(), responseJson["message"].Get<string>());

            return responseJson;
        }
    }
}
