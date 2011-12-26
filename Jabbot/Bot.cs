﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Hosting;
using Jabbot.Models;
using Jabbot.Sprockets;
using SignalR.Client.Hubs;

namespace Jabbot
{
    public class Bot
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _chat;
        private readonly string _password;
        private readonly List<ISprocket> _sprockets = new List<ISprocket>();
        private readonly HashSet<string> _rooms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private const string ExtensionsFolder = "Sprockets";

        private ComposablePartCatalog _catalog = null;
        private CompositionContainer _container = null;


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
            get
            {
                return _connection.Credentials;
            }
            set
            {
                _connection.Credentials = value;
            }
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
        /// Remove all sprockets
        /// </summary>
        public void ClearSprockets()
        {
            _sprockets.Clear();
        }

        /// <summary>
        /// Connects to the chat session
        /// </summary>
        public void PowerUp()
        {
            if (!_connection.IsActive)
            {
                InitializeContainer();

                _chat.On<dynamic, string>("addMessage", ProcessMessage);

                _chat.On("leave", OnLeave);

                _chat.On("addUser", OnJoin);

                _chat.On<IEnumerable<string>>("logOn", OnLogOn);

                // Start the connection and wait
                _connection.Start().Wait();

                // Join the chat
                var success = _chat.Invoke<bool>("Join").Result;

                if (!success)
                {
                    // Setup the name of the bot
                    Send(String.Format("/nick {0} {1}", Name, _password));

                    IntializeSprockets();
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
            Send("/join " + room);

            // Add the room to the list
            _rooms.Add(room);
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

        private void ProcessMessage(dynamic message, string room)
        {
            // Run this on another thread since the signalr client doesn't like it
            // when we spend a long time processing messages synchronously
            Task.Factory.StartNew(() =>
            {
                string content = message.Content;
                string name = message.User.Name;

                // Ignore replies from self
                if (name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // We're going to process commands for the bot here
                var chatMessage = new ChatMessage(WebUtility.HtmlDecode(content), name, room);

                if (MessageReceived != null)
                {
                    MessageReceived(chatMessage);
                }

                // Loop over the registered sprockets
                foreach (var handler in _sprockets)
                {
                    // Stop at the first one that handled the message
                    if (handler.Handle(chatMessage, this))
                    {
                        break;
                    }
                 }
            })
            .ContinueWith(task =>
            {
                // Just write to debug output if it failed
                if (task.IsFaulted)
                {
                    Debug.WriteLine("JABBOT: Failed to process messages. {0}", task.Exception.GetBaseException());
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

        private void InitializeContainer()
        {
            var container = CreateCompositionContainer();
            // Add all the sprockets to the sprocket list
            foreach (var sprocket in container.GetExportedValues<ISprocket>())
            {
                AddSprocket(sprocket);
            }
        }

        private void IntializeSprockets()
        {
            var container = CreateCompositionContainer();
            // Run all sprocket initializers
            foreach (var sprocketInitializer in container.GetExportedValues<ISprocketInitializer>())
            {
                try
                {
                    sprocketInitializer.Initialize(this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to Initialize {0}:{1}", sprocketInitializer.GetType().Name, ex.GetBaseException().Message);
                }
            }
        }

        private CompositionContainer CreateCompositionContainer()
        {
            if (_container == null)
            {
            {
                string extensionsPath = GetExtensionsPath();

                // If the extensions folder exists then use them
                if (Directory.Exists(extensionsPath))
                {
                    _catalog = new AggregateCatalog(
                                new AssemblyCatalog(typeof(Bot).Assembly),
                                new DirectoryCatalog(extensionsPath, "*.dll"));
                }
                else
                {
                    _catalog = new AssemblyCatalog(typeof(Bot).Assembly);
                }

                _container = new CompositionContainer(_catalog);
            }
            return _container;
        }

        private static string GetExtensionsPath()
        {
            string rootPath = null;
            if (HostingEnvironment.IsHosted)
            {

                rootPath = HostingEnvironment.ApplicationPhysicalPath;
            }
            else
            {
                rootPath = Directory.GetCurrentDirectory();
            }

            return Path.Combine(rootPath, ExtensionsFolder);
        }

        private void Send(string command)
        {
            _chat.Invoke("send", command).Wait();
        }
    }
}
