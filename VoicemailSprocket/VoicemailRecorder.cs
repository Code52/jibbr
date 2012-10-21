using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;

namespace VoicemailSprocket
{
    internal class VoicemailRecorder : RegexSprocket
    {
        internal const string RecordCommand = "record \'.*\'";
        public IList<Voicemail> Voicemails = new List<Voicemail>();

        public override Regex Pattern
        {
            get { return new Regex(RecordCommand);}
        }

        public int VoicemailCount { get { return Voicemails.Count; } }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            Voicemails.Add(new Voicemail(){Sender = message.Sender, Message = message.Content.Split('\'')[1]});
            NotifyAllUsers(bot);
        }

        private void NotifyAllUsers(IBot bot)
        {
            var allUsersInSameRoomsAsJibber = GetAllUsersInSameRoomsAsJibber(bot);
            foreach (var user in allUsersInSameRoomsAsJibber)
                bot.PrivateReply(user, string.Format("{0} has a new voicemail for you. There are {1} in total", bot.Name, Voicemails.Count));
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