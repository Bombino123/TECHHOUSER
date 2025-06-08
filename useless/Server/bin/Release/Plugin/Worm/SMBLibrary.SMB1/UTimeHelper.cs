using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class UTimeHelper
{
	public static readonly DateTime MinUTimeValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

	public static DateTime ReadUTime(byte[] buffer, int offset)
	{
		uint num = LittleEndianConverter.ToUInt32(buffer, offset);
		DateTime minUTimeValue = MinUTimeValue;
		return minUTimeValue.AddSeconds(num);
	}

	public static DateTime ReadUTime(byte[] buffer, ref int offset)
	{
		offset += 4;
		return ReadUTime(buffer, offset - 4);
	}

	public static DateTime? ReadNullableUTime(byte[] buffer, int offset)
	{
		uint num = LittleEndianConverter.ToUInt32(buffer, offset);
		if (num != 0)
		{
			DateTime minUTimeValue = MinUTimeValue;
			return minUTimeValue.AddSeconds(num);
		}
		return null;
	}

	public static DateTime? ReadNullableUTime(byte[] buffer, ref int offset)
	{
		offset += 4;
		return ReadNullableUTime(buffer, offset - 4);
	}

	public static void WriteUTime(byte[] buffer, int offset, DateTime time)
	{
		uint value = (uint)(time - MinUTimeValue).TotalSeconds;
		LittleEndianWriter.WriteUInt32(buffer, offset, value);
	}

	public static void WriteUTime(byte[] buffer, ref int offset, DateTime time)
	{
		WriteUTime(buffer, offset, time);
		offset += 4;
	}

	public static void WriteUTime(byte[] buffer, int offset, DateTime? time)
	{
		uint value = 0u;
		if (time.HasValue)
		{
			value = (uint)(time.Value - MinUTimeValue).TotalSeconds;
		}
		LittleEndianWriter.WriteUInt32(buffer, offset, value);
	}

	public static void WriteUTime(byte[] buffer, ref int offset, DateTime? time)
	{
		WriteUTime(buffer, offset, time);
		offset += 4;
	}
}
