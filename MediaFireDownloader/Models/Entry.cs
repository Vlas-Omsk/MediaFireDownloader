using System;

namespace MediaFireDownloader.Models
{
    public abstract class Entry
    {
        public string Key { get; }
        public string Name { get; }

        public Entry(string key, string name)
        {
            Key = key;
            Name = name;
        }
    }
}
