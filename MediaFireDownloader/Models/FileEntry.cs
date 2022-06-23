using System;

namespace MediaFireDownloader.Models
{
    public sealed class FileEntry : Entry
    {
        public string Hash { get; }
        public ulong Size { get; }

        public FileEntry(string key, string hash, string name, ulong size) : base(key, name)
        {
            Hash = hash;
            Size = size;
        }
    }
}
