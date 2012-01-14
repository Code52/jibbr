using TinyMessenger;

namespace Jabbot.AspNetBotHost.Modules
{
    public class TalkMessage : ITinyMessage
    {
        public object Sender { get; set; }
        public string Text { get; set; }
    }
}