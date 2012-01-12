using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace BlogRss
{
    public class BlogFeed : IAnnounce, ISprocket
    {
        public TimeSpan Interval
        {
            get { return TimeSpan.FromSeconds(30); }
        }

        //TODO: take input about new blogs to add to feed collection
        //TODO: determine how often feed is checked

        public void Execute(Bot bot)
        {
            //check feed

            //if something new 
            //compose message for Jabbr
            var message = "currently no blogs in RSS feed, nothing to report, will report nothing again in 30 seconds.";

            //some formatting about blog post, some meta data
            //take this as an opportunity to show the command to add another subscription

            //deliver to channels
            foreach (var room in bot.Rooms)
            {
                bot.Say(message, room);
            }
        }

        public bool Handle(ChatMessage message, Bot bot)
        {
            return true;
        }
    }
}
