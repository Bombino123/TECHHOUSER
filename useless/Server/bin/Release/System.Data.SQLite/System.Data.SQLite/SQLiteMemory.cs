namespace System.Data.SQLite;

internal static class SQLiteMemory
{
	private static bool CanUseSize64()
	{
		if (UnsafeNativeMethods.sqlite3_libversion_number() >= 3008007)
		{
			return true;
		}
		return false;
	}

	public static IntPtr Allocate(int size)
	{
		return UnsafeNativeMethods.sqlite3_malloc(size);
	}

	public static IntPtr Allocate64(ulong size)
	{
		return UnsafeNativeMethods.sqlite3_malloc64(size);
	}

	public static IntPtr AllocateUntracked(int size)
	{
		return UnsafeNativeMethods.sqlite3_malloc(size);
	}

	public static IntPtr Allocate64Untracked(ulong size)
	{
		return UnsafeNativeMethods.sqlite3_malloc64(size);
	}

	public static int Size(IntPtr pMemory)
	{
		return UnsafeNativeMethods.sqlite3_malloc_size_interop(pMemory);
	}

	public static ulong Size64(IntPtr pMemory)
	{
		return UnsafeNativeMethods.sqlite3_msize(pMemory);
	}

	public static void Free(IntPtr pMemory)
	{
		UnsafeNativeMethods.sqlite3_free(pMemory);
	}

	public static void FreeUntracked(IntPtr pMemory)
	{
		UnsafeNativeMethods.sqlite3_free(pMemory);
	}
}
