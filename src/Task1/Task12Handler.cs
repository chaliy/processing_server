using System;
using System.ComponentModel.Composition;
using ProcessingServer.Handling;
using Task1Contract;

namespace Task1
{
	[Export(typeof(ITaskHandler))]
	public class Task1Handler2 : SimpleTaskHandler<Task1Msg2>
	{		
		public override void Handle(Task1Msg2 arg)
		{			
			// This handler should never be executed... 
			// If it runs this indicates that something wrong with selecting
			// task handlers.
			Console.WriteLine("If you seen this something probably wrong!");
		}
	}	
}
