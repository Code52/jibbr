using System.Collections.Generic;
using Jabbot.Sprockets.Core;
using Nancy;

namespace Jabbot.AspNetBotHost.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IEnumerable<IAnnounce> sprockets, Bot bot)
        {
            Get["/"] = _ => View["Home/Index", sprockets];
            Get["/Rooms"] = _ => View["Home/Rooms", bot.Rooms];
        }
    }
}