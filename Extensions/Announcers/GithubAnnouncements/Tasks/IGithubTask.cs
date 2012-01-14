using System.ComponentModel.Composition;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    [InheritedExport]
    public interface IGitHubTask
    {
        string Name { get; }
        void ExecuteTask(Bot bot, string baseUrl, string repositoryName);
    }
}
