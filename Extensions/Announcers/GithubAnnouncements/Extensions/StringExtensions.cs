using System;
using System.Net;
using Newtonsoft.Json;

namespace GithubAnnouncements.Extensions
{
    public static class StringExtensions
    {
        private static WebClient localClient = new WebClient();

        public static T GetResponse<T>(this string url)
        {
            lock(localClient)
            {
                var response = localClient.DownloadString(url);
                return JsonConvert.DeserializeObject<T>(response);    
            }
        }

        public static string Append(this string baseUrl, string action)
        {
            return string.Format("{0}{1}", GitHub.UrlFormat, action);
        }
    }
}