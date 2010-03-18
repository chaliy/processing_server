using ProcessingServer;
using Task1Contract;

namespace Usage
{
	class Program
	{
		static void Main(string[] args)
		{
			var msg = new Task1Msg();
			Client.Post(msg);
		}
	}
}
