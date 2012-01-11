using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using Analects.SettingsService;
using Jabbot;
using Jabbot.Sprockets.Core;
using Newtonsoft.Json;

namespace GithubAnnouncements
{
    public class GithubAnnouncements : IAnnounce
    {
        const string UrlFormat = "https://api.github.com/repos/{0}/{1}";
        const string ProjectCommitsFeed = "/commits";
        const string ProjectPullRequestsFeed = "/pulls";
        const string ProjectWatchersFeed = "/watchers";
        const string ProjectForksFeed = "/forks";
        const string ProjectIssuesFeed = "/issues";
        private const string LatestCommitKey = "LastCommitSHA";

        private readonly ISettingsService _storage;
        private readonly string _account;
        private readonly string _repo;
        private readonly string _apiUrl;
        readonly WebClient _client = new WebClient();

        [ImportingConstructor]
        public GithubAnnouncements(ISettingsService storage)
        {
            _storage = storage;
            _account = "Code52";
            _repo = "jibbr";
            _apiUrl = string.Format(UrlFormat, _account, _repo);
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
            if (latestCommit != null) return;

            var lastCommit = "";
            if (_storage.ContainsKey(LatestCommitKey))
            {
                lastCommit = _storage.Get<string>(LatestCommitKey);
            }

            var latestSHA = latestCommit.commit.tree.sha.ToString();

            if (lastCommit == latestSHA)
                return;

            // set new value;
            _storage.Set(LatestCommitKey, latestSHA);
            _storage.Save();

            // find new commits since 
            var groupedCommits = commits.TakeWhile(c => c.commit.tree.sha != lastCommit).GroupBy(c => c.committer.login);

            // create string to send as message
            var sb = new StringBuilder();
            sb.AppendFormat("There are new commits on the {0}/{1} repository\r\n", _account, _repo);
            foreach (var committer in groupedCommits)
            {
                sb.AppendFormat("{0} had {1} commits\r\n", committer.Key, committer.Count());
            }

            // split into rows and send to rooms
            var rows = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var row in rows)
                bot.SayToAllRooms(row);
        }

        private void NotifyPullRequests(Bot bot)
        {
            var pullRequests = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectPullRequestsFeed));

            // TODO: message if new pull requests raised
            // TODO: message if pull requests merged
        }

       
        private void NotifyForks(Bot bot)
        {
            var forks = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectForksFeed));

            // TODO: inspect forks for new commits
        }


        private void NotifyIssues(Bot bot)
        {
            var issues = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectIssuesFeed));

            // TODO: message if new issues raised
        }


        private void NotifyWatchers(Bot bot)
        {
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
