using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Jabbot;
using Jabbot.CommandSprockets;
using TweetSharp;

namespace TwitterSprocket
{
	public class TwitterAnnouncer : IAnnounce
	{
		private static readonly string ConsumerKey = ConfigurationManager.AppSettings["Twitter.ConsumerKey"];
		private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["Twitter.ConsumerSecret"];
		private static readonly string Token = ConfigurationManager.AppSettings["Twitter.Token"];
		private static readonly string TokenSecret = ConfigurationManager.AppSettings["Twitter.TokenSecret"];

		private readonly int tweetLimit;
		private readonly string twitterName;
		private DateTime lastRun = DateTime.Now;
		private TwitterStatus latestTweet;

		public TwitterAnnouncer()
		{
			tweetLimit = 5;

			twitterName = "code_52";
		}

		public TimeSpan Interval
		{
			get { return TimeSpan.FromSeconds(10); }
		}

		public void Execute(Bot bot)
		{
			var now = DateTime.Now;

			if (now.Subtract(lastRun) < TimeSpan.FromSeconds(20))
			{
				return;
			}

			Debug.WriteLine(string.Format("Fetching from tha twatters! - {0:HH.mm.ss} < {1:HH.mm.ss}", lastRun, now));

			lastRun = now;

			var twitterService = new TwitterService(ConsumerKey, ConsumerSecret);

			twitterService.AuthenticateWith(Token, TokenSecret);

			List<TwitterStatus> latestTweets;

			if (latestTweet == null)
			{
				latestTweets = twitterService.ListTweetsOnSpecifiedUserTimeline(twitterName).ToList();
			}
			else
			{
				latestTweets = twitterService.ListTweetsOnSpecifiedUserTimelineSince(twitterName, latestTweet.Id).ToList();
			}

			if (!latestTweets.Any())
			{
				return;
			}

			latestTweet = latestTweets.First();

			var tweets = latestTweets.Take(tweetLimit).ToList();

			foreach (var room in bot.Rooms)
			{
				bot.Say(string.Format("Latests tweets from @{0}", twitterName), room);

				foreach (var tweet in tweets)
				{
					bot.Say(tweet.TextDecoded, room);
				}
			}
		}
	}
}