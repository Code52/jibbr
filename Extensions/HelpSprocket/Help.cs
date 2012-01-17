using System.Linq;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace HelpSprocket
{
	public class Help : ISprocket
	{
		public bool Handle(ChatMessage message, IBot bot)
		{
			var acceptedCommands = new[] { bot.Name + " help", "@" + bot.Name + " help" };

			if (acceptedCommands.Contains(message.Content.Trim()))
			{
				bot.PrivateReply(message.Sender, "A list of commands this bot currently supports:\n\thelp");

				return true;
			}

			return false;
		}

		public string SprocketName
		{
			get { return "Help sprocket"; }
		}
	}
}