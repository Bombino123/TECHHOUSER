using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace System.Data.SQLite;

internal sealed class SQLiteStreamChangeSet : SQLiteChangeSetBase, ISQLiteChangeSet, IEnumerable<ISQLiteChangeSetMetadataItem>, IEnumerable, IDisposable
{
	private SQLiteStreamAdapter inputStreamAdapter;

	private SQLiteStreamAdapter outputStreamAdapter;

	private Stream inputStream;

	private Stream outputStream;

	private SQLiteChangeSetStartFlags startFlags;

	private bool disposed;

	internal SQLiteStreamChangeSet(Stream inputStream, Stream outputStream, SQLiteConnectionHandle handle, SQLiteConnectionFlags connectionFlags)
		: base(handle, connectionFlags)
	{
		this.inputStream = inputStream;
		this.outputStream = outputStream;
	}

	internal SQLiteStreamChangeSet(Stream inputStream, Stream outputStream, SQLiteConnectionHandle handle, SQLiteConnectionFlags connectionFlags, SQLiteChangeSetStartFlags startFlags)
		: base(handle, connectionFlags)
	{
		this.inputStream = inputStream;
		this.outputStream = outputStream;
		this.startFlags = startFlags;
	}

	private void CheckInputStream()
	{
		if (inputStream == null)
		{
			throw new InvalidOperationException("input stream unavailable");
		}
		if (inputStreamAdapter == null)
		{
			inputStreamAdapter = new SQLiteStreamAdapter(inputStream, GetFlags());
		}
	}

	private void CheckOutputStream()
	{
		if (outputStream == null)
		{
			throw new InvalidOperationException("output stream unavailable");
		}
		if (outputStreamAdapter == null)
		{
			outputStreamAdapter = new SQLiteStreamAdapter(outputStream, GetFlags());
		}
	}

	public ISQLiteChangeSet Invert()
	{
		CheckDisposed();
		CheckInputStream();
		CheckOutputStream();
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_invert_strm(inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, outputStreamAdapter.GetOutputDelegate(), IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_invert_strm");
		}
		return null;
	}

	public ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet)
	{
		CheckDisposed();
		CheckInputStream();
		CheckOutputStream();
		if (!(changeSet is SQLiteStreamChangeSet sQLiteStreamChangeSet))
		{
			throw new ArgumentException("not a stream based change set", "changeSet");
		}
		sQLiteStreamChangeSet.CheckInputStream();
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_concat_strm(inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, sQLiteStreamChangeSet.inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, outputStreamAdapter.GetOutputDelegate(), IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_concat_strm");
		}
		return null;
	}

	public void Apply(SessionConflictCallback conflictCallback, object clientData)
	{
		CheckDisposed();
		Apply(conflictCallback, null, clientData);
	}

	public void Apply(SessionConflictCallback conflictCallback, SessionTableFilterCallback tableFilterCallback, object clientData)
	{
		CheckDisposed();
		CheckInputStream();
		if (conflictCallback == null)
		{
			throw new ArgumentNullException("conflictCallback");
		}
		UnsafeNativeMethods.xSessionFilter @delegate = GetDelegate(tableFilterCallback, clientData);
		UnsafeNativeMethods.xSessionConflict delegate2 = GetDelegate(conflictCallback, clientData);
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_apply_strm(GetIntPtr(), inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, @delegate, delegate2, IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_apply_strm");
		}
	}

	public IEnumerator<ISQLiteChangeSetMetadataItem> GetEnumerator()
	{
		if (startFlags != 0)
		{
			return new SQLiteStreamChangeSetEnumerator(inputStream, GetFlags(), startFlags);
		}
		return new SQLiteStreamChangeSetEnumerator(inputStream, GetFlags());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteStreamChangeSet).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing)
			{
				if (outputStreamAdapter != null)
				{
					outputStreamAdapter.Dispose();
					outputStreamAdapter = null;
				}
				if (inputStreamAdapter != null)
				{
					inputStreamAdapter.Dispose();
					inputStreamAdapter = null;
				}
				if (outputStream != null)
				{
					outputStream = null;
				}
				if (inputStream != null)
				{
					inputStream = null;
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
