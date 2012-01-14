using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GithubAnnouncements.Extensions;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    public class ForksTask : IGitHubTask
    {
        const string ProjectForksFeed = "/forks";
        const string ForkStatusKey = "ForkStatus";

        readonly ISettingsService _settings;

        [ImportingConstructor]
        public ForksTask(ISettingsService settings)
        {
            _settings = settings;
        }

        public string Name
        {
            get { return "Check Forks"; }
        }

        public void ExecuteTask(IBot bot, string baseUrl, string repositoryName)
        {
            var fullUrl = baseUrl.Append(ProjectForksFeed);
            var forks = fullUrl.GetResponse<IEnumerable<dynamic>>();

            var feeds = forks.ToDictionary(f => f.owner.login, f => f.url);
            var existingForkStatus = _settings.GetValue<IDictionary<string, string>>(ForkStatusKey, () => new Dictionary<string, string>());

            foreach (var fork in feeds)
            {
                CheckForkStatus(fork, existingForkStatus, bot);
            }

            _settings.Set(ForkStatusKey, existingForkStatus);
            _settings.Save();
        }

        private static void CheckForkStatus(KeyValuePair<dynamic, dynamic> fork, IDictionary<string, string> existingForkStatus, IBot bot)
        {
            string id = fork.Key.ToString();
            string url = fork.Value.ToString() + "/commits";
            var commits = url.GetResponse<IEnumerable<dynamic>>().ToList();
            bot.ProcessForkStatus(id, existingForkStatus, commits);
        }
    }
}
