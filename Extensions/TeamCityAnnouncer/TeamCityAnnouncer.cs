using System;
using System.Collections.Generic;
using System.Linq;
using Jabbot;
using Jabbot.Sprockets.Core;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;
using TeamCityAnnouncer.Extensions;

namespace TeamCityAnnouncer
{
    public class TeamCityAnnouncer : IAnnounce
    {
        public const string ipAddressOrHostNameOfCCServer = "teamcity.codebetter.com";

        public static readonly string[] Projects = { };
        public static Dictionary<Project, Build> MappedProjects = new Dictionary<Project, Build>();

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(10); }
        }

        private void NotifyRooms(Bot bot, Project project, Build lastBuild)
        {
            foreach (var room in bot.Rooms)
            {
                bot.Say(
                    string.Format("TeamCity Build Server: {0} / {1} - {2} ({3}) ",
                                  project.WebUrl,
                                  project.Name,
                                  lastBuild.Status,
                                  lastBuild.FormattedStartDate()), room);
            }
        }

        public void Execute(Bot bot)
        {
            var client = new TeamCityClient(ipAddressOrHostNameOfCCServer);
            client.Connect("teamcitysharpuser", "qwerty");
            var projects = client.AllProjects();

            foreach (var project in projects)
            {
                if (Projects.Contains(project.Name))
                {
                    var mappedProject = MappedProjects.FirstOrDefault(x => x.Key.Name == project.Name);
                    if (mappedProject.Key == null)
                    {
                        var build = client.GetLastBuild(project);
                        MappedProjects.Add(project, build);
                        NotifyRooms(bot, project, build);
                    }
                    else
                    {
                        var mappedProjectBuild = mappedProject.Value;
                        var onlineProjectBuild = client.GetLastBuild(project);
                        if (mappedProjectBuild.StartDate != onlineProjectBuild.StartDate)
                        {
                            MappedProjects.Remove(mappedProject.Key);
                            MappedProjects.Add(project, onlineProjectBuild);
                            NotifyRooms(bot, project, onlineProjectBuild);
                        }
                    }
                }
            }
        }
    }
}
