using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using PinkJson;

namespace MediaFireDownloader.WebRequests
{
    public class Response
    {
        public string Content { get; internal set; }
        public IReadOnlyDictionary<string, string> Headers { get; internal set; }

        internal void SetHeaders(WebHeaderCollection collection)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < collection.Count; ++i)
            {
                string key = collection.GetKey(i);
                foreach (string value in collection.GetValues(i))
                    headers.Add(key, value);
            }
            Headers = headers;
        }

        public Json GetJson()
        {
            return new Json(Content);
        }
    }
}
