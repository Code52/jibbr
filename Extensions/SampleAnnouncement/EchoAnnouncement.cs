using System;
using System.ComponentModel.Composition;
using Jabbot;
using Jabbot.CommandSprockets;

namespace SampleAnnouncement
{
    [Export(typeof(IAnnounce))]
    public class EchoAnnouncement : IAnnounce
    {
        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(1); }
        }

        public void Execute(Bot bot)
        {
            var offset = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            var startingPoint = DateTime.UtcNow;
            var now = startingPoint.Add(offset.BaseUtcOffset);

            if (offset.IsDaylightSavingTime(startingPoint))
                now = now.AddHours(1);

            var message = string.Format("The time in AEDST (Sydney time) is now {0}", now.ToString("hh:mm:ss, dd MMMM yy"));

            foreach (var room in bot.Rooms)
            {
                bot.Say(message, room);
            }
        }
    }
}
