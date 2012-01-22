using Moq;
using Xunit;
using Jabbot;
using Jabbot.Models;
using VotingSprocket;

namespace ExtensionTests
{
    public class VoteSprocketTests
    {
        IBot _bot = null;
        Mock<IBot> _botMock = null;
        VoteSprocket _sprocket = null;

        public void SetupTestData()
        {
            _botMock = new Mock<IBot>();
            _botMock.Setup(b => b.Name).Returns("myself");
            _botMock.Setup(b => b.Rooms).Returns(new[] { "theroom", "lobby" });
            _bot = _botMock.Object;

            _sprocket = new VoteSprocket();
        }

        [Fact]
        public void HandleShouldNotifySenderThatTheyMustSupplyAQuestionWhenNoQuestionIsGiven()
        {
            SetupTestData();
            var message = new ChatMessage("poll", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.True(wasHandled, "Should have handled the message.");
            _botMock.Verify(b => b.PrivateReply(message.Sender, "To start a poll use: poll <roomname> <question>"));
        }

        [Fact]
        public void HandleShouldNotHandlePublicPollMessage()
        {
            SetupTestData();
            var message = new ChatMessage("poll", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.False(wasHandled, "Should not have handeled the message.");
        }

        [Fact]
        public void HandleShouldHandleAPrivatePollMessage()
        {
            SetupTestData(); 
            var message = new ChatMessage("poll theroom Some question?", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.True(wasHandled, "Should have handeled the message.");
        }

        [Fact]
        public void HandleShouldNotHandleAPrivateNonPollMessage()
        {
            SetupTestData();
            var message = new ChatMessage("foobar", "someoneelse", _bot.Name);
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.False(wasHandled, "Should not have handeled the message.");
        }

        [Fact]
        public void HandleShouldSayPollQuestionToRoomSpecified()
        {
            SetupTestData();
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.Say("A poll has started: Which color to use?", "theroom"));
            _botMock.Verify(b => b.Say("Poll will close in 2 minutes. Public reply with vote 1 or vote 2 etc...", "theroom"));
        }

        [Fact]
        public void HandleShouldReplyToPrivateVoteMessageOnAcceptedVotingProcedure()
        {
            SetupTestData();
            var message = new ChatMessage("vote 1", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("someoneelse", "You must cast your vote publicy in the room the poll is in."));
        }

        [Fact]
        public void HandleShouldHandleAPublicVoteMessage()
        {
            SetupTestData();
            var message = new ChatMessage("vote 1", "someoneelse", "theroom");
            bool wasHandled = _sprocket.Handle(message, _bot);

            Assert.True(wasHandled, "Should have handled the vote.");
        }

        [Fact]
        public void SprocketShouldSayWhenPollIsClosedAndTheVotingResults()
        {
            SetupTestData();
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

        [Fact]
        public void SprocketShouldReportNoVotesWhenNoVotesCast()
        {
            SetupTestData();
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);
            

#if DEBUG
            System.Threading.Thread.Sleep(1000);
#else
            System.Threading.Thread.Sleep((2 * 60 * 1000) + 500);
#endif
            _botMock.Verify(b => b.Say("No votes were cast for the poll.", "theroom"));
        }

        [Fact]
        public void HandleShouldNotifySenderTheyCannotVoteMoreThanOnceInAPoll()
        {
            SetupTestData();
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 1", "someone", "theroom");
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("vote 2", "someone", "theroom");
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("someone", "You can only cast one vote for the current poll in this room."));
        }

        [Fact]
        public void HandleShouldAllowOnlyOnePollPerRoomAtATime()
        {
            SetupTestData();
            var message = new ChatMessage("poll theroom Which color to use?", "someoneelse", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("anotherperson", "A poll is already in effect for theroom."));
        }

        [Fact]
        public void HandleShouldAllowOnePollInEachDifferentRoom()
        {
            SetupTestData();
            var message = new ChatMessage("poll theroom Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            message = new ChatMessage("poll lobby Which car to buy?", "gina", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.Say("A poll has started: Which food to bake?", "theroom"));
            _botMock.Verify(b => b.Say("A poll has started: Which car to buy?", "lobby"));

        }

        [Fact]
        public void HandleShouldReplyToSenderWhenRoomSpecifiedDoesNotExist()
        {
            SetupTestData();
            var message = new ChatMessage("poll roomIamNotin Which food to bake?", "anotherperson", _bot.Name);
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("anotherperson", "You are not in the room you specified for the poll."));
            _botMock.Verify(b => b.PrivateReply("anotherperson", "To start a poll use: poll <roomname> <question>"));
        }

        [Fact]
        public void HandleShouldPrivateReplyToSenderWhenTheyVoteButNoPollIsInEffect()
        {
            SetupTestData();
            var message = new ChatMessage("vote 1", "anotherperson", "theroom");
            _sprocket.Handle(message, _bot);

            _botMock.Verify(b => b.PrivateReply("anotherperson", "There is no active poll for you to vote on."));
            _botMock.Verify(b => b.PrivateReply("anotherperson", "To start a poll use: poll <roomname> <question>"));
        }
    }
}
