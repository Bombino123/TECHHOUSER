using System;
using System.Runtime.InteropServices;
using System.Text;

namespace dnlib.IO;

internal sealed class AlignedNativeMemoryDataStream : DataStream
{
	private unsafe readonly byte* data;

	public unsafe AlignedNativeMemoryDataStream(byte* data)
	{
		this.data = data;
	}

	public unsafe override void ReadBytes(uint offset, void* destination, int length)
	{
		byte* ptr = data + offset;
		byte* ptr2 = (byte*)destination;
		int num = length / 4;
		length %= 4;
		for (int i = 0; i < num; i++)
		{
			*ptr2 = *ptr;
			ptr2++;
			ptr++;
			*ptr2 = *ptr;
			ptr2++;
			ptr++;
			*ptr2 = *ptr;
			ptr2++;
			ptr++;
			*ptr2 = *ptr;
			ptr2++;
			ptr++;
		}
		int num2 = 0;
		while (num2 < length)
		{
			*ptr2 = *ptr;
			num2++;
			ptr++;
			ptr2++;
		}
	}

	public unsafe override void ReadBytes(uint offset, byte[] destination, int destinationIndex, int length)
	{
		Marshal.Copy((IntPtr)(data + offset), destination, destinationIndex, length);
	}

	public unsafe override byte ReadByte(uint offset)
	{
		return data[offset];
	}

	public unsafe override ushort ReadUInt16(uint offset)
	{
		byte* ptr = data + offset;
		return (ushort)(*(ptr++) | (*ptr << 8));
	}

	public unsafe override uint ReadUInt32(uint offset)
	{
		byte* ptr = data + offset;
		return (uint)(*(ptr++) | (*(ptr++) << 8) | (*(ptr++) << 16) | (*ptr << 24));
	}

	public unsafe override ulong ReadUInt64(uint offset)
	{
		byte* ptr = data + offset;
		return *(ptr++) | ((ulong)(*(ptr++)) << 8) | ((ulong)(*(ptr++)) << 16) | ((ulong)(*(ptr++)) << 24) | ((ulong)(*(ptr++)) << 32) | ((ulong)(*(ptr++)) << 40) | ((ulong)(*(ptr++)) << 48) | ((ulong)(*ptr) << 56);
	}

	public unsafe override float ReadSingle(uint offset)
	{
		byte* ptr = data + offset;
		uint num = (uint)(*(ptr++) | (*(ptr++) << 8) | (*(ptr++) << 16) | (*ptr << 24));
		return *(float*)(&num);
	}

	public unsafe override double ReadDouble(uint offset)
	{
		byte* ptr = data + offset;
		ulong num = *(ptr++) | ((ulong)(*(ptr++)) << 8) | ((ulong)(*(ptr++)) << 16) | ((ulong)(*(ptr++)) << 24) | ((ulong)(*(ptr++)) << 32) | ((ulong)(*(ptr++)) << 40) | ((ulong)(*(ptr++)) << 48) | ((ulong)(*ptr) << 56);
		return *(double*)(&num);
	}

	public unsafe override string ReadUtf16String(uint offset, int chars)
	{
		return new string((char*)(data + offset), 0, chars);
	}

	public unsafe override string ReadString(uint offset, int length, Encoding encoding)
	{
		return new string((sbyte*)(data + offset), 0, length, encoding);
	}

	public unsafe override bool TryGetOffsetOf(uint offset, uint endOffset, byte value, out uint valueOffset)
	{
		byte* ptr = data;
		byte* ptr2 = ptr + offset;
		uint num = (endOffset - offset) / 4;
		for (uint num2 = 0u; num2 < num; num2++)
		{
			if (*ptr2 == value)
			{
				valueOffset = (uint)(ptr2 - ptr);
				return true;
			}
			ptr2++;
			if (*ptr2 == value)
			{
				valueOffset = (uint)(ptr2 - ptr);
				return true;
			}
			ptr2++;
			if (*ptr2 == value)
			{
				valueOffset = (uint)(ptr2 - ptr);
				return true;
			}
			ptr2++;
			if (*ptr2 == value)
			{
				valueOffset = (uint)(ptr2 - ptr);
				return true;
			}
			ptr2++;
		}
		for (byte* ptr3 = ptr + endOffset; ptr2 != ptr3; ptr2++)
		{
			if (*ptr2 == value)
			{
				valueOffset = (uint)(ptr2 - ptr);
				return true;
			}
		}
		valueOffset = 0u;
		return false;
	}
}
