using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace BlogRss
{
    public class BlogFeed : IAnnounce, ISprocket
    {
        private static readonly string _feedList = ConfigurationManager.AppSettings["Blogs.FeedList"];
        private static readonly string _hoursOldThreshold = ConfigurationManager.AppSettings["Blogs.HoursOldThreshold"];

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(60); }
        }

        //TODO: take input about new blogs to add to feed collection
        //TODO: determine how often feed is checked

        /// <summary>
        /// This method uses 2 configuration variables, feed list, and hour threshold see private members
        /// Using these 2, it will download the XML feed from the supplied feeds
        /// Comparing the publish date of each post, if less than X hours old, will announce new posts to the bot.Rooms
        /// If there are no new posts it will be silent
        /// based on a hard coded interval of 90 minutes for each blog feed check
        /// </summary>
        /// <param name="bot"></param>
        public void Execute(Bot bot)
        {
            var userAndUrlPairs = BuildUserNameAndBlogUrlPairs(bot);

            foreach (var pair in userAndUrlPairs)
            {
                //check feed (only 10 most recent)
                var feed = RssReader.ProcessEntireFeed(pair.Url).Take(10);

                //if something new 
                //compose message for Jabbr
                int hoursOld;
                if (!Int32.TryParse(_hoursOldThreshold, out hoursOld))
                    hoursOld = 48; //failed to read from config default to 48 hours

                var newEntries = feed.Where(f => Math.Abs((DateTime.Now - f.PublishDate).TotalHours) < hoursOld).ToList();

                if (newEntries.Any())
                {
                    var makePlural = "";
                    if (newEntries.Count > 1)
                        makePlural = "s";

                    //some formatting about blog post, some meta data 
                    //take this as an opportunity to show the command to add another subscription
                    var message = String.Format("New post{2} from {0}{1}", pair.User, Environment.NewLine, makePlural);
                    foreach (var entry in newEntries)
                    {
                        /*if(String.IsNullOrWhiteSpace(entry.Summary))
                            entry.Summary = entry.PostText.Substring(0, 100) + " ... ";*/
                        message += String.Format("Title: {0}{2}Link: {1}{2}", entry.Title, entry.Url, Environment.NewLine);
                    }

                    //deliver to channels
                    foreach (var room in bot.Rooms)
                    {
                        bot.Say(message, room);
                    }
                }
            }
        }

        public bool Handle(ChatMessage message, Bot bot)
        {
            return true;
        }

        private IEnumerable<UserAndBlogPair> BuildUserNameAndBlogUrlPairs(Bot bot)
        {
            var blogs = _feedList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToList();

            var userAndUrlPairs = new List<UserAndBlogPair>();
            // Sorry this looks fragile, but haven't refactored it yet.
            try
            {
                for (var p = 0; p < blogs.Count(); p++)
                {
                    if (p % 2 == 0)
                        userAndUrlPairs.Add(new UserAndBlogPair { User = blogs[p] });
                    else
                    {
                        userAndUrlPairs.Last().Url = blogs[p];
                    }
                }

                var x = userAndUrlPairs.Count;
            }
            catch (Exception)
            {
                foreach (var room in bot.Rooms)
                {
                    bot.Say("Having some trouble processing user and feed configuration, expecting pairs of users and feeds, comma delimited", room);
                }
            }

            return userAndUrlPairs;
        }

        private class UserAndBlogPair
        {
            public String User { get; set; }
            public String Url { get; set; }
        }
    }
}
