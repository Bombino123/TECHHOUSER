using System;

namespace NAudio.Wave;

public class SilenceProvider : IWaveProvider
{
	public WaveFormat WaveFormat { get; private set; }

	public SilenceProvider(WaveFormat wf)
	{
		WaveFormat = wf;
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		Array.Clear(buffer, offset, count);
		return count;
	}
}
