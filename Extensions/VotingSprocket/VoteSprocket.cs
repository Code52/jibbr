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
        private const string Poll_Command_Description = "To start a poll use: poll <roomname> <question>";

        private Regex _pollCommand = new Regex("^poll[ ]*(?<room>[a-zA-Z0-9_-]*)[ ]*(?<question>.*)$", RegexOptions.IgnoreCase);
        private Regex _voteCommand = new Regex("^vote[ ]*(?<vote>[0-9]*)$", RegexOptions.IgnoreCase);

        private Dictionary<string, Poll> _roomPolls = new Dictionary<string, Poll>();
        private IBot _bot;
        
        public bool Handle(ChatMessage message, IBot bot)
        {
            _bot = bot;

            var pollMatch = _pollCommand.Match(message.Content);
            var voteMatch = _voteCommand.Match(message.Content);
            
            if ((!pollMatch.Success && !voteMatch.Success)
                || (pollMatch.Success && message.Receiver != bot.Name)
                ) return false;

            if (pollMatch.Success)
            {
                string room = pollMatch.Groups["room"].Value;
                string question = pollMatch.Groups["question"].Value;

                if (string.IsNullOrWhiteSpace(room) || string.IsNullOrWhiteSpace(question))
                {
                    bot.PrivateReply(message.Sender, Poll_Command_Description);
                }
                else if (_roomPolls.ContainsKey(room))
                {
                    bot.PrivateReply(message.Sender, "A poll is already in effect for " + room + ".");
                }
                else if (!bot.Rooms.Contains(room))
                {
                    bot.PrivateReply(message.Sender, "You are not in the room you specified for the poll.");
                    bot.PrivateReply(message.Sender, Poll_Command_Description);
                }
                else
                {
                    _roomPolls.Add(room, new Poll());

                    string broadcast = "A poll has started: " + question;
                    bot.Say(broadcast, room);
                    bot.Say("Poll will close in 2 minutes. Public reply with vote 1 or vote 2 etc...", room);

                    // I don't like breaking the Law of Demeter here. But adhering to it for the
                    // timer would mean giving the poll the room (okay with) and the bot/ClosePoll.. (not so okay with).
                    // Yes that means something is probably wrong here and I should fix it but it's late and
                    // my mind needs a break.
                    _roomPolls[room].Timer.Elapsed += (s, e) => ClosePollAndSendResults(room);
                    _roomPolls[room].Timer.Start();
                }
            }
            else if (voteMatch.Success)
            {
                string room = message.Receiver;
                string vote = voteMatch.Groups["vote"].Value;

                if (room == bot.Name)
                {
                    bot.PrivateReply(message.Sender, "You must cast your vote publicy in the room the poll is in.");
                } 
                else if (!_roomPolls.ContainsKey(room))
                {
                    bot.PrivateReply(message.Sender, "There is no active poll for you to vote on.");
                    bot.PrivateReply(message.Sender, Poll_Command_Description);
                }                
                else
                {                    
                    if (_roomPolls[room].HasAlreadyVoted(message.Sender))
                    {
                        bot.PrivateReply(message.Sender, "You can only cast one vote for the current poll in this room.");
                    }
                    else
                    {
                        _roomPolls[room].CastBallot(message.Sender, vote);
                    }
                }
            }

            return true;
        }

        private void ClosePollAndSendResults(string room)
        {
            if (!_roomPolls.ContainsKey(room)) return;

            _roomPolls[room].Timer.Stop();

            if (!_roomPolls[room].Votes.Any())
            {
                _bot.Say("No votes were cast for the poll.", room);
            }
            else
            {
                string message = "poll results ";
                foreach (var kvp in _roomPolls[room].Votes)
                {
                    message += string.Format("{0} vote{2} for ({1}). ", kvp.Value, kvp.Key, kvp.Value == 1 ? "" : "s");
                }
                _roomPolls.Remove(room);

                _bot.Say(message.Trim(), room);
            }
        }
    }
}
