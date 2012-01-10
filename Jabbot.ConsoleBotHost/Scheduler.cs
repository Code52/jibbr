using System;
using System.Collections.Generic;
using System.Threading;
using Jabbot.CommandSprockets;

namespace Jabbot.ConsoleBotHost
{
    public class Scheduler : IScheduler
    {
        private Timer _timer;
        private IDictionary<IAnnounce, DateTime> scheduledAnnouncements = new Dictionary<IAnnounce, DateTime>();
        private Bot _bot;

        public void Start(IEnumerable<IAnnounce> tasks, Bot bot)
        {
            _bot = bot;

            var startTime = DateTime.Now;
            foreach (var task in tasks)
            {
                scheduledAnnouncements.Add(task, startTime.Add(task.Interval));
            }

            _timer = new Timer(HandleResult, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
        }

        private void HandleResult(object state)
        {
            var now = DateTime.Now;

            foreach (var scheduleItem in scheduledAnnouncements)
            {
                var announcement = scheduleItem.Key;

                if (scheduleItem.Value < now)
                {
                    announcement.Execute(_bot);
                    scheduledAnnouncements[announcement] = now.Add(announcement.Interval);
                }
            }
        }

        public void Stop()
        {
            _timer.Change(TimeSpan.MaxValue, TimeSpan.MaxValue);
        }
    }
}