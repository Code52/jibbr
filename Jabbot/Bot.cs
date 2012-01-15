using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Jabbot.Models;
using Jabbot.Sprockets.Core;
using SignalR.Client.Hubs;

namespace Jabbot
{
    public class Bot : IBot
    {
        readonly HubConnection _connection;
        readonly IHubProxy _chat;
        readonly string _password;
        readonly List<ISprocket> _sprockets = new List<ISprocket>();
        readonly List<IUnhandledMessageSprocket> _unhandledMessageSprockets = new List<IUnhandledMessageSprocket>();
        readonly HashSet<string> _rooms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Bot(string url, string name, string password)
        {
            Name = name;
            _password = password;
            _connection = new HubConnection(url);
            _chat = _connection.CreateProxy("JabbR.Chat");
        }

        public string Name { get; private set; }

        public ICredentials Credentials
        {
            get { return _connection.Credentials; }
            set { _connection.Credentials = value; }
        }

        public event Action Disconnected
        {
            add { _connection.Closed += value; }
            remove { _connection.Closed -= value; }
        }

        public event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// Add a sprocket to the bot instance
        /// </summary>
        public void AddSprocket(ISprocket sprocket)
        {
            _sprockets.Add(sprocket);
        }

        /// <summary>
        /// Remove a sprocket from the bot instance
        /// </summary>
        public void RemoveSprocket(ISprocket sprocket)
        {
            _sprockets.Remove(sprocket);
        }

        /// <summary>
        /// Add a sprocket to the bot instance
        /// </summary>
        public void AddUnhandledMessageSprocket(IUnhandledMessageSprocket sprocket)
        {
            _unhandledMessageSprockets.Add(sprocket);
        }

        /// <summary>
        /// Remove a sprocket from the bot instance
        /// </summary>
        public void RemoveUnhandledMessageSprocket(IUnhandledMessageSprocket sprocket)
        {
            _unhandledMessageSprockets.Remove(sprocket);
        }

        /// <summary>
        /// Remove all sprockets
        /// </summary>
        public void ClearSprockets()
        {
            _sprockets.Clear();
        }

        /// <summary>
        /// Connects to the chat session
        /// </summary>
        public void PowerUp(IEnumerable<ISprocketInitializer> sprocketInitializers = null)
        {
            if (!_connection.IsActive)
            {
                _chat.On<dynamic, string>("addMessage", ProcessMessage);
                _chat.On<string, string, string>("sendPrivateMessage", ProcessPrivateMessage);
                _chat.On("leave", OnLeave);
                _chat.On("addUser", OnJoin);
                _chat.On<IEnumerable<string>>("logOn", OnLogOn);

                _chat.On<dynamic, string>("addUser", ProcessRoomArrival);

                // Start the connection and wait
                _connection.Start(SignalR.Client.Transports.Transport.LongPolling).Wait();

                // Join the chat
                var success = _chat.Invoke<bool>("Join").Result;

                if (!success)
                {
                    // Setup the name of the bot
                    Send(String.Format("/nick {0} {1}", Name, _password));

                    if (sprocketInitializers != null)
                        IntializeSprockets(sprocketInitializers);
                }
            }
        }

        /// <summary>
        /// Creates a new room
        /// </summary>
        /// <param name="room">room to create</param>
        public void CreateRoom(string room)
        {
            Send("/create " + room);

            // Add the room to the list
            _rooms.Add(room);
        }

        /// <summary>
        /// Joins a chat room. Changes this to the active room for future messages.
        /// </summary>
        public void Join(string room)
        {
            if (_rooms.Contains(room)) return;

            Send("/join " + room);

            // Add the room to the list
            _rooms.Add(room);
        }

        /// <summary>
        /// Leaves a chat room. 
        /// </summary>
        public void Leave(string room)
        {
            if (!_rooms.Contains(room)) return;

            Send("/leave " + room);

            // Add the room to the list
            _rooms.Remove(room);
        }

        /// <summary>
        /// Sets the Bot's gravatar email
        /// </summary>
        /// <param name="gravatarEmail"></param>
        public void Gravatar(string gravatarEmail)
        {
            Send("/gravatar " + gravatarEmail);
        }

        /// <summary>
        /// Say something to the active room.
        /// </summary>
        /// <param name="what">what to say</param>
        /// <param name="room">the room to say it to</param>
        public void Say(string what, string room)
        {
            try
            {
                // Set the active room
                _chat["activeRoom"] = room;

                Say(what);
            }
            finally
            {
                // Reset the active room to null
                _chat["activeRoom"] = null;
            }
        }

        /// <summary>
        /// Reply to someone
        /// </summary>
        /// <param name="who">the person you want the bot to reply to</param>
        /// <param name="what">what you want the bot to say</param>
        /// <param name="room">the room to say it to</param>
        public void Reply(string who, string what, string room)
        {
            if (who == null)
            {
                throw new ArgumentNullException("who");
            }

            if (what == null)
            {
                throw new ArgumentNullException("what");
            }

            Say(String.Format("@{0} {1}", who, what), room);
        }

        public void PrivateReply(string who, string what)
        {
            if (who == null)
            {
                throw new ArgumentNullException("who");
            }

            if (what == null)
            {
                throw new ArgumentNullException("what");
            }

            Send(String.Format("/msg {0} {1}", who, what));
        }

        /// <summary>
        /// List of rooms the bot is in.
        /// </summary>
        public IEnumerable<string> Rooms
        {
            get
            {
                return _rooms;
            }
        }

        public IEnumerable<dynamic> GetUsers(string room)
        {
            var users = new List<dynamic>();

            var result = _chat.Invoke<dynamic>("getRoomInfo", room).Result;

            if (result == null) throw new Exception("Invalid Room");

            var dynamicusers = result.Users;

            if (dynamicusers == null)
            {
                return users;
            }

            foreach (var u in dynamicusers)
            {
                users.Add(u);
            }

            return users;
        }

        public dynamic GetUserInfo(string room, string user)
        {
            var users = new Dictionary<string, dynamic>();

            var dynamicusers = GetUsers(room);

            foreach (var u in dynamicusers)
            {
                users.Add(u.Name.ToString(), u);
            }

            if (!users.ContainsKey(user)) return null;

            return users[user];
        }

        public IEnumerable<string> GetRoomOwners(string room)
        {
            var owners = new List<string>();

            var result = _chat.Invoke<dynamic>("getRoomInfo", room).Result;

            if (result == null) throw new Exception("Invalid Room");

            foreach (var owner in result.Owners)
            {
                owners.Add(owner.ToString());
            }

            return owners;
        }

        public dynamic GetRooms()
        {
            var result = _chat.Invoke<dynamic>("getRooms").Result;

            return result;
        }

        public void ChangeNote(string note)
        {
            Send(String.Format("/note {0}", note));
        }

        public void Nudge(string user)
        {
            Send(String.Format("/nudge {0}", user));
        }

        public void SendAdministrativeCommand(string command)
        {
            if (!command.StartsWith("/"))
            {
                throw new InvalidOperationException("Only commands are allowed");
            }

            Send(command);
        }

        /// <summary>
        /// Disconnect the bot from the chat session. Leaves all rooms the bot entered
        /// </summary>
        public void ShutDown()
        {
            // Leave all the rooms ever joined
            foreach (var room in _rooms)
            {
                Send(String.Format("/leave {0}", room));
            }

            _connection.Stop();
        }

        private void Say(string what)
        {
            if (what == null)
            {
                throw new ArgumentNullException("what");
            }

            if (what.StartsWith("/"))
            {
                throw new InvalidOperationException("Commands are not allowed");
            }

            Send(what);
        }

        private void ProcessPrivateMessage(string sender, string receiver, string message)
        {
            if (sender.Equals(receiver))
            {
                return;
            }

            var chatMessage = new ChatMessage(WebUtility.HtmlDecode(message), sender, receiver);

            ProcessChatMessages(chatMessage);
        }

        private void ProcessMessage(dynamic message, string room)
        {
            // Run this on another thread since the signalr client doesn't like it
            // when we spend a long time processing messages synchronously
            string content = message.Content;
            string name = message.User.Name;

            // Ignore replies from self
            if (name.Equals(Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // We're going to process commands for the bot here
            var chatMessage = new ChatMessage(WebUtility.HtmlDecode(content), name, room);

            ProcessChatMessages(chatMessage);
        }

        private void ProcessChatMessages(ChatMessage message)
        {
            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine(string.Format("PCM: {0} - {1} - {2}", message.Sender, message.Receiver, message.Content));

                if (MessageReceived != null)
                {
                    MessageReceived(message);
                }

                var handled = false;

                foreach (var handler in _sprockets)
                {
                    if (handler.Handle(message, this))
                    {
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    // Loop over the unhandled message sprockets
                    foreach (var handler in _unhandledMessageSprockets)
                    {
                        // Stop at the first one that handled the message
                        if (handler.Handle(message, this))
                        {
                            break;
                        }
                    }

                }
            })
            .ContinueWith(task =>
            {
                // Just write to debug output if it failed
                if (task.IsFaulted)
                {
                    var aggregateException = task.Exception;
                    if (aggregateException != null)
                        Debug.WriteLine("JABBOT: Failed to process messages. {0}", aggregateException.GetBaseException());
                }
            });
        }

        private void ProcessRoomArrival(dynamic message, string room)
        {
            string name = message.Name;

            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine(string.Format("PCM: {0} - {1}", name, room));

                var handled = false;

                foreach (var handler in _sprockets)
                {
                    if (handler.Handle(new ChatMessage("[JABBR] - " + name + " just entered " + room, name, room), this))
                    {
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    // Loop over the unhandled message sprockets
                    foreach (var handler in _unhandledMessageSprockets)
                    {
                        // Stop at the first one that handled the message
                        if (handler.Handle(message, this))
                        {
                            break;
                        }
                    }

                }
            })
            .ContinueWith(task =>
            {
                // Just write to debug output if it failed
                if (task.IsFaulted)
                {
                    var aggregateException = task.Exception;
                    if (aggregateException != null)
                        Debug.WriteLine("JABBOT: Failed to process messages. {0}", aggregateException.GetBaseException());
                }
            });
        }

        private void OnLeave(dynamic user)
        {

        }

        private void OnJoin(dynamic user)
        {

        }

        private void OnLogOn(IEnumerable<string> rooms)
        {
            foreach (var room in rooms)
            {
                _rooms.Add(room);
            }
        }

        private void IntializeSprockets(IEnumerable<ISprocketInitializer> sprockets)
        {
            // Run all sprocket initializers
            foreach (var sprocketInitializer in sprockets)
            {
                try
                {
                    sprocketInitializer.Initialize(this);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Unable to Initialize {0}:{1}", sprocketInitializer.GetType().Name, ex.GetBaseException().Message));
                }
            }
        }

        private void Send(string command)
        {
            _chat.Invoke("send", command).Wait();
        }
    }
}
