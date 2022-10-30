using System;

namespace MediaFireDownloader
{
    internal sealed class Config
    {
        public int ThreadsCount { get; private set; } = 20;
        public string Cookies { get; private set; }
        public bool DontUseSsl { get; private set; }
    }
}
