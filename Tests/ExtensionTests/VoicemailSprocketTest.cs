using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.CommandSprockets;
using Jabbot.Models;
using Jabbot.Sprockets;
using Jabbot.Sprockets.Core;
using Moq;
using Xunit;
using Match = System.Text.RegularExpressions.Match;

namespace ExtensionTests
{
    public class VoicemailSprocketTest
    {
        [Fact]
        public void WillNotifyNewlyArrivedUsersOfVoicemails()
        {
            //Setup
            const string recordingUser = "Jim";
            const string newlyArrivedUser = "James";
            const string roomName = "TestRoom";
            dynamic room = new {Name = roomName};
            const string contents = "Some message";
            IList<string> usersInRoom = new List<string>();

            var mockBot = new Mock<IBot>();
            var bot = mockBot.Object;
            mockBot.Setup(b => b.GetRooms()).Returns(new [] { room });
            mockBot.Setup(b => b.GetUsers(It.Is<string>(r => r ==roomName))).Returns(()=>usersInRoom);

            var voicemailSprocket = new VoicemailSprocket();
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "record", contents), recordingUser, bot.Name), bot);
            
            
            //Act
            usersInRoom.Add(newlyArrivedUser);
            voicemailSprocket.Handle(new ChatMessage(VoicemailSprocket.SystemCommand + newlyArrivedUser + " just entered " + roomName, newlyArrivedUser, bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(newlyArrivedUser, It.Is<string>(what => what == string.Format("@{0} said '{1}'", newlyArrivedUser, contents))));
        }
    }

    public class VoicemailSprocket : RegexSprocket
    {
        internal const string SystemCommand = "[JABBR] - ";

        public VoicemailSprocket()
        {
            RecordVoicemailSprocket = new RecordVoicemailSprocket();
            _newlyArrivedUser = new NewlyArrivedUser(RecordVoicemailSprocket);
        }

        private RecordVoicemailSprocket RecordVoicemailSprocket;
        private NewlyArrivedUser _newlyArrivedUser;

        public override Regex Pattern
        {
            get { return new Regex(string.Format("({0})|({1})", RecordVoicemailSprocket.RecordCommand, NewlyArrivedUser.UserArrivedNotification), RegexOptions.IgnoreCase); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            _newlyArrivedUser.Handle(message, bot);
            RecordVoicemailSprocket.Handle(message, bot);
        }

        protected IList<string> Users { get; set; }
    }

    public class RecordVoicemailSprocket : RegexSprocket
    {
        public IList<string> Voicemails = new List<string>();
        internal const string RecordCommand = "record \'.*\'";

        public override Regex Pattern
        {
            get { return new Regex(RecordCommand);}
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            Voicemails.Add(message.Content.Split('\'')[1]);
        }
    }


    internal class NewlyArrivedUser : RegexSprocket
    {
        private readonly RecordVoicemailSprocket _recordVoicemailSprocket;
        private readonly IList<string> _userNames = new List<string>();

        public NewlyArrivedUser(RecordVoicemailSprocket recordVoicemailSprocket)
        {
            if (recordVoicemailSprocket == null) throw new ArgumentNullException("recordVoicemailSprocket");
            _recordVoicemailSprocket = recordVoicemailSprocket;
        }

        public const string UserArrivedNotification = @"\[JABBR\] - .* just entered .*";

        private IEnumerable<string> GetRoomNames(IBot bot)
        {
            foreach (var room in bot.GetRooms())
                yield return room.Name;
        }

        public override Regex Pattern
        {
            get { return new Regex(UserArrivedNotification, RegexOptions.IgnoreCase); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            var indexOfUserName = VoicemailSprocket.SystemCommand.Length;
            string username = message.Content.Substring(indexOfUserName, message.Content.IndexOf(' ', indexOfUserName) - indexOfUserName);
            _userNames.Add(username);

            foreach (var voicemail in _recordVoicemailSprocket.Voicemails)
                bot.PrivateReply(username, string.Format("@{0} said '{1}'", username, voicemail));
        }
    }
}
