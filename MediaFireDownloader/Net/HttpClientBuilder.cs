using System;
using System.Net.Http;
using System.Net;

namespace MediaFireDownloader.Net
{
    internal sealed class HttpClientBuilder
    {
        public string UserAgent { get; set; }
        public CookieContainer CookieContainer { get; set; }

        public HttpClient Create()
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.All
            };
            if (CookieContainer != null)
                handler.CookieContainer = CookieContainer;

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-GB,en-US;q=0.9,en;q=0.8");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            return httpClient;
        }
    }
}
