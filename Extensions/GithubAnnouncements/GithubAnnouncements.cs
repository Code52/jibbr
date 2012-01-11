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
        const string ProjectCommitsFeed = "/commits";
        const string ProjectPullRequestsFeed = "/pulls";
        const string ProjectWatchersFeed = "/watchers";
        const string ProjectForksFeed = "/forks";
        const string ProjectIssuesFeed = "/issues";

        private readonly ILocalStorage _storage;
        private readonly string _apiUrl;
        readonly WebClient _client = new WebClient();
        
        public GithubAnnouncements(ILocalStorage storage, string apiUrl)
        {
            _storage = storage;
            _apiUrl = apiUrl;
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.Zero; }
        }

        public void Execute(Bot bot)
        {
            var commits = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectCommitsFeed));
            // TODO: message if new commits found

            return;
            var pullRequests = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectPullRequestsFeed));
            // TODO: message if new pull requests raised
            // TODO: message if pull requests merged
            var forks = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectForksFeed));
            // TODO: inspect forks for new commits
            var issues = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectIssuesFeed));
            // TODO: message if new issues raised
            var watchers = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectWatchersFeed));
            // TODO: check for new watchers (or people no longer watching) and notify
        }

        private string GetFullUrl(string feedLink)
        {
            return string.Format("{0}{1}", _apiUrl, feedLink);
        }

        private T GetResponse<T>(string url)
        {
            var response = _client.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
