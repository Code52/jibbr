using System;
using System.Collections.Generic;
using System.Net;
using Jabbot;
using Jabbot.Sprockets.Core;
using Newtonsoft.Json;

namespace GithubAnnouncements
{
    public class GithubAnnouncements : IAnnounce
    {
        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        public void Execute(Bot bot)
        {
            // TODO: query these services
            // fetch commit feed from https://github.com/Code52/jibbr/commits/master.atom
            // outstanding pull requests: https://api.github.com/repos/Code52/jibbr/pulls
            // current issues: https://api.github.com/repos/Code52/jibbr/issues
            // current forks: https://api.github.com/repos/Code52/jibbr/forks
            // current watchers: https://api.github.com/repos/Code52/jibbr/watchers

            var client = new WebClient();
            var commitsResponse = client.DownloadString("https://api.github.com/repos/Code52/jibbr/pulls");

            var commits = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(commitsResponse);

        }
    }
}
