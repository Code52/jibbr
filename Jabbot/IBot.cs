using System;
using System.Collections.Generic;
using System.Net;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace Jabbot
{
    public interface IBot
    {
        string Name { get; }
        ICredentials Credentials { get; set; }

        /// <summary>
        /// List of rooms the bot is in.
        /// </summary>
        IEnumerable<string> Rooms { get; }

        event Action Disconnected;
        event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// Add a sprocket to the bot instance
        /// </summary>
        void AddSprocket(ISprocket sprocket);

        /// <summary>
        /// Remove a sprocket from the bot instance
        /// </summary>
        void RemoveSprocket(ISprocket sprocket);

        /// <summary>
        /// Add a sprocket to the bot instance
        /// </summary>
        void AddUnhandledMessageSprocket(IUnhandledMessageSprocket sprocket);

        /// <summary>
        /// Remove a sprocket from the bot instance
        /// </summary>
        void RemoveUnhandledMessageSprocket(IUnhandledMessageSprocket sprocket);

        /// <summary>
        /// Remove all sprockets
        /// </summary>
        void ClearSprockets();

        /// <summary>
        /// Connects to the chat session
        /// </summary>
        void PowerUp(IEnumerable<ISprocketInitializer> sprocketInitializers = null);

        /// <summary>
        /// Creates a new room
        /// </summary>
        /// <param name="room">room to create</param>
        void CreateRoom(string room);

        /// <summary>
        /// Joins a chat room. Changes this to the active room for future messages.
        /// </summary>
        void Join(string room);

        /// <summary>
        /// Leaves a chat room. 
        /// </summary>
        void Leave(string room);

        /// <summary>
        /// Sets the Bot's gravatar email
        /// </summary>
        /// <param name="gravatarEmail"></param>
        void Gravatar(string gravatarEmail);

        /// <summary>
        /// Say something to the active room.
        /// </summary>
        /// <param name="what">what to say</param>
        /// <param name="room">the room to say it to</param>
        void Say(string what, string room);

        /// <summary>
        /// Reply to someone
        /// </summary>
        /// <param name="who">the person you want the bot to reply to</param>
        /// <param name="what">what you want the bot to say</param>
        /// <param name="room">the room to say it to</param>
        void Reply(string who, string what, string room);

        void PrivateReply(string who, string what);
        IEnumerable<object> GetUsers(string room);
        dynamic GetUserInfo(string room, string user);
        IEnumerable<string> GetRoomOwners(string room);
        dynamic GetRooms();
        void ChangeNote(string note);
        void Nudge(string user);
        void SendAdministrativeCommand(string command);

        /// <summary>
        /// Disconnect the bot from the chat session. Leaves all rooms the bot entered
        /// </summary>
        void ShutDown();
    }
}