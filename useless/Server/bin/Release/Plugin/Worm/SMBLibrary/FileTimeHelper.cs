using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileTimeHelper
{
	public static readonly DateTime MinFileTimeValue = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static DateTime ReadFileTime(byte[] buffer, int offset)
	{
		long num = LittleEndianConverter.ToInt64(buffer, offset);
		if (num >= 0)
		{
			return DateTime.FromFileTimeUtc(num);
		}
		throw new InvalidDataException("FILETIME cannot be negative");
	}

	public static DateTime ReadFileTime(byte[] buffer, ref int offset)
	{
		offset += 8;
		return ReadFileTime(buffer, offset - 8);
	}

	public static void WriteFileTime(byte[] buffer, int offset, DateTime time)
	{
		long value = time.ToFileTimeUtc();
		LittleEndianWriter.WriteInt64(buffer, offset, value);
	}

	public static void WriteFileTime(byte[] buffer, ref int offset, DateTime time)
	{
		WriteFileTime(buffer, offset, time);
		offset += 8;
	}

	public static DateTime? ReadNullableFileTime(byte[] buffer, int offset)
	{
		long num = LittleEndianConverter.ToInt64(buffer, offset);
		if (num > 0)
		{
			return DateTime.FromFileTimeUtc(num);
		}
		if (num == 0L)
		{
			return null;
		}
		throw new InvalidDataException("FILETIME cannot be negative");
	}

	public static DateTime? ReadNullableFileTime(byte[] buffer, ref int offset)
	{
		offset += 8;
		return ReadNullableFileTime(buffer, offset - 8);
	}

	public static void WriteFileTime(byte[] buffer, int offset, DateTime? time)
	{
		long value = 0L;
		if (time.HasValue)
		{
			value = time.Value.ToFileTimeUtc();
		}
		LittleEndianWriter.WriteInt64(buffer, offset, value);
	}

	public static void WriteFileTime(byte[] buffer, ref int offset, DateTime? time)
	{
		WriteFileTime(buffer, offset, time);
		offset += 8;
	}

	public static SetFileTime ReadSetFileTime(byte[] buffer, int offset)
	{
		return SetFileTime.FromFileTimeUtc(LittleEndianConverter.ToInt64(buffer, offset));
	}

	public static void WriteSetFileTime(byte[] buffer, int offset, SetFileTime time)
	{
		long value = time.ToFileTimeUtc();
		LittleEndianWriter.WriteInt64(buffer, offset, value);
	}
}
