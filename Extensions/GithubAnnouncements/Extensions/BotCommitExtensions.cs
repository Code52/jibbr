using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot;

namespace GithubAnnouncements.Extensions
{
    public static class BotCommitExtensions
    {
        public static void ProcessCommits(this IBot bot, string repositoryName ,string lastCommit, IEnumerable<dynamic> commits)
        {
            // find new commits since 
            var groupedCommits = commits.TakeWhile(c => c.commit.tree.sha != lastCommit && DateTime.Parse(c.commit.committer.date.ToString()) > DateTime.Now.AddDays(-1)).GroupBy(c => c.committer.login);

            // create string to send as message
            var sb = new StringBuilder();
            sb.AppendFormat("github: there are new commits in the {0} repository\r\n", repositoryName);
            foreach (var committer in groupedCommits)
            {
                sb.AppendFormat("{0} had {1} commits\r\n", committer.Key, committer.Count());
            }

            // split into rows and send to rooms
            var rows = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var row in rows)
                bot.SayToAllRooms(row);
        }
    }
}
