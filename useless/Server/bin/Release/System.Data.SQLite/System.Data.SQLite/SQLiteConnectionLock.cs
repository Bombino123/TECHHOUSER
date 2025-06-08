using System.Globalization;

namespace System.Data.SQLite;

internal abstract class SQLiteConnectionLock : IDisposable
{
	private const string LockNopSql = "SELECT 1;";

	private const string StatementMessageFormat = "Connection lock object was {0} with statement {1}";

	private SQLiteConnectionHandle handle;

	private SQLiteConnectionFlags flags;

	private IntPtr statement;

	private bool disposed;

	public SQLiteConnectionLock(SQLiteConnectionHandle handle, SQLiteConnectionFlags flags, bool autoLock)
	{
		this.handle = handle;
		this.flags = flags;
		if (autoLock)
		{
			Lock();
		}
	}

	protected SQLiteConnectionHandle GetHandle()
	{
		return handle;
	}

	protected SQLiteConnectionFlags GetFlags()
	{
		return flags;
	}

	protected IntPtr GetIntPtr()
	{
		if (handle == null)
		{
			throw new InvalidOperationException("Connection lock object has an invalid handle.");
		}
		IntPtr intPtr = handle;
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("Connection lock object has an invalid handle pointer.");
		}
		return intPtr;
	}

	public void Lock()
	{
		CheckDisposed();
		if (statement != IntPtr.Zero)
		{
			return;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteString.Utf8IntPtrFromString("SELECT 1;", ref length);
			IntPtr ptrRemain = IntPtr.Zero;
			int nRemain = 0;
			string message = "sqlite3_prepare_interop";
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_prepare_interop(GetIntPtr(), intPtr, length, ref statement, ref ptrRemain, ref nRemain);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, message);
			}
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
			}
		}
	}

	public void Unlock()
	{
		CheckDisposed();
		if (!(statement == IntPtr.Zero))
		{
			string message = "sqlite3_finalize_interop";
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_finalize_interop(statement);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, message);
			}
			statement = IntPtr.Zero;
		}
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
			throw new ObjectDisposedException(typeof(SQLiteConnectionLock).Name);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (disposed || !(statement != IntPtr.Zero))
			{
				return;
			}
			try
			{
				if (HelperMethods.LogPrepare(GetFlags()))
				{
					SQLiteLog.LogMessage(SQLiteErrorCode.Misuse, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Connection lock object was {0} with statement {1}", disposing ? "disposed" : "finalized", statement));
				}
			}
			catch
			{
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteConnectionLock()
	{
		Dispose(disposing: false);
	}
}
