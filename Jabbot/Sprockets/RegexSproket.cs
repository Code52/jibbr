using System;
using System.Text.RegularExpressions;
using Jabbot.Models;
using Jabbot.Sprokets;

namespace Jabbot.Sprockets
{
    public abstract class RegexSproket : ISproket
    {
        public abstract string Pattern { get; }

        public bool Handle(ChatMessage message, Bot bot)
        {
            if (String.IsNullOrEmpty(Pattern))
            {
                return false;
            }

            var regex = new Regex(Pattern);

            Match match;
            if (!(match = regex.Match(message.Content)).Success)
            {
                return false;
            }

            ProcessMatch(match, message, bot);

            return true;
        }

        protected abstract void ProcessMatch(Match match, ChatMessage message, Bot bot);
    }
}
