using System;

namespace NAudio.Wave.SampleProviders;

public class MonoToStereoSampleProvider : ISampleProvider
{
	private readonly ISampleProvider source;

	private float[] sourceBuffer;

	public WaveFormat WaveFormat { get; private set; }

	public float LeftVolume { get; set; }

	public float RightVolume { get; set; }

	public MonoToStereoSampleProvider(ISampleProvider source)
	{
		LeftVolume = 1f;
		RightVolume = 1f;
		if (source.WaveFormat.Channels != 1)
		{
			throw new ArgumentException("Source must be mono");
		}
		this.source = source;
		WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, 2);
	}

	public int Read(float[] buffer, int offset, int count)
	{
		int count2 = count / 2;
		int num = offset;
		EnsureSourceBuffer(count2);
		int num2 = source.Read(sourceBuffer, 0, count2);
		for (int i = 0; i < num2; i++)
		{
			buffer[num++] = sourceBuffer[i] * LeftVolume;
			buffer[num++] = sourceBuffer[i] * RightVolume;
		}
		return num2 * 2;
	}

	private void EnsureSourceBuffer(int count)
	{
		if (sourceBuffer == null || sourceBuffer.Length < count)
		{
			sourceBuffer = new float[count];
		}
	}
}
