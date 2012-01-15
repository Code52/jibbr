using System.Net;
using IronJS;

namespace Jabbot.AspNetBotHost.Modules
{
    public class HttpObject : CommonObject
    {
        public string Url;
        public override string ClassName
        {
            get
            {
                return "Http";
            }
        }

        public HttpObject(string url, Environment env, CommonObject prototype)
            : base(env, env.Maps.Base, prototype)
        {
            Put("url", url, DescriptorAttrs.Immutable);
            Url = url;
        }

        public static string HttpGet(FunctionObject _, CommonObject that, FunctionObject function)
        {
            var self = that.CastTo<HttpObject>();
            var wc = new WebClient();
            return wc.DownloadString(self.Url);
        }
    }
}