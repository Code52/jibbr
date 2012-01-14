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
            _bot.AssertWasCalled(b => b.Say("Poll will close in 2 minutes.", "theroom"));
        }

        [TestMethod]
        public void HandleShouldNotHandleAPrivateVoteMessage()
        {
            var message = new ChatMessage("vote 1", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsFalse(wasHandled, "Should not have handled the vote.");
        }

        [TestMethod]
        public void HandleShouldHandleAPublicVoteMessage()
        {
            var message = new ChatMessage("vote 1", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handled the vote.");
        }

        [TestMethod]
        public void HandleShouldSayWhenPollIsClosedAndTheVotingResults()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message.Receiver = "theroom";
            message.Content = "vote 1";
            _sprocket.Handle(message, _bot);
            message.Content = "vote 1"; 
            _sprocket.Handle(message, _bot);
            message.Content = "vote 2"; 
            _sprocket.Handle(message, _bot);

#if DEBUG
            System.Threading.Thread.Sleep(1000);
#else
            System.Threading.Thread.Sleep(2.5 * 60 * 1000);
#endif
            _bot.AssertWasCalled(b => b.Say("poll results 2 votes for (1). 1 vote for (2).", "theroom"));
        }

        [TestMethod]
        public void SprocketShouldReportNoVotesWhenNoVotesCast()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);
            

#if DEBUG
            System.Threading.Thread.Sleep(1000);
#else
            System.Threading.Thread.Sleep(2.5 * 60 * 1000);
#endif
            _bot.AssertWasCalled(b => b.Say("No votes were cast for the poll.", "theroom"));
        }

        // should accept only one vote from a person.

        [TestMethod]
        public void HandleShouldAllowOnlyOnePollPerRoomAtATime()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);
                        
            _bot.AssertWasCalled(b => b.PrivateReply("anotherperson", "A poll is already in effect for theroom."));
        }
    }
}
