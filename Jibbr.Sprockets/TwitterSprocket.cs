using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Jabbot.CommandSprockets;
using TweetSharp;

namespace Jibbr.Sprockets
{
	public class TwitterSprocket : CommandSprocket
	{
		private static readonly string ConsumerKey = ConfigurationManager.AppSettings["Twitter.ConsumerKey"];
		private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["Twitter.ConsumerSecret"];
		private static readonly string Token = ConfigurationManager.AppSettings["Twitter.Token"];
		private static readonly string TokenSecret = ConfigurationManager.AppSettings["Twitter.TokenSecret"];

		public override IEnumerable<string> SupportedInitiators
		{
			get { yield return "twitter"; }
		}

		public override IEnumerable<string> SupportedCommands
		{
			get
			{
				yield return "read";
			}
		}

		public override bool ExecuteCommand()
		{
			var twitterService = new TwitterService(ConsumerKey, ConsumerSecret);

			twitterService.AuthenticateWith(Token, TokenSecret);

			int tweetLimit = 1;

			if (HasArguments)
			{
				int parsedLimit;

				if (int.TryParse(Arguments[0], out parsedLimit))
				{
					tweetLimit = parsedLimit;
				}
			}

			Bot.Say("fetching " + tweetLimit + " message(s) from ze twitter?", Message.Room);

			var tweets = twitterService.ListTweetsOnSpecifiedUserTimeline("code_52").Take(tweetLimit);

			foreach (var tweet in tweets)
			{
				Bot.Say(tweet.TextDecoded, Message.Room);
			}

			return true;
		}
	}
}