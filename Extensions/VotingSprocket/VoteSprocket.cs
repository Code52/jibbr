using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot.Sprockets.Core;
using Jabbot.Models;
using Jabbot;

namespace VotingSprocket
{
    public class VoteSprocket : IMessageHandler
    {
        public bool Handle(ChatMessage message, Bot bot)
        {
            if (message.Content.StartsWith("poll") && message.Receiver != bot.Name) return false;

            return true;
        }
    }
}
