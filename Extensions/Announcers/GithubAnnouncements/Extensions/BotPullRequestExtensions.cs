using System.Collections.Generic;
using Jabbot;

namespace GithubAnnouncements.Extensions
{
    public static class BotPullRequestExtensions
    {
        public static void ProcessClosedPullRequests(this IBot bot, IEnumerable<dynamic> closedPullRequests, IDictionary<int, string> existingPullRequests)
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

        public static void ProcessOpenPullRequests(this IBot bot, string repositoryName, IDictionary<int, string> existingPullRequests, IEnumerable<dynamic> openPullRequests)
        {
            foreach (var request in openPullRequests)
            {
                int id;
                if (!int.TryParse(request.number.ToString(), out id))
                    continue;

                if (existingPullRequests.ContainsKey(id))
                    continue;

                // send message
                string firstLine = string.Format("github: {0} has opened a pull request named '{1}' for {2}",
                                                 request.user.login,
                                                 request.title,
                                                 repositoryName);
                string secondLine = string.Format("Please review the request at {0}", request.html_url);
                bot.SayToAllRooms(firstLine);
                bot.SayToAllRooms(secondLine);

                // track request
                existingPullRequests.Add(id, "open");
            }
        }

    }
}
