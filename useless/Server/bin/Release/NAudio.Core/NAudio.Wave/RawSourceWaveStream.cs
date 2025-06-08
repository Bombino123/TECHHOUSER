using System.IO;

namespace NAudio.Wave;

public class RawSourceWaveStream : WaveStream
{
	private readonly Stream sourceStream;

	private readonly WaveFormat waveFormat;

	public override WaveFormat WaveFormat => waveFormat;

	public override long Length => sourceStream.Length;

	public override long Position
	{
		get
		{
			return sourceStream.Position;
		}
		set
		{
			sourceStream.Position = value - value % waveFormat.BlockAlign;
		}
	}

	public RawSourceWaveStream(Stream sourceStream, WaveFormat waveFormat)
	{
		this.sourceStream = sourceStream;
		this.waveFormat = waveFormat;
	}

	public RawSourceWaveStream(byte[] byteStream, int offset, int count, WaveFormat waveFormat)
	{
		sourceStream = new MemoryStream(byteStream, offset, count);
		this.waveFormat = waveFormat;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		try
		{
			return sourceStream.Read(buffer, offset, count);
		}
		catch (EndOfStreamException)
		{
			return 0;
		}
	}
}
