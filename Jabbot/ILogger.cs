using System.ComponentModel.Composition;

namespace Jabbot
{
    [InheritedExport]
    public interface ILogger
    {
        void Display(string message);
        void DisplayFormat(string format, params object[] parameters);
    }
}
