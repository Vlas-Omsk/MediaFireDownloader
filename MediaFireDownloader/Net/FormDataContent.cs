using System;
using System.Net.Http;
using System.Text;

namespace MediaFireDownloader.Net
{
    internal sealed class FormDataContent : StringContent
    {
        private const string _defaultMediaType = "application/x-www-form-urlencoded";

        public FormDataContent(FormData data) : this(data, Encoding.UTF8, _defaultMediaType)
        {
        }

        public FormDataContent(FormData data, Encoding encoding) : this(data, encoding, _defaultMediaType)
        {
        }

        public FormDataContent(FormData data, Encoding encoding, string mediaType)
            : base(data.ToString(), encoding, mediaType)
        {
        }
    }
}
