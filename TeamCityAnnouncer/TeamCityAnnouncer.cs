using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot.Sprockets.Core;
using Jabbot;

namespace TeamCityAnnouncer
{
    public class TeamCityAnnouncer : IAnnounce
    {

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(10); }
        }

        public void Execute(Bot bot)
        {
         
        }
    }
}
