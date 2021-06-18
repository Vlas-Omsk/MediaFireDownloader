using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MediaFireDownloader.WebRequests;

namespace MediaFireDownloader
{
    class FileEntry : Entry
    {
        public string Hash { get; private set; }
        public ulong Size { get; private set; }

        public FileEntry(string key, string hash, string name, ulong size) : base(key, name)
        {
            Hash = hash;
            Size = size;
        }

        public string GetDownloadLink()
        {
            var options = RequestOptions.GET;
            options.Url = "https://www.mediafire.com/file/" + Key;
            var response = web.SendRequest(options);
            var match = Regex.Match(response.Content, "\"https://download.*\"");
            return match.Value.Substring(1, match.Value.Length - 2);
        }
    }
}
