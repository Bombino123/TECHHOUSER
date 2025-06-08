using System;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class BigEndianConverter
{
	public static ushort ToUInt16(byte[] buffer, int offset)
	{
		return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
	}

	public static short ToInt16(byte[] buffer, int offset)
	{
		return (short)ToUInt16(buffer, offset);
	}

	public static uint ToUInt32(byte[] buffer, int offset)
	{
		return (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3]);
	}

	public static int ToInt32(byte[] buffer, int offset)
	{
		return (int)ToUInt32(buffer, offset);
	}

	public static ulong ToUInt64(byte[] buffer, int offset)
	{
		return ((ulong)ToUInt32(buffer, offset) << 32) | ToUInt32(buffer, offset + 4);
	}

	public static long ToInt64(byte[] buffer, int offset)
	{
		return (long)ToUInt64(buffer, offset);
	}

	public static Guid ToGuid(byte[] buffer, int offset)
	{
		return new Guid(ToUInt32(buffer, offset), ToUInt16(buffer, offset + 4), ToUInt16(buffer, offset + 6), buffer[offset + 8], buffer[offset + 9], buffer[offset + 10], buffer[offset + 11], buffer[offset + 12], buffer[offset + 13], buffer[offset + 14], buffer[offset + 15]);
	}

	public static byte[] GetBytes(ushort value)
	{
		return new byte[2]
		{
			(byte)((uint)(value >> 8) & 0xFFu),
			(byte)(value & 0xFFu)
		};
	}

	public static byte[] GetBytes(short value)
	{
		return GetBytes((ushort)value);
	}

	public static byte[] GetBytes(uint value)
	{
		return new byte[4]
		{
			(byte)((value >> 24) & 0xFFu),
			(byte)((value >> 16) & 0xFFu),
			(byte)((value >> 8) & 0xFFu),
			(byte)(value & 0xFFu)
		};
	}

	public static byte[] GetBytes(int value)
	{
		return GetBytes((uint)value);
	}

	public static byte[] GetBytes(ulong value)
	{
		byte[] array = new byte[8];
		Array.Copy(GetBytes((uint)(value >> 32)), 0, array, 0, 4);
		Array.Copy(GetBytes((uint)(value & 0xFFFFFFFFu)), 0, array, 4, 4);
		return array;
	}

	public static byte[] GetBytes(long value)
	{
		return GetBytes((ulong)value);
	}

	public static byte[] GetBytes(Guid value)
	{
		byte[] array = value.ToByteArray();
		if (BitConverter.IsLittleEndian)
		{
			byte b = array[0];
			array[0] = array[3];
			array[3] = b;
			b = array[1];
			array[1] = array[2];
			array[2] = b;
			b = array[4];
			array[4] = array[5];
			array[5] = b;
			b = array[6];
			array[6] = array[7];
			array[7] = b;
		}
		return array;
	}
}
