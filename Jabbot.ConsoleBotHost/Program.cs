using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Jabbot.Sprockets.Core;

namespace Jabbot.ConsoleBotHost
{
    class Program
    {
        private static readonly string _serverUrl = ConfigurationManager.AppSettings["Bot.Server"];
        private static readonly string _botName = ConfigurationManager.AppSettings["Bot.Name"];
        private static readonly string _botPassword = ConfigurationManager.AppSettings["Bot.Password"];
        private static readonly string _botRooms = ConfigurationManager.AppSettings["Bot.RoomList"];
        private static bool _appShouldExit = false;


        private const string ExtensionsFolder = "Sprockets";

        static void Main(string[] args)
        {
            Console.WriteLine("Jabbot Bot Runner Starting...");
            while (!_appShouldExit)
            {
                RunBot();
            }
        }

        private static void RunBot()
        {
            var container = CreateCompositionContainer();
            var announcements = container.GetExportedValues<IAnnounce>();

            var logger = container.GetExportedValue<ILogger>();
            logger.Write("Connecting to {0} ...", _serverUrl);

            var scheduler = new Scheduler { Logger = logger };

            try
            {
                var bot = new Bot(_serverUrl, _botName, _botPassword);

                foreach (var s in container.GetExportedValues<ISprocket>())
                    bot.AddSprocket(s);

                bot.PowerUp();
                JoinRooms(bot, logger);

                var users = bot.GetUsers(bot.Rooms.First());
                logger.Write("Found {0} users in the room", users.Count());
                
                scheduler.Start(announcements, bot);

                logger.Write("Press enter to quit...");
                Console.ReadLine();

                scheduler.Stop();
                bot.ShutDown();

                _appShouldExit = true;
            }
            catch (Exception e)
            {
                logger.Write("ERROR: " + e.GetBaseException().Message);
            }
        }

        private static void JoinRooms(Bot bot, ILogger logger)
        {
            foreach (var room in _botRooms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()))
            {
                logger.Write("Joining {0}...", room);
                if (TryCreateRoomIfNotExists(room, bot))
                {
                    bot.Join(room);
                    logger.WriteMessage("OK");
                }
                else
                {
                    logger.WriteMessage("Failed");
                }
            }
        }
        private static bool TryCreateRoomIfNotExists(string roomName, Bot bot)
        {
            try
            {
                bot.CreateRoom(roomName);
            }
            catch (AggregateException e)
            {
                if (!e.GetBaseException().Message.Equals(string.Format("The room '{0}' already exists", roomName),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static CompositionContainer CreateCompositionContainer()
        {
            ComposablePartCatalog catalog;

            var extensionsPath = GetExtensionsPath();

            // If the extensions folder exists then use them
            if (Directory.Exists(extensionsPath))
            {
                catalog = new AggregateCatalog(new AssemblyCatalog(typeof(Bot).Assembly), new DirectoryCatalog(extensionsPath, "*.dll"));
            }
            else
            {
                catalog = new AssemblyCatalog(typeof(Bot).Assembly);
            }

            return new CompositionContainer(catalog);
        }

        private static string GetExtensionsPath()
        {
            var rootPath = HostingEnvironment.IsHosted
                ? HostingEnvironment.ApplicationPhysicalPath
                : Directory.GetCurrentDirectory();

            return Path.Combine(rootPath, ExtensionsFolder);
        }
    }
}
