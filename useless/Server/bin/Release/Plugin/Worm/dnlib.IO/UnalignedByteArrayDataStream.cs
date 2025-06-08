using System;
using System.Runtime.InteropServices;
using System.Text;

namespace dnlib.IO;

internal sealed class UnalignedByteArrayDataStream : DataStream
{
	private readonly byte[] data;

	public UnalignedByteArrayDataStream(byte[] data)
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

	public unsafe override uint ReadUInt32(uint offset)
	{
		fixed (byte* ptr = data)
		{
			return *(uint*)(ptr + offset);
		}
	}

	public unsafe override ulong ReadUInt64(uint offset)
	{
		fixed (byte* ptr = data)
		{
			return *(ulong*)(ptr + offset);
		}
	}

	public unsafe override float ReadSingle(uint offset)
	{
		fixed (byte* ptr = data)
		{
			return *(float*)(ptr + offset);
		}
	}

	public unsafe override double ReadDouble(uint offset)
	{
		fixed (byte* ptr = data)
		{
			return *(double*)(ptr + offset);
		}
	}

	public unsafe override Guid ReadGuid(uint offset)
	{
		fixed (byte* ptr = data)
		{
			return *(Guid*)(ptr + offset);
		}
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
