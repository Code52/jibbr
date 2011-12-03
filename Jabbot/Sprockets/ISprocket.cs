using System.ComponentModel.Composition;
using Jabbot.Models;

namespace Jabbot.Sprokets
{
    /// <summary>
    /// Sprokets are extension points for bots. When an incoming message comes in, a sproket
    /// will get a change to do something with it.
    /// </summary>
    [InheritedExport]
    public interface ISproket
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
