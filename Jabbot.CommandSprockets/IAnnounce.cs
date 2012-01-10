using System;
using System.ComponentModel.Composition;

namespace Jabbot.CommandSprockets
{
	[InheritedExport]
	public interface IAnnounce
	{
		TimeSpan Interval { get; }
		void Execute(Bot bot);
	}
}
