using System.Collections.Generic;
using Jabbot.Sprockets.Core;
using Nancy;

namespace Jabbot.AspNetBotHost.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IEnumerable<IAnnounce> sprockets, Bot bot)
        {
            Get["/"] = _ => View["Home/Index", new { Sprockets = sprockets, Bot = bot }];
            Get["/Rooms"] = _ => View["Home/Rooms", bot.Rooms];
        }
    }
}