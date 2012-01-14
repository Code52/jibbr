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

        public void ExecuteTask(Bot bot, string baseUrl, string repositoryName)
        {
            var fullUrl = baseUrl.Append(OpenPullRequestsFeed);
            var openPullRequests = fullUrl.GetResponse<IEnumerable<dynamic>>();
            var url = baseUrl.Append(ClosedPullRequestsFeed);
            var closedPullRequests = url.GetResponse<IEnumerable<dynamic>>();

            var existingPullRequests = _settings.GetValue<IDictionary<int, string>>(MergeRequestsKey, () => new Dictionary<int, string>());
            
            bot.ProcessClosedPullRequests(closedPullRequests, existingPullRequests);
            bot.ProcessOpenPullRequests(repositoryName, existingPullRequests, openPullRequests);

            _settings.Set(MergeRequestsKey, existingPullRequests);
            _settings.Save();
        }
    }
}