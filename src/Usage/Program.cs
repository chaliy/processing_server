using System;
using ProcessingServer;
using Task1Contract;

namespace Usage
{
	class Program
	{
		static void Main(string[] args)
		{			
			for (var i = 0; i < 5; i++)
			{
				Client.Post(new Task1Msg
				            	{
				            		Data = Guid.NewGuid().ToString()
				            	}, "TestContext");
				Console.WriteLine(i);
			}						
		}
	}

	internal class d
	{
		
	}
}
