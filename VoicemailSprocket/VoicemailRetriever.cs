using System;
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
        private readonly VoicemailRecorder voicemailRecorder;
        internal const string RetrieveCommand = "retrieve";
        public IList<string> Voicemails = new List<string>();

        public VoicemailRetriever(VoicemailRecorder voicemailRecorder)
        {
            if (voicemailRecorder == null) throw new ArgumentNullException("voicemailRecorder");
            this.voicemailRecorder = voicemailRecorder;
        }

        public override Regex Pattern
        {
            get { return new Regex(RetrieveCommand);}
        }

        public int VoicemailCount { get { return Voicemails.Count; } }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            SendAllVoicemailsToUser(message.Sender, bot);
        }

        private void SendAllVoicemailsToUser(string recipient, IBot bot)
        {
            foreach (var voicemail in voicemailRecorder.Voicemails)
                bot.PrivateReply(recipient, string.Format("{0} said '{1}'", voicemail.Sender, voicemail.Message));
        }
    }
}