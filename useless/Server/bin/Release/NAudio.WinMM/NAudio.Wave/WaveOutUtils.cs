using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave;

public static class WaveOutUtils
{
	public static float GetWaveOutVolume(IntPtr hWaveOut, object lockObject)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MmResult val;
		int dwVolume;
		lock (lockObject)
		{
			val = WaveInterop.waveOutGetVolume(hWaveOut, out dwVolume);
		}
		MmException.Try(val, "waveOutGetVolume");
		return (float)(dwVolume & 0xFFFF) / 65535f;
	}

	public static void SetWaveOutVolume(float value, IntPtr hWaveOut, object lockObject)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (value < 0f)
		{
			throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
		}
		if (value > 1f)
		{
			throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
		}
		int dwVolume = (int)(value * 65535f) + ((int)(value * 65535f) << 16);
		MmResult val;
		lock (lockObject)
		{
			val = WaveInterop.waveOutSetVolume(hWaveOut, dwVolume);
		}
		MmException.Try(val, "waveOutSetVolume");
	}

	public static long GetPositionBytes(IntPtr hWaveOut, object lockObject)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		lock (lockObject)
		{
			MmTime mmTime = default(MmTime);
			mmTime.wType = 4u;
			MmException.Try(WaveInterop.waveOutGetPosition(hWaveOut, ref mmTime, Marshal.SizeOf(mmTime)), "waveOutGetPosition");
			if (mmTime.wType != 4)
			{
				throw new Exception($"waveOutGetPosition: wType -> Expected {4}, Received {mmTime.wType}");
			}
			return mmTime.cb;
		}
	}
}
