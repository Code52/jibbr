using System;
using SignalR.Client.Hubs;

namespace Jabbot
{
    public class Bot
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _chat;
        private readonly string _name;
        private readonly string _password;

        public Bot(string url, string name, string password)
        {
            _name = name;
            _password = password;
            _connection = new HubConnection(url);
            _chat = _connection.CreateProxy("JabbR.Chat");
        }

        public event Action Disconnected
        {
            add
            {
                _connection.Closed += value;
            }
            remove
            {
                _connection.Closed -= value;
            }
        }

        public event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// Connects to the chat session
        /// </summary>
        public void Initialize()
        {
            if (!_connection.IsActive)
            {
                _chat.On("addMessage", ProcessMessage);

                // Start the connection and wait
                _connection.Start().Wait();

                // Join the chat
                var success = _chat.Invoke<bool>("Join").Result;

                if (!success)
                {
                    // Setup the name of the bot
                    string nickCommand = String.Format("/nick {0} {1}", _name, _password);
                    _chat.Invoke("send", nickCommand).Wait();   
                }
            }
        }

        /// <summary>
        /// Joins a chat room. Changes this to the active room for future messages.
        /// </summary>
        /// <param name="room">room to join</param>
        public void Join(string room)
        {
            _chat.Invoke("send", "/join " + room).Wait();

            _chat["activeRoom"] = room;
        }

        /// <summary>
        /// Say something to the active room
        /// </summary>
        /// <param name="what">what to say</param>
        public void Say(string what)
        {
            _chat.Invoke("send", what).Wait();
        }

        /// <summary>
        /// Reply to someone
        /// </summary>
        /// <param name="who">the person you want the bot to reply to</param>
        /// <param name="what">what you want the bot to say</param>
        public void Reply(string who, string what)
        {
            Say(String.Format("@{0} {1}", who, what));
        }

        /// <summary>
        /// Disconnect the bot from the chat session
        /// </summary>
        public void ShutDown()
        {
            _connection.Stop();
        }

        private void ProcessMessage(dynamic message)
        {            
            if (MessageReceived != null)
            {
                string content = message.Content;
                string name = message.User.Name;

                // Ignore replies from self
                if (name.Equals(_name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // We're going to process commands for the bot here
                var chatMessage = new ChatMessage(content, name);
                MessageReceived(chatMessage);
            }
        }
    }
}
