using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot.Models;

namespace Jabbot.CommandSprockets
{
    public abstract class CommandSprocket : Sprockets.ISprocket
    {
        public abstract IEnumerable<string> SupportedInitiators { get; }
        public abstract IEnumerable<string> SupportedCommands { get; }
        public abstract bool ExecuteCommand();
        public string CurrentIntitiator { get; protected set; }
        public string CurrentCommand { get; protected set; }
        public string[] CurrentArguments { get; protected set; }
        public ChatMessage CurrentMessage { get; protected set; }
        public Bot Bot { get; protected set; }
        public bool HasArguments { get { return CurrentArguments.Length > 0; } }


        public virtual bool MayHandle(string initiator, string command)
        {
            return SupportedInitiators.Any(i => i.Equals(initiator, StringComparison.OrdinalIgnoreCase)) &&
                SupportedCommands.Any(c => c.Equals(command, StringComparison.OrdinalIgnoreCase));
        }

        public virtual bool Handle(ChatMessage message, Bot bot)
        {
            try
            {

                string[] args = message.Content
                       .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                CurrentIntitiator = args.Length > 0 ? args[0] : string.Empty;
                CurrentCommand = args.Length > 1 ? args[1] : string.Empty;
                CurrentMessage = message;
                Bot = bot;

                if (MayHandle(CurrentIntitiator, CurrentCommand))
                {
                    CurrentArguments = args.Skip(2).ToArray();
                    return ExecuteCommand();
                }
            }
            catch (InvalidOperationException e)
            {
                Bot.PrivateReply(CurrentMessage.FromUser, e.GetBaseException().Message);
            }
            return false;
        }
    }
}
