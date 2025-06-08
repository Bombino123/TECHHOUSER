using System.IO;

namespace System.Data.SQLite;

internal sealed class SQLiteStreamChangeSetIterator : SQLiteChangeSetIterator
{
	private SQLiteStreamAdapter streamAdapter;

	private bool disposed;

	private SQLiteStreamChangeSetIterator(SQLiteStreamAdapter streamAdapter, IntPtr iterator, bool ownHandle)
		: base(iterator, ownHandle)
	{
		this.streamAdapter = streamAdapter;
	}

	public static SQLiteStreamChangeSetIterator Create(Stream stream, SQLiteConnectionFlags connectionFlags)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SQLiteStreamAdapter sQLiteStreamAdapter = null;
		SQLiteStreamChangeSetIterator sQLiteStreamChangeSetIterator = null;
		IntPtr zero = IntPtr.Zero;
		try
		{
			sQLiteStreamAdapter = new SQLiteStreamAdapter(stream, connectionFlags);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_start_strm(ref zero, sQLiteStreamAdapter.GetInputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_start_strm");
			}
			sQLiteStreamChangeSetIterator = new SQLiteStreamChangeSetIterator(sQLiteStreamAdapter, zero, ownHandle: true);
		}
		finally
		{
			if (sQLiteStreamChangeSetIterator == null)
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.sqlite3changeset_finalize(zero);
					zero = IntPtr.Zero;
				}
				if (sQLiteStreamAdapter != null)
				{
					sQLiteStreamAdapter.Dispose();
					sQLiteStreamAdapter = null;
				}
			}
		}
		return sQLiteStreamChangeSetIterator;
	}

	public static SQLiteStreamChangeSetIterator Create(Stream stream, SQLiteConnectionFlags connectionFlags, SQLiteChangeSetStartFlags startFlags)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SQLiteStreamAdapter sQLiteStreamAdapter = null;
		SQLiteStreamChangeSetIterator sQLiteStreamChangeSetIterator = null;
		IntPtr zero = IntPtr.Zero;
		try
		{
			sQLiteStreamAdapter = new SQLiteStreamAdapter(stream, connectionFlags);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_start_v2_strm(ref zero, sQLiteStreamAdapter.GetInputDelegate(), IntPtr.Zero, startFlags);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_start_v2_strm");
			}
			sQLiteStreamChangeSetIterator = new SQLiteStreamChangeSetIterator(sQLiteStreamAdapter, zero, ownHandle: true);
		}
		finally
		{
			if (sQLiteStreamChangeSetIterator == null)
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.sqlite3changeset_finalize(zero);
					zero = IntPtr.Zero;
				}
				if (sQLiteStreamAdapter != null)
				{
					sQLiteStreamAdapter.Dispose();
					sQLiteStreamAdapter = null;
				}
			}
		}
		return sQLiteStreamChangeSetIterator;
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteStreamChangeSetIterator).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			_ = disposed;
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
