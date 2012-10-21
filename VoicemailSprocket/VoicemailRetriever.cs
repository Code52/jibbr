using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;

namespace VoicemailSprocket
{
    internal class VoicemailRetriever : RegexSprocket
    {
        internal const string RetrieveCommand = "retrieve";
        public IList<string> Voicemails = new List<string>();

        public override Regex Pattern
        {
            get { return new Regex(RetrieveCommand);}
        }

        public int VoicemailCount { get { return Voicemails.Count; } }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            SendAllVoicemailsToUser()
        }

        private void SendAllVoicemailsToUser(string recipient, IBot bot)
        {
            foreach (var voicemail in Voicemails)
                bot.PrivateReply(recipient, string.Format("@{0} said '{1}'", recipient, voicemail));
        }
    }
}