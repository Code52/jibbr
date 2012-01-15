using System.Collections.Generic;
using Jabbot;

namespace GithubAnnouncements.Extensions
{
    public static class BotIssuesExtensions
    {
        public static void ProcessOpenIssues(this IBot bot, string repo, IEnumerable<dynamic> openIssues, IDictionary<int, string> existingIssues)
        {
            foreach (var issue in openIssues)
            {
                int id;
                if (!int.TryParse(issue.number.ToString(), out id))
                    continue;

                if (!existingIssues.ContainsKey(id))
                {
                    // send message
                    string firstLine = string.Format("github: {0} has opened a new issue named '{1}' for {2}",
                                                     issue.user.login,
                                                     issue.title,
                                                     repo);
                    string secondLine = string.Format("View the discussion at {0}", issue.html_url);
                    bot.SayToAllRooms(firstLine);
                    bot.SayToAllRooms(secondLine);

                    // track request
                    existingIssues.Add(id, "open");
                }

                // TODO: check for new comments
                // TODO: track history of comments
            }
        }

        public static void ProcessClosedIssues(this IBot bot, IEnumerable<dynamic> closedIssues, IDictionary<int, string> existingIssues)
        {
            foreach (var issue in closedIssues)
            {
                int id;
                if (!int.TryParse(issue.number.ToString(), out id))
                    continue;

                if (!existingIssues.ContainsKey(id))
                    continue;

                // send message
                string firstLine = string.Format("github: {0}'s issue '{1}' has been closed",
                                                 issue.user.login,
                                                 issue.title);

                bot.SayToAllRooms(firstLine);

                // cleanup request
                existingIssues.Remove(id);
            }
        }

    }
}
