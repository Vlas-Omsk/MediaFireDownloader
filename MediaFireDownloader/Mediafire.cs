using MediaFireDownloader.Models;
using PinkJson2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using WebLib;

namespace MediaFireDownloader
{
    public static class Mediafire
    {
        private const string _apiEndPoint = "http://www.mediafire.com/api/1.4/";

        public static string GetDownloadLink(FileEntry file)
        {
            var request = HttpRequest.GET;
            request.Url = "http://www.mediafire.com/file/" + file.Key;

            using (var response = request.GetResponse())
            {
                var match = Regex.Match(response.GetText(), "\"http://download.*?\"");
                if (!match.Success)
                    throw new Exception("The file is blocked or not available");
                return match.Value.Substring(1, match.Value.Length - 2);
            }
        }

        public static FolderEntry[] GetFolders(FolderEntry folder)
        {
            var entries = new List<FolderEntry>();
            var moveNext = true;
            var chunk = 1;

            while (moveNext)
            {
                var request = HttpRequest.GET;
                request.Url = _apiEndPoint;
                request.SetPath("/folder/get_content.php");
                request.SetQueryParam("content_type", "folders");
                request.SetQueryParam("filter", "all");
                request.SetQueryParam("order_by", "name");
                request.SetQueryParam("order_direction", "asc");
                request.SetQueryParam("chunk", chunk);
                request.SetQueryParam("chunk_size", 1000);
                request.SetQueryParam("version", "1.5");
                request.SetQueryParam("folder_key", folder.Key);
                request.SetQueryParam("response_format", "json");

                IJson json;
                using (var response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetStream()))
                    json = Json.Parse(streamReader);

                TryUnwrapError(json);

                foreach (var file in json["response"]["folder_content"]["folders"].Get<JsonArray>())
                {
                    entries.Add(new FolderEntry(
                        file["folderkey"].Get<string>(),
                        file["name"].Get<string>()
                    ));
                }

                chunk++;
                moveNext = json["response"]["folder_content"]["more_chunks"].Get<string>() == "yes";
            }

            return entries.ToArray();
        }

        public static FileEntry[] GetFiles(FolderEntry folder)
        {
            List<FileEntry> entries = new List<FileEntry>();
            bool moveNext = true;
            int chunk = 1;

            while (moveNext)
            {
                var request = HttpRequest.GET;
                request.Url = _apiEndPoint;
                request.SetPath("/folder/get_content.php");
                request.SetQueryParam("content_type", "files");
                request.SetQueryParam("filter", "all");
                request.SetQueryParam("order_by", "name");
                request.SetQueryParam("order_direction", "asc");
                request.SetQueryParam("chunk", chunk);
                request.SetQueryParam("chunk_size", 1000);
                request.SetQueryParam("version", "1.5");
                request.SetQueryParam("folder_key", folder.Key);
                request.SetQueryParam("response_format", "json");

                IJson json;
                using (var response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetStream()))
                    json = Json.Parse(streamReader);

                TryUnwrapError(json);

                foreach (var file in json["response"]["folder_content"]["files"].Get<JsonArray>())
                {
                    entries.Add(new FileEntry(
                        file["quickkey"].Get<string>(),
                        file["hash"].Get<string>(),
                        file["filename"].Get<string>(),
                        file["size"].Get<ulong>()
                    ));
                }

                chunk++;
                moveNext = json["response"]["folder_content"]["more_chunks"].Get<string>() == "yes";
            }

            return entries.ToArray();
        }

        public static FolderEntry GetFolder(string folderKey)
        {
            var request = HttpRequest.POST;
            request.Url = _apiEndPoint;
            request.SetPath("/folder/get_info.php");
            var data = new Dictionary<string, string>();
            data.Add("recursive", "yes");
            data.Add("folder_key", folderKey);
            data.Add("response_format", "json");
            request.SetData(data, Encoding.UTF8);
            request.SetHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            using (var response = request.GetResponse())
            using (var streamReader = new StreamReader(response.GetStream()))
            {
                var json = Json.Parse(streamReader);
                TryUnwrapError(json);

                return new FolderEntry(
                    folderKey,
                    json["response"]["folder_info"]["name"].Get<string>()
                );
            }
        }

        private static void TryUnwrapError(IJson json)
        {
            var response = json["response"];

            if (response["result"].Get<string>().Equals("error", StringComparison.OrdinalIgnoreCase))
                throw new MediafireException(response["error"].Get<int>(), response["message"].Get<string>());
        }
    }
}
