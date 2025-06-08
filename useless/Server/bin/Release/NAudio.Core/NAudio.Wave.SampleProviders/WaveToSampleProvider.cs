using System;

namespace NAudio.Wave.SampleProviders;

public class WaveToSampleProvider : SampleProviderConverterBase
{
	public WaveToSampleProvider(IWaveProvider source)
		: base(source)
	{
		if (source.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
		{
			throw new ArgumentException("Must be already floating point");
		}
	}

	public unsafe override int Read(float[] buffer, int offset, int count)
	{
		int num = count * 4;
		EnsureSourceBuffer(num);
		int num2 = source.Read(sourceBuffer, 0, num);
		int result = num2 / 4;
		int num3 = offset;
		fixed (byte* ptr = &sourceBuffer[0])
		{
			float* ptr2 = (float*)ptr;
			int num4 = 0;
			int num5 = 0;
			while (num4 < num2)
			{
				buffer[num3++] = ptr2[num5];
				num4 += 4;
				num5++;
			}
		}
		return result;
	}
}
