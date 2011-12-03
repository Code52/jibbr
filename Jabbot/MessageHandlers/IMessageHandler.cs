using System.ComponentModel.Composition;

namespace Jabbot.MessageHandlers
{
    /// <summary>
    /// Extension point for bots. When an incoming message comes in, a message handler
    /// will get a change to do something with it.
    /// </summary>
    [InheritedExport]
    public interface IMessageHandler
    {
        /// <summary>
        /// Allows the user to handle a message incoming from the bot's room
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <param name="bot">bot instance</param>
        /// <returns>true if handled, false if not</returns>
        bool Handle(ChatMessage message, Bot bot);
    }
}
