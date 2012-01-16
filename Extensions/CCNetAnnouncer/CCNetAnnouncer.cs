using System;
using System.Collections.Generic;
using System.Linq;
using Jabbot;
using Jabbot.Sprockets.Core;
using ThoughtWorks.CruiseControl.Remote;

namespace CCNetAnnouncer
{
    public class CCNetAnnouncer : IAnnounce
    {
        public CCNetAnnouncer()
        {
            Statuses = new List<ProjectStatus>();
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        public const string ipAddressOrHostNameOfCCServer = ""; // Complete this value             
        public readonly string[] Projects = { };
        public List<ProjectStatus> Statuses { get; set; }

        public void Execute(Bot bot)
        {
            if (string.IsNullOrWhiteSpace(ipAddressOrHostNameOfCCServer) || !Projects.Any()) return;

            var client = new CruiseServerHttpClient(string.Format("http://{0}/ccnet/", ipAddressOrHostNameOfCCServer));
            foreach (var projectStatus in client.GetProjectStatus())
            {
                var isNew = false;

                if (Projects.Contains(projectStatus.Name))
                {
                    var status = Statuses.FirstOrDefault(s => s.Name == projectStatus.Name);
                    if (status == null)
                    {
                        status = projectStatus;
                        Statuses.Add(status);
                        isNew = true;
                    }

                    if (status.LastBuildDate != projectStatus.LastBuildDate || isNew)
                    {
                        status.LastBuildDate = projectStatus.LastBuildDate;

                        foreach (var room in bot.Rooms)
                        {
                            bot.Say(
                                string.Format("CCNET Build Server: {0} / {1} - {2} ({3}) - {4}", projectStatus.WebURL,
                                              projectStatus.Name,
                                              projectStatus.BuildStatus, projectStatus.LastBuildLabel,
                                              projectStatus.LastBuildDate.ToString()), room);
                        }
                    }
                }
            }

        }
    }
}
