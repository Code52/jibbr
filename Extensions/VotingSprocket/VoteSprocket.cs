using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot.Sprockets.Core;
using Jabbot.Models;
using Jabbot;
using System.Text.RegularExpressions;

namespace VotingSprocket
{
    public class VoteSprocket : IMessageHandler
    {
        private Regex _command = new Regex("^poll (?<room>[a-zA-Z0-9_-]+) (?<question>.+)$");

        public bool Handle(ChatMessage message, IBot bot)
        {
            var pollMatch = _command.Match(message.Content);

            if (pollMatch.Success && message.Receiver != bot.Name) return false;
            if (!pollMatch.Success) return false;

            string broadcast = "A poll has started: " + pollMatch.Groups["question"];
            bot.Say(broadcast, pollMatch.Groups["room"].ToString());

            return true;
        }
    }
}
