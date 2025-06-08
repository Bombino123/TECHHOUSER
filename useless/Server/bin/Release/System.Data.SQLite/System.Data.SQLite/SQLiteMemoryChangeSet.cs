using System.Collections;
using System.Collections.Generic;

namespace System.Data.SQLite;

internal sealed class SQLiteMemoryChangeSet : SQLiteChangeSetBase, ISQLiteChangeSet, IEnumerable<ISQLiteChangeSetMetadataItem>, IEnumerable, IDisposable
{
	private byte[] rawData;

	private SQLiteChangeSetStartFlags startFlags;

	private bool disposed;

	internal SQLiteMemoryChangeSet(byte[] rawData, SQLiteConnectionHandle handle, SQLiteConnectionFlags connectionFlags)
		: base(handle, connectionFlags)
	{
		this.rawData = rawData;
		startFlags = SQLiteChangeSetStartFlags.None;
	}

	internal SQLiteMemoryChangeSet(byte[] rawData, SQLiteConnectionHandle handle, SQLiteConnectionFlags connectionFlags, SQLiteChangeSetStartFlags startFlags)
		: base(handle, connectionFlags)
	{
		this.rawData = rawData;
		this.startFlags = startFlags;
	}

	public ISQLiteChangeSet Invert()
	{
		CheckDisposed();
		SQLiteSessionHelpers.CheckRawData(rawData);
		IntPtr intPtr = IntPtr.Zero;
		IntPtr pOut = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteBytes.ToIntPtr(rawData, ref length);
			int nOut = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_invert(length, intPtr, ref nOut, ref pOut);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_invert");
			}
			return new SQLiteMemoryChangeSet(SQLiteBytes.FromIntPtr(pOut, nOut), GetHandle(), GetFlags());
		}
		finally
		{
			if (pOut != IntPtr.Zero)
			{
				SQLiteMemory.FreeUntracked(pOut);
				pOut = IntPtr.Zero;
			}
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
			}
		}
	}

	public ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet)
	{
		CheckDisposed();
		SQLiteSessionHelpers.CheckRawData(rawData);
		if (!(changeSet is SQLiteMemoryChangeSet sQLiteMemoryChangeSet))
		{
			throw new ArgumentException("not a memory based change set", "changeSet");
		}
		SQLiteSessionHelpers.CheckRawData(sQLiteMemoryChangeSet.rawData);
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr pOut = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteBytes.ToIntPtr(rawData, ref length);
			int length2 = 0;
			intPtr2 = SQLiteBytes.ToIntPtr(sQLiteMemoryChangeSet.rawData, ref length2);
			int nOut = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_concat(length, intPtr, length2, intPtr2, ref nOut, ref pOut);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_concat");
			}
			return new SQLiteMemoryChangeSet(SQLiteBytes.FromIntPtr(pOut, nOut), GetHandle(), GetFlags());
		}
		finally
		{
			if (pOut != IntPtr.Zero)
			{
				SQLiteMemory.FreeUntracked(pOut);
				pOut = IntPtr.Zero;
			}
			if (intPtr2 != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr2);
				intPtr2 = IntPtr.Zero;
			}
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
			}
		}
	}

	public void Apply(SessionConflictCallback conflictCallback, object clientData)
	{
		CheckDisposed();
		Apply(conflictCallback, null, clientData);
	}

	public void Apply(SessionConflictCallback conflictCallback, SessionTableFilterCallback tableFilterCallback, object clientData)
	{
		CheckDisposed();
		SQLiteSessionHelpers.CheckRawData(rawData);
		if (conflictCallback == null)
		{
			throw new ArgumentNullException("conflictCallback");
		}
		UnsafeNativeMethods.xSessionFilter @delegate = GetDelegate(tableFilterCallback, clientData);
		UnsafeNativeMethods.xSessionConflict delegate2 = GetDelegate(conflictCallback, clientData);
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			int length = 0;
			intPtr = SQLiteBytes.ToIntPtr(rawData, ref length);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_apply(GetIntPtr(), length, intPtr, @delegate, delegate2, IntPtr.Zero);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_apply");
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

	public IEnumerator<ISQLiteChangeSetMetadataItem> GetEnumerator()
	{
		if (startFlags != 0)
		{
			return new SQLiteMemoryChangeSetEnumerator(rawData, startFlags);
		}
		return new SQLiteMemoryChangeSetEnumerator(rawData);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteMemoryChangeSet).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing && rawData != null)
			{
				rawData = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
