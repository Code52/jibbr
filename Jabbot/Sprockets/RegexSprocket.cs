﻿using System.Text.RegularExpressions;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace Jabbot.Sprockets
{
    public abstract class RegexSprocket : ISprocket
    {
        public abstract Regex Pattern { get; }

        public bool Handle(ChatMessage message, Bot bot)
        {
            if (Pattern == null)
            {
                return false;
            }
            
            Match match;
            if (!(match = Pattern.Match(message.Content)).Success)
            {
                return false;
            }

            ProcessMatch(match, message, bot);

            return true;
        }

        protected abstract void ProcessMatch(Match match, ChatMessage message, Bot bot);
    }
}
