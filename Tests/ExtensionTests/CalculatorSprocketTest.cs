using Jabbot;
using Jabbot.Extensions;
using Jabbot.Models;
using Moq;
using Xunit;


namespace ExtensionTests
{
    public class CalculatorSprocketTest
    {
        private readonly CalculatorSprocket _calculatorSprocket;
        private readonly Mock<IBot> _botMock;
    
        public CalculatorSprocketTest()
        {
            _calculatorSprocket = new CalculatorSprocket();
            _botMock = new Mock<IBot>();
        }

        [Fact]
        public void AcceptsInfoAndHelpCommand()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "calc", "info"), "Simon", "jibbr");
            var chatMessage2 = new ChatMessage(string.Format("{0} {1}", "calc", "help"), "Simon", "jibbr");

            //act
            _calculatorSprocket.Handle(chatMessage, _botMock.Object);
            _calculatorSprocket.Handle(chatMessage2, _botMock.Object);

            //assert
            _botMock.Verify(b => b.PrivateReply(It.Is<string>(who => who.Contains("Simon")), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void CanRequestValidCalculation()
        {
            //arrange
            var expression = "2 * 2";
            var chatMessage = new ChatMessage(string.Format("{0} {1} {2}", "calc", "expr", expression), "Simon", "jibbr");

            //act
            _calculatorSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Equals(string.Format("{0} = {1}", expression, "4"))), It.IsAny<string>()));
        }

        [Fact]
        public void CanRequestInValidCalculation()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1} {2}", "calc", "expr", "2 *"), "Simon", "jibbr");

            //act
            _calculatorSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("Sorry")), It.IsAny<string>()));
        }

        [Fact]
        public void CanCalculateSquareRoot()
        {
            //arrange
            var expression = "sqrt(16)";
            var chatMessage = new ChatMessage(string.Format("{0} {1} {2}", "calc", "expr", expression), "Simon", "jibbr");

            //act
            _calculatorSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Equals(string.Format("{0} = {1}", expression, "4"))), It.IsAny<string>()));
        }
    }
}
