using System.ComponentModel.Composition;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    [InheritedExport]
    public interface IGitHubTask
    {
        void ExecuteTask(Bot bot, string baseUrl, string repositoryName);
    }
}
