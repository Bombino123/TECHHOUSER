using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class AudioMeterInformationChannels
{
	private readonly IAudioMeterInformation audioMeterInformation;

	public int Count
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioMeterInformation.GetMeteringChannelCount(out var pnChannelCount));
			return pnChannelCount;
		}
	}

	public float this[int index]
	{
		get
		{
			int count = Count;
			if (index >= count)
			{
				throw new ArgumentOutOfRangeException("index", $"Peak index cannot be greater than number of channels ({count})");
			}
			float[] array = new float[Count];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Marshal.ThrowExceptionForHR(audioMeterInformation.GetChannelsPeakValues(array.Length, gCHandle.AddrOfPinnedObject()));
			gCHandle.Free();
			return array[index];
		}
	}

	internal AudioMeterInformationChannels(IAudioMeterInformation parent)
	{
		audioMeterInformation = parent;
	}
}
