using PinkJson2;
using System;

namespace MediaFireDownloader.Models
{
    internal sealed class FileEntry
    {
        [JsonProperty("quickkey")]
        public string Key { get; set; }
        [JsonProperty("filename")]
        public string Name { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("size")]
        public ulong Size { get; set; }
    }
}
