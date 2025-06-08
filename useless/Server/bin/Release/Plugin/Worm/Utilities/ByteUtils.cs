using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class ByteUtils
{
	public static byte[] Concatenate(byte[] a, byte[] b)
	{
		byte[] array = new byte[a.Length + b.Length];
		Array.Copy(a, 0, array, 0, a.Length);
		Array.Copy(b, 0, array, a.Length, b.Length);
		return array;
	}

	public static bool AreByteArraysEqual(byte[] array1, byte[] array2)
	{
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (array1[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static byte[] XOR(byte[] array1, byte[] array2)
	{
		if (array1.Length == array2.Length)
		{
			return XOR(array1, 0, array2, 0, array1.Length);
		}
		throw new ArgumentException("Arrays must be of equal length");
	}

	public static byte[] XOR(byte[] array1, int offset1, byte[] array2, int offset2, int length)
	{
		if (offset1 + length <= array1.Length && offset2 + length <= array2.Length)
		{
			byte[] array3 = new byte[length];
			for (int i = 0; i < length; i++)
			{
				array3[i] = (byte)(array1[offset1 + i] ^ array2[offset2 + i]);
			}
			return array3;
		}
		throw new ArgumentOutOfRangeException();
	}

	public static long CopyStream(Stream input, Stream output)
	{
		return CopyStream(input, output, long.MaxValue);
	}

	public static long CopyStream(Stream input, Stream output, long count)
	{
		int num = (int)Math.Min(1048576L, count);
		byte[] buffer = new byte[num];
		long num2 = 0L;
		while (num2 < count)
		{
			int count2 = (int)Math.Min(num, count - num2);
			int num3 = input.Read(buffer, 0, count2);
			num2 += num3;
			output.Write(buffer, 0, num3);
			if (num3 == 0)
			{
				return num2;
			}
		}
		return num2;
	}
}
