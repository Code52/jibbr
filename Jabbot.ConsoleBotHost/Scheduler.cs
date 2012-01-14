using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Jabbot.Sprockets.Core;

namespace Jabbot.ConsoleBotHost
{
    public class Scheduler : IScheduler
    {
        private readonly Timer _timer = new Timer { Interval = 60000 };
        private readonly IDictionary<IAnnounce, DateTime> _scheduledAnnouncements = new Dictionary<IAnnounce, DateTime>();
        private Bot _bot;

        public ILogger Logger { get; set; }

        public void Start(IEnumerable<IAnnounce> tasks, Bot bot)
        {
            _bot = bot;

            var startTime = DateTime.Now;
            foreach (var task in tasks)
            {
                _scheduledAnnouncements.Add(task, startTime.Add(task.Interval));
            }

            _timer.Elapsed += HandleResult;
            _timer.Start();
        }

        private void HandleResult(object state, ElapsedEventArgs elapsedEventArgs)
        {
            var now = DateTime.Now;

            var currentItems = _scheduledAnnouncements.Where(c => c.Value < now).ToList();

            foreach (var scheduleItem in currentItems)
            {
                var announcement = scheduleItem.Key;

                try
                {
                    Logger.Write("Announcer '{0}' started at '{1}'", announcement.Name, DateTime.UtcNow);
                    announcement.Execute(_bot);
                    Logger.Write("Announcer '{0}' finished at '{1}'", announcement.Name, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    Logger.Write("Exception occurred: '{0}'", ex.Message);
                    Logger.WriteMessage("Stacktrace: " + ex.StackTrace);
                    Logger.WriteMessage("Announcer Type: " + announcement.GetType());
                }

                _scheduledAnnouncements[announcement] = now.Add(announcement.Interval);
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}