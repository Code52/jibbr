using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Jabbot;
using Jabbot.CommandSprockets;
using Jabbot.Models;
using Moq;
using Xunit;

namespace ExtensionTests
{
    public class VoicemailSprocketTest
    {
        //Mocks, Stubs & Constants
        private readonly Mock<IBot> mockBot;
        private readonly IBot bot;
        private const string Jibbr = "jibbr";
        private const string ExampleContents = "Some message";

        //SUT
        private VoicemailSprocket.VoicemailSprocket voicemailSprocket;

        public VoicemailSprocketTest()
        {
            mockBot = new Mock<IBot>();
            mockBot.Setup(b => b.Name).Returns(Jibbr);
            bot = mockBot.Object;

            dynamic room1 = new DynamicRoom() { Name = "room1", Users = new List<DynamicUser> { new DynamicUser() { Name = "Claire" }, new DynamicUser() { Name = "mads" } } };
            dynamic room2 = new DynamicRoom() { Name = "room2", Users = new List<DynamicUser> { new DynamicUser() { Name = "Claire" }, new DynamicUser() { Name = "bryce" }, new DynamicUser() { Name = "vicky" } } };
            mockBot.Setup(b => b.GetRooms()).Returns(new List<dynamic>() { room1, room2 });

            voicemailSprocket = new VoicemailSprocket.VoicemailSprocket();
        }

        [Fact]
        public void CanNotifyAllUsersOfNewVoicemails()
        {
            //Setup
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(It.IsAny<string>(), It.Is<string>(what => what == string.Format("{0} has a new voicemail for you. There are {1} in total", Jibbr, 3))), Times.Exactly(4));
        }

        [Fact]
        public void WillNotifyNewlyArrivedUsersOfVoicemails()
        {
            //Setup
            const string newlyArrivedUser = "James";

            voicemailSprocket = new VoicemailSprocket.VoicemailSprocket();
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage("[JABBR] - " + newlyArrivedUser + " just entered " + "TestRoom", newlyArrivedUser, bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(newlyArrivedUser, It.Is<string>(what => what == string.Format("{0} has {1} new voicemail for you", Jibbr, "1"))));
        }

        [Fact]
        public void CanRetrieveVoicemails()
        {
            //Setup
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage("retrieve", "Claire", bot.Name), bot);

            //Test
             mockBot.Verify(b => b.PrivateReply(It.Is<string>(s => s == "Claire"), string.Format(@"Jim said '{0}'", ExampleContents)), Times.Exactly(3));
        }

        [Fact]
        public void CanClearYourOwnVoicemails()
        {
            //Setup
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail record", ExampleContents), "Claire", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail clear", ExampleContents), "Jim", bot.Name), bot);

            //Test
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "voicemail retrieve", ExampleContents), "Giselle", bot.Name), bot);
            mockBot.Verify(b => b.PrivateReply(It.Is<string>(s => s == "Giselle"), string.Format(@"Claire said '{0}'", ExampleContents)), Times.Exactly(1));
        }
    }

    public class DynamicUser : DynamicObject
    {
        public string Name { get; set; }
    }

    public class DynamicRoom : DynamicObject
    {
        public List<DynamicUser> Users { get; set; }

        public string Name { get; set; }
    }
}
