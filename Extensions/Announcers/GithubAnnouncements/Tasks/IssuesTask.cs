﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    public class IssuesTask : IGitHubTask
    {
        const string OpenProjectIssuesFeed = "/issues?state=open";
        const string ClosedProjectIssuesFeed = "/issues?state=closed";
        const string IssuesKey = "Issues";

        readonly ISettingsService _settings;

        [ImportingConstructor]
        public IssuesTask(ISettingsService settings)
        {
            _settings = settings;
        }

        public void ExecuteTask(Bot bot, string baseUrl, string repositoryName)
        {
            var url = baseUrl.Append(OpenProjectIssuesFeed);
            var openIssues = url.GetResponse<IEnumerable<dynamic>>();
            url = baseUrl.Append(ClosedProjectIssuesFeed);
            var closedIssues = url.GetResponse<IEnumerable<dynamic>>();

            var existingIssues = _settings.GetValue<IDictionary<int, string>>(IssuesKey, () => new Dictionary<int, string>());

            bot.ProcessClosedIssues(closedIssues, existingIssues);
            bot.ProcessOpenIssues(repositoryName, openIssues, existingIssues);

            _settings.Set(IssuesKey, existingIssues);
            _settings.Save();
        }
    }
}