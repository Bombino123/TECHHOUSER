using System;
using System.IO;
using NAudio.Wave.SampleProviders;

namespace NAudio.Wave;

public class AudioFileReader : WaveStream, ISampleProvider
{
	private WaveStream readerStream;

	private readonly SampleChannel sampleChannel;

	private readonly int destBytesPerSample;

	private readonly int sourceBytesPerSample;

	private readonly long length;

	private readonly object lockObject;

	public string FileName { get; }

	public override WaveFormat WaveFormat => sampleChannel.WaveFormat;

	public override long Length => length;

	public override long Position
	{
		get
		{
			return SourceToDest(((Stream)(object)readerStream).Position);
		}
		set
		{
			lock (lockObject)
			{
				((Stream)(object)readerStream).Position = DestToSource(value);
			}
		}
	}

	public float Volume
	{
		get
		{
			return sampleChannel.Volume;
		}
		set
		{
			sampleChannel.Volume = value;
		}
	}

	public AudioFileReader(string fileName)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		lockObject = new object();
		FileName = fileName;
		CreateReaderStream(fileName);
		sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
		sampleChannel = new SampleChannel((IWaveProvider)(object)readerStream, false);
		destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
		length = SourceToDest(((Stream)(object)readerStream).Length);
	}

	private void CreateReaderStream(string fileName)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
		{
			readerStream = (WaveStream)new WaveFileReader(fileName);
			if ((int)readerStream.WaveFormat.Encoding != 1 && (int)readerStream.WaveFormat.Encoding != 3)
			{
				readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
				readerStream = (WaveStream)new BlockAlignReductionStream(readerStream);
			}
		}
		else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				readerStream = (WaveStream)(object)new Mp3FileReader(fileName);
			}
			else
			{
				readerStream = (WaveStream)new MediaFoundationReader(fileName);
			}
		}
		else if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
		{
			readerStream = (WaveStream)new AiffFileReader(fileName);
		}
		else
		{
			readerStream = (WaveStream)new MediaFoundationReader(fileName);
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		WaveBuffer val = new WaveBuffer(buffer);
		int count2 = count / 4;
		return Read(val.FloatBuffer, offset / 4, count2) * 4;
	}

	public int Read(float[] buffer, int offset, int count)
	{
		lock (lockObject)
		{
			return sampleChannel.Read(buffer, offset, count);
		}
	}

	private long SourceToDest(long sourceBytes)
	{
		return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
	}

	private long DestToSource(long destBytes)
	{
		return sourceBytesPerSample * (destBytes / destBytesPerSample);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && readerStream != null)
		{
			((Stream)(object)readerStream).Dispose();
			readerStream = null;
		}
		((Stream)this).Dispose(disposing);
	}
}
