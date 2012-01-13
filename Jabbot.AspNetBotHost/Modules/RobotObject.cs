using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronJS;
using TinyMessenger;

namespace Jabbot.AspNetBotHost.Modules
{
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
            TinyMessengerHub.Instance.Publish(new TalkMessage() { Text = TypeConverter.ToString(c) });
        }

        public static List<string> Match { get; set; }


    }
}

