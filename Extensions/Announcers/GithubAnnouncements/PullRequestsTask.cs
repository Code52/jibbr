using System.Collections.Generic;
using Jabbot;

namespace GithubAnnouncements
{
    public class PullRequestsTask : IGitHubTask
    {
        private readonly ISettingsService _settings;

        public PullRequestsTask(ISettingsService settings)
        {
            _settings = settings;
        }

        const string OpenPullRequestsFeed = "/pulls?state=open";
        const string ClosedPullRequestsFeed = "/pulls?state=closed";
        const string MergeRequestsKey = "MergeRequests";

        public void ExecuteTask(Bot bot, string account, string repository)
        {
            var fullUrl = GetFullUrl(OpenPullRequestsFeed);
            var openPullRequests = fullUrl.GetResponse<IEnumerable<dynamic>>();
            var url = GetFullUrl(ClosedPullRequestsFeed);
            var closedPullRequests = url.GetResponse<IEnumerable<dynamic>>();

            var existingPullRequests = _settings.GetValue<IDictionary<int, string>>(MergeRequestsKey, () => new Dictionary<int, string>());
            var name = string.Format("{0}/{1}", account, repository);

            bot.ProcessClosedPullRequests(closedPullRequests, existingPullRequests);
            bot.ProcessOpenPullRequests(name, existingPullRequests, openPullRequests);

            _settings.Set(MergeRequestsKey, existingPullRequests);
            _settings.Save();
        }

        private static string GetFullUrl(string feedLink)
        {
            return string.Format("{0}{1}", GitHub.UrlFormat, feedLink);
        }
    }
}