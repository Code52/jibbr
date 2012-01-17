using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;

namespace TeamCityAnnouncer.Extensions
{
    static class TeamCitySharpEx
    {
        public static Build GetLastBuild(this TeamCityClient client, Project project)
        {
            var buildConfig = client.BuildConfigsByProjectId(project.Id).FirstOrDefault();
            var typeLocator = BuildTypeLocator.WithId(buildConfig.Id);
            var buildLocator = BuildLocator.WithDimensions(typeLocator);
            var builds = client.BuildsByBuildLocator(buildLocator);
            return builds.FirstOrDefault();
        }

        public static DateTime FormattedStartDate(this Build build)
        {
            return DateTime.ParseExact(build.StartDate, "yyyyMMdd'T'HHmmss-ffff", null);
        }
    }
}
