using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Jabbot;
using Jabbot.Models;
using VotingSprocket;

namespace VotingSprocketTests
{
    [TestFixture]
    public class VoteSprocketTests
    {
        IBot _bot = null;
        Mock<IBot> _botMock = null;
        VoteSprocket _sprocket = null;

        [SetUp]
        public void SetupTestData()
        {
            _botMock = new Mock<IBot>();
            _botMock.Setup(b => b.Name).Returns("myself");
            _botMock.Setup(b => b.Rooms).Returns(new[] { "theroom", "lobby" });
            _bot = _botMock.Object;

            _sprocket = new VoteSprocket();
        }

        [Test]
        public void HandleShouldNotifySenderThatTheyMustSupplyAQuestionWhenNoQuestionIsGiven()
        {
            var message = new ChatMessage("poll", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handled the message.");
            _botMock.Verify(b => b.PrivateReply(message.Sender, "To start a poll use: poll <roomname> <question>"));
        }

        [Test]
        public void HandleShouldNotHandlePublicPollMessage()
        {
            var message = new ChatMessage("poll", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsFalse(wasHandled, "Should not have handeled the message.");
        }

        [Test]
        public void HandleShouldHandleAPrivatePollMessage()
        {
            var message = new ChatMessage("poll theroom Some question?", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handeled the message.");
        }

        [Test]
        public void HandleShouldNotHandleAPrivateNonPollMessage()
        {
            var message = new ChatMessage("foobar", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsFalse(wasHandled, "Should not have handeled the message.");
        }

        [Test]
        public void HandleShouldSayPollQuestionToRoomSpecified()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.Say("A poll has started: Which color to use?", "theroom"));
            _botMock.Verify(b => b.Say("Poll will close in 2 minutes. Public reply with vote 1 or vote 2 etc...", "theroom"));
        }

        [Test]
        public void HandleShouldReplyToPrivateVoteMessageOnAcceptedVotingProcedure()
        {
            var message = new ChatMessage("vote 1", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("someoneelse", "You must cast your vote publicy in the room the poll is in."));
        }

        [Test]
        public void HandleShouldHandleAPublicVoteMessage()
        {
            var message = new ChatMessage("vote 1", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.IsTrue(wasHandled, "Should have handled the vote.");
        }

        [Test]
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
            _botMock.Verify(b => b.Say("poll results 2 votes for (1). 1 vote for (2).", "theroom"));
        }

        [Test]
        public void SprocketShouldReportNoVotesWhenNoVotesCast()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);
            

#if DEBUG
            System.Threading.Thread.Sleep(1000);
#else
            System.Threading.Thread.Sleep((2 * 60 * 1000) + 500);
#endif
            _botMock.Verify(b => b.Say("No votes were cast for the poll.", "theroom"));
        }

        [Test]
        public void HandleShouldNotifySenderTheyCannotVoteMoreThanOnceInAPoll()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 1", "someone", "theroom");
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 2", "someone", "theroom");
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("someone", "You can only cast one vote for the current poll in this room."));
        }

        [Test]
        public void HandleShouldAllowOnlyOnePollPerRoomAtATime()
        {
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("anotherperson", "A poll is already in effect for theroom."));
        }

        [Test]
        public void HandleShouldAllowOnePollInEachDifferentRoom()
        {
            var message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll lobby Which car to buy?", "gina", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.Say("A poll has started: Which food to bake?", "theroom"));
            _botMock.Verify(b => b.Say("A poll has started: Which car to buy?", "lobby"));

        }

        [Test]
        public void HandleShouldReplyToSenderWhenRoomSpecifiedDoesNotExist()
        {
            var message = new ChatMessage("poll roomIamNotin Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("anotherperson", "You are not in the room you specified for the poll."));
            _botMock.Verify(b => b.PrivateReply("anotherperson", "To start a poll use: poll <roomname> <question>"));
        }

        [Test]
        public void HandleShouldPrivateReplyToSenderWhenTheyVoteButNoPollIsInEffect()
        {
            var message = new ChatMessage("vote 1", "anotherperson", "theroom");
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("anotherperson", "There is no active poll for you to vote on."));
            _botMock.Verify(b => b.PrivateReply("anotherperson", "To start a poll use: poll <roomname> <question>"));
        }
    }
}
