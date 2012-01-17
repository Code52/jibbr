using System.ComponentModel.Composition;

namespace Jabbot.Sprockets.Core
{
    /// <summary>
    /// Sprockets are extension points for bots. When an incoming message comes in, a sprocket
    /// will get a change to do something with it.
    /// </summary>
    [InheritedExport]
    public interface ISprocket : IMessageHandler
    {
    	string SprocketName { get; }
    }
}
