using System;
using System.Collections.Generic;
using System.Text;
using MediaFireDownloader.WebRequests;
using PinkJson;

namespace MediaFireDownloader
{
    class FolderEntry : Entry
    {
        public bool IsRoot { get; private set; }

        public FolderEntry(string key, string name) : base(key, name)
        {
            IsRoot = true;
        }

        public FolderEntry(string key) : base(key, null)
        {
            SetInfo();
            IsRoot = true;
        }

        private void SetInfo()
        {
            var options = RequestOptions.POST;
            options.Url = "https://www.mediafire.com/api/1.4/folder/get_info.php";
            var data = new Dictionary<string, string>();
            data.Add("recursive", "yes");
            data.Add("folder_key", Key);
            data.Add("response_format", "json");
            options.SetData(data, null);
            options.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            var response = web.SendRequest(options);
            var responseJson = response.GetJson();
            Name = responseJson["response"]["folder_info"]["name"].Get<string>();
        }

        public List<FileEntry> GetFiles()
        {
            List<FileEntry> entries = new List<FileEntry>();
            bool moveNext = true;
            int chunk = 1;

            while (moveNext)
            {
                var options = RequestOptions.GET;
                options.Url = "https://www.mediafire.com/api/1.4/folder/get_content.php";
                options.AddQueryParam("content_type", "files");
                options.AddQueryParam("filter", "all");
                options.AddQueryParam("order_by", "name");
                options.AddQueryParam("order_direction", "asc");
                options.AddQueryParam("chunk", chunk);
                options.AddQueryParam("chunk_size", 1000);
                options.AddQueryParam("version", "1.5");
                options.AddQueryParam("folder_key", Key);
                options.AddQueryParam("response_format", "json");
                var response = web.SendRequest(options);
                var responseJson = response.GetJson();

                foreach (JsonArrayObject file in responseJson["response"]["folder_content"]["files"].Get<JsonArray>())
                {
                    var entry = new FileEntry(
                        file["quickkey"].Get<string>(),
                        file["hash"].Get<string>(),
                        file["filename"].Get<string>(),
                        file["size"].Get<ulong>()
                    );
                    entry.Destination = Destination + "\\" + Name;
                    entries.Add(entry);
                }

                chunk++;
                moveNext = responseJson["response"]["folder_content"]["more_chunks"].Get<string>() == "yes";
            }

            return entries;
        }

        public List<FolderEntry> GetFolders()
        {
            List<FolderEntry> entries = new List<FolderEntry>();
            bool moveNext = true;
            int chunk = 1;

            while (moveNext)
            {
                var options = RequestOptions.GET;
                options.Url = "https://www.mediafire.com/api/1.4/folder/get_content.php";
                options.AddQueryParam("content_type", "folders");
                options.AddQueryParam("filter", "all");
                options.AddQueryParam("order_by", "name");
                options.AddQueryParam("order_direction", "asc");
                options.AddQueryParam("chunk", chunk);
                options.AddQueryParam("chunk_size", 1000);
                options.AddQueryParam("version", "1.5");
                options.AddQueryParam("folder_key", Key);
                options.AddQueryParam("response_format", "json");
                var response = web.SendRequest(options);
                var responseJson = response.GetJson();

                foreach (JsonArrayObject file in responseJson["response"]["folder_content"]["folders"].Get<JsonArray>())
                {
                    var entry = new FolderEntry(
                        file["folderkey"].Get<string>(),
                        file["name"].Get<string>()
                    );
                    entry.IsRoot = false;
                    entry.Destination = Destination + "\\" + Name;
                    entries.Add(entry);
                }

                chunk++;
                moveNext = responseJson["response"]["folder_content"]["more_chunks"].Get<string>() == "yes";
            }

            return entries;
        }
    }
}
