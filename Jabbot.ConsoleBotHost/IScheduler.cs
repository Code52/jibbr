using System.Collections.Generic;
using Jabbot.CommandSprockets;

namespace Jabbot.ConsoleBotHost
{
    public interface IScheduler
    {
        void Start(IEnumerable<IAnnounce> tasks, Bot bot);
        void Stop();
    }
}
