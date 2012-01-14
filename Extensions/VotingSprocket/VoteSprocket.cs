using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot.Sprockets.Core;
using Jabbot.Models;
using Jabbot;
using System.Text.RegularExpressions;
using System.Timers;

namespace VotingSprocket
{
    public class VoteSprocket : ISprocket
    {
        private Regex _pollCommand = new Regex("^poll (?<room>[a-zA-Z0-9_-]+) (?<question>.+)$");
        private Regex _voteCommand = new Regex("^vote (?<vote>[0-9]+)$");
        private Dictionary<string, Dictionary<int, int>> _polls = new Dictionary<string,Dictionary<int,int>>();
        private Dictionary<string, Timer> _pollTimers = new Dictionary<string, Timer>();
        private IBot _bot;
        
        public bool Handle(ChatMessage message, IBot bot)
        {
            var pollMatch = _pollCommand.Match(message.Content);
            if (pollMatch.Success && message.Receiver != bot.Name) return false;
            
            var voteMatch = _voteCommand.Match(message.Content);
            if (!pollMatch.Success && !voteMatch.Success) return false;
            if (voteMatch.Success && message.Receiver == bot.Name) return false;

            if (pollMatch.Success)
            {
                _bot = bot;
                string room = pollMatch.Groups["room"].Value;

                if (_polls.ContainsKey(room))
                {
                    bot.PrivateReply(message.Sender, "A poll is already in effect for " + room +".");
                }
                else
                {
                    _polls.Add(room, new Dictionary<int, int>());
#if DEBUG
                    _pollTimers.Add(room, new Timer(250));
#else
                    _pollTimers.Add(room, new Timer(2 * 60 * 1000));
#endif

                    string broadcast = "A poll has started: " + pollMatch.Groups["question"].Value;
                    bot.Say(broadcast, room);
                    bot.Say("Poll will close in 2 minutes.", room);

                    _pollTimers[room].Elapsed += (s, e) => ClosePollAndSendResults(room);
                    _pollTimers[room].Start();
                }
            }
            else if (voteMatch.Success && _polls.ContainsKey(message.Receiver))
            {
                int vote = Convert.ToInt16(voteMatch.Groups["vote"].Value);
                if (!_polls[message.Receiver].ContainsKey(vote)) _polls[message.Receiver].Add(vote, 0);

                _polls[message.Receiver][vote]++;
            }

            return true;
        }

        private void ClosePollAndSendResults(string room)
        {
            _pollTimers[room].Stop();

            if (!_polls[room].Any())
            {
                _bot.Say("No votes were cast for the poll.", room);
            }
            else
            {
                string message = "poll results ";
                foreach (var kvp in _polls[room])
                {
                    message += string.Format("{0} vote{2} for ({1}). ", kvp.Value, kvp.Key, kvp.Value == 1 ? "" : "s");
                }
                _polls.Remove(room);
                _pollTimers.Remove(room);

                _bot.Say(message.Trim(), room);
            }
        }
    }
}
