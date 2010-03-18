using System;
using ProcessingServer;
using Task1Contract;

namespace Usage
{
	class Program
	{
		static void Main(string[] args)
		{
			var msg = new Task1Msg();
			for (var i = 0; i < 10; i++)
			{
				Client.Post(msg);
				Console.WriteLine(i);
			}			
		}
	}
}
