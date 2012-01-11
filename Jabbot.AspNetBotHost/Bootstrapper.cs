using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Jabbot.Sprockets.Core;
using Nancy;

namespace Jabbot.AspNetBotHost
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly string _serverUrl = ConfigurationManager.AppSettings["Bot.Server"];
        private static readonly string _botName = ConfigurationManager.AppSettings["Bot.Name"];
        private static readonly string _botPassword = ConfigurationManager.AppSettings["Bot.Password"];
        private static readonly string _botRooms = ConfigurationManager.AppSettings["Bot.RoomList"];
        private static bool _appShouldExit = false;

        private const string ExtensionsFolder = "Sprockets";

        private readonly Bot _bot = new Bot();
        protected override void ConfigureApplicationContainer(TinyIoC.TinyIoCContainer container)
        {
            var mefcontainer = CreateCompositionContainer();

            container.Register(mefcontainer.GetExportedValues<IAnnounce>());
            container.Register(new Bot(_serverUrl, _botName, _botPassword));

            //base.ConfigureApplicationContainer(container);
        }

        private static CompositionContainer CreateCompositionContainer()
        {
            ComposablePartCatalog catalog;

            var extensionsPath = GetExtensionsPath();

            //If the extensions folder exists then use them
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