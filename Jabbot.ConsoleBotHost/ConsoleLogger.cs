using System;

namespace Jabbot.ConsoleBotHost
{
    public class ConsoleLogger : ILogger
    {
        public void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(string format, params object[] parameters)
        {
            Console.WriteLine(format, parameters);
        }
    }
}
