using System;
using System.Net;
using Newtonsoft.Json;

namespace GithubAnnouncements
{
    public static class StringExtensions
    {
        public static T GetResponse<T>(this string url)
        {
            var client = new WebClient();
            var response = client.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(response);
        }

        [Obsolete]
        public static string GetFullUrl(this string feedLink)
        {
            return string.Format("{0}{1}", GitHub.UrlFormat, feedLink);
        }

        public static string Append(this string baseUrl, string action)
        {
            return string.Format("{0}{1}", GitHub.UrlFormat, action);
        }
    }
}