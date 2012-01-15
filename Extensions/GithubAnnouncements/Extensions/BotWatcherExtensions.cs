using System.Collections.Generic;
using System.Linq;
using Jabbot;

namespace GithubAnnouncements.Extensions
{
    public static class BotWatcherExtensions
    {
        public static IList<string> ProcessWatchers(this IBot bot, string repositoryName, IList<string> existingWatchers, IEnumerable<dynamic> watchers)
        {
            var currentWatchers = watchers.Select(c => c.login.ToString()).Cast<string>().ToList();
            var newWatchers = currentWatchers.Except(existingWatchers).ToList();
            foreach (var w in newWatchers)
            {
                bot.SayToAllRooms(string.Format("{0} is now watching {1}", w, repositoryName));
            }

            var noLongerWatching = existingWatchers.Except(currentWatchers).ToList();
            foreach (var w in noLongerWatching)
            {
                bot.SayToAllRooms(string.Format("{0} is no longer watching {1}", w, repositoryName));
            }

            return existingWatchers.Concat(newWatchers).Except(noLongerWatching).ToList();
        }

    }
}
