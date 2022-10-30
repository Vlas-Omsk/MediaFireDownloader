using PinkJson2;
using System;

namespace MediaFireDownloader.Models
{
    internal sealed class FolderEntry : IEntry
    {
        [JsonProperty("folderkey")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
