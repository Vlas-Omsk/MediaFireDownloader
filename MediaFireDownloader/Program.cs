using System;
using System.Linq;
using System.Net;
using MediaFireDownloader.Net;
using ProcessArguments;

namespace MediaFireDownloader
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            string folderKey;
            string destination;
            var logger = new ConsoleLogger();
            var argsLength = Array.FindIndex(args, x => x.StartsWith("--"));

            if (argsLength == -1)
                argsLength = args.Length;

            switch (argsLength)
            {
                default:
                    logger.Error("Use MediaFireDownloader <folder key> [destination path]");
                    return;
                case 1:
                    folderKey = args[0];
                    destination = ".";
                    break;
                case 2:
                    folderKey = args[0];
                    destination = args[1];
                    break;
            }

            Config config;

            try
            {
                config = ArgumentsDeserializer.Deserialize<Config>(args.Skip(argsLength));
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is InvalidProcessArgumentException)
                {
                    foreach (var innerEx in ex.InnerExceptions)
                        logger.Error(innerEx.Message);
                    return;
                }

                throw;
            }

            var httpClientBuilder = new HttpClientBuilder()
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:106.0) Gecko/20100101 Firefox/106.0"
            };
            if (!string.IsNullOrEmpty(config.Cookies))
            {
                httpClientBuilder.CookieContainer = ParseCookies(config.Cookies, !config.DontUseSsl);

                var cookies = httpClientBuilder.CookieContainer.GetCookies(new Uri("https://mediafire.com"));

                if (cookies["ukey"] == null || cookies["user"] == null)
                {
                    logger.Error("The cookies line should contain a cookie named 'ukey' and 'user'");
                    return;
                }
            }

            var downloader = new Downloader(logger, config.ThreadsCount, httpClientBuilder.Create(), !config.DontUseSsl);
            downloader.Start(folderKey, destination).GetAwaiter().GetResult();
        }

        private static CookieContainer ParseCookies(string str, bool secure)
        {
            var cookies = new CookieContainer();

            foreach (var cookieStr in str.Split(';'))
            {
                var nameValue = cookieStr.Split('=');

                cookies.Add(new Cookie()
                {
                    Name = nameValue[0].Trim(),
                    Value = nameValue[1].Trim(),
                    Domain = "mediafire.com",
                    Secure = secure,
                });
            }

            return cookies;
        }
    }
}
