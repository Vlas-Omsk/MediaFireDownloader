using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace MediaFireDownloader.WebRequests
{
    public class RequestOptions
    {
        public string Url { get; set; }
        public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string Method { get; set; }
        public byte[] Data { get; set; }

        public void AddHeader(string name, object value)
        {
            Headers.Add(name, value.ToString());
        }

        public void AddQueryParam(string key, object value)
        {
            Query.Add(key, value.ToString());
        }

        public void SetData(string data, Encoding encoding)
        {
            if (string.IsNullOrEmpty(data))
                return;
            if (encoding is null)
                encoding = Encoding.UTF8;
            Data = encoding.GetBytes(data);
        }

        public void SetData(Dictionary<string, string> collection, Encoding encoding)
        {
            if (collection.Count > 0)
                SetData(CreateQueryString(collection), encoding);
        }

        private string CreateQueryString(Dictionary<string, string> collection)
        {
            string result = "";

            foreach (var param in collection)
                result += Uri.EscapeDataString(param.Key) + "=" + Uri.EscapeDataString(param.Value) + "&";
            result = result.Substring(0, result.Length - 1);

            return result;
        }

        internal HttpWebRequest GetHttpWebRequest()
        {
            if (!Uri.IsWellFormedUriString(Url, UriKind.RelativeOrAbsolute))
                throw new UriFormatException();
            string url = Url;
            if (Query.Count > 0)
                url += "?" + CreateQueryString(Query);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            if (string.IsNullOrEmpty(Method))
                throw new Exception("Empty method name");
            request.Method = Method;

            if (Headers.Count > 0) {
                foreach (var header in Headers)
                    request.SetOption(header.Key, header.Value);
            }

            if (Data != null && Data.Length > 0 &&
                !Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                !Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(request.ContentType))
                    throw new Exception("Empty Content-Type");

                request.ContentLength = Data.LongLength;
                using (var stream = request.GetRequestStream())
                    stream.Write(Data);
            }

            return request;
        }

        public static RequestOptions GET {
            get
            {
                var options = new RequestOptions()
                {
                    Method = "GET"
                };
                options.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.118 Safari/537.36");
                
                return options;
            }
        }
        public static RequestOptions POST
        {
            get
            {
                var options = new RequestOptions()
                {
                    Method = "POST"
                };
                options.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.118 Safari/537.36");

                return options;
            }
        }
    }
}
