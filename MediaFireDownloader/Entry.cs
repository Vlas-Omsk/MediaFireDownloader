using System;
using System.Collections.Generic;
using System.Text;
using MediaFireDownloader.WebRequests;

namespace MediaFireDownloader
{
    abstract class Entry
    {
        public string Key { get; private set; }
        public string Name { get; set; }
        public string Destination { get; set; }

        protected Web web = new Web();

        public Entry(string key, string name)
        {
            Key = key;
            Name = name;
            Destination = ".";
        }
    }
}
