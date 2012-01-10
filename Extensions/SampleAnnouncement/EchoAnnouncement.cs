using System;
using System.Globalization;
using Jabbot;
using Jabbot.CommandSprockets;

namespace SampleAnnouncement
{
    public class EchoAnnouncement : IAnnounce
    {
        public TimeSpan Interval
        {
            get { return TimeSpan.FromHours(1); }
        }

        public void Execute(Bot bot)
        {
            var offset = TimeZoneInfo.FindSystemTimeZoneById("Australian Eastern Standard Time");
            var now = DateTime.UtcNow.Add(offset.BaseUtcOffset);

            var message = string.Format("The time in AEDST (Sydney time) is now {0}", now.ToString("hh:mm:ss, dd MMMM yy"));

            foreach (var room in bot.Rooms)
            {
                bot.Say(message, room);
            }
        }
    }
}
