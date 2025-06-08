using System;
using System.IO;
using System.Text;

namespace Leb128;

internal class LEB128Decoder
{
	public static ulong ReadLEB128(Stream stream)
	{
		int num = 0;
		ulong num2 = 0uL;
		byte b;
		do
		{
			int num3 = stream.ReadByte();
			if (num3 < 0)
			{
				throw new InvalidOperationException();
			}
			b = (byte)num3;
			num2 |= (ulong)((long)(b & 0x7F) << num);
			num += 7;
		}
		while ((b & 0x80u) != 0);
		return num2;
	}

	public static long ReadLEB128Int64(Stream stream)
	{
		long num = 0L;
		int num2 = 0;
		bool flag = true;
		bool flag2 = false;
		while (flag)
		{
			int num3 = stream.ReadByte();
			if (num3 < 0)
			{
				throw new InvalidOperationException();
			}
			byte num4 = (byte)num3;
			flag = (num4 & 0x80) != 0;
			flag2 = (num4 & 0x40) != 0;
			long num5 = (long)num4 & 0x7FL;
			num |= num5 << num2;
			num2 += 7;
		}
		if (num2 < 64 && flag2)
		{
			num |= -1L << num2;
		}
		return num;
	}

	public static int ReadLEB128Int32(Stream stream)
	{
		return (int)ReadLEB128(stream);
	}

	public static bool ReadLEB128Bool(Stream stream)
	{
		return ReadLEB128(stream) != 0;
	}

	public static double ReadLEB128Double(Stream stream)
	{
		return BitConverter.ToDouble(ReadLEB128Bytes(stream), 0);
	}

	public static float ReadLEB128Float(Stream stream)
	{
		return BitConverter.ToSingle(ReadLEB128Bytes(stream), 0);
	}

	public static byte[] ReadLEB128Bytes(Stream stream)
	{
		int num = (int)ReadLEB128(stream);
		byte[] array = new byte[num];
		stream.Read(array, 0, num);
		return array;
	}

	public static string ReadLEB128String(Stream stream)
	{
		return Encoding.UTF8.GetString(ReadLEB128Bytes(stream));
	}
}
