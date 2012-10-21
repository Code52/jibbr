using System.Collections;
using System.Collections.Generic;
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
        }

        [Fact]
        public void CanNotifyAllUsersOfNewVoicemails()
        {
            //Setup
            mockBot.Setup(b => b.GetRooms()).Returns(new List<dynamic>()
                {
                    new {Name = "room1", Users = new {Name = "claire, mads"}},
                    new {Name = "room2", Users = new {Name = "claire, bryce, vicky"}}
                });
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "record", ExampleContents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "record", ExampleContents), "Jim", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "record", ExampleContents), "Jim", bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(It.IsAny<string>(), It.Is<string>(what => what == string.Format("{0} has a new voicemail for you. There are {1} in total", Jibbr, 3))));
        }

        [Fact]
        public void WillNotifyNewlyArrivedUsersOfVoicemails()
        {
            //Setup
            const string newlyArrivedUser = "James";

            voicemailSprocket = new VoicemailSprocket.VoicemailSprocket();
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "record", ExampleContents), "Jim", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage("[JABBR] - " + newlyArrivedUser + " just entered " + "TestRoom", newlyArrivedUser, bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(newlyArrivedUser, It.Is<string>(what => what == string.Format("{0} has {1} new voicemail for you", Jibbr, "1"))));
        }
    }
}
