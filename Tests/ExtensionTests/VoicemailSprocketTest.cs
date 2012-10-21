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
        [Fact]
        public void WillNotifyNewlyArrivedUsersOfVoicemails()
        {
            //Setup
            const string newlyArrivedUser = "James";
            const string voicemailContents = "Some message";
            const string jibbr = "jibbr";

            var mockBot = new Mock<IBot>();
            mockBot.Setup(b => b.Name).Returns(jibbr);
            var bot = mockBot.Object;

            var voicemailSprocket = new VoicemailSprocket.VoicemailSprocket();
            voicemailSprocket.Handle(new ChatMessage(string.Format("{0} '{1}'", "record", voicemailContents), "Jim", bot.Name), bot);

            //Act
            voicemailSprocket.Handle(new ChatMessage("[JABBR] - " + newlyArrivedUser + " just entered " + "TestRoom", newlyArrivedUser, bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(newlyArrivedUser, It.Is<string>(what => what == string.Format("{0} has {1} new voicemail for you", jibbr, "1"))));
        }
    }
}
