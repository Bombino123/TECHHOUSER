using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SMB1Helper
{
	public static DateTime? ReadNullableFileTime(byte[] buffer, int offset)
	{
		long num = LittleEndianConverter.ToInt64(buffer, offset);
		if (num >= 0)
		{
			return DateTime.FromFileTimeUtc(num);
		}
		if (num == 0L)
		{
			return null;
		}
		return DateTime.UtcNow.Subtract(TimeSpan.FromTicks(num));
	}

	public static DateTime? ReadNullableFileTime(byte[] buffer, ref int offset)
	{
		offset += 8;
		return ReadNullableFileTime(buffer, offset - 8);
	}

	public static DateTime ReadSMBDate(byte[] buffer, int offset)
	{
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset);
		int year = ((num & 0xFE00) >> 9) + 1980;
		int month = (num & 0x1E0) >> 5;
		int day = num & 0x1F;
		return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
	}

	public static void WriteSMBDate(byte[] buffer, int offset, DateTime date)
	{
		int num = date.Year - 1980;
		int month = date.Month;
		int day = date.Day;
		ushort value = (ushort)((num << 9) | (month << 5) | day);
		LittleEndianWriter.WriteUInt16(buffer, offset, value);
	}

	public static TimeSpan ReadSMBTime(byte[] buffer, int offset)
	{
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset);
		int hours = (num & 0xF800) >> 11;
		int minutes = (num & 0x7E0) >> 5;
		int seconds = num & 0x1F;
		return new TimeSpan(hours, minutes, seconds);
	}

	public static void WriteSMBTime(byte[] buffer, int offset, TimeSpan time)
	{
		ushort value = (ushort)((time.Hours << 11) | (time.Minutes << 5) | time.Seconds);
		LittleEndianWriter.WriteUInt16(buffer, offset, value);
	}

	public static DateTime ReadSMBDateTime(byte[] buffer, int offset)
	{
		DateTime dateTime = ReadSMBDate(buffer, offset);
		TimeSpan value = ReadSMBTime(buffer, offset + 2);
		return dateTime.Add(value);
	}

	public static void WriteSMBDateTime(byte[] buffer, int offset, DateTime dateTime)
	{
		WriteSMBDate(buffer, offset, dateTime.Date);
		WriteSMBTime(buffer, offset + 2, dateTime.TimeOfDay);
	}

	public static DateTime? ReadNullableSMBDateTime(byte[] buffer, int offset)
	{
		if (LittleEndianConverter.ToUInt32(buffer, offset) != 0)
		{
			return ReadSMBDateTime(buffer, offset);
		}
		return null;
	}

	public static void WriteSMBDateTime(byte[] buffer, int offset, DateTime? dateTime)
	{
		if (dateTime.HasValue)
		{
			WriteSMBDateTime(buffer, offset, dateTime.Value);
		}
		else
		{
			LittleEndianWriter.WriteUInt32(buffer, offset, 0u);
		}
	}

	public static string ReadSMBString(byte[] buffer, int offset, bool isUnicode)
	{
		if (isUnicode)
		{
			return ByteReader.ReadNullTerminatedUTF16String(buffer, offset);
		}
		return ByteReader.ReadNullTerminatedAnsiString(buffer, offset);
	}

	public static string ReadSMBString(byte[] buffer, ref int offset, bool isUnicode)
	{
		if (isUnicode)
		{
			return ByteReader.ReadNullTerminatedUTF16String(buffer, ref offset);
		}
		return ByteReader.ReadNullTerminatedAnsiString(buffer, ref offset);
	}

	public static void WriteSMBString(byte[] buffer, int offset, bool isUnicode, string value)
	{
		if (isUnicode)
		{
			ByteWriter.WriteNullTerminatedUTF16String(buffer, offset, value);
		}
		else
		{
			ByteWriter.WriteNullTerminatedAnsiString(buffer, offset, value);
		}
	}

	public static void WriteSMBString(byte[] buffer, ref int offset, bool isUnicode, string value)
	{
		if (isUnicode)
		{
			ByteWriter.WriteNullTerminatedUTF16String(buffer, ref offset, value);
		}
		else
		{
			ByteWriter.WriteNullTerminatedAnsiString(buffer, ref offset, value);
		}
	}

	public static string ReadFixedLengthString(byte[] buffer, ref int offset, bool isUnicode, int byteCount)
	{
		if (isUnicode)
		{
			return ByteReader.ReadUTF16String(buffer, ref offset, byteCount / 2);
		}
		return ByteReader.ReadAnsiString(buffer, ref offset, byteCount);
	}

	public static void WriteFixedLengthString(byte[] buffer, ref int offset, bool isUnicode, string value)
	{
		if (isUnicode)
		{
			ByteWriter.WriteUTF16String(buffer, ref offset, value);
		}
		else
		{
			ByteWriter.WriteAnsiString(buffer, ref offset, value);
		}
	}
}
