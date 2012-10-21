using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;

namespace VoicemailSprocket
{
    internal class UserRegistry : RegexSprocket
    {
        public const string UserArrivedNotification = @"\[JABBR\] - .* just entered .*";
        private readonly VoicemailRecorder voicemailRecorder;
        private readonly IList<string> userNames = new List<string>();

        public override Regex Pattern
        {
            get { return new Regex(UserArrivedNotification, RegexOptions.IgnoreCase); }
        }

        public UserRegistry(VoicemailRecorder voicemailRecorder)
        {
            if (voicemailRecorder == null) throw new ArgumentNullException("voicemailRecorder");
            this.voicemailRecorder = voicemailRecorder;
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            var username = ExtractUsernameFromMessage(message);
            
            userNames.Add(username);

            NotifyOfWaitingVoiceMails(username, bot);

        }
        private void NotifyOfWaitingVoiceMails(string username, IBot bot)
        {
            bot.PrivateReply(username, string.Format("{0} has {1} new voicemail for you", bot.Name, voicemailRecorder.VoicemailCount));
        }

        private static string ExtractUsernameFromMessage(ChatMessage message)
        {
            var indexOfUserName = "[JABBR] - ".Length;
            var username = message.Content.Substring(indexOfUserName, message.Content.IndexOf(' ', indexOfUserName) - indexOfUserName);
            return username;
        }
    }
}