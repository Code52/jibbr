using System;
using System.Collections.Generic;
using System.Linq;
using Jabbot;
using Jabbot.Sprockets.Core;
using TeamCitySharp;

namespace TeamCityAnnouncer
{
    public class TeamCityAnnouncer : IAnnounce
    {
        public const string IpAddressOrHostNameOfCCServer = "teamcity.codebetter.com";
        public static readonly string[] Projects = { };
        public static List<ProjectModel> MappedProjects = new List<ProjectModel>();

        private TeamCityClient _client;

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(10); }
        }

        private void NotifyRooms(Bot bot, ProjectModel project)
        {
            foreach (var room in bot.Rooms)
            {
                bot.Say(
                    string.Format("TeamCity Build Server: {0} / {1} - {2} ({3}) ",
                                  project.BuildConfigName,
                                  project.ProjectName,
                                  project.LastBuildTime,
                                  project.LastBuildStatus), room);
            }
        }

        private void Update(Bot bot, ProjectModel project)
        {
            if (Projects.Contains(project.ProjectName))
            {
                if (MappedProjects.All(x => x.ProjectName != project.ProjectName))
                {
                    MappedProjects.Add(project);
                    NotifyRooms(bot, project);
                }
                else
                {
                    var mappedProject = MappedProjects.FirstOrDefault(x => x.ProjectName == project.ProjectName);
                    if (mappedProject != null)
                    {
                        if (project.LastBuildTime != mappedProject.LastBuildTime)
                        {
                            MappedProjects.Remove(mappedProject);
                            MappedProjects.Add(project);
                            NotifyRooms(bot, project);
                        }
                    }
                }
            }
        }

        public void Execute(Bot bot)
        {
            if (string.IsNullOrWhiteSpace(IpAddressOrHostNameOfCCServer) || !Projects.Any()) return;

            _client = new TeamCityClient(IpAddressOrHostNameOfCCServer);
            _client.Connect("teamcitysharpuser", "qwerty");
            var allProjects = _client.AllProjects();
            var allBuildConfigs = _client.AllBuildConfigs();
            
            foreach (var currentProject in allProjects)
            {
                var buildConfigs = allBuildConfigs.Where(buildConfig => currentProject.Id == buildConfig.Project.Id);

                foreach (var currentBuildConfig in buildConfigs)
                {
                    var build = _client.LastBuildByBuildConfigId(currentBuildConfig.Id);

                    var project = new ProjectModel(currentProject.Name, currentProject.Id, currentBuildConfig.Name, build.StartDate,
                                                     build.Status, build.StatusText);
                    Update(bot, project);
                }
            }
        }
    }
}
