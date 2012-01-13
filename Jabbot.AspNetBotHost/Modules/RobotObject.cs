using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronJS;
using TinyMessenger;

namespace Jabbot.AspNetBotHost.Modules
{
    public class RobotObject : CommonObject
    {
        public override string ClassName
        {
            get
            {
                return "Robot";
            }
        }

        public RobotObject(Environment env, Schema map, CommonObject prototype)
            : base(env, map, prototype)
        {
            Put("match", new[] { "test", "echo" });
        }

        public RobotObject(Environment env, CommonObject prototype)
            : base(env, prototype)
        {
            Put("match", new[] { "test", "echo" });
        }

        public static void Respond(CommonObject c, FunctionObject f)
        {
            var r = (RegExpObject)c;
            BotHostModule.HubotScripts.Add(r.RegExp, f);
        }

        public static void Send(CommonObject c)
        {
            TinyMessengerHub.Instance.Publish(new TalkMessage() { Text = TypeConverter.ToString(c) });
        }
    }
}

