using System;
using System.ComponentModel.Composition;
using ProcessingServer.Contract;

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
			Console.WriteLine("Handled");
		}
	}
}
