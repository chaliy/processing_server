using System;
using ProcessingServer;
using Task1Contract;

namespace Usage
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new Client.TaskProcessingClient("http://localhost:1066");
			for (var i = 0; i < 500; i++)
			{
				client.Post(new Task1Msg
				            	{
				            		Data = Guid.NewGuid().ToString()
				            	}, "TestContext", "AnotherExample");
				Console.WriteLine(i);
			}						
		}
	}

	internal class d
	{
		
	}
}
