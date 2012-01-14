﻿using System.ComponentModel.Composition;
using Jabbot;

namespace GithubAnnouncements
{
    [InheritedExport]
    public interface IGitHubTask
    {
        void ExecuteTask(Bot bot, string account, string repository);
    }
}
