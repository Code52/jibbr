using System.ComponentModel.Composition;

namespace Jabbot
{
    [InheritedExport]
    public interface ILogger
    {
        void WriteMessage(string message);
        void Write(string format, params object[] parameters);
    }
}
