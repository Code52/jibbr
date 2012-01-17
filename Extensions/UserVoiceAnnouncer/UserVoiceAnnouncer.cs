using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Jabbot;
using Jabbot.Sprockets.Core;
using Newtonsoft.Json;

namespace UserVoiceAnnouncer
{
    public class UserVoiceAnnouncer : IAnnounce
    {
    	public string AnnouncerName
    	{
    		get { return "UserVoice Announcer"; }
    	}
    	public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        public DateTime LastUpdate { get; set; }

        public const string APIKey = "zQX9WWRrXjaBvyDiOM3A";
        public const string SuggestionsChannel = "code52";
        public const string ForumID = "143105";

        public static bool Lock { get; set; }

        public void Execute(Bot bot)
        {
            if (Lock == true) return;
            Lock = true;

            Debug.WriteLine(string.Format("Fetching from Uservoice! - {0:HH.mm.ss}", DateTime.Now));

            var client = new WebClient();

            var suggestions = GetSuggestions(client).ToList();

            foreach (var suggestion in suggestions.Where(s => DateTime.Parse(s.created_at.ToString()) > LastUpdate && DateTime.Parse(s.created_at.ToString()) > DateTime.Now.AddDays(-1)).OrderBy(s => DateTime.Parse(s.created_at.ToString())))
            {
                    foreach (var room in bot.Rooms)
                    {
                        bot.Say(
                            string.Format("{0}", suggestion.url), room);
                    }

                LastUpdate = DateTime.Parse(suggestion.created_at.ToString());
            }

            Lock = false;
        }

        private static IEnumerable<dynamic> GetSuggestions(WebClient client)
        {
            var uvResponse = client.DownloadString(new Uri(string.Format("http://{0}.uservoice.com/api/v1/forums/{1}/suggestions.json?per_page=50&sort=newest&client={2}", SuggestionsChannel, ForumID, APIKey)));
            var suggestions = JsonConvert.DeserializeObject<dynamic>(uvResponse).suggestions;

            return suggestions;
        }
    }
}
