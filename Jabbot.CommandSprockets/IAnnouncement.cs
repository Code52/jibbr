using System;

namespace Jabbot.CommandSprockets
{
    public interface IAnnouncement
    {
        TimeSpan Interval { get; }
        void Execute(Bot bot);
    }
}
