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
        IBot _bot = null;
        VoteSprocket _sprocket = null;

        [TestInitialize]
        public void SetupTestData()
        {
            _bot = MockRepository.GenerateStub<IBot>();
            _bot.Stub(b => b.Name).Return("myself");
            _sprocket = new VoteSprocket();
        }

        [TestMethod]
        public void HandleShouldNotHandlePublicPollMessage()
        {
            var message = new ChatMessage("poll", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsFalse(wasHandled, "Should not have handeled the message.");
        }

        [TestMethod]
        public void HandleShouldHandleAPrivatePollMessage()
        {
            var message = new ChatMessage("poll theroom Some question?", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handeled the message.");
        }

        [TestMethod]
        public void HandleshouldNotHandleAPrivateNonPollMessage()
        {
            var message = new ChatMessage("foobar", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsFalse(wasHandled, "Should not have handeled the message.");
        }

        [TestMethod]
        public void HandleShouldSayPollQuestionToRoomSpecified()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.Say("A poll has started: Which color to use?", "theroom"));
        }
    }
}
