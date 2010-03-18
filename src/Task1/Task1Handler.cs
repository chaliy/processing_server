using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using ProcessingServer.Handling;

namespace Task1
{
	[Export(typeof(ITaskHandler))]
	public class Task1Handler : ITaskHandler
	{
		public bool CanHandle(HandlerContext arg)
		{			
			return true;
		}

		public void Handle(HandlerContext arg)
		{
			Console.WriteLine("Task1 handler staring! Yay!");
			for(var i = 0; i < 500000; i++)
			{
				Regex.IsMatch(Guid.NewGuid().ToString(), i.ToString());
			}
			Console.WriteLine("Task1 handler completed! Yay-yay!");
		}
	}
}
