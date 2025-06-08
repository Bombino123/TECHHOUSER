using System;
using System.IO;

namespace dnlib.IO;

internal sealed class DataReaderStream : Stream
{
	private DataReader reader;

	private long position;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => reader.Length;

	public override long Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public DataReaderStream(in DataReader reader)
	{
		this.reader = reader;
		position = reader.Position;
	}

	public override void Flush()
	{
	}

	private bool CheckAndSetPosition()
	{
		if ((ulong)position > (ulong)reader.Length)
		{
			return false;
		}
		reader.Position = (uint)position;
		return true;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			Position = offset;
			break;
		case SeekOrigin.Current:
			Position += offset;
			break;
		case SeekOrigin.End:
			Position = Length + offset;
			break;
		}
		return Position;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (!CheckAndSetPosition())
		{
			return 0;
		}
		int num = (int)Math.Min((uint)count, reader.BytesLeft);
		reader.ReadBytes(buffer, offset, num);
		Position += num;
		return num;
	}

	public override int ReadByte()
	{
		if (!CheckAndSetPosition() || !reader.CanRead(1u))
		{
			return -1;
		}
		Position++;
		return reader.ReadByte();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
