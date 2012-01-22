using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Jabbot;
using Jabbot.Sprockets.Core;
using Newtonsoft.Json;

namespace BitbucketAnnouncer
{
    public class BitbucketAnnouncer : IAnnounce
    {
        public BitbucketAnnouncer()
        {
            Followers = new List<string>();
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        public DateTime LastUpdate_Commits { get; set; }
        public DateTime LastUpdate_PROpen { get; set; }
        public DateTime LastUpdate_PRClosed { get; set; }
        public DateTime LastUpdate_IssuesOpen { get; set; }
        public DateTime LastUpdate_IssuesOther { get; set; }
        public List<string> Followers { get; set; }

        public const string Api = "https://api.bitbucket.org/1.0/repositories/";
        public const string User = "jespern";
        public const string Repository = "django-piston";

        public static bool Lock { get; set; }

        public void Execute(Bot bot)
        {
            if (Lock == true) return;
            Lock = true;

            Debug.WriteLine(string.Format("Fetching from BitBucket! - {0:HH.mm.ss}", DateTime.Now));

            var client = new WebClient();

            ExecuteFollowers(bot, client);
            ExecuteCommits(bot, client);
            ExecutePullRequests(bot, client);
            ExecuteIssues(bot, client);

            Lock = false;
        }

        private void ExecuteCommits(Bot bot, WebClient client)
        {
            var commits = GetCommits(client).ToList();

            foreach (var c in commits.Where(c => DateTime.Parse(c.created_on.ToString()) > LastUpdate_Commits && DateTime.Parse(c.created_on.ToString()) > DateTime.Now.AddDays(-1)).OrderBy(c => DateTime.Parse(c.created_on.ToString())))
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say(
                        string.Format("Commit to {0}/{1} - {2} ({3}) {4}", User, Repository,
                                      c.user == null ? "" : c.user.username.ToString(), DateTime.Parse(c.created_on.ToString()),
                                      c.description == null ? "" : "- " + c.description.ToString()), room);
                }

                LastUpdate_Commits = DateTime.Parse(c.created_on.ToString());
            }
        }

        private static IEnumerable<dynamic> GetCommits(WebClient client)
        {
            var eventsResponse = client.DownloadString(new Uri(string.Format("{0}{1}/{2}/events/?type=commit", Api, User, Repository)));
            var events = JsonConvert.DeserializeObject<dynamic>(eventsResponse).events;

            return events;
        }

        private void ExecutePullRequests(Bot bot, WebClient client)
        {
            var pr = GetOpenedPullRequests(client).ToList();

            foreach (var c in pr.Where(c => DateTime.Parse(c.created_on.ToString()) > LastUpdate_PROpen && DateTime.Parse(c.created_on.ToString()) > DateTime.Now.AddDays(-1)).OrderBy(c => DateTime.Parse(c.created_on.ToString())))
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say(
                        string.Format("Pull request submitted to {0}/{1} - {2} ({3})", User, Repository,
                                      c.user == null ? "" : c.user.username.ToString(), DateTime.Parse(c.created_on.ToString())), room);
                }

                LastUpdate_PROpen = DateTime.Parse(c.created_on.ToString());
            }

            pr = GetClosedPullRequests(client).ToList();

            foreach (var c in pr.Where(c => DateTime.Parse(c.created_on.ToString()) > LastUpdate_PRClosed && DateTime.Parse(c.created_on.ToString()) > DateTime.Now.AddDays(-1)).OrderBy(c => DateTime.Parse(c.created_on.ToString())))
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say(
                        string.Format("Pull request closed {0}/{1} - {2} ({3})", User, Repository,
                                      c.user == null ? "" : c.user.username.ToString(), DateTime.Parse(c.created_on.ToString())), room);
                }

                LastUpdate_PRClosed = DateTime.Parse(c.created_on.ToString());
            }

        }

        private static IEnumerable<dynamic> GetOpenedPullRequests(WebClient client)
        {
            var eventsResponse = client.DownloadString(new Uri(string.Format("{0}{1}/{2}/events/?type=pullrequest_created", Api, User, Repository)));
            var events = JsonConvert.DeserializeObject<dynamic>(eventsResponse).events;

            return events;
        }

        private static IEnumerable<dynamic> GetClosedPullRequests(WebClient client)
        {
            var eventsResponse = client.DownloadString(new Uri(string.Format("{0}{1}/{2}/events/?type=pullrequest_fulfilled", Api, User, Repository)));
            var events = JsonConvert.DeserializeObject<dynamic>(eventsResponse).events;

            return events;
        }

        private void ExecuteIssues(Bot bot, WebClient client)
        {
            var issues = GetOpenedIssues(client).ToList();

            foreach (var i in issues.Where(i => DateTime.Parse(i.created_on.ToString()) > LastUpdate_IssuesOpen && DateTime.Parse(i.created_on.ToString()) > DateTime.Now.AddDays(-1)).OrderBy(i => DateTime.Parse(i.created_on.ToString())))
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say(
                        string.Format("Issue opened {0}/{1} - {2} ({3}) {4}",
                                      User, Repository,
                                      i.reported_by == null ? "" : i.reported_by.username.ToString(), DateTime.Parse(i.created_on.ToString()),
                                      i.content == null ? "" : "- " + i.content.ToString()), room);
                }

                LastUpdate_IssuesOpen = DateTime.Parse(i.created_on.ToString());
            }

            issues = GetOtherIssues(client).ToList();

            foreach (var i in issues.Where(i => DateTime.Parse(i.created_on.ToString()) > LastUpdate_IssuesOther && DateTime.Parse(i.created_on.ToString()) > DateTime.Now.AddDays(-1)).OrderBy(i => DateTime.Parse(i.created_on.ToString())))
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say(
                        string.Format("Issue updated to {0} {1}/{2} - {3} ({4}) {5}", i.@status == null ? "" : "- " + i.@status,
                                      User, Repository,
                                      i.reported_by == null ? "" : i.reported_by.username.ToString(), DateTime.Parse(i.created_on.ToString()),
                                      i.content == null ? "" : "- " + i.content.ToString()), room);
                }

                LastUpdate_IssuesOther = DateTime.Parse(i.created_on.ToString());
            }

        }

        private static IEnumerable<dynamic> GetOpenedIssues(WebClient client)
        {
            var eventsResponse = client.DownloadString(new Uri(string.Format("{0}{1}/{2}/issues/?status=new&status=open", Api, User, Repository)));
            var issues = JsonConvert.DeserializeObject<dynamic>(eventsResponse).issues;

            return issues;
        }

        private static IEnumerable<dynamic> GetOtherIssues(WebClient client)
        {
            var eventsResponse = client.DownloadString(new Uri(string.Format("{0}{1}/{2}/issues/?status=!new", Api, User, Repository)));
            var issues = JsonConvert.DeserializeObject<dynamic>(eventsResponse).issues;

            var list = new List<dynamic>();

            foreach (var i in issues)
            {
                if (i.status.ToString() != "open") list.Add(i);
            }

            return list;
        }

        private void ExecuteFollowers(Bot bot, WebClient client)
        {
            var followers = GetFollowers(client).ToList();

            // Probably the first time run, so don't notify about every follower
            if (Followers.Count == 0 && followers.Count > 5) Followers = followers;

            foreach (var f in followers.Where(f => !Followers.Contains(f)).OrderBy(f => f))
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say(
                        string.Format("{0}/{1} Now followed by {2}", User, Repository, f), room);

                    Followers.Add(f);
                }
            }
        }

        private static IEnumerable<string> GetFollowers(WebClient client)
        {
            var followersResponse = client.DownloadString(new Uri(string.Format("{0}{1}/{2}/followers/", Api, User, Repository)));
            var followers = JsonConvert.DeserializeObject<dynamic>(followersResponse).followers;

            var list = new List<string>();

            foreach (var f in followers)
            {
                list.Add(f.username.ToString());
            }

            return list;
        }

    }
}
