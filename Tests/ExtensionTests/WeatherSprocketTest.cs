using Jabbot;
using Jabbot.Extensions;
using Jabbot.Models;
using Moq;
using Xunit;

namespace ExtensionTests
{
    public class WeatherSprocketTest
    {
        private readonly WeatherSprocket _weatherSprocket;
        private readonly Mock<IBot> _botMock;
    
        public WeatherSprocketTest()
        {
            _weatherSprocket = new WeatherSprocket();
            _botMock = new Mock<IBot>();
        }

        [Fact]
        public void CanRequestValidZipCode()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "weather", "93063"), "Ethan", "jibbr");

            //act
            _weatherSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("Simi Valley")), It.IsAny<string>()));
        }

        [Fact]
        public void CanRequestInValidZipCode()
        {
            //arrange
            var chatMessage = new ChatMessage(string.Format("{0} {1}", "weather", "00000"), "Ethan", "jibbr");

            //act
            _weatherSprocket.Handle(chatMessage, _botMock.Object);

            //assert
            _botMock.Verify(b => b.Say(It.Is<string>(what => what.Contains("00000 is not a valid U.S. zipcode")), It.IsAny<string>()));
        }
    }
}
