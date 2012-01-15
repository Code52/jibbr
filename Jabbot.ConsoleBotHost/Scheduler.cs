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
            Logger.Write("Scheduler started at {0}", CurrentTime);
            _bot = bot;

            var startTime = DateTime.Now;
            foreach (var task in tasks)
            {
                var nextExecution = startTime.Add(task.Interval);
                Logger.Write("Task {0} should be executing at {1}", task.Name, nextExecution);
                _scheduledAnnouncements.Add(task, nextExecution);
            }

            _timer.Elapsed += HandleResult;
            _timer.Start();
        }

        private void HandleResult(object state, ElapsedEventArgs elapsedEventArgs)
        {
            Logger.Write("Tick occurred at {0}", CurrentTime);
            var now = DateTime.Now;

            var currentItems = _scheduledAnnouncements.Where(c => c.Value <= now).ToList();
            Logger.Write("{0} announcers to process", currentItems.Count);

            foreach (var scheduleItem in currentItems)
            {
                var announcement = scheduleItem.Key;

                try
                {
                    Logger.Write("Announcer '{0}' started at '{1}'", announcement.Name, CurrentTime);
                    announcement.Execute(_bot);
                    Logger.Write("Announcer '{0}' finished at '{1}'", announcement.Name, CurrentTime);
                }
                catch (Exception ex)
                {
                    Logger.Write("Exception occurred: '{0}'", ex.Message);
                    Logger.WriteMessage("Stacktrace: " + ex.StackTrace);
                    Logger.WriteMessage("Announcer Type: " + announcement.GetType());
                }

                var nextExecution = now.Add(announcement.Interval);
                Logger.Write("Task {0} should be executing at {1}", announcement.Name, nextExecution);
                _scheduledAnnouncements[announcement] = nextExecution;
            }

            Logger.WriteMessage("Going to sleep");
        }

        public void Stop()
        {
            Logger.Write("Tick occurred at {0}", CurrentTime);
            _timer.Stop();
        }

        public DateTime CurrentTime { get { return DateTime.Now; } }
    }
}