using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using IronJS;
using IronJS.Hosting;
using IronJS.Native;
using Jabbot.Sprockets.Core;
using Nancy;
using System.Diagnostics;
using MomentApp;
using TinyIoC;
using Environment = IronJS.Environment;

namespace Jabbot.AspNetBotHost.Modules
{
    public class BotHostModule : NancyModule
    {
        private static readonly string _hostBaseUrl = ConfigurationManager.AppSettings["Application.HostBaseUrl"];
        private static readonly string _botRooms = ConfigurationManager.AppSettings["Bot.RoomList"];
        private static readonly string _momentApiKey = ConfigurationManager.AppSettings["Moment.ApiKey"];
        public static Bot _bot;

        public BotHostModule(Bot bot)
            : base("bot")
        {
            _bot = bot;


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
            var initializers = TinyIoCContainer.Current.ResolveAll<ISprocketInitializer>();
            _bot.PowerUp(initializers);
            JoinRooms(_bot);

            _bot.MessageReceived += BotMessageReceived;
            LoadCoffeeScript();
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
        static void BotMessageReceived(Models.ChatMessage obj)
        {
            foreach (var k in HubotScripts.Keys)
            {
                if (k.IsMatch(obj.Content))
                {
                    var f = HubotScripts[k];
                    f.Call(f.Env.Globals, f.Env.Globals.Get("bot"));

                }
            }
        }



        private static void LoadCoffeeScript()
        {
            CompileCoffeeScriptUsingIronJs(System.IO.File.ReadAllText(@"C:\Code\Code52\jibbr\Jabbot.AspNetBotHost\Resources\coffee-script.js"), System.IO.File.ReadAllText(@"C:\Code\Code52\jibbr\Jabbot.AspNetBotHost\Resources\ping.coffee"));
        }

        public static CSharp.Context context;
        public static Environment env;
        private static void CompileCoffeeScriptUsingIronJs(string coffeeCompiler, string input)
        {
            context = new CSharp.Context();
            env = context.Environment;

            //Create the JS object
            var robotConstructor = Utils.CreateConstructor<Func<FunctionObject, CommonObject, double, CommonObject>>(context.Environment, 1, Construct);

            //setup the prototype (methods) on teh JS object
            var robotPrototype = context.Environment.NewObject();
            robotPrototype.Prototype = context.Environment.Prototypes.Object;
            var respond = Utils.CreateFunction<Action<CommonObject, FunctionObject>>(context.Environment, 0, RobotObject.Respond);
            var send = Utils.CreateFunction<Action<CommonObject>>(context.Environment, 0, RobotObject.Send);

            //attach the methods
            robotPrototype.Put("respond", respond);
            robotPrototype.Put("send", send);

            //attach the prototype
            robotConstructor.Put("prototype", robotPrototype, DescriptorAttrs.Immutable);
            context.SetGlobal("robot", robotConstructor);
            context.Execute(@"var bot = new robot();");

            context.Execute(coffeeCompiler);
            context.Execute("var compile = function (src) { return CoffeeScript.compile(src, { bare: true }); };");
            var compile = context.GetGlobalAs<FunctionObject>("compile");
            var result = compile.Call(context.Globals, input);
            var output = IronJS.TypeConverter.ToString(result);

            context.Execute(@"
                    var bot = new robot();
                    function module() { }"+output+@"
                    module.exports(bot);");
            /*
            context.Execute(@"
                var bot = new robot(); 
                bot.respond(/PING$/i, function(msg) { return msg.send('PONG'); });
            ");*/
        }

        public static void AttachScript(string input)
        {
            var compile = context.GetGlobalAs<FunctionObject>("compile");
            var result = compile.Call(context.Globals, input);
            var output = IronJS.TypeConverter.ToString(result);
            context.Execute(@"
                    var bot = new robot();
                    function module() { }" + output + @"
                    module.exports(bot);");
        }

        public static Dictionary<Regex, FunctionObject> HubotScripts = new Dictionary<Regex, FunctionObject>();

        static CommonObject Construct(FunctionObject ctor, CommonObject _, double x)
        {
            var prototype = ctor.GetT<CommonObject>("prototype");
            return new RobotObject(ctor.Env, prototype);
        }
    }

    public class RobotObject : CommonObject
    {
        public RobotObject(Environment env, Schema map, CommonObject prototype)
            : base(env, map, prototype)
        {
        }

        public RobotObject(Environment env, CommonObject prototype)
            : base(env, prototype)
        {
        }

        public static void Respond(CommonObject c, FunctionObject f)
        {
            var r = (RegExpObject)c;
            BotHostModule.HubotScripts.Add(r.RegExp, f);
        }

        public static void Send(CommonObject c)
        {
            BotHostModule._bot.Say(TypeConverter.ToString(c), BotHostModule._bot.Rooms.First());
        }
    }
}