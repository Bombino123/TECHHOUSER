using System.Runtime.InteropServices;

namespace System.Data.SQLite;

internal static class SQLiteBytes
{
	public static byte[] FromIntPtr(IntPtr pValue, int length)
	{
		if (pValue == IntPtr.Zero)
		{
			return null;
		}
		if (length == 0)
		{
			return new byte[0];
		}
		byte[] array = new byte[length];
		Marshal.Copy(pValue, array, 0, length);
		return array;
	}

	public static IntPtr ToIntPtr(byte[] value)
	{
		int length = 0;
		return ToIntPtr(value, ref length);
	}

	public static IntPtr ToIntPtr(byte[] value, ref int length)
	{
		if (value == null)
		{
			return IntPtr.Zero;
		}
		length = value.Length;
		if (length == 0)
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = SQLiteMemory.Allocate(length);
		if (intPtr == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		Marshal.Copy(value, 0, intPtr, length);
		return intPtr;
	}
}
