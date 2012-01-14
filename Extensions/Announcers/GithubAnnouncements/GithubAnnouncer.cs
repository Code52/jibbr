using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using Jabbot;
using Jabbot.Sprockets.Core;
using Newtonsoft.Json;

namespace GithubAnnouncements
{
    public class GithubAnnouncer : IAnnounce
    {
        const string UrlFormat = "https://api.github.com/repos/{0}/{1}";
        const string ProjectWatchersFeed = "/watchers";
        const string OpenProjectIssuesFeed = "/issues?state=open";
        const string ClosedProjectIssuesFeed = "/issues?state=closed";
        const string IssuesKey = "Issues";
        const string WatchersKey = "Watchers";
        const string Account = "Code52";
        const string Repo = "jibbr";

        readonly ISettingsService _storage;

        readonly string _apiUrl;
        readonly WebClient _client = new WebClient();

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IGitHubTask> Tasks { get; set; }

        [ImportingConstructor]
        public GithubAnnouncer(ISettingsService storage)
        {
            _storage = storage;
            _apiUrl = string.Format(UrlFormat, Account, Repo);
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.Zero; }
        }

        public void Execute(Bot bot)
        {
            var baseUrl = string.Format(GitHub.UrlFormat, Account, Repo);
            foreach (var task in Tasks)
            {
                task.ExecuteTask(bot, baseUrl, RepositoryName);
            }
            
            NotifyIssues(bot);
            NotifyWatchers(bot);
        }

        private void NotifyIssues(Bot bot)
        {
            var openIssues = GetResponse<IEnumerable<dynamic>>(GetFullUrl(OpenProjectIssuesFeed));
            var closedIssues = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ClosedProjectIssuesFeed));

            IDictionary<int, string> existingIssues = new Dictionary<int, string>();

            if (_storage.ContainsKey(IssuesKey))
            {
                existingIssues = _storage.Get<IDictionary<int, string>>(IssuesKey);
            }

            bot.ProcessClosedIssues(closedIssues, existingIssues);
            bot.ProcessOpenIssues(RepositoryName, openIssues, existingIssues);

            _storage.Set(IssuesKey, existingIssues);
            _storage.Save();
        }

        private void NotifyWatchers(Bot bot)
        {
            var currentWatchers = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectWatchersFeed));
            var existingWatchers = _storage.GetValue<IList<string>>(WatchersKey, () => new List<string>());

            existingWatchers = bot.ProcessWatchers(RepositoryName, existingWatchers, currentWatchers);

            _storage.Set(WatchersKey, existingWatchers);
            _storage.Save();
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
        
        private static string RepositoryName
        {
            get { return string.Format("{0}/{1}", Account, Repo); }
        }
    }
}