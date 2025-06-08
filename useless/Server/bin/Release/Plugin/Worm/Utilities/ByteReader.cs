using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Utilities;

[ComVisible(true)]
public class ByteReader
{
	public static byte ReadByte(byte[] buffer, int offset)
	{
		return buffer[offset];
	}

	public static byte ReadByte(byte[] buffer, ref int offset)
	{
		offset++;
		return buffer[offset - 1];
	}

	public static byte[] ReadBytes(byte[] buffer, int offset, int length)
	{
		byte[] array = new byte[length];
		Array.Copy(buffer, offset, array, 0, length);
		return array;
	}

	public static byte[] ReadBytes(byte[] buffer, ref int offset, int length)
	{
		offset += length;
		return ReadBytes(buffer, offset - length, length);
	}

	public static string ReadAnsiString(byte[] buffer, int offset, int count)
	{
		return Encoding.GetEncoding(28591).GetString(buffer, offset, count);
	}

	public static string ReadAnsiString(byte[] buffer, ref int offset, int count)
	{
		offset += count;
		return ReadAnsiString(buffer, offset - count, count);
	}

	public static string ReadUTF16String(byte[] buffer, int offset, int numberOfCharacters)
	{
		int count = numberOfCharacters * 2;
		return Encoding.Unicode.GetString(buffer, offset, count);
	}

	public static string ReadUTF16String(byte[] buffer, ref int offset, int numberOfCharacters)
	{
		int num = numberOfCharacters * 2;
		offset += num;
		return ReadUTF16String(buffer, offset - num, numberOfCharacters);
	}

	public static string ReadNullTerminatedAnsiString(byte[] buffer, int offset)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (char c = (char)ReadByte(buffer, offset); c != 0; c = (char)ReadByte(buffer, offset))
		{
			stringBuilder.Append(c);
			offset++;
		}
		return stringBuilder.ToString();
	}

	public static string ReadNullTerminatedUTF16String(byte[] buffer, int offset)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (char c = (char)LittleEndianConverter.ToUInt16(buffer, offset); c != 0; c = (char)LittleEndianConverter.ToUInt16(buffer, offset))
		{
			stringBuilder.Append(c);
			offset += 2;
		}
		return stringBuilder.ToString();
	}

	public static string ReadNullTerminatedAnsiString(byte[] buffer, ref int offset)
	{
		string text = ReadNullTerminatedAnsiString(buffer, offset);
		offset += text.Length + 1;
		return text;
	}

	public static string ReadNullTerminatedUTF16String(byte[] buffer, ref int offset)
	{
		string text = ReadNullTerminatedUTF16String(buffer, offset);
		offset += text.Length * 2 + 2;
		return text;
	}

	public static byte[] ReadBytes(Stream stream, int count)
	{
		MemoryStream memoryStream = new MemoryStream();
		ByteUtils.CopyStream(stream, memoryStream, count);
		return memoryStream.ToArray();
	}

	public static byte[] ReadAllBytes(Stream stream)
	{
		MemoryStream memoryStream = new MemoryStream();
		ByteUtils.CopyStream(stream, memoryStream);
		return memoryStream.ToArray();
	}

	public static string ReadAnsiString(Stream stream, int length)
	{
		byte[] bytes = ReadBytes(stream, length);
		return Encoding.GetEncoding(28591).GetString(bytes);
	}

	public static string ReadNullTerminatedAnsiString(Stream stream)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (char c = (char)stream.ReadByte(); c != 0; c = (char)stream.ReadByte())
		{
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}
}
