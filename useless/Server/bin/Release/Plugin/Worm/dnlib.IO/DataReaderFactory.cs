using System;
using System.Runtime.InteropServices;

namespace dnlib.IO;

[ComVisible(true)]
public abstract class DataReaderFactory : IDisposable
{
	public abstract string Filename { get; }

	public abstract uint Length { get; }

	public virtual event EventHandler DataReaderInvalidated
	{
		add
		{
		}
		remove
		{
		}
	}

	public DataReader CreateReader()
	{
		return CreateReader(0u, Length);
	}

	public abstract DataReader CreateReader(uint offset, uint length);

	private static void ThrowArgumentOutOfRangeException(string paramName)
	{
		throw new ArgumentOutOfRangeException(paramName);
	}

	private static void Throw_CreateReader_2(int offset, int length)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		throw new ArgumentOutOfRangeException("length");
	}

	public DataReader CreateReader(uint offset, int length)
	{
		if (length < 0)
		{
			ThrowArgumentOutOfRangeException("length");
		}
		return CreateReader(offset, (uint)length);
	}

	public DataReader CreateReader(int offset, uint length)
	{
		if (offset < 0)
		{
			ThrowArgumentOutOfRangeException("offset");
		}
		return CreateReader((uint)offset, length);
	}

	public DataReader CreateReader(int offset, int length)
	{
		if (offset < 0 || length < 0)
		{
			Throw_CreateReader_2(offset, length);
		}
		return CreateReader((uint)offset, (uint)length);
	}

	protected DataReader CreateReader(DataStream stream, uint offset, uint length)
	{
		uint length2 = Length;
		if (offset > length2)
		{
			offset = length2;
		}
		if ((ulong)((long)offset + (long)length) > (ulong)length2)
		{
			length = length2 - offset;
		}
		return new DataReader(stream, offset, length);
	}

	public abstract void Dispose();
}
