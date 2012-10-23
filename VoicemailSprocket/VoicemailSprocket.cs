using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace VoicemailSprocket
{
    public class VoicemailSprocket : ISprocket
    {
        public VoicemailSprocket()
        {
            voicemailRecorder = new VoicemailRecorder();
            userRegistry = new UserRegistry(voicemailRecorder);
        }

        private readonly VoicemailRecorder voicemailRecorder;
        private readonly UserRegistry userRegistry;

        public bool Handle(ChatMessage message, IBot bot)
        {
            return userRegistry.Handle(message, bot) ||
                   voicemailRecorder.Handle(message, bot);
        }
    }
}