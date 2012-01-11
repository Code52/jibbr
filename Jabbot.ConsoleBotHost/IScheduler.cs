using System.Collections.Generic;
using Jabbot.Sprockets.Core;

namespace Jabbot.ConsoleBotHost
{
    public interface IScheduler
    {
        void Start(IEnumerable<IAnnounce> tasks, Bot bot);
        void Stop();
    }
}
