using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jabbot.ConsoleBotHost
{
    class Program
    {
        private static readonly string _serverUrl = "http://jabbr-bots.apphb.com";
        private static readonly string _botName = "JabbrTheHut";
        private static readonly string _botPassword = "DemoPassword";
        private static readonly string _defaultIdleRoom = "The-Cantina";

        static void Main(string[] args)
        {
            Console.WriteLine("Jabbot Bot Runner Starting...");
            Bot bot = new Bot(_serverUrl, _botName, _botPassword);
            bot.PowerUp();
            if (TryCreateRoomIfNotExists(_defaultIdleRoom, bot))
            {
                bot.Join(_defaultIdleRoom);
                bot.Say("Greetings, I live.", _defaultIdleRoom);
                Console.Write("Press enter to quit...");    
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("An error was encoutered creating the default room.");
            }

            bot.ShutDown();

        }

        private static bool TryCreateRoomIfNotExists(string roomName, Bot bot)
        {
            try
            {
                bot.CreateRoom(_defaultIdleRoom);
            }
            catch (AggregateException e)
            {
                if (!e.InnerExceptions.FirstOrDefault().Message.Contains("exists"))
                    return false;
            }

            return true;
        }
    }
}
