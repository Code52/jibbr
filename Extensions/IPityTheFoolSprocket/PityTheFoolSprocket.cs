using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;

namespace IPityTheFoolSprocket
{
	public class PityTheFoolSprocket : RegexSprocket
	{
		public override Regex Pattern
		{
			get { return new Regex(@".*(?:fool|pity)+.*", RegexOptions.IgnoreCase); }
		}

		protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
		{
			bot.Say("http://xamldev.dk/IPityTheFool.gif", message.Receiver);
		}

		public override string SprocketName
		{
			get { return "I Pity the Fool Sprocket"; }
		}
	}
}