using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BlogRss.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            

            try
            {
                var debugOutput = "{0}----------------{0}{1}{0}{2}{0}{3}";

                //var x = RssReader.ProcessEntireFeed(@"http://www.paulstovell.com/feed").Take(4).ToList();

                var _feedList =
                    @"@NickJosevski,http://blog.nick.josevski.com/feed, @shanselman, http://feeds.feedburner.com/ScottHanselman,@code_52,http://code52.org/rss.xml";

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
                catch (Exception ex)
                {
                    var o = ex.Message;
                    throw;
                }

                var feed = RssReader.ProcessEntireFeed(@"http://blog.nick.josevski.com/feed").Take(10);

                var newEntries = feed.Where(f => Math.Abs((DateTime.Now - f.PublishDate).Days) < 7).ToList();

                if (newEntries.Any())
                {
                    var message = String.Format("New post from {0}{1}", "Nick", Environment.NewLine);
                    foreach (var entry in newEntries)
                    {
                        message += String.Format("{0}{1}", entry.Title, Environment.NewLine);
                    }
                }

                //x.ForEach(i => System.Console.WriteLine(debugOutput, Environment.NewLine, i.Title, i.PublishDateText, i.Url));
                /*
                x = RssReader.ProcessEntireFeed(@"http://code52.org/rss.xml").Take(4).ToList();
                x.ForEach(i => System.Console.WriteLine(debugOutput, Environment.NewLine, i.Title, i.PublishDateText, i.Url));
                */
                
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            System.Console.WriteLine("any key to exit");
            System.Console.ReadLine();

        }

        private class UserAndBlogPair
        {
            public String User { get; set; }
            public String Url { get; set; }
        }

    }
}
