using System.Collections.Generic;
using NAudio.Wave;

namespace Plugin.Handler;

internal class HandlerDeviceInfo
{
	public static string[] Device()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < WaveIn.DeviceCount; i++)
		{
			list.Add(WaveIn.GetCapabilities(i).ProductName);
		}
		return list.ToArray();
	}
}
