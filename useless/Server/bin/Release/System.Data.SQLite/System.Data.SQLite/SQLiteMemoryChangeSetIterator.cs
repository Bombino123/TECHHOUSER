namespace System.Data.SQLite;

internal sealed class SQLiteMemoryChangeSetIterator : SQLiteChangeSetIterator
{
	private IntPtr pData;

	private bool disposed;

	private SQLiteMemoryChangeSetIterator(IntPtr pData, IntPtr iterator, bool ownHandle)
		: base(iterator, ownHandle)
	{
		this.pData = pData;
	}

	public static SQLiteMemoryChangeSetIterator Create(byte[] rawData)
	{
		SQLiteSessionHelpers.CheckRawData(rawData);
		SQLiteMemoryChangeSetIterator sQLiteMemoryChangeSetIterator = null;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteBytes.ToIntPtr(rawData, ref length);
			if (intPtr == IntPtr.Zero)
			{
				throw new SQLiteException(SQLiteErrorCode.NoMem, null);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_start(ref zero, length, intPtr);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_start");
			}
			sQLiteMemoryChangeSetIterator = new SQLiteMemoryChangeSetIterator(intPtr, zero, ownHandle: true);
		}
		finally
		{
			if (sQLiteMemoryChangeSetIterator == null)
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.sqlite3changeset_finalize(zero);
					zero = IntPtr.Zero;
				}
				if (intPtr != IntPtr.Zero)
				{
					SQLiteMemory.Free(intPtr);
					intPtr = IntPtr.Zero;
				}
			}
		}
		return sQLiteMemoryChangeSetIterator;
	}

	public static SQLiteMemoryChangeSetIterator Create(byte[] rawData, SQLiteChangeSetStartFlags flags)
	{
		SQLiteSessionHelpers.CheckRawData(rawData);
		SQLiteMemoryChangeSetIterator sQLiteMemoryChangeSetIterator = null;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteBytes.ToIntPtr(rawData, ref length);
			if (intPtr == IntPtr.Zero)
			{
				throw new SQLiteException(SQLiteErrorCode.NoMem, null);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_start_v2(ref zero, length, intPtr, flags);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_start_v2");
			}
			sQLiteMemoryChangeSetIterator = new SQLiteMemoryChangeSetIterator(intPtr, zero, ownHandle: true);
		}
		finally
		{
			if (sQLiteMemoryChangeSetIterator == null)
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.sqlite3changeset_finalize(zero);
					zero = IntPtr.Zero;
				}
				if (intPtr != IntPtr.Zero)
				{
					SQLiteMemory.Free(intPtr);
					intPtr = IntPtr.Zero;
				}
			}
		}
		return sQLiteMemoryChangeSetIterator;
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteMemoryChangeSetIterator).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		try
		{
			if (!disposed && pData != IntPtr.Zero)
			{
				SQLiteMemory.Free(pData);
				pData = IntPtr.Zero;
			}
		}
		finally
		{
			disposed = true;
		}
	}
}
