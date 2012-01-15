using System;
using System.Collections.Generic;
using Jabbot;
using Jabbot.Models;
using Moq;
using NUnit.Framework;
using Q = QuizSprocket;

namespace ExtensionTests
{
    [TestFixture]
    public class QuizSprocketTest
    {
        private Q.QuizSprocket _quizSprocket;
        private Mock<IBot> _botMock;
        [SetUp]
        public void SetUp()
        {
            _quizSprocket = new Q.QuizSprocket();
            _botMock = new Mock<IBot>();
        }

        [Test]
        public void RepliesToCorrectRoom()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "score"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.IsAny<string>(), It.Is<string>(room => room.Equals("jibbr"))));
        }

        [Test]
        public void AcceptsInfoAndHelpCommand()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "info"), "Simon", "jibbr");
            var chatMessage2 = new ChatMessage(string.Format("{0} {1}", "quiz", "help"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);
            _quizSprocket.Handle(chatMessage2, _botMock.Object);

            //assert
            _botMock.Verify(b => b.PrivateReply(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void InfoContainsSender()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "info"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.PrivateReply(It.Is<string>(who => who.Equals("Simon")), It.Is<string>(what => what.Contains("Simon"))));
        }

        [Test]
        public void CanAskCelebQuestion()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "celeb"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("?")), It.IsAny<string>()));
        }

        [Test]
        public void CanAnswerQuestion()
        {
            //arrange
            var askQuestion = new ChatMessage(string.Format("{0} {1}", "quiz", "celeb"), "Simon", "jibbr");
            var answerQuestion = new ChatMessage(string.Format("{0} {1} {2}", "quiz", "answer", "abcdefg"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(askQuestion, _botMock.Object);
            _quizSprocket.Handle(answerQuestion, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("?")), It.IsAny<string>()));
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("Correct") || what.Contains("Wrong")), It.IsAny<string>()));
        }

        [Test]
        public void WillNotAllowANewQuestionBeforeThePreviousWasAnswered()
        {
            //arrange
            var askQuestion = new ChatMessage(string.Format("{0} {1}", "quiz", "celeb"), "Simon", "jibbr");
            var askQuestion2 = new ChatMessage(string.Format("{0} {1}", "quiz", "celeb"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(askQuestion, _botMock.Object);
            _quizSprocket.Handle(askQuestion2, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("?")), It.IsAny<string>()));
            _botMock.Verify(b => b.PrivateReply(It.Is<string>(what => what.Equals("A question is currently waiting for the correct answer")), It.IsAny<string>()));
        }

        [Test]
        public void CanAnswerQuestionCorrect()
        {
            //arrange
            var askQuestion = new ChatMessage(string.Format("{0} {1}", "quiz", "celeb"), "Simon", "jibbr");
            var answerQuestion = new ChatMessage(string.Format("{0} {1} {2}", "quiz", "answer", "b"), "Simon", "jibbr");
            _quizSprocket.CelebrityQuestions = new List<Tuple<string, string, int>>()
                                                   {
                                                       new Tuple<string, string, int>("a", "b", 10)
                                                   };
            //act
            _quizSprocket.Handle(askQuestion, _botMock.Object);
            _quizSprocket.Handle(answerQuestion, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Equals("a")), It.IsAny<string>()));
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("Correct")), It.IsAny<string>()));
        }

        [Test]
        public void CanAnswerQuestionCorrectWithAnswerWithSpace()
        {
            //arrange
            var askQuestion = new ChatMessage(string.Format("{0} {1}", "quiz", "celeb"), "Simon", "jibbr");
            var answerQuestion = new ChatMessage(string.Format("{0} {1} {2}", "quiz", "answer", "b c"), "Simon", "jibbr");
            _quizSprocket.CelebrityQuestions = new List<Tuple<string, string, int>>()
                                                   {
                                                       new Tuple<string, string, int>("a", "b c", 10)
                                                   };
            //act
            _quizSprocket.Handle(askQuestion, _botMock.Object);
            _quizSprocket.Handle(answerQuestion, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Equals("a")), It.IsAny<string>()));
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("Correct")), It.IsAny<string>()));
        }

        [Test]
        public void BotRepliesWithNoScoreWhenNoScoreExists()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "score"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Equals("No score recorded for Simon")), It.IsAny<string>()));
        }

        [Test]
        public void BotRepliesWithScoreWhenItExists()
        {
            //arrange
            CanAnswerQuestionCorrect();
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "score"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("10")), It.IsAny<string>()));
        }

        [Test]
        public void BotRepliesWithScoreboardWhenItExists()
        {
            //arrange
            CanAnswerQuestionCorrect();
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "quiz", "top10"), "Simon", "jibbr");

            //act
            _quizSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("1") && what.Contains("Simon") && what.Contains("10")), It.IsAny<string>()));
        }

    }
}
