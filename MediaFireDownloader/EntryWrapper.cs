using System;

namespace MediaFireDownloader
{
    internal sealed class EntryWrapper<T>
    {
        public EntryWrapper(string destination, T entry)
        {
            Destination = destination;
            Entry = entry;
        }

        public string Destination { get; }
        public T Entry { get; }
    }
}
