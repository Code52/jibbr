using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Jabbot.Sprockets
{
    [InheritedExport]
    public interface ISprocketInitializer
    {
        void Initialize(Bot bot);
    }
}
