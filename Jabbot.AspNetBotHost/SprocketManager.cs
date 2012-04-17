using System.Collections.Generic;
using Jabbot.Sprockets.Core;

namespace Jabbot.AspNetBotHost
{
    public class SprocketManager : Dictionary<int, ISprocket>
    {
        public SprocketManager(IEnumerable<ISprocket> sprockets)
        {
            var count = 0;
            foreach (var sprocket in sprockets)
            {
                base.Add(count++, sprocket);
            }
        }
    }
}