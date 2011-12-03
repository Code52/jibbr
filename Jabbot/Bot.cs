using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Jabbot.MessageHandlers;
using SignalR.Client.Hubs;

namespace Jabbot
{
    public class Bot
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _chat;
        private readonly string _name;
        private readonly string _password;
        private readonly ConcurrentDictionary<string, ChatUser> _users = new ConcurrentDictionary<string, ChatUser>(StringComparer.OrdinalIgnoreCase);

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

                _chat.On("leave", OnLeave);

                _chat.On("addUser", OnJoin);

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

            // Extract users from this room and store them locally
            dynamic roomInfo = _chat.Invoke<dynamic>("GetRoomInfo", room).Result;

            foreach (dynamic user in roomInfo.Users)
            {
                AddUser(user);
            }
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
        /// Returns users in the current room
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ChatUser> GetUsers()
        {
            return _users.Values.ToList();
        }

        /// <summary>
        /// Returns user from the current room by name
        /// </summary>
        /// <param name="name">name of the user</param>
        public ChatUser GetUserByName(string name)
        {
            ChatUser user;
            if (_users.TryGetValue(name, out user))
            {
                return user;
            }

            return null;
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

            string content = message.Content;
            string name = message.User.Name;

            // Ignore replies from self
            if (name.Equals(_name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // We're going to process commands for the bot here
            var chatMessage = new ChatMessage(content, name);

            if (MessageReceived != null)
            {
                MessageReceived(chatMessage);
            }

            foreach (var handler in GetMessageHandlers())
            {
                // Stop at the first one that handled the message
                if (handler.Handle(chatMessage, this))
                {
                    break;
                }
            }
        }

        private void OnLeave(dynamic user)
        {
            RemoveUser(user);
        }

        private void OnJoin(dynamic user)
        {
            AddUser(user);
        }

        private void RemoveUser(dynamic user)
        {
            string name = user.Name;
            ChatUser dummy;
            _users.TryRemove(name, out dummy);
        }

        private void AddUser(dynamic user)
        {
            string name = user.Name;
            string hash = user.Hash;
            _users[name] = new ChatUser
            {
                Name = name,
                GravatarHash = hash
            };
        }

        private IList<IMessageHandler> GetMessageHandlers()
        {
            // TODO: Allow passing a directory where we look for commands that get added/removed dynamically
            var container = new CompositionContainer(new AssemblyCatalog(typeof(Bot).Assembly));
            return container.GetExportedValues<IMessageHandler>().ToList();
        }

    }
}
