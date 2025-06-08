using System;
using System.IO;
using NAudio.Wave.Compression;

namespace NAudio.Wave;

public class WaveFormatConversionStream : WaveStream
{
	private readonly WaveFormatConversionProvider conversionProvider;

	private readonly WaveFormat targetFormat;

	private readonly long length;

	private long position;

	private readonly WaveStream sourceStream;

	private bool isDisposed;

	public override long Position
	{
		get
		{
			return position;
		}
		set
		{
			value -= value % ((WaveStream)this).BlockAlign;
			long num = EstimateDestToSource(value);
			((Stream)(object)sourceStream).Position = num;
			position = EstimateSourceToDest(((Stream)(object)sourceStream).Position);
			conversionProvider.Reposition();
		}
	}

	public override long Length => length;

	public override WaveFormat WaveFormat => targetFormat;

	public WaveFormatConversionStream(WaveFormat targetFormat, WaveStream sourceStream)
	{
		this.sourceStream = sourceStream;
		this.targetFormat = targetFormat;
		conversionProvider = new WaveFormatConversionProvider(targetFormat, (IWaveProvider)(object)sourceStream);
		length = EstimateSourceToDest((int)((Stream)(object)sourceStream).Length);
		position = 0L;
	}

	public static WaveStream CreatePcmStream(WaveStream sourceStream)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		if ((int)sourceStream.WaveFormat.Encoding == 1)
		{
			return sourceStream;
		}
		WaveFormat val = AcmStream.SuggestPcmFormat(sourceStream.WaveFormat);
		if (val.SampleRate < 8000)
		{
			if ((int)sourceStream.WaveFormat.Encoding != 163)
			{
				throw new InvalidOperationException("Invalid suggested output format, please explicitly provide a target format");
			}
			val = new WaveFormat(8000, 16, 1);
		}
		return (WaveStream)(object)new WaveFormatConversionStream(val, sourceStream);
	}

	[Obsolete("can be unreliable, use of this method not encouraged")]
	public int SourceToDest(int source)
	{
		return (int)EstimateSourceToDest(source);
	}

	private long EstimateSourceToDest(long source)
	{
		long num = source * targetFormat.AverageBytesPerSecond / sourceStream.WaveFormat.AverageBytesPerSecond;
		return num - num % targetFormat.BlockAlign;
	}

	private long EstimateDestToSource(long dest)
	{
		long num = dest * sourceStream.WaveFormat.AverageBytesPerSecond / targetFormat.AverageBytesPerSecond;
		return (int)(num - num % sourceStream.WaveFormat.BlockAlign);
	}

	[Obsolete("can be unreliable, use of this method not encouraged")]
	public int DestToSource(int dest)
	{
		return (int)EstimateDestToSource(dest);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = conversionProvider.Read(buffer, offset, count);
		position += num;
		return num;
	}

	protected override void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				((Stream)(object)sourceStream).Dispose();
				conversionProvider.Dispose();
			}
		}
		((Stream)this).Dispose(disposing);
	}
}
