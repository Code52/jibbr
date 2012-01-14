using IronJS;
using TinyMessenger;

namespace Jabbot.AspNetBotHost.Modules
{
    //https://github.com/github/hubot/blob/master/src/robot.coffee
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

        }

        public RobotObject(Environment env, CommonObject prototype)
            : base(env, prototype)
        {

        }

        public static void Respond(CommonObject c, FunctionObject f)
        {
            var r = (RegExpObject)c;
            BotHostModule.HubotRespond.Add(r.RegExp, f);
        }

        public static void Hear(CommonObject c, FunctionObject f)
        {
            var r = (RegExpObject)c;
            BotHostModule.HubotListen.Add(r.RegExp, f);
        }

        public static void Send(CommonObject c)
        {
            TinyMessengerHub.Instance.Publish(new TalkMessage() { Text = TypeConverter.ToString(c) });
        }

        public static CommonObject Random(CommonObject c)
        {
            // Math.floor(Math.random() * items.length)
            return c;
        }
    }
}

