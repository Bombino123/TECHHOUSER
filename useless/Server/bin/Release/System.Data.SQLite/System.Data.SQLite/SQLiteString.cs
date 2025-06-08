using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SQLite;

internal static class SQLiteString
{
	private static int ThirtyBits = 1073741823;

	private static readonly Encoding Utf8Encoding = Encoding.UTF8;

	public static byte[] GetUtf8BytesFromString(string value)
	{
		if (value == null)
		{
			return null;
		}
		return Utf8Encoding.GetBytes(value);
	}

	public static string GetStringFromUtf8Bytes(byte[] bytes)
	{
		if (bytes == null)
		{
			return null;
		}
		return Utf8Encoding.GetString(bytes);
	}

	public static int ProbeForUtf8ByteLength(IntPtr pValue, int limit)
	{
		int i = 0;
		if (pValue != IntPtr.Zero && limit > 0)
		{
			for (; Marshal.ReadByte(pValue, i) != 0 && i < limit; i++)
			{
			}
		}
		return i;
	}

	public static string StringFromUtf8IntPtr(IntPtr pValue)
	{
		return StringFromUtf8IntPtr(pValue, ProbeForUtf8ByteLength(pValue, ThirtyBits));
	}

	public static string StringFromUtf8IntPtr(IntPtr pValue, int length)
	{
		if (pValue == IntPtr.Zero)
		{
			return null;
		}
		if (length > 0)
		{
			byte[] array = new byte[length];
			Marshal.Copy(pValue, array, 0, length);
			return GetStringFromUtf8Bytes(array);
		}
		return string.Empty;
	}

	public static IntPtr Utf8IntPtrFromString(string value)
	{
		return Utf8IntPtrFromString(value, tracked: true);
	}

	public static IntPtr Utf8IntPtrFromString(string value, bool tracked)
	{
		int length = 0;
		return Utf8IntPtrFromString(value, tracked, ref length);
	}

	public static IntPtr Utf8IntPtrFromString(string value, ref int length)
	{
		return Utf8IntPtrFromString(value, tracked: true, ref length);
	}

	public static IntPtr Utf8IntPtrFromString(string value, bool tracked, ref int length)
	{
		if (value == null)
		{
			return IntPtr.Zero;
		}
		IntPtr zero = IntPtr.Zero;
		byte[] utf8BytesFromString = GetUtf8BytesFromString(value);
		if (utf8BytesFromString == null)
		{
			return IntPtr.Zero;
		}
		length = utf8BytesFromString.Length;
		zero = ((!tracked) ? SQLiteMemory.AllocateUntracked(length + 1) : SQLiteMemory.Allocate(length + 1));
		if (zero == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		Marshal.Copy(utf8BytesFromString, 0, zero, length);
		Marshal.WriteByte(zero, length, 0);
		return zero;
	}

	public static string[] StringArrayFromUtf8SizeAndIntPtr(int argc, IntPtr argv)
	{
		if (argc < 0)
		{
			return null;
		}
		if (argv == IntPtr.Zero)
		{
			return null;
		}
		string[] array = new string[argc];
		int num = 0;
		int num2 = 0;
		while (num < array.Length)
		{
			IntPtr intPtr = SQLiteMarshal.ReadIntPtr(argv, num2);
			array[num] = ((intPtr != IntPtr.Zero) ? StringFromUtf8IntPtr(intPtr) : null);
			num++;
			num2 += IntPtr.Size;
		}
		return array;
	}

	public static IntPtr[] Utf8IntPtrArrayFromStringArray(string[] values, bool tracked)
	{
		if (values == null)
		{
			return null;
		}
		IntPtr[] array = new IntPtr[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Utf8IntPtrFromString(values[i], tracked);
		}
		return array;
	}
}
