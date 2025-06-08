using System;
using System.IO;

namespace dnlib.DotNet;

internal static class StrongNameUtils
{
	public static byte[] ReadBytesReverse(this BinaryReader reader, int len)
	{
		byte[] array = reader.ReadBytes(len);
		if (array.Length != len)
		{
			throw new InvalidKeyException("Can't read more bytes");
		}
		Array.Reverse((Array)array);
		return array;
	}

	public static void WriteReverse(this BinaryWriter writer, byte[] data)
	{
		byte[] array = (byte[])data.Clone();
		Array.Reverse((Array)array);
		writer.Write(array);
	}
}
