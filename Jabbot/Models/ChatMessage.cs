namespace Jabbot.Models
{
    public class ChatMessage
    {
        public ChatMessage(string content, string userName, string room)
        {
            Content = content;
            FromUser = userName;
            Room = room;
        }

        public string FromUser { get; set; }
        public string Content { get; set; }
        public string Room { get; set; }
    }
}
