using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using IronJS;
using IronJS.Hosting;
using IronJS.Native;
using Jabbot.Sprockets.Core;
using Nancy;
using System.Diagnostics;
using MomentApp;
using TinyMessenger;
using Environment = IronJS.Environment;

namespace Jabbot.AspNetBotHost.Modules
{
    public class BotHostModule : NancyModule
    {
        private static readonly string _hostBaseUrl = ConfigurationManager.AppSettings["Application.HostBaseUrl"];
        private static readonly string _botRooms = ConfigurationManager.AppSettings["Bot.RoomList"];
        private static readonly string _momentApiKey = ConfigurationManager.AppSettings["Moment.ApiKey"];
        public static Bot _bot;
        private readonly IEnumerable<ISprocket> _sprockets;
        private readonly IEnumerable<ISprocketInitializer> _sprocketInitializers;
        public static Dictionary<Regex, FunctionObject> HubotRespond = new Dictionary<Regex, FunctionObject>();
        public static Dictionary<Regex, FunctionObject> HubotListen = new Dictionary<Regex, FunctionObject>();
        public BotHostModule(Bot bot, IEnumerable<ISprocket> sprockets, IEnumerable<ISprocketInitializer> sprocketInitializers)
            : base("bot")
        {
            _bot = bot;
            _sprockets = sprockets;
            _sprocketInitializers = sprocketInitializers;

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

            Get["/launch"] = _ =>
            {
                //TODO: verify there is an auth token
                LoadCoffeeScript();
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
            Post["/send/{room}"] = _ =>
                                       {
                                           _bot.Say(Request.Form.Message, _.Room);
                                           return Response.AsRedirect("/");
                                       };

            Post["/attach"] = _ =>
                                  {
                                      AttachScript(Request.Form.script);
                                      return Response.AsRedirect("/");
                                  };

            Post["/disable/{sprocket}"] = _ =>
                                              {
                                                  _bot.DisableSprocket(_.Sprocket);
                                                  return Response.AsRedirect("/");
                                              };

            Post["/enable/{sprocket}"] = _ =>
                                              {
                                                  _bot.EnableSprocket(_.Sprocket);
                                                  return Response.AsRedirect("/");
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

        private void StartBot()
        {
            if (!_hostBaseUrl.Contains("localhost"))
            {
                ScheduleKeepAlive(_hostBaseUrl + "/keepalive");
            }
            foreach (var sprocket in _sprockets)
                _bot.AddSprocket(sprocket);

            _bot.PowerUp(_sprocketInitializers);
            JoinRooms(_bot);

            _bot.MessageReceived += BotMessageReceived;
            LoadCoffeeScript();
            TinyMessengerHub.Instance.Subscribe<TalkMessage>(m => _bot.Say(m.Text, _bot.Rooms.First()));
        }

        private static void ShutDownBot()
        {
            _bot.ShutDown();
            _bot = null;
        }

        private static void JoinRooms(Bot bot)
        {
            foreach (var room in _botRooms.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()))
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

        static void BotMessageReceived(Models.ChatMessage obj)
        {
            obj.Content = Regex.Replace(obj.Content, @"<(.|\n)*?>", string.Empty);

            Respond(obj.Content.StartsWith(_bot.Name + " ") ? HubotRespond : HubotListen, obj.Content);
        }

        static void Respond(Dictionary<Regex, FunctionObject> d, string content)
        {
            foreach (var k in d.Keys)
            {
                if (!k.IsMatch(content))
                    continue;

                var m = k.Matches(content);
                var f = HubotRespond[k];

                var bot = f.Env.Globals.GetT<RobotObject>("bot");
                var array = f.Env.NewArray();
                for (int i = 0; i < m[0].Groups.Count; i++)
                {
                    array.Put(i.ToString(CultureInfo.InvariantCulture), m[0].Groups[i].ToString());
                }
                bot.Prototype.Put("match", array);
                f.Call(f.Env.Globals, bot);
            }
        }


        private static void LoadCoffeeScript()
        {
            CompileCoffeeScriptUsingIronJs(
                System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~/Resources/coffee-script.js")),
                System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~/Resources/ping.coffee")));
        }

        static CSharp.Context context;
        private static void CompileCoffeeScriptUsingIronJs(string coffeeCompiler, string input)
        {
            context = new CSharp.Context();

            //Create the JS object
            var robotConstructor = Utils.CreateConstructor<Func<FunctionObject, CommonObject, double, CommonObject>>(context.Environment, 1, Construct);
            var httpConstructor = Utils.CreateConstructor<Func<FunctionObject, CommonObject, string, CommonObject>>(context.Environment, 1, ConstructHttp);

            //setup the prototype (methods) on teh JS object
            var robotPrototype = context.Environment.NewObject();
            robotPrototype.Prototype = context.Environment.Prototypes.Object;
            var respond = Utils.CreateFunction<Action<CommonObject, FunctionObject>>(context.Environment, 0, RobotObject.Respond);
            var hear = Utils.CreateFunction<Action<CommonObject, FunctionObject>>(context.Environment, 0, RobotObject.Hear);
            var send = Utils.CreateFunction<Action<CommonObject>>(context.Environment, 0, RobotObject.Send);
            var get = Utils.CreateFunction<Func<FunctionObject, CommonObject, FunctionObject, string>>(context.Environment, 0, HttpObject.HttpGet);
            var random = Utils.CreateFunction<Func<CommonObject, CommonObject>>(context.Environment, 1, RobotObject.Random);

            var httpPrototype = context.Environment.NewObject();
            httpPrototype.Prototype = context.Environment.Prototypes.Object;
            httpPrototype.Put("get", get);
            httpConstructor.Put("prototype", httpPrototype, DescriptorAttrs.Immutable);

            //attach the methods
            robotPrototype.Put("respond", respond);
            robotPrototype.Put("send", send);
            robotPrototype.Put("hear", hear);
            robotPrototype.Put("http", httpConstructor);
            robotPrototype.Put("random", random);

            //attach the prototype
            robotConstructor.Put("prototype", robotPrototype, DescriptorAttrs.Immutable);
            context.SetGlobal("robot", robotConstructor);
            context.Execute(@"var bot = new robot();");

            var result = context.Execute("2 + 2");
            var result2 = context.Execute("bot.match");
            //Setup and compile CoffeeScript
            context.Execute(coffeeCompiler);
            context.Execute("var compile = function (src) { return CoffeeScript.compile(src, { bare: true }); };");

            //Attach basic ping script
            AttachScript(input);

        }

        public static void AttachScript(string input)
        {
            var compile = context.GetGlobalAs<FunctionObject>("compile");
            var result = compile.Call(context.Globals, input);
            var output = TypeConverter.ToString(result);
            context.Execute(@"function module() { }" + output + @" module.exports(bot);");
        }

        static CommonObject Construct(FunctionObject ctor, CommonObject _, double x)
        {
            var prototype = ctor.GetT<CommonObject>("prototype");
            return new RobotObject(ctor.Env, prototype);
        }

        static CommonObject ConstructHttp(FunctionObject ctor, CommonObject _, string x)
        {
            var prototype = ctor.GetT<CommonObject>("prototype");
            return new HttpObject(x, ctor.Env, prototype);
        }
    }
}