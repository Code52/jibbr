using System.ComponentModel.Composition;
using Jabbot;

namespace GithubAnnouncements.Tasks
{
    [InheritedExport]
    public interface IGitHubTask
    {
        string Name { get; }
        void ExecuteTask(IBot bot, string baseUrl, string repositoryName);
    }
}
