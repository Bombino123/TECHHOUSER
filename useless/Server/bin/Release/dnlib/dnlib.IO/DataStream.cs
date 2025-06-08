using System;
using System.Text;

namespace dnlib.IO;

public abstract class DataStream
{
	public unsafe abstract void ReadBytes(uint offset, void* destination, int length);

	public abstract void ReadBytes(uint offset, byte[] destination, int destinationIndex, int length);

	public abstract byte ReadByte(uint offset);

	public virtual sbyte ReadSByte(uint offset)
	{
		return (sbyte)ReadByte(offset);
	}

	public virtual bool ReadBoolean(uint offset)
	{
		return ReadByte(offset) != 0;
	}

	public abstract ushort ReadUInt16(uint offset);

	public virtual short ReadInt16(uint offset)
	{
		return (short)ReadUInt16(offset);
	}

	public virtual char ReadChar(uint offset)
	{
		return (char)ReadUInt16(offset);
	}

	public abstract uint ReadUInt32(uint offset);

	public virtual int ReadInt32(uint offset)
	{
		return (int)ReadUInt32(offset);
	}

	public abstract ulong ReadUInt64(uint offset);

	public virtual long ReadInt64(uint offset)
	{
		return (long)ReadUInt64(offset);
	}

	public abstract float ReadSingle(uint offset);

	public abstract double ReadDouble(uint offset);

	public virtual Guid ReadGuid(uint offset)
	{
		return new Guid(ReadUInt32(offset), ReadUInt16(offset + 4), ReadUInt16(offset + 6), ReadByte(offset + 8), ReadByte(offset + 9), ReadByte(offset + 10), ReadByte(offset + 11), ReadByte(offset + 12), ReadByte(offset + 13), ReadByte(offset + 14), ReadByte(offset + 15));
	}

	public virtual decimal ReadDecimal(uint offset)
	{
		int lo = ReadInt32(offset);
		int mid = ReadInt32(offset + 4);
		int hi = ReadInt32(offset + 8);
		int num = ReadInt32(offset + 12);
		byte scale = (byte)(num >> 16);
		bool isNegative = (num & 0x80000000u) != 0;
		return new decimal(lo, mid, hi, isNegative, scale);
	}

	public abstract string ReadUtf16String(uint offset, int chars);

	public abstract string ReadString(uint offset, int length, Encoding encoding);

	public abstract bool TryGetOffsetOf(uint offset, uint endOffset, byte value, out uint valueOffset);
}
