using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using ProcessingServer.Handling;

namespace Task1
{
	[Export(typeof(ITaskHandler))]
	public class Task1Handler : ITaskHandler
	{
		public bool CanHandle(IHandlerContext ctx)
		{
			return true;
		}

		public void Handle(IHandlerContext ctx)
		{
			ctx.Trace("Task1 handler staring! Yay!");			
			for(var i = 0; i < 100000; i++)
			{
				Regex.IsMatch(Guid.NewGuid().ToString(), i.ToString());
			}
			ctx.Trace("Task1 handler completed! Yay-yay!");
		}
	}
}
