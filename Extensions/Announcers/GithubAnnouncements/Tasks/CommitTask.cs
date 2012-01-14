using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GithubAnnouncements.Extensions;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    public class CommitTask : IGitHubTask
    {
        const string ProjectCommitsFeed = "/commits";
        readonly ISettingsService _settings;

        [ImportingConstructor]
        public CommitTask(ISettingsService settings)
        {
            _settings = settings;
        }

        const string LatestCommitKey = "LastCommitSHA";

        public string Name
        {
            get { return "Check commits"; }
        }

        public void ExecuteTask(IBot bot, string baseUrl, string repositoryName)
        {
            var fullUrl = baseUrl.Append(ProjectCommitsFeed);
            var commits = fullUrl.GetResponse<IEnumerable<dynamic>>().ToList();
            var latestCommit = commits.FirstOrDefault();
            if (latestCommit == null) return;

            var lastCommit = _settings.GetValue(LatestCommitKey, () => "");
            var latestSHA = latestCommit.commit.tree.sha.ToString();

            if (lastCommit == latestSHA)
                return;

            _settings.Set(LatestCommitKey, latestSHA);
            _settings.Save();

            bot.ProcessCommits(repositoryName, lastCommit, commits);
        }
    }
}