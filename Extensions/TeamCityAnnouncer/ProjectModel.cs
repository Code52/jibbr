using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityAnnouncer
{
    public class ProjectModel
    {
        public string ProjectName { get; private set; }
        public string ProjectId { get; private set; }
        public string BuildConfigName { get; private set; }
        public string LastBuildTime { get; private set; }
        public string LastBuildStatus { get; private set; }
        public string LastBuildStatusText { get; private set; }

        public ProjectModel(string projectName, string projectId, string buildConfigName, string lastBuildTime, string lastBuildStatus, string lastBuildStatusText)
        {
            this.ProjectName = projectName;
            this.ProjectId = projectId;
            this.BuildConfigName = buildConfigName;
            this.LastBuildTime = DateTime.ParseExact(lastBuildTime, "yyyyMMdd'T'HHmmss-ffff", System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss");
            this.LastBuildStatus = lastBuildStatus;
            this.LastBuildStatusText = lastBuildStatusText;
        }
    }
}
