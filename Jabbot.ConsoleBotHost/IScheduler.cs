using System.Collections.Generic;
using Jabbot.CommandSprockets;
using Jabbot.Sprockets.Core;

namespace Jabbot.ConsoleBotHost
{
    public interface IScheduler
    {
        void Start(IEnumerable<IAnnounce> tasks, Bot bot);
        void Stop();
    }
}
