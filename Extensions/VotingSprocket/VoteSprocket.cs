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
        private Regex _pollCommand = new Regex("^poll (?<room>[a-zA-Z0-9_-]+) (?<question>.+)$", RegexOptions.IgnoreCase);
        private Regex _voteCommand = new Regex("^vote (?<vote>[0-9]+)$", RegexOptions.IgnoreCase);
        private Dictionary<string, Dictionary<int, int>> _polls = new Dictionary<string,Dictionary<int,int>>();
        private Dictionary<string, Timer> _pollTimers = new Dictionary<string, Timer>();
        private Dictionary<string, List<string>> _pollBallots = new Dictionary<string, List<string>>();
        private IBot _bot;
        
        public bool Handle(ChatMessage message, IBot bot)
        {
            _bot = bot;

            var pollMatch = _pollCommand.Match(message.Content);
            var voteMatch = _voteCommand.Match(message.Content);
            
            if (pollMatch.Success && message.Receiver != bot.Name) return false;            
            if (!pollMatch.Success && !voteMatch.Success) return false;

            if (pollMatch.Success)
            {
                string room = pollMatch.Groups["room"].Value;
                
                if (_polls.ContainsKey(room))
                {
                    bot.PrivateReply(message.Sender, "A poll is already in effect for " + room +".");
                }
                else if (!bot.Rooms.Contains(room))
                {
                    bot.PrivateReply(message.Sender, "You are not in the room you specified for the poll.");
                    bot.PrivateReply(message.Sender, "To start a poll use: poll <roomname> <question>");
                }
                else
                {
                    _polls.Add(room, new Dictionary<int, int>());
                    _pollBallots.Add(room, new List<string>());
#if DEBUG
                    _pollTimers.Add(room, new Timer(250));
#else
                    _pollTimers.Add(room, new Timer(2 * 60 * 1000));
#endif

                    string broadcast = "A poll has started: " + pollMatch.Groups["question"].Value;
                    bot.Say(broadcast, room);
                    bot.Say("Poll will close in 2 minutes. Public reply with vote 1 or vote 2 etc...", room);

                    _pollTimers[room].Elapsed += (s, e) => ClosePollAndSendResults(room);
                    _pollTimers[room].Start();
                }
            }
            else if (voteMatch.Success && _polls.ContainsKey(message.Receiver))
            {
                int vote = Convert.ToInt16(voteMatch.Groups["vote"].Value);
                if (_pollBallots[message.Receiver].Contains(message.Sender))
                {
                    bot.PrivateReply(message.Sender, "You can only cast one vote for the current poll in this room.");
                }
                else
                {
                    _pollBallots[message.Receiver].Add(message.Sender);

                    if (!_polls[message.Receiver].ContainsKey(vote)) _polls[message.Receiver].Add(vote, 0);

                    _polls[message.Receiver][vote]++;
                }
            }
            else if (voteMatch.Success && message.Receiver == bot.Name)
            {
                bot.PrivateReply(message.Sender, "You must cast your vote publicy in the room the poll is in.");
            }
            else if (voteMatch.Success && !_pollBallots.ContainsKey(message.Receiver))
            {
                bot.PrivateReply(message.Sender, "There is no active poll for you to vote on.");
                bot.PrivateReply(message.Sender, "To start a poll use: poll <roomname> <question>");
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
