using System;
using NAudio.Wave.Compression;

namespace NAudio.Wave;

public class WaveFormatConversionProvider : IWaveProvider, IDisposable
{
	private readonly AcmStream conversionStream;

	private readonly IWaveProvider sourceProvider;

	private readonly int preferredSourceReadSize;

	private int leftoverDestBytes;

	private int leftoverDestOffset;

	private int leftoverSourceBytes;

	private bool isDisposed;

	public WaveFormat WaveFormat { get; }

	public WaveFormatConversionProvider(WaveFormat targetFormat, IWaveProvider sourceProvider)
	{
		this.sourceProvider = sourceProvider;
		WaveFormat = targetFormat;
		conversionStream = new AcmStream(sourceProvider.WaveFormat, targetFormat);
		preferredSourceReadSize = Math.Min(sourceProvider.WaveFormat.AverageBytesPerSecond, conversionStream.SourceBuffer.Length);
		preferredSourceReadSize -= preferredSourceReadSize % sourceProvider.WaveFormat.BlockAlign;
	}

	public void Reposition()
	{
		leftoverDestBytes = 0;
		leftoverDestOffset = 0;
		leftoverSourceBytes = 0;
		conversionStream.Reposition();
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		int i = 0;
		if (count % WaveFormat.BlockAlign != 0)
		{
			count -= count % WaveFormat.BlockAlign;
		}
		int num5;
		for (; i < count; i += num5)
		{
			int num = Math.Min(count - i, leftoverDestBytes);
			if (num > 0)
			{
				Array.Copy(conversionStream.DestBuffer, leftoverDestOffset, buffer, offset + i, num);
				leftoverDestOffset += num;
				leftoverDestBytes -= num;
				i += num;
			}
			if (i >= count)
			{
				break;
			}
			int num2 = Math.Min(preferredSourceReadSize, conversionStream.SourceBuffer.Length - leftoverSourceBytes);
			int num3 = sourceProvider.Read(conversionStream.SourceBuffer, leftoverSourceBytes, num2) + leftoverSourceBytes;
			if (num3 == 0)
			{
				break;
			}
			int sourceBytesConverted;
			int num4 = conversionStream.Convert(num3, out sourceBytesConverted);
			if (sourceBytesConverted == 0)
			{
				break;
			}
			leftoverSourceBytes = num3 - sourceBytesConverted;
			if (leftoverSourceBytes > 0)
			{
				Buffer.BlockCopy(conversionStream.SourceBuffer, sourceBytesConverted, conversionStream.SourceBuffer, 0, leftoverSourceBytes);
			}
			if (num4 <= 0)
			{
				break;
			}
			int val = count - i;
			num5 = Math.Min(num4, val);
			if (num5 < num4)
			{
				leftoverDestBytes = num4 - num5;
				leftoverDestOffset = num5;
			}
			Array.Copy(conversionStream.DestBuffer, 0, buffer, i + offset, num5);
		}
		return i;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
			conversionStream?.Dispose();
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	~WaveFormatConversionProvider()
	{
		Dispose(disposing: false);
	}
}
