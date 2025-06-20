using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Data.SQLite;

internal static class SQLiteMarshal
{
	public static IntPtr IntPtrForOffset(IntPtr pointer, int offset)
	{
		return new IntPtr(pointer.ToInt64() + offset);
	}

	public static int RoundUp(int size, int alignment)
	{
		int num = alignment - 1;
		return (size + num) & ~num;
	}

	public static int NextOffsetOf(int offset, int size, int alignment)
	{
		return RoundUp(offset + size, alignment);
	}

	public static int ReadInt32(IntPtr pointer, int offset)
	{
		return Marshal.ReadInt32(pointer, offset);
	}

	public static long ReadInt64(IntPtr pointer, int offset)
	{
		return Marshal.ReadInt64(pointer, offset);
	}

	public static double ReadDouble(IntPtr pointer, int offset)
	{
		return BitConverter.Int64BitsToDouble(Marshal.ReadInt64(pointer, offset));
	}

	public static IntPtr ReadIntPtr(IntPtr pointer, int offset)
	{
		return Marshal.ReadIntPtr(pointer, offset);
	}

	public static void WriteInt32(IntPtr pointer, int offset, int value)
	{
		Marshal.WriteInt32(pointer, offset, value);
	}

	public static void WriteInt64(IntPtr pointer, int offset, long value)
	{
		Marshal.WriteInt64(pointer, offset, value);
	}

	public static void WriteDouble(IntPtr pointer, int offset, double value)
	{
		Marshal.WriteInt64(pointer, offset, BitConverter.DoubleToInt64Bits(value));
	}

	public static void WriteIntPtr(IntPtr pointer, int offset, IntPtr value)
	{
		Marshal.WriteIntPtr(pointer, offset, value);
	}

	public static int GetHashCode(object value, bool identity)
	{
		if (identity)
		{
			return RuntimeHelpers.GetHashCode(value);
		}
		return value?.GetHashCode() ?? 0;
	}
}
