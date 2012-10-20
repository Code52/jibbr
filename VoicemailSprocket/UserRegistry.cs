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
        private readonly VoiceMailbox voiceMailbox;
        private readonly IList<string> userNames = new List<string>();

        public override Regex Pattern
        {
            get { return new Regex(UserArrivedNotification, RegexOptions.IgnoreCase); }
        }

        public UserRegistry(VoiceMailbox voiceMailbox)
        {
            if (voiceMailbox == null) throw new ArgumentNullException("voiceMailbox");
            this.voiceMailbox = voiceMailbox;
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            var username = ExtractUsernameFromMessage(message);
            userNames.Add(username);

            voiceMailbox.SendVoiceMails(username, bot);

        }

        private static string ExtractUsernameFromMessage(ChatMessage message)
        {
            var indexOfUserName = "[JABBR] - ".Length;
            var username = message.Content.Substring(indexOfUserName, message.Content.IndexOf(' ', indexOfUserName) - indexOfUserName);
            return username;
        }
    }
}