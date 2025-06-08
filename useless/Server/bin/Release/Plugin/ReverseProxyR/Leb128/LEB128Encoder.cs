using System;
using System.IO;
using System.Text;

namespace Leb128;

internal class LEB128Encoder
{
	public static void WriteLEB128(Stream stream, ulong value)
	{
		do
		{
			byte b = (byte)(value & 0x7F);
			value >>= 7;
			if (value != 0L)
			{
				b = (byte)(b | 0x80u);
			}
			stream.WriteByte(b);
		}
		while (value != 0L);
	}

	public static void WriteLEB128(Stream stream, long value)
	{
		bool flag = true;
		while (flag)
		{
			byte b = (byte)(value & 0x7F);
			value >>= 7;
			bool flag2 = (b & 0x40) != 0;
			flag = (value != 0L || flag2) && !(value == -1 && flag2);
			if (flag)
			{
				b = (byte)(b | 0x80u);
			}
			stream.WriteByte(b);
		}
	}

	public static void WriteLEB128(Stream stream, int value)
	{
		WriteLEB128(stream, (ulong)value);
	}

	public static void WriteLEB128(Stream stream, bool value)
	{
		WriteLEB128(stream, (ulong)(value ? 1 : 0));
	}

	public static void WriteLEB128(Stream stream, double value)
	{
		WriteLEB128(stream, BitConverter.GetBytes(value));
	}

	public static void WriteLEB128(Stream stream, float value)
	{
		WriteLEB128(stream, BitConverter.GetBytes(value));
	}

	public static void WriteLEB128(Stream stream, byte[] bytes)
	{
		WriteLEB128(stream, (ulong)bytes.Length);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteLEB128(Stream stream, string value)
	{
		WriteLEB128(stream, Encoding.UTF8.GetBytes(value));
	}
}
