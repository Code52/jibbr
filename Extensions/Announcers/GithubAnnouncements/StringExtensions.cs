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
    }
}