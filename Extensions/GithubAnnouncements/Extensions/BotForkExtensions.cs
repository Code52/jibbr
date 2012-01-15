using System;
using System.Collections.Generic;
using System.Linq;
using Jabbot;

namespace GithubAnnouncements.Extensions
{
    public static class BotForkExtensions
    {
        public static void ProcessForkStatus(this IBot bot, string id, IDictionary<string, string> existingForkStatus, List<dynamic> commits)
        {
            var latestCommit = commits.FirstOrDefault();
            if (latestCommit == null) return;

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
            bot.SayToAllRooms(String.Format("{0} added {1} new commits to his fork", id, newCommits.Count<object>()));
        }
    }
}