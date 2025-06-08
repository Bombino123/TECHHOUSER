using NAudio.Dsp;

namespace NAudio.Wave;

public class SimpleCompressorEffect : ISampleProvider
{
	private readonly ISampleProvider sourceStream;

	private readonly SimpleCompressor simpleCompressor;

	private readonly int channels;

	private readonly object lockObject = new object();

	public double MakeUpGain
	{
		get
		{
			return simpleCompressor.MakeUpGain;
		}
		set
		{
			lock (lockObject)
			{
				simpleCompressor.MakeUpGain = value;
			}
		}
	}

	public double Threshold
	{
		get
		{
			return simpleCompressor.Threshold;
		}
		set
		{
			lock (lockObject)
			{
				simpleCompressor.Threshold = value;
			}
		}
	}

	public double Ratio
	{
		get
		{
			return simpleCompressor.Ratio;
		}
		set
		{
			lock (lockObject)
			{
				simpleCompressor.Ratio = value;
			}
		}
	}

	public double Attack
	{
		get
		{
			return simpleCompressor.Attack;
		}
		set
		{
			lock (lockObject)
			{
				simpleCompressor.Attack = value;
			}
		}
	}

	public double Release
	{
		get
		{
			return simpleCompressor.Release;
		}
		set
		{
			lock (lockObject)
			{
				simpleCompressor.Release = value;
			}
		}
	}

	public bool Enabled { get; set; }

	public WaveFormat WaveFormat => sourceStream.WaveFormat;

	public SimpleCompressorEffect(ISampleProvider sourceStream)
	{
		this.sourceStream = sourceStream;
		channels = sourceStream.WaveFormat.Channels;
		simpleCompressor = new SimpleCompressor(5.0, 10.0, sourceStream.WaveFormat.SampleRate);
		simpleCompressor.Threshold = 16.0;
		simpleCompressor.Ratio = 6.0;
		simpleCompressor.MakeUpGain = 16.0;
	}

	public int Read(float[] array, int offset, int count)
	{
		lock (lockObject)
		{
			int num = sourceStream.Read(array, offset, count);
			if (Enabled)
			{
				for (int i = 0; i < num; i += channels)
				{
					double @in = array[offset + i];
					double in2 = ((channels == 1) ? 0f : array[offset + i + 1]);
					simpleCompressor.Process(ref @in, ref in2);
					array[offset + i] = (float)@in;
					if (channels > 1)
					{
						array[offset + i + 1] = (float)in2;
					}
				}
			}
			return num;
		}
	}
}
