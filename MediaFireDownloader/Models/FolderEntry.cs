using PinkJson2;
using System;

namespace MediaFireDownloader.Models
{
    internal sealed class FolderEntry
    {
        [JsonProperty("folderkey")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
