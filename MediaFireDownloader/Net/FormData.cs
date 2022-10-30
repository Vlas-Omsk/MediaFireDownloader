using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaFireDownloader.Net
{
    internal sealed class FormData : Dictionary<string, string>
    {
        public override string ToString()
        {
            return string.Join(
                '&',
                this.Select(x =>
                    Uri.EscapeDataString(x.Key) +
                    '=' +
                    Uri.EscapeDataString(x.Value)
                )
            );
        }
    }
}
