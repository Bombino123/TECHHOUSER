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
			_ = (string)LEB128.Read(data)[0];
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
