using System.ComponentModel.Composition;

namespace Jabbot.Sprockets.Core
{
    /// <summary>
    /// This interface is used to allow messages not handled by the primary message flow
    /// a "last chance" before the message goes unhandled
    /// </summary>
    [InheritedExport]
    public interface IUnhandledMessageSprocket : IMessageHandler
    {

    }
}
