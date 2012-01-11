using System.ComponentModel.Composition;
﻿
namespace Jabbot.Sprockets.Core
{
    [InheritedExport]
    public interface ISprocketInitializer
    {
        void Initialize(Bot bot);
    }
}
