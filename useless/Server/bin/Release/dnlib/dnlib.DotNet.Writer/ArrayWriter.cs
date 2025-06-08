using System;

namespace dnlib.DotNet.Writer;

public struct ArrayWriter
{
	private readonly byte[] data;

	private int position;

	public int Position
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

	public ArrayWriter(byte[] data)
	{
		this.data = data;
		position = 0;
	}

	public void WriteSByte(sbyte value)
	{
		data[position++] = (byte)value;
	}

	public void WriteByte(byte value)
	{
		data[position++] = value;
	}

	public void WriteInt16(short value)
	{
		data[position++] = (byte)value;
		data[position++] = (byte)(value >> 8);
	}

	public void WriteUInt16(ushort value)
	{
		data[position++] = (byte)value;
		data[position++] = (byte)(value >> 8);
	}

	public unsafe void WriteInt32(int value)
	{
		int num = position;
		fixed (byte* ptr = data)
		{
			*(int*)(ptr + num) = value;
		}
		position = num + 4;
	}

	public unsafe void WriteUInt32(uint value)
	{
		int num = position;
		fixed (byte* ptr = data)
		{
			*(uint*)(ptr + num) = value;
		}
		position = num + 4;
	}

	public unsafe void WriteInt64(long value)
	{
		int num = position;
		fixed (byte* ptr = data)
		{
			*(long*)(ptr + num) = value;
		}
		position = num + 8;
	}

	public unsafe void WriteUInt64(ulong value)
	{
		int num = position;
		fixed (byte* ptr = data)
		{
			*(ulong*)(ptr + num) = value;
		}
		position = num + 8;
	}

	public unsafe void WriteSingle(float value)
	{
		int num = position;
		fixed (byte* ptr = data)
		{
			*(float*)(ptr + num) = value;
		}
		position = num + 4;
	}

	public unsafe void WriteDouble(double value)
	{
		int num = position;
		fixed (byte* ptr = data)
		{
			*(double*)(ptr + num) = value;
		}
		position = num + 8;
	}

	public void WriteBytes(byte[] source)
	{
		WriteBytes(source, 0, source.Length);
	}

	public void WriteBytes(byte[] source, int index, int length)
	{
		Array.Copy(source, index, data, position, length);
		position += length;
	}
}
