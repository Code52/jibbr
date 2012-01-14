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

        [ImportingConstructor]
        public GithubAnnouncer(ISettingsService storage)
        {
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.Zero; }
        }

        public void Execute(Bot bot)
        {
            var baseUrl = string.Format(GitHub.UrlFormat, Account, Repo);
            foreach (var task in Tasks)
            {
                task.ExecuteTask(bot, baseUrl, RepositoryName);
            }
        }

        private static string RepositoryName
        {
            get { return string.Format("{0}/{1}", Account, Repo); }
        }
    }
}