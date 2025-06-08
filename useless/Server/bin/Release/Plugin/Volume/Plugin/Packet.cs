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
			if ((string)array[0] == "Volume")
			{
				VolumeController.SetVolume((int)array[1]);
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
