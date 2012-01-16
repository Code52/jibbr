using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Jabbot;
using Jabbot.Sprockets.Core;
using TweetSharp;

namespace TwitterAnnouncer
{
	public class TwitterAnnouncer : IAnnounce
	{
		private static readonly string ConsumerKey = ConfigurationManager.AppSettings["Twitter.ConsumerKey"];
		private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["Twitter.ConsumerSecret"];
		private static readonly string Token = ConfigurationManager.AppSettings["Twitter.Token"];
		private static readonly string TokenSecret = ConfigurationManager.AppSettings["Twitter.TokenSecret"];
		private static readonly string TwitterUserName = ConfigurationManager.AppSettings["Twitter.UserName"];

		private readonly int tweetLimit = 5;
		private DateTime lastRun = DateTime.Now;
		private TwitterStatus latestMention;
		private TwitterStatus latestTweet;
		private int mentionLimit = 3;

		public TimeSpan Interval
		{
			get { return TimeSpan.FromMinutes(5); }
		}

		public void Execute(Bot bot)
		{
			var now = DateTime.Now;

			Debug.WriteLine(string.Format("Fetching from tha twatters! - {0:HH.mm.ss} < {1:HH.mm.ss}", lastRun, now));

			lastRun = now;

			var twitterService = new TwitterService(ConsumerKey, ConsumerSecret);

			twitterService.AuthenticateWith(Token, TokenSecret);

			List<TwitterStatus> latestTweets;

			if (latestTweet == null)
			{
				latestTweets = twitterService.ListTweetsOnUserTimeline(tweetLimit).ToList();
			}
			else
			{
				latestTweets = twitterService.ListTweetsOnUserTimelineSince(latestTweet.Id, tweetLimit).ToList();
			}

			if (latestTweets.Any())
			{
				latestTweet = latestTweets.First();

				foreach (var room in bot.Rooms)
				{
					bot.Say(string.Format("Latests tweets from @{0}", TwitterUserName), room);

					foreach (var tweet in latestTweets)
					{
						bot.Say(tweet.TextDecoded, room);
					}
				}
			}

			List<TwitterStatus> latestMentions;

			if (latestMention == null)
			{
				latestMentions = twitterService.ListTweetsMentioningMe(mentionLimit).ToList();
			}
			else
			{
				latestMentions = twitterService.ListTweetsMentioningMeSince(latestMention.Id, mentionLimit).ToList();
			}

			if (latestMentions.Any())
			{
				latestMention = latestMentions.First();

				foreach (var room in bot.Rooms)
				{
					bot.Say(string.Format("Latest mentions of @{0}", "cyberzeddk"), room);

					foreach (var tweet in latestMentions)
					{
						bot.Say(tweet.TextDecoded, room);
					}
				}
			}
		}
	}
}