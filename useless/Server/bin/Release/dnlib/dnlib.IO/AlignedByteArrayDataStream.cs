using System;
using System.Runtime.InteropServices;
using System.Text;

namespace dnlib.IO;

internal sealed class AlignedByteArrayDataStream : DataStream
{
	private readonly byte[] data;

	public AlignedByteArrayDataStream(byte[] data)
	{
		this.data = data;
	}

	public unsafe override void ReadBytes(uint offset, void* destination, int length)
	{
		Marshal.Copy(data, (int)offset, (IntPtr)destination, length);
	}

	public override void ReadBytes(uint offset, byte[] destination, int destinationIndex, int length)
	{
		Array.Copy(data, (int)offset, destination, destinationIndex, length);
	}

	public override byte ReadByte(uint offset)
	{
		return data[offset];
	}

	public override ushort ReadUInt16(uint offset)
	{
		int num = (int)offset;
		byte[] array = data;
		return (ushort)(array[num++] | (array[num] << 8));
	}

	public override uint ReadUInt32(uint offset)
	{
		int num = (int)offset;
		byte[] array = data;
		return (uint)(array[num++] | (array[num++] << 8) | (array[num++] << 16) | (array[num] << 24));
	}

	public override ulong ReadUInt64(uint offset)
	{
		int num = (int)offset;
		byte[] array = data;
		return array[num++] | ((ulong)array[num++] << 8) | ((ulong)array[num++] << 16) | ((ulong)array[num++] << 24) | ((ulong)array[num++] << 32) | ((ulong)array[num++] << 40) | ((ulong)array[num++] << 48) | ((ulong)array[num] << 56);
	}

	public unsafe override float ReadSingle(uint offset)
	{
		int num = (int)offset;
		byte[] array = data;
		uint num2 = (uint)(array[num++] | (array[num++] << 8) | (array[num++] << 16) | (array[num] << 24));
		return *(float*)(&num2);
	}

	public unsafe override double ReadDouble(uint offset)
	{
		int num = (int)offset;
		byte[] array = data;
		ulong num2 = array[num++] | ((ulong)array[num++] << 8) | ((ulong)array[num++] << 16) | ((ulong)array[num++] << 24) | ((ulong)array[num++] << 32) | ((ulong)array[num++] << 40) | ((ulong)array[num++] << 48) | ((ulong)array[num] << 56);
		return *(double*)(&num2);
	}

	public unsafe override string ReadUtf16String(uint offset, int chars)
	{
		fixed (byte* ptr = data)
		{
			return new string((char*)(ptr + offset), 0, chars);
		}
	}

	public unsafe override string ReadString(uint offset, int length, Encoding encoding)
	{
		fixed (byte* ptr = data)
		{
			return new string((sbyte*)(ptr + offset), 0, length, encoding);
		}
	}

	public unsafe override bool TryGetOffsetOf(uint offset, uint endOffset, byte value, out uint valueOffset)
	{
		fixed (byte* ptr = data)
		{
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
}
