using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class LockingRange
{
	public const int Length32 = 10;

	public const int Length64 = 20;

	public ushort PID;

	public ulong ByteOffset;

	public ulong LengthInBytes;

	public void Write32(byte[] buffer, ref int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, ref offset, PID);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)ByteOffset);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)LengthInBytes);
	}

	public void Write64(byte[] buffer, ref int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, ref offset, PID);
		offset += 2;
		LittleEndianWriter.WriteUInt64(buffer, ref offset, ByteOffset);
		LittleEndianWriter.WriteUInt64(buffer, ref offset, LengthInBytes);
	}

	public static LockingRange Read32(byte[] buffer, ref int offset)
	{
		return new LockingRange
		{
			PID = LittleEndianReader.ReadUInt16(buffer, ref offset),
			ByteOffset = LittleEndianReader.ReadUInt32(buffer, ref offset),
			LengthInBytes = LittleEndianReader.ReadUInt32(buffer, ref offset)
		};
	}

	public static LockingRange Read64(byte[] buffer, ref int offset)
	{
		LockingRange obj = new LockingRange
		{
			PID = LittleEndianReader.ReadUInt16(buffer, ref offset)
		};
		offset += 2;
		obj.ByteOffset = LittleEndianReader.ReadUInt64(buffer, ref offset);
		obj.LengthInBytes = LittleEndianReader.ReadUInt64(buffer, ref offset);
		return obj;
	}
}
