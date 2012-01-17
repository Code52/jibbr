using System;
using System.ComponentModel.Composition;

namespace Jabbot.Sprockets.Core
{
    /// <summary>
    /// Announcements are tasks that occur periodically, and can manipulate an active Bot
    /// </summary>
    [InheritedExport]
    public interface IAnnounce
    {
    	string AnnouncerName { get; }

        TimeSpan Interval { get; }
        void Execute(Bot bot);
    }
}
