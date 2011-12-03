namespace Jabbot.Models
{
    public class ChatMessage
    {
        public ChatMessage(string content, string userName)
        {
            Content = content;
            FromUser = userName;
        }

        public string FromUser { get; set; }
        public string Content { get; set; }        
    }
}
