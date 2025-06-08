using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class AudioClockClient : IDisposable
{
	private IAudioClock audioClockClientInterface;

	public int Characteristics
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioClockClientInterface.GetCharacteristics(out var characteristics));
			return (int)characteristics;
		}
	}

	public ulong Frequency
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioClockClientInterface.GetFrequency(out var frequency));
			return frequency;
		}
	}

	public ulong AdjustedPosition
	{
		get
		{
			int num = 0;
			ulong position;
			ulong qpcPosition;
			while (!GetPosition(out position, out qpcPosition) && ++num != 5)
			{
			}
			if (Stopwatch.IsHighResolution)
			{
				ulong num2 = ((ulong)((decimal)Stopwatch.GetTimestamp() * 10000000m / (decimal)Stopwatch.Frequency) - qpcPosition) * Frequency / 10000000;
				return position + num2;
			}
			return position;
		}
	}

	public bool CanAdjustPosition => Stopwatch.IsHighResolution;

	internal AudioClockClient(IAudioClock audioClockClientInterface)
	{
		this.audioClockClientInterface = audioClockClientInterface;
	}

	public bool GetPosition(out ulong position, out ulong qpcPosition)
	{
		int position2 = audioClockClientInterface.GetPosition(out position, out qpcPosition);
		if (position2 == -1)
		{
			return false;
		}
		Marshal.ThrowExceptionForHR(position2);
		return true;
	}

	public void Dispose()
	{
		if (audioClockClientInterface != null)
		{
			Marshal.ReleaseComObject(audioClockClientInterface);
			audioClockClientInterface = null;
			GC.SuppressFinalize(this);
		}
	}
}
