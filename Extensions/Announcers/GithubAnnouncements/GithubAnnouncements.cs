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

        readonly ISettingsService _storage;
        readonly string _account;
        readonly string _repo;
        readonly string _apiUrl;
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
            if (latestCommit == null) return;

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
            sb.AppendFormat("github: there are new commits in the {0}/{1} repository\r\n", _account, _repo);
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
            var openPullRequests = GetResponse<IEnumerable<dynamic>>(GetFullUrl(OpenPullRequestsFeed));
            var closedPullRequests = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ClosedPullRequestsFeed));

            IDictionary<int, string> existingPullRequests = new Dictionary<int, string>();

            if (_storage.ContainsKey(MergeRequestsKey))
            {
                existingPullRequests = _storage.Get<IDictionary<int, string>>(MergeRequestsKey);
            }

            ProcessClosedPullRequests(bot, closedPullRequests, existingPullRequests);

            ProcessOpenPullRequests(bot, existingPullRequests, openPullRequests);

            _storage.Set(MergeRequestsKey, existingPullRequests);
            _storage.Save();
        }

        private static void ProcessClosedPullRequests(Bot bot, IEnumerable<dynamic> closedPullRequests, IDictionary<int, string> existingPullRequests)
        {
            foreach (var request in closedPullRequests)
            {
                int id;
                if (!int.TryParse(request.number.ToString(), out id))
                    continue;

                if (!existingPullRequests.ContainsKey(id))
                    continue;

                // send message
                string firstLine = string.Format("github: {0}'s pull request '{1}' has been closed",
                                                 request.user.login,
                                                 request.title);

                var result = request.merged_at.ToString();
                var secondLine = !string.IsNullOrWhiteSpace(result)
                                     ? "The merge request was accepted. Awesome!"
                                     : "The merge request was not accepted. Sadface!";

                bot.SayToAllRooms(firstLine);
                bot.SayToAllRooms(secondLine);

                // cleanup request
                existingPullRequests.Remove(id);
            }
        }

        private void ProcessOpenPullRequests(Bot bot, IDictionary<int, string> existingPullRequests, IEnumerable<dynamic> openPullRequests)
        {
            foreach (var request in openPullRequests)
            {
                int id;
                if (!int.TryParse(request.number.ToString(), out id))
                    continue;

                if (existingPullRequests.ContainsKey(id))
                    continue;

                // send message
                string firstLine = string.Format("github: {0} has opened a pull request named '{1}' for {2}/{3}",
                                                 request.user.login,
                                                 request.title,
                                                 _account,
                                                 _repo);
                string secondLine = string.Format("Please review the request at {0}", request.html_url);
                bot.SayToAllRooms(firstLine);
                bot.SayToAllRooms(secondLine);

                // track request
                existingPullRequests.Add(id, "open");
            }
        }

        private void NotifyForks(Bot bot)
        {
            var forks = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectForksFeed));

            var feeds = forks.ToDictionary(f => f.owner.login, f => f.url);

            IDictionary<string, string> existingForkStatus = new Dictionary<string, string>();
            if (_storage.ContainsKey(ForkStatusKey))
            {
                existingForkStatus = _storage.Get<IDictionary<string, string>>(ForkStatusKey);
            }

            foreach (var fork in feeds)
            {
                NotifyForkStatus(fork, existingForkStatus, bot);
            }

            _storage.Set(ForkStatusKey, existingForkStatus);
            _storage.Save();
        }

        private void NotifyForkStatus(KeyValuePair<dynamic, dynamic> fork, IDictionary<string, string> existingForkStatus, Bot bot)
        {
            string url = fork.Value.ToString() + "/commits";
            var commits = GetResponse<IEnumerable<dynamic>>(url).ToList();
            var latestCommit = commits.FirstOrDefault();
            if (latestCommit == null) return;

            string id = fork.Key.ToString();

            var latestSHA = latestCommit.commit.tree.sha.ToString();
            if (!existingForkStatus.ContainsKey(id))
            {
                existingForkStatus.Add(id, latestSHA);
                return;
            }

            var lastCommit = existingForkStatus[id];
            if (lastCommit == latestSHA)
                return;

            existingForkStatus[id] = latestSHA;

            // find new commits since 
            var newCommits = commits.TakeWhile(c => c.commit.tree.sha != lastCommit);

            // create string to send as message
            bot.SayToAllRooms(string.Format("{0} added {1} new commits to his fork", id, newCommits.Count()));
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
            var repo = string.Format("{0}/{1}", _account, _repo);
            bot.ProcessOpenIssues(repo, openIssues, existingIssues);

            _storage.Set(IssuesKey, existingIssues);
            _storage.Save();
        }

        private void NotifyWatchers(Bot bot)
        {
            var watchers = GetResponse<IEnumerable<dynamic>>(GetFullUrl(ProjectWatchersFeed));

            var currentWatchers = watchers.Select(c => c.login.ToString()).Cast<string>().ToList();

            IList<string> existingWatchers = new List<string>();
            if (_storage.ContainsKey(WatchersKey))
            {
                existingWatchers = _storage.Get<IList<string>>(WatchersKey);
            }

            var newWatchers = currentWatchers.Except(existingWatchers).ToList();
            foreach (var w in newWatchers)
            {
                var repoName = string.Format("{0}/{1}", _account, _repo);
                bot.SayToAllRooms(string.Format("{0} is now watching {1}", w, repoName));
            }

            var noLongerWatching = existingWatchers.Except(currentWatchers).ToList();
            foreach (var w in noLongerWatching)
            {
                var repoName = string.Format("{0}/{1}", _account, _repo);
                bot.SayToAllRooms(string.Format("{0} is no longer watching {1}", w, repoName));
            }

            existingWatchers = existingWatchers.Concat(newWatchers).Except(noLongerWatching).ToList();

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
    }
}