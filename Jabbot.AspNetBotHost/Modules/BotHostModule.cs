using System;
using System.Linq;
using System.Configuration;
using Nancy;
using System.Diagnostics;
using MomentApp;

namespace Jabbot.AspNetBotHost.Modules
{
    public class BotHostModule : NancyModule
    {
        private static readonly string _hostBaseUrl = ConfigurationManager.AppSettings["Application.HostBaseUrl"];
        private static readonly string _botRooms = ConfigurationManager.AppSettings["Bot.RoomList"];
        private static readonly string _momentApiKey = ConfigurationManager.AppSettings["Moment.ApiKey"];
        private static Bot _bot;

        public BotHostModule(Bot bot) : base("bot")
        {
            _bot = bot;

            if (string.IsNullOrEmpty(_bot.Name))
                StartBot();
            Get["/start"] = _ =>
            {
                try
                {
                    StartBot();
                    return "Bot Started";
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            };

            Get["/stop"] = _ =>
            {
                try
                {
                    ShutDownBot();
                    return "Bot Shut Down";
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            };

            // This is for ensuring that the process doesn't die permanently -- 
            // We create a task with MomentApp (TBD whether we will use this permanently
            Get["/keepalive"] = _ =>
            {
                ScheduleKeepAlive(Request.Url.ToString());
                return "OK";
            };

            Post["/launch"] = _ =>
            {
                //verify there is an auth token

                return "";
            };

            Post["/join"] = _ =>
                                {
                                    _bot.Join(Request.Form.Room);
                                    return Response.AsRedirect("/Rooms");
                                };

            Get["/leave"] = _ =>
            {
                _bot.Leave(Request.Query.Room);
                return Response.AsRedirect("/Rooms");
            };
        }



        private static void ScheduleKeepAlive(string Url)
        {
            new Moment(_momentApiKey).ScheduleJob(new Job()
            {
                at = DateTime.Now.AddMinutes(5),
                method = "GET",
                uri = new Uri(Url)
            });
        }

        private static void StartBot()
        {
            if (!_hostBaseUrl.Contains("localhost"))
            {
                ScheduleKeepAlive(_hostBaseUrl + "/keepalive");
            }
            //if (_bot != null)
            //{
            //    _bot.ShutDown();
            //}

            _bot.PowerUp();
            JoinRooms(_bot);

        }

        private static void ShutDownBot()
        {
            _bot.ShutDown();
            _bot = null;
        }

        private static void JoinRooms(Bot bot)
        {
            foreach (var room in _botRooms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()))
            {
                Trace.Write("Joining {0}...", room);
                if (TryCreateRoomIfNotExists(room, bot))
                {
                    bot.Join(room);
                    Trace.WriteLine("OK");
                }
                else
                {
                    Trace.WriteLine("Failed");
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
    }
}