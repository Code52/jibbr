using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets.Core;
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
            const string room = "TestRoom";
            const string contents = "Some message";
            

            var voicemailSprocket = new VoicemailSprocket();
            var mockBot = new Mock<IBot>();
            var bot = mockBot.Object;

            //Act
            voicemailSprocket.Handle(new ChatMessage(string.Format(@"record ""{0}""", contents), "Jim", bot.Name), bot);
            voicemailSprocket.Handle(new ChatMessage("[JABBR] - " + newlyArrivedUser + " just entered " + room, newlyArrivedUser, bot.Name), bot);

            //Test
            mockBot.Verify(b => b.PrivateReply(It.IsAny<string>(), It.Is<string>(what => what == string.Format(@"@James said ""{0}""",contents))));
        }
    }

    public class VoicemailSprocket : ISprocket
    {
        public bool Handle(ChatMessage message, IBot bot)
        {
            throw new NotImplementedException();
        }
    }
}
