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
    public class GithubAnnouncements : IAnnounce
    {
        const string UrlFormat = "https://api.github.com/repos/{0}/{1}";
        const string ProjectCommitsFeed = "/commits";
        const string OpenPullRequestsFeed = "/pulls?state=open";
        const string ClosedPullRequestsFeed = "/pulls?state=closed";
        const string ProjectWatchersFeed = "/watchers";
        const string ProjectForksFeed = "/forks";
        const string OpenProjectIssuesFeed = "/issues?state=open";
        const string ClosedProjectIssuesFeed = "/issues?state=closed";
        const string LatestCommitKey = "LastCommitSHA";
        const string MergeRequestsKey = "MergeRequests";
        const string IssuesKey = "Issues";
        const string LatestCommentsKey = "Comments";
        const string WatchersKey = "Watchers";
        const string ForkStatusKey = "ForkStatus";
        const string Account = "Code52";
        const string Repo = "jibbr";

        readonly ISettingsService _storage;
        readonly string _apiUrl;
        readonly WebClient _client = new WebClient();

        [ImportingConstructor]
        public GithubAnnouncements(ISettingsService storage)
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
            NotifyLatestCommit(bot);
            NotifyPullRequests(bot);
            NotifyForks(bot);
            NotifyIssues(bot);
            NotifyWatchers(bot);
        }

        private void NotifyLatestCommit(Bot bot)
        {
            var commits = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectCommitsFeed)).ToList();
            var latestCommit = commits.FirstOrDefault();
            if (latestCommit == null) return;

            var lastCommit = GetValue(LatestCommitKey, () => "");
            var latestSHA = latestCommit.commit.tree.sha.ToString();

            if (lastCommit == latestSHA)
                return;

            _storage.Set(LatestCommitKey, latestSHA);
            _storage.Save();

            bot.ProcessCommits(RepositoryName, lastCommit, commits);
        }

        private void NotifyPullRequests(Bot bot)
        {
            var openPullRequests = GetResponse<IEnumerable<dynamic>>(GetFullUrl(OpenPullRequestsFeed));
            var closedPullRequests = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ClosedPullRequestsFeed));

            var existingPullRequests = GetValue<IDictionary<int, string>>(MergeRequestsKey, () => new Dictionary<int, string>());

            bot.ProcessClosedPullRequests(closedPullRequests, existingPullRequests);
            bot.ProcessOpenPullRequests(RepositoryName, existingPullRequests, openPullRequests);

            _storage.Set(MergeRequestsKey, existingPullRequests);
            _storage.Save();
        }

        private void NotifyForks(Bot bot)
        {
            var forks = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectForksFeed));

            var feeds = forks.ToDictionary(f => f.owner.login, f => f.url);
            var existingForkStatus = GetValue<IDictionary<string, string>>(ForkStatusKey, () => new Dictionary<string, string>());
            
            foreach (var fork in feeds)
            {
                NotifyForkStatus(fork, existingForkStatus, bot);
            }

            _storage.Set(ForkStatusKey, existingForkStatus);
            _storage.Save();
        }

        private void NotifyForkStatus(KeyValuePair<dynamic, dynamic> fork, IDictionary<string, string> existingForkStatus, Bot bot)
        {
            string id = fork.Key.ToString();
            string url = fork.Value.ToString() + "/commits";
            var commits = GetResponse<IEnumerable<dynamic>>(url).ToList();
            bot.ProcessForkStatus(id, existingForkStatus, commits);
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
            var existingWatchers = GetValue<IList<string>>(WatchersKey, () => new List<string>());

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
        
        private string RepositoryName
        {
            get { return string.Format("{0}/{1}", Account, Repo); }
        }

        private T GetValue<T>(string key, Func<T> defaultValue)
        {
            return _storage.ContainsKey(key) ? _storage.Get<T>(key) : defaultValue();
        }
    }
}