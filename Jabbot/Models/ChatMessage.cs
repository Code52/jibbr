namespace Jabbot.Models
{
	public class ChatMessage
	{
		public ChatMessage(string content, string sender, string receiver)
		{
			Content = content;
			Sender = sender;
			Receiver = receiver;
		}

		public string Sender { get; set; }
		public string Content { get; set; }
		public string Receiver { get; set; }
		public bool IsPrivate { get; set; }
	}
}
