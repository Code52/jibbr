using System;

namespace Jabbot
{
    public class ConsoleLogger : ILogger
    {
        public void Display(string message)
        {
            Console.WriteLine(message);
        }

        public void DisplayFormat(string format, params object[] parameters)
        {
            Console.WriteLine(format, parameters);
        }
    }
}
