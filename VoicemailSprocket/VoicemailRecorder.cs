using System.Collections.Generic;
using System.Linq;
using Jabbot;
using Jabbot.CommandSprockets;

namespace VoicemailSprocket
{
    internal class VoicemailRecorder : CommandSprocket
    {
        private IList<Voicemail> voicemails = new List<Voicemail>();
        
        public int VoicemailCount { get { return voicemails.Count; } }
 
        public override IEnumerable<string> SupportedInitiators
        {
            get { yield return "voicemail"; }
        }

        public override IEnumerable<string> SupportedCommands
        {
            get 
            { 
                yield return "record";
                yield return "clear";
                yield return "retrieve";
            }
        }

        public override bool ExecuteCommand()
        {
            switch (Command)
            {
                case "clear":
                    return ClearAllMessages();
                case "record":
                    return RecordMessage();
                case "retrieve":
                    return Retrieve();
                default:
                    return false;
            }
        }

        private bool Retrieve()
        {
            foreach (var voicemail in voicemails)
                Bot.PrivateReply(Message.Sender, string.Format("{0} said '{1}'", voicemail.Sender, voicemail.Message));

            return false;
        }

        private bool ClearAllMessages()
        {
            voicemails = voicemails.Where(v => v.Sender != Message.Sender).ToList();

            return true;
        }

        private bool RecordMessage()
        {
            voicemails.Add(new Voicemail() { Sender = Message.Sender, Message = Message.Content.Split('\'')[1] });
            NotifyAllUsers(Bot);

            return true;
        }

        private void NotifyAllUsers(IBot bot)
        {
            var allUsersInSameRoomsAsJibber = GetAllUsersInSameRoomsAsJibber(bot);
            foreach (var user in allUsersInSameRoomsAsJibber)
                bot.PrivateReply(user, string.Format("{0} has a new voicemail for you. There are {1} in total", bot.Name, voicemails.Count));
        }

        private static IEnumerable<string> GetAllUsersInSameRoomsAsJibber(IBot bot)
        {
            var rooms = GetRooms(bot);
            var users = rooms.SelectMany<dynamic, dynamic>(r => r.Users);
            var userNames = users.Select(u => u.Name);
            var distinctUserNames = userNames.Distinct().Cast<string>();
            return distinctUserNames;
        }

        private static IEnumerable<dynamic> GetRooms(IBot bot)
        {
            return bot.GetRooms();
        }
    }
}