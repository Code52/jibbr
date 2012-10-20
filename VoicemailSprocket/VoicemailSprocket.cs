using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace VoicemailSprocket
{
    public class VoicemailSprocket : ISprocket
    {
        public VoicemailSprocket()
        {
            voiceMailbox = new VoiceMailbox();
            userRegistry = new UserRegistry(voiceMailbox);
        }

        private readonly VoiceMailbox voiceMailbox;
        private readonly UserRegistry userRegistry;

        public bool Handle(ChatMessage message, IBot bot)
        {
            return userRegistry.Handle(message, bot) ||
                   voiceMailbox.Handle(message, bot);
        }
    }
}