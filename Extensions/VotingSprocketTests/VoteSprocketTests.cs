using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Jabbot;
using VotingSprocket;
using Jabbot.Models;
using Moq;

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
            _bot.Stub(b => b.Rooms).Return(new[] { "theroom", "lobby" });

            _sprocket = new VoteSprocket();
        }

        [TestMethod]
        public void HandleShouldNotifySenderThatTheyMustSupplyAQuestionWhenNoQuestionIsGiven()
        {
            var message = new ChatMessage("poll", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handled the message.");
            _bot.AssertWasCalled(b => b.PrivateReply(message.Sender, "To start a poll use: poll <roomname> <question>"));
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
        public void HandleShouldNotHandleAPrivateNonPollMessage()
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
            _bot.AssertWasCalled(b => b.Say("Poll will close in 2 minutes. Public reply with vote 1 or vote 2 etc...", "theroom"));
        }

        [TestMethod]
        public void HandleShouldReplyToPrivateVoteMessageOnAcceptedVotingProcedure()
        {
            var message = new ChatMessage("vote 1", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.PrivateReply("someoneelse", "You must cast your vote publicy in the room the poll is in."));
        }

        [TestMethod]
        public void HandleShouldHandleAPublicVoteMessage()
        {
            var message = new ChatMessage("vote 1", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handled the vote.");
        }

        [TestMethod]
        public void SprocketShouldSayWhenPollIsClosedAndTheVotingResults()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 1", "someoneelse", "theroom");
            _sprocket.Handle(message, _bot);
            message = new ChatMessage("vote 1", "johnny", "theroom");
            _sprocket.Handle(message, _bot);
            message = new ChatMessage("vote 2", "gina", "theroom");
            _sprocket.Handle(message, _bot);

#if DEBUG
            System.Threading.Thread.Sleep(1000);
#else
            System.Threading.Thread.Sleep((2 * 60 * 1000) + 500);
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
            System.Threading.Thread.Sleep((2 * 60 * 1000) + 500);
#endif
            _bot.AssertWasCalled(b => b.Say("No votes were cast for the poll.", "theroom"));
        }

        [TestMethod]
        public void HandleShouldNotifySenderTheyCannotVoteMoreThanOnceInAPoll()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 1", "someone", "theroom");
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 2", "someone", "theroom");
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.PrivateReply("someone", "You can only cast one vote for the current poll in this room."));
        }

        [TestMethod]
        public void HandleShouldAllowOnlyOnePollPerRoomAtATime()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.PrivateReply("anotherperson", "A poll is already in effect for theroom."));
        }

        [TestMethod]
        public void HandleShouldAllowOnePollInEachDifferentRoom()
        {
            var message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll lobby Which car to buy?", "gina", _bot.Name);
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.Say("A poll has started: Which food to bake?", "theroom"));
            _bot.AssertWasCalled(b => b.Say("A poll has started: Which car to buy?", "lobby"));

        }

        [TestMethod]
        public void HandleShouldReplyToSenderWhenRoomSpecifiedDoesNotExist()
        {
            var message = new ChatMessage("poll roomIamNotin Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.PrivateReply("anotherperson", "You are not in the room you specified for the poll."));
            _bot.AssertWasCalled(b => b.PrivateReply("anotherperson", "To start a poll use: poll <roomname> <question>"));
        }

        [TestMethod]
        public void HandleShouldPrivateReplyToSenderWhenTheyVoteButNoPollIsInEffect()
        {
            var message = new ChatMessage("vote 1", "anotherperson", "theroom");
            _sprocket.Handle(message, _bot);

            _bot.AssertWasCalled(b => b.PrivateReply("anotherperson", "There is no active poll for you to vote on."));
            _bot.AssertWasCalled(b => b.PrivateReply("anotherperson", "To start a poll use: poll <roomname> <question>"));
        }
    }
}
