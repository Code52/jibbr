using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot.Sprockets;
using System.Text.RegularExpressions;
using Jabbot.Models;
using Jabbot;

namespace AutoClrfSprocket
{
    public class AutoclrfSprocket : RegexSprocket
    {
        public override Regex Pattern
        {
            get { return new Regex(@".*(?:autoclrf)+.*", RegexOptions.IgnoreCase); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            bot.Say("http://code52.org/img/week2-day2-autocrlf.jpg", message.Receiver);
            bot.Say("http://code52.org/line-endings.html You need it buddy!", message.Receiver);
        }
    }
}
