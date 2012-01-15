using System.Collections.Generic;
using System.ComponentModel.Composition;
using GithubAnnouncements.Extensions;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    public class WatchersTask : IGitHubTask
    {
        const string ProjectWatchersFeed = "/watchers";

        const string WatchersKey = "Watchers";

        readonly ISettingsService _settings;

        [ImportingConstructor]
        public WatchersTask(ISettingsService settings)
        {
            _settings = settings;
        }

        public string Name
        {
            get { return "Check Watchers"; }
        }

        public void ExecuteTask(IBot bot, string baseUrl, string repositoryName)
        {
            var url = baseUrl.Append(ProjectWatchersFeed);
            var currentWatchers = url.GetResponse<IEnumerable<dynamic>>();
            var existingWatchers = _settings.GetValue<IList<string>>(WatchersKey, () => new List<string>());

            existingWatchers = bot.ProcessWatchers(repositoryName, existingWatchers, currentWatchers);

            _settings.Set(WatchersKey, existingWatchers);
            _settings.Save();
        }
    }
}
