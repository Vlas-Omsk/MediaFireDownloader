using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Reflection;

namespace MediaFireDownloader.WebRequests
{
    public static class HttpWebRequestExtensions
    {
        static Dictionary<string, PropertyInfo> OptionProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        static HttpWebRequestExtensions()
        {
            Type type = typeof(HttpWebRequest);

            foreach (PropertyInfo property in type.GetProperties())
                OptionProperties[property.Name] = property;
        }

        public static void SetOption(this HttpWebRequest request, string name, object value)
        {
            string optionName = name.Replace("-", "");
            if (OptionProperties.ContainsKey(optionName))
            {
                PropertyInfo property = OptionProperties[optionName];
                property.SetValue(request, value);
            }
            else
                request.Headers[name] = value.ToString();
        }
    }
}
