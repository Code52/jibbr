using System;

namespace Jabbot.CommandSprockets
{
    public interface IAnnounce
    {
        TimeSpan Interval { get; }
        void Execute(Bot bot);
    }
}
