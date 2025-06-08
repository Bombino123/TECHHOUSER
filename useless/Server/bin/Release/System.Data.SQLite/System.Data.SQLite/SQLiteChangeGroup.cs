using System.IO;

namespace System.Data.SQLite;

internal sealed class SQLiteChangeGroup : ISQLiteChangeGroup, IDisposable
{
	private SQLiteSessionStreamManager streamManager;

	private SQLiteConnectionFlags flags;

	private IntPtr changeGroup;

	private bool disposed;

	public SQLiteChangeGroup(SQLiteConnectionFlags flags)
	{
		this.flags = flags;
		InitializeHandle();
	}

	private void CheckHandle()
	{
		if (changeGroup == IntPtr.Zero)
		{
			throw new InvalidOperationException("change group not open");
		}
	}

	private void InitializeHandle()
	{
		if (!(changeGroup != IntPtr.Zero))
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_new(ref changeGroup);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_new");
			}
		}
	}

	private void InitializeStreamManager()
	{
		if (streamManager == null)
		{
			streamManager = new SQLiteSessionStreamManager(flags);
		}
	}

	private SQLiteStreamAdapter GetStreamAdapter(Stream stream)
	{
		InitializeStreamManager();
		return streamManager.GetAdapter(stream);
	}

	public void AddChangeSet(byte[] rawData)
	{
		CheckDisposed();
		CheckHandle();
		SQLiteSessionHelpers.CheckRawData(rawData);
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteBytes.ToIntPtr(rawData, ref length);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_add(changeGroup, length, intPtr);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_add");
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

	public void AddChangeSet(Stream stream)
	{
		CheckDisposed();
		CheckHandle();
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SQLiteStreamAdapter streamAdapter = GetStreamAdapter(stream);
		if (streamAdapter == null)
		{
			throw new SQLiteException("could not get or create adapter for input stream");
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_add_strm(changeGroup, streamAdapter.GetInputDelegate(), IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_add_strm");
		}
	}

	public void CreateChangeSet(ref byte[] rawData)
	{
		CheckDisposed();
		CheckHandle();
		IntPtr pData = IntPtr.Zero;
		try
		{
			int nData = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_output(changeGroup, ref nData, ref pData);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_output");
			}
			rawData = SQLiteBytes.FromIntPtr(pData, nData);
		}
		finally
		{
			if (pData != IntPtr.Zero)
			{
				SQLiteMemory.FreeUntracked(pData);
				pData = IntPtr.Zero;
			}
		}
	}

	public void CreateChangeSet(Stream stream)
	{
		CheckDisposed();
		CheckHandle();
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SQLiteStreamAdapter streamAdapter = GetStreamAdapter(stream);
		if (streamAdapter == null)
		{
			throw new SQLiteException("could not get or create adapter for output stream");
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_output_strm(changeGroup, streamAdapter.GetOutputDelegate(), IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_output_strm");
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
			throw new ObjectDisposedException(typeof(SQLiteChangeGroup).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		try
		{
			if (!disposed)
			{
				if (disposing && streamManager != null)
				{
					streamManager.Dispose();
					streamManager = null;
				}
				if (changeGroup != IntPtr.Zero)
				{
					UnsafeNativeMethods.sqlite3changegroup_delete(changeGroup);
					changeGroup = IntPtr.Zero;
				}
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteChangeGroup()
	{
		Dispose(disposing: false);
	}
}
