using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using ProcessingServer.Handling;
using Task1Contract;

namespace Task1
{
	[Export(typeof(ITaskHandler))]
	public class Task1Handler : SimpleTaskHandler<Task1Msg>
	{		
		public override void Handle(Task1Msg arg)
		{			
			for (var i = 0; i < 500000; i++)
			{
				Regex.IsMatch(Guid.NewGuid().ToString(), i.ToString());
			}			
		}
	}	
}
