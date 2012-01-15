using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GithubAnnouncements.Tasks;
using Jabbot;
using Jabbot.Sprockets.Core;

namespace GithubAnnouncements
{
    public class GithubAnnouncer : IAnnounce
    {
        const string Account = "Code52";
        const string Repo = "jibbr";

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IGitHubTask> Tasks { get; set; }

        [Import]
        public ILogger Log { get; set; }

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(10); }
        }

        public string Name
        {
            get { return "Github Bot"; }
        }

        public void Execute(Bot bot)
        {
            var baseUrl = string.Format(GitHub.UrlFormat, Account, Repo);
            foreach (var task in Tasks)
            {
                Log.Write("Task '{0}' started at '{1}'", task.Name, DateTime.UtcNow);
                try
                {
                    task.ExecuteTask(bot, baseUrl, RepositoryName);
                }
                catch (Exception ex)
                {
                    Log.Write("Exception occurred: '{0}'", ex.Message);
                    Log.WriteMessage("Stack Trace: " + ex.StackTrace);
                    Log.WriteMessage("Announcer Type: " + task.GetType());
                }
                
                Log.Write("Task '{0}' finished at '{1}'", task.Name, DateTime.UtcNow);
            }
        }

        private static string RepositoryName
        {
            get { return string.Format("{0}/{1}", Account, Repo); }
        }
    }
}