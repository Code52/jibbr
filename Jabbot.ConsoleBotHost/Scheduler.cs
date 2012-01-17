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

		public void Start(IEnumerable<IAnnounce> tasks, Bot bot)
		{
			_bot = bot;

			var startTime = DateTime.Now;
			foreach (var task in tasks)
			{
				Debug.WriteLine(string.Format("Adding: {0}", task.AnnouncerName));

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

				announcement.Execute(_bot);
				_scheduledAnnouncements[announcement] = now.Add(announcement.Interval);
			}
		}

		public void Stop()
		{
			_timer.Stop();
		}
	}
}