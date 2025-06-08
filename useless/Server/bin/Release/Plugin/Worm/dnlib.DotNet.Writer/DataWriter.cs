using System;
using System.IO;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class DataWriter
{
	private readonly Stream stream;

	private readonly byte[] buffer;

	private const int BUFFER_LEN = 8;

	internal Stream InternalStream => stream;

	public long Position
	{
		get
		{
			return stream.Position;
		}
		set
		{
			stream.Position = value;
		}
	}

	public DataWriter(Stream stream)
	{
		if (stream == null)
		{
			ThrowArgumentNullException("stream");
		}
		this.stream = stream;
		buffer = new byte[8];
	}

	private static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	private static void ThrowArgumentOutOfRangeException(string message)
	{
		throw new ArgumentOutOfRangeException(message);
	}

	public void WriteBoolean(bool value)
	{
		stream.WriteByte(value ? ((byte)1) : ((byte)0));
	}

	public void WriteSByte(sbyte value)
	{
		stream.WriteByte((byte)value);
	}

	public void WriteByte(byte value)
	{
		stream.WriteByte(value);
	}

	public void WriteInt16(short value)
	{
		byte[] array = buffer;
		array[0] = (byte)value;
		array[1] = (byte)(value >> 8);
		stream.Write(array, 0, 2);
	}

	public void WriteUInt16(ushort value)
	{
		byte[] array = buffer;
		array[0] = (byte)value;
		array[1] = (byte)(value >> 8);
		stream.Write(array, 0, 2);
	}

	public void WriteInt32(int value)
	{
		byte[] array = buffer;
		array[0] = (byte)value;
		array[1] = (byte)(value >> 8);
		array[2] = (byte)(value >> 16);
		array[3] = (byte)(value >> 24);
		stream.Write(array, 0, 4);
	}

	public void WriteUInt32(uint value)
	{
		byte[] array = buffer;
		array[0] = (byte)value;
		array[1] = (byte)(value >> 8);
		array[2] = (byte)(value >> 16);
		array[3] = (byte)(value >> 24);
		stream.Write(array, 0, 4);
	}

	public void WriteInt64(long value)
	{
		byte[] array = buffer;
		array[0] = (byte)value;
		array[1] = (byte)(value >> 8);
		array[2] = (byte)(value >> 16);
		array[3] = (byte)(value >> 24);
		array[4] = (byte)(value >> 32);
		array[5] = (byte)(value >> 40);
		array[6] = (byte)(value >> 48);
		array[7] = (byte)(value >> 56);
		stream.Write(array, 0, 8);
	}

	public void WriteUInt64(ulong value)
	{
		byte[] array = buffer;
		array[0] = (byte)value;
		array[1] = (byte)(value >> 8);
		array[2] = (byte)(value >> 16);
		array[3] = (byte)(value >> 24);
		array[4] = (byte)(value >> 32);
		array[5] = (byte)(value >> 40);
		array[6] = (byte)(value >> 48);
		array[7] = (byte)(value >> 56);
		stream.Write(array, 0, 8);
	}

	public unsafe void WriteSingle(float value)
	{
		uint num = *(uint*)(&value);
		byte[] array = buffer;
		array[0] = (byte)num;
		array[1] = (byte)(num >> 8);
		array[2] = (byte)(num >> 16);
		array[3] = (byte)(num >> 24);
		stream.Write(array, 0, 4);
	}

	public unsafe void WriteDouble(double value)
	{
		ulong num = *(ulong*)(&value);
		byte[] array = buffer;
		array[0] = (byte)num;
		array[1] = (byte)(num >> 8);
		array[2] = (byte)(num >> 16);
		array[3] = (byte)(num >> 24);
		array[4] = (byte)(num >> 32);
		array[5] = (byte)(num >> 40);
		array[6] = (byte)(num >> 48);
		array[7] = (byte)(num >> 56);
		stream.Write(array, 0, 8);
	}

	public void WriteBytes(byte[] source)
	{
		stream.Write(source, 0, source.Length);
	}

	public void WriteBytes(byte[] source, int index, int length)
	{
		stream.Write(source, index, length);
	}

	public void WriteCompressedUInt32(uint value)
	{
		Stream stream = this.stream;
		if (value <= 127)
		{
			stream.WriteByte((byte)value);
		}
		else if (value <= 16383)
		{
			stream.WriteByte((byte)((value >> 8) | 0x80u));
			stream.WriteByte((byte)value);
		}
		else if (value <= 536870911)
		{
			byte[] array = buffer;
			array[0] = (byte)((value >> 24) | 0xC0u);
			array[1] = (byte)(value >> 16);
			array[2] = (byte)(value >> 8);
			array[3] = (byte)value;
			stream.Write(array, 0, 4);
		}
		else
		{
			ThrowArgumentOutOfRangeException("UInt32 value can't be compressed");
		}
	}

	public void WriteCompressedInt32(int value)
	{
		Stream stream = this.stream;
		uint num = (uint)value >> 31;
		if (-64 <= value && value <= 63)
		{
			uint num2 = (uint)((value & 0x3F) << 1) | num;
			stream.WriteByte((byte)num2);
		}
		else if (-8192 <= value && value <= 8191)
		{
			uint num3 = (uint)((value & 0x1FFF) << 1) | num;
			stream.WriteByte((byte)((num3 >> 8) | 0x80u));
			stream.WriteByte((byte)num3);
		}
		else if (-268435456 <= value && value <= 268435455)
		{
			uint num4 = (uint)((value & 0xFFFFFFF) << 1) | num;
			byte[] array = buffer;
			array[0] = (byte)((num4 >> 24) | 0xC0u);
			array[1] = (byte)(num4 >> 16);
			array[2] = (byte)(num4 >> 8);
			array[3] = (byte)num4;
			stream.Write(array, 0, 4);
		}
		else
		{
			ThrowArgumentOutOfRangeException("Int32 value can't be compressed");
		}
	}

	public static int GetCompressedUInt32Length(uint value)
	{
		if (value <= 127)
		{
			return 1;
		}
		if (value <= 16383)
		{
			return 2;
		}
		if (value <= 536870911)
		{
			return 4;
		}
		ThrowArgumentOutOfRangeException("UInt32 value can't be compressed");
		return 0;
	}
}
