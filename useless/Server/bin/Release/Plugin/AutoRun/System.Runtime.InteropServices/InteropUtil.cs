namespace System.Runtime.InteropServices;

internal static class InteropUtil
{
	private const int cbBuffer = 256;

	public static T ToStructure<T>(IntPtr ptr)
	{
		return (T)Marshal.PtrToStructure(ptr, typeof(T));
	}

	public static IntPtr StructureToPtr(object value)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
		Marshal.StructureToPtr(value, intPtr, fDeleteOld: false);
		return intPtr;
	}

	public static void AllocString(ref IntPtr ptr, ref uint size)
	{
		FreeString(ref ptr, ref size);
		if (size == 0)
		{
			size = 256u;
		}
		ptr = Marshal.AllocHGlobal(256);
	}

	public static void FreeString(ref IntPtr ptr, ref uint size)
	{
		if (ptr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(ptr);
			ptr = IntPtr.Zero;
			size = 0u;
		}
	}

	public static string GetString(IntPtr pString)
	{
		return Marshal.PtrToStringUni(pString);
	}

	public static bool SetString(ref IntPtr ptr, ref uint size, string value = null)
	{
		string @string = GetString(ptr);
		if (value == string.Empty)
		{
			value = null;
		}
		if (string.CompareOrdinal(@string, value) != 0)
		{
			FreeString(ref ptr, ref size);
			if (value != null)
			{
				ptr = Marshal.StringToHGlobalUni(value);
				size = (uint)(value.Length + 1);
			}
			return true;
		}
		return false;
	}

	public static T[] ToArray<TS, T>(IntPtr ptr, int count) where TS : IConvertible
	{
		T[] array = new T[count];
		int num = Marshal.SizeOf(typeof(TS));
		for (int i = 0; i < count; i++)
		{
			TS val = ToStructure<TS>(new IntPtr(ptr.ToInt64() + i * num));
			array[i] = (T)Convert.ChangeType(val, typeof(T));
		}
		return array;
	}

	public static T[] ToArray<T>(IntPtr ptr, int count)
	{
		T[] array = new T[count];
		int num = Marshal.SizeOf(typeof(T));
		for (int i = 0; i < count; i++)
		{
			IntPtr ptr2 = new IntPtr(ptr.ToInt64() + i * num);
			array[i] = ToStructure<T>(ptr2);
		}
		return array;
	}
}
