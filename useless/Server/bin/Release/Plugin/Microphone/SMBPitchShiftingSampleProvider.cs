using System;
using NAudio.Wave;

public class SMBPitchShiftingSampleProvider : ISampleProvider
{
	private ISampleProvider SourceStream;

	private WaveFormat WFormat;

	private float Pitch = 1f;

	private int _FFTSize;

	private long _osamp;

	private SMBPitchShifter ShifterLeft = new SMBPitchShifter();

	private SMBPitchShifter ShifterRight = new SMBPitchShifter();

	private const float LIM_THRESH = 0.95f;

	private const float LIM_RANGE = 0.050000012f;

	private const float M_PI_2 = (float)Math.PI / 2f;

	public WaveFormat WaveFormat => WFormat;

	public float PitchFactor
	{
		get
		{
			return Pitch;
		}
		set
		{
			Pitch = value;
		}
	}

	public SMBPitchShiftingSampleProvider(ISampleProvider SourceProvider)
		: this(SourceProvider, 4096, 4L, 1f)
	{
	}

	public SMBPitchShiftingSampleProvider(ISampleProvider SourceProvider, int FFTSize, long osamp, float InitialPitch)
	{
		SourceStream = SourceProvider;
		WFormat = SourceProvider.WaveFormat;
		_FFTSize = FFTSize;
		_osamp = osamp;
		PitchFactor = InitialPitch;
	}

	public int Read(float[] buffer, int offset, int count)
	{
		int num = SourceStream.Read(buffer, offset, count);
		if (Pitch == 1f)
		{
			return num;
		}
		if (WFormat.Channels == 1)
		{
			float[] array = new float[num];
			int num2 = 0;
			for (int i = offset; i <= num + offset - 1; i++)
			{
				array[num2] = buffer[i];
				num2++;
			}
			ShifterLeft.PitchShift(Pitch, num, _FFTSize, _osamp, WFormat.SampleRate, array);
			num2 = 0;
			for (int j = offset; j <= num + offset - 1; j++)
			{
				buffer[j] = Limiter(array[num2]);
				num2++;
			}
			return num;
		}
		if (WFormat.Channels == 2)
		{
			float[] array2 = new float[num >> 1];
			float[] array3 = new float[num >> 1];
			int num3 = 0;
			for (int k = offset; k <= num + offset - 1; k += 2)
			{
				array2[num3] = buffer[k];
				array3[num3] = buffer[k + 1];
				num3++;
			}
			ShifterLeft.PitchShift(Pitch, num >> 1, _FFTSize, _osamp, WFormat.SampleRate, array2);
			ShifterRight.PitchShift(Pitch, num >> 1, _FFTSize, _osamp, WFormat.SampleRate, array3);
			num3 = 0;
			for (int l = offset; l <= num + offset - 1; l += 2)
			{
				buffer[l] = Limiter(array2[num3]);
				buffer[l + 1] = Limiter(array3[num3]);
				num3++;
			}
			return num;
		}
		throw new Exception("Shifting of more than 2 channels is currently not supported.");
	}

	private float Limiter(float Sample)
	{
		float num = 0f;
		if (0.95f < Sample)
		{
			num = (Sample - 0.95f) / 0.050000012f;
			return (float)(Math.Atan(num) / 1.5707963705062866 * 0.050000011920928955 + 0.949999988079071);
		}
		if (Sample < -0.95f)
		{
			num = (0f - (Sample + 0.95f)) / 0.050000012f;
			return 0f - (float)(Math.Atan(num) / 1.5707963705062866 * 0.050000011920928955 + 0.949999988079071);
		}
		return Sample;
	}
}
