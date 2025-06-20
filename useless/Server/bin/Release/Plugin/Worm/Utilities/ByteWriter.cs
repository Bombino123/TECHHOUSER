using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Utilities;

[ComVisible(true)]
public class ByteWriter
{
	public static void WriteByte(byte[] buffer, int offset, byte value)
	{
		buffer[offset] = value;
	}

	public static void WriteByte(byte[] buffer, ref int offset, byte value)
	{
		buffer[offset] = value;
		offset++;
	}

	public static void WriteBytes(byte[] buffer, int offset, byte[] bytes)
	{
		WriteBytes(buffer, offset, bytes, bytes.Length);
	}

	public static void WriteBytes(byte[] buffer, ref int offset, byte[] bytes)
	{
		WriteBytes(buffer, offset, bytes);
		offset += bytes.Length;
	}

	public static void WriteBytes(byte[] buffer, int offset, byte[] bytes, int length)
	{
		Array.Copy(bytes, 0, buffer, offset, length);
	}

	public static void WriteBytes(byte[] buffer, ref int offset, byte[] bytes, int length)
	{
		Array.Copy(bytes, 0, buffer, offset, length);
		offset += length;
	}

	public static void WriteAnsiString(byte[] buffer, int offset, string value)
	{
		WriteAnsiString(buffer, offset, value, value.Length);
	}

	public static void WriteAnsiString(byte[] buffer, ref int offset, string value)
	{
		WriteAnsiString(buffer, ref offset, value, value.Length);
	}

	public static void WriteAnsiString(byte[] buffer, int offset, string value, int maximumLength)
	{
		Array.Copy(Encoding.GetEncoding(28591).GetBytes(value), 0, buffer, offset, Math.Min(value.Length, maximumLength));
	}

	public static void WriteAnsiString(byte[] buffer, ref int offset, string value, int fieldLength)
	{
		WriteAnsiString(buffer, offset, value, fieldLength);
		offset += fieldLength;
	}

	public static void WriteUTF16String(byte[] buffer, int offset, string value)
	{
		WriteUTF16String(buffer, offset, value, value.Length);
	}

	public static void WriteUTF16String(byte[] buffer, ref int offset, string value)
	{
		WriteUTF16String(buffer, ref offset, value, value.Length);
	}

	public static void WriteUTF16String(byte[] buffer, int offset, string value, int maximumNumberOfCharacters)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		int length = Math.Min(value.Length, maximumNumberOfCharacters) * 2;
		Array.Copy(bytes, 0, buffer, offset, length);
	}

	public static void WriteUTF16String(byte[] buffer, ref int offset, string value, int numberOfCharacters)
	{
		WriteUTF16String(buffer, offset, value, numberOfCharacters);
		offset += numberOfCharacters * 2;
	}

	public static void WriteNullTerminatedAnsiString(byte[] buffer, int offset, string value)
	{
		WriteAnsiString(buffer, offset, value);
		WriteByte(buffer, offset + value.Length, 0);
	}

	public static void WriteNullTerminatedAnsiString(byte[] buffer, ref int offset, string value)
	{
		WriteNullTerminatedAnsiString(buffer, offset, value);
		offset += value.Length + 1;
	}

	public static void WriteNullTerminatedUTF16String(byte[] buffer, int offset, string value)
	{
		WriteUTF16String(buffer, offset, value);
		WriteBytes(buffer, offset + value.Length * 2, new byte[2]);
	}

	public static void WriteNullTerminatedUTF16String(byte[] buffer, ref int offset, string value)
	{
		WriteNullTerminatedUTF16String(buffer, offset, value);
		offset += value.Length * 2 + 2;
	}

	public static void WriteBytes(Stream stream, byte[] bytes)
	{
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteBytes(Stream stream, byte[] bytes, int count)
	{
		stream.Write(bytes, 0, count);
	}

	public static void WriteAnsiString(Stream stream, string value)
	{
		WriteAnsiString(stream, value, value.Length);
	}

	public static void WriteAnsiString(Stream stream, string value, int fieldLength)
	{
		byte[] bytes = Encoding.GetEncoding(28591).GetBytes(value);
		stream.Write(bytes, 0, Math.Min(bytes.Length, fieldLength));
		if (bytes.Length < fieldLength)
		{
			byte[] array = new byte[fieldLength - bytes.Length];
			stream.Write(array, 0, array.Length);
		}
	}

	public static void WriteUTF8String(Stream stream, string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteUTF16String(Stream stream, string value)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteUTF16BEString(Stream stream, string value)
	{
		byte[] bytes = Encoding.BigEndianUnicode.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}
}
