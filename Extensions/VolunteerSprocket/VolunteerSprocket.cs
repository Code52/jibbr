using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;

namespace VolunteerSprocket
{
    public class VolunteerSprocket : RegexSprocket
    {
        public override Regex Pattern
        {
            get { return new Regex(@"[-_./""\w\s]*volunteer some[-_./""\w\s]*"); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            Debug.WriteLine("Volunteering!");
            if (message.Content.StartsWith(bot.Name) || message.Content.StartsWith("@" + bot.Name))
            {
                var users = bot.GetUsers(message.Receiver).ToList<dynamic>();

                users.RemoveAll(u => u.Name == bot.Name);

                if(users.Count == 0)
                {
                    bot.Say("Bot, you can't tell yourself to do that", message.Receiver);
                    return;
                }

                var random = new Random();

                var randomUser = random.Next(0, users.Count() - 1);

                bot.Say(string.Format("I volunteer {0} for that!", users[randomUser].Name), message.Receiver);
            }
        }
    }
}
