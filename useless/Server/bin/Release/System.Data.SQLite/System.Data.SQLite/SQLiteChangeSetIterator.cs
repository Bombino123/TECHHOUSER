namespace System.Data.SQLite;

internal class SQLiteChangeSetIterator : IDisposable
{
	private IntPtr iterator;

	private bool ownHandle;

	private bool disposed;

	protected SQLiteChangeSetIterator(IntPtr iterator, bool ownHandle)
	{
		this.iterator = iterator;
		this.ownHandle = ownHandle;
	}

	internal void CheckHandle()
	{
		if (iterator == IntPtr.Zero)
		{
			throw new InvalidOperationException("iterator is not open");
		}
	}

	internal IntPtr GetIntPtr()
	{
		return iterator;
	}

	public bool Next()
	{
		CheckDisposed();
		CheckHandle();
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_next(iterator);
		return sQLiteErrorCode switch
		{
			SQLiteErrorCode.Ok => throw new SQLiteException(SQLiteErrorCode.Ok, "sqlite3changeset_next: unexpected result Ok"), 
			SQLiteErrorCode.Row => true, 
			SQLiteErrorCode.Done => false, 
			_ => throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_next"), 
		};
	}

	public static SQLiteChangeSetIterator Attach(IntPtr iterator)
	{
		return new SQLiteChangeSetIterator(iterator, ownHandle: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteChangeSetIterator).Name);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && iterator != IntPtr.Zero)
			{
				if (ownHandle)
				{
					UnsafeNativeMethods.sqlite3changeset_finalize(iterator);
				}
				iterator = IntPtr.Zero;
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteChangeSetIterator()
	{
		Dispose(disposing: false);
	}
}
