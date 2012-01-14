using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Jabbot;
using VotingSprocket;
using Jabbot.Models;

namespace VotingSprocketTests
{
    [TestClass]
    public class VoteSprocketTests
    {
        [TestMethod]
        public void HandleShouldNotHandlePublicPollMessage()
        {
            var bot = new Bot("", "myself", "");
            var message = new ChatMessage("poll", "someoneelse", "theroom");
            var sprocket = new VoteSprocket();
            
            bool wasHandled = sprocket.Handle(message, bot);

            Assert.IsFalse(wasHandled, "Should not have handeled a public poll message.");
        }
    }
}
