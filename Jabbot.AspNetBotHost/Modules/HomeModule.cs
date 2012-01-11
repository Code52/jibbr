using System.Collections.Generic;
using Jabbot.Sprockets.Core;
using Nancy;

namespace Jabbot.AspNetBotHost.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IEnumerable<IAnnounce> sprockets, Bot bot)
        {
            Get["/"] = _ => View["Home", sprockets];
            Get["/Rooms"] = _ => View["Rooms", bot.Rooms];
            Post["/bot/launch"] = _ =>
                                      {
                                          //verify there is an auth token

                                          return "";
                                      };
        }
    }
}