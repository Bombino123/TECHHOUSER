using System.Globalization;
using System.IO;

namespace System.Data.SQLite;

internal sealed class SQLiteSession : SQLiteConnectionLock, ISQLiteSession, IDisposable
{
	private SQLiteSessionStreamManager streamManager;

	private string databaseName;

	private IntPtr session;

	private UnsafeNativeMethods.xSessionFilter xFilter;

	private SessionTableFilterCallback tableFilterCallback;

	private object tableFilterClientData;

	private bool disposed;

	public SQLiteSession(SQLiteConnectionHandle handle, SQLiteConnectionFlags flags, string databaseName)
		: base(handle, flags, autoLock: true)
	{
		this.databaseName = databaseName;
		InitializeHandle();
	}

	private void CheckHandle()
	{
		if (session == IntPtr.Zero)
		{
			throw new InvalidOperationException("session is not open");
		}
	}

	private void InitializeHandle()
	{
		if (!(session != IntPtr.Zero))
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_create(GetIntPtr(), SQLiteString.GetUtf8BytesFromString(databaseName), ref session);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_create");
			}
		}
	}

	private UnsafeNativeMethods.xSessionFilter ApplyTableFilter(SessionTableFilterCallback callback, object clientData)
	{
		tableFilterCallback = callback;
		tableFilterClientData = clientData;
		if (callback == null)
		{
			if (xFilter != null)
			{
				xFilter = null;
			}
			return null;
		}
		if (xFilter == null)
		{
			xFilter = Filter;
		}
		return xFilter;
	}

	private void InitializeStreamManager()
	{
		if (streamManager == null)
		{
			streamManager = new SQLiteSessionStreamManager(GetFlags());
		}
	}

	private SQLiteStreamAdapter GetStreamAdapter(Stream stream)
	{
		InitializeStreamManager();
		return streamManager.GetAdapter(stream);
	}

	private int Filter(IntPtr context, IntPtr pTblName)
	{
		try
		{
			return tableFilterCallback(tableFilterClientData, SQLiteString.StringFromUtf8IntPtr(pTblName)) ? 1 : 0;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(GetFlags()))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "xSessionFilter", ex));
				}
			}
			catch
			{
			}
		}
		return 0;
	}

	public bool IsEnabled()
	{
		CheckDisposed();
		CheckHandle();
		return UnsafeNativeMethods.sqlite3session_enable(session, -1) != 0;
	}

	public void SetToEnabled()
	{
		CheckDisposed();
		CheckHandle();
		UnsafeNativeMethods.sqlite3session_enable(session, 1);
	}

	public void SetToDisabled()
	{
		CheckDisposed();
		CheckHandle();
		UnsafeNativeMethods.sqlite3session_enable(session, 0);
	}

	public bool IsIndirect()
	{
		CheckDisposed();
		CheckHandle();
		return UnsafeNativeMethods.sqlite3session_indirect(session, -1) != 0;
	}

	public void SetToIndirect()
	{
		CheckDisposed();
		CheckHandle();
		UnsafeNativeMethods.sqlite3session_indirect(session, 1);
	}

	public void SetToDirect()
	{
		CheckDisposed();
		CheckHandle();
		UnsafeNativeMethods.sqlite3session_indirect(session, 0);
	}

	public bool IsEmpty()
	{
		CheckDisposed();
		CheckHandle();
		return UnsafeNativeMethods.sqlite3session_isempty(session) != 0;
	}

	public long GetMemoryBytesInUse()
	{
		CheckDisposed();
		CheckHandle();
		return UnsafeNativeMethods.sqlite3session_memory_used(session);
	}

	public void AttachTable(string name)
	{
		CheckDisposed();
		CheckHandle();
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_attach(session, SQLiteString.GetUtf8BytesFromString(name));
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3session_attach");
		}
	}

	public void SetTableFilter(SessionTableFilterCallback callback, object clientData)
	{
		CheckDisposed();
		CheckHandle();
		UnsafeNativeMethods.sqlite3session_table_filter(session, ApplyTableFilter(callback, clientData), IntPtr.Zero);
	}

	public void CreateChangeSet(ref byte[] rawData)
	{
		CheckDisposed();
		CheckHandle();
		IntPtr pChangeSet = IntPtr.Zero;
		try
		{
			int nChangeSet = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_changeset(session, ref nChangeSet, ref pChangeSet);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_changeset");
			}
			rawData = SQLiteBytes.FromIntPtr(pChangeSet, nChangeSet);
		}
		finally
		{
			if (pChangeSet != IntPtr.Zero)
			{
				SQLiteMemory.FreeUntracked(pChangeSet);
				pChangeSet = IntPtr.Zero;
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
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_changeset_strm(session, streamAdapter.GetOutputDelegate(), IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3session_changeset_strm");
		}
	}

	public void CreatePatchSet(ref byte[] rawData)
	{
		CheckDisposed();
		CheckHandle();
		IntPtr pPatchSet = IntPtr.Zero;
		try
		{
			int nPatchSet = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_patchset(session, ref nPatchSet, ref pPatchSet);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_patchset");
			}
			rawData = SQLiteBytes.FromIntPtr(pPatchSet, nPatchSet);
		}
		finally
		{
			if (pPatchSet != IntPtr.Zero)
			{
				SQLiteMemory.FreeUntracked(pPatchSet);
				pPatchSet = IntPtr.Zero;
			}
		}
	}

	public void CreatePatchSet(Stream stream)
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
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_patchset_strm(session, streamAdapter.GetOutputDelegate(), IntPtr.Zero);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3session_patchset_strm");
		}
	}

	public void LoadDifferencesFromTable(string fromDatabaseName, string tableName)
	{
		CheckDisposed();
		CheckHandle();
		if (fromDatabaseName == null)
		{
			throw new ArgumentNullException("fromDatabaseName");
		}
		if (tableName == null)
		{
			throw new ArgumentNullException("tableName");
		}
		IntPtr errMsg = IntPtr.Zero;
		try
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_diff(session, SQLiteString.GetUtf8BytesFromString(fromDatabaseName), SQLiteString.GetUtf8BytesFromString(tableName), ref errMsg);
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				return;
			}
			string text = null;
			if (errMsg != IntPtr.Zero)
			{
				text = SQLiteString.StringFromUtf8IntPtr(errMsg);
				if (!string.IsNullOrEmpty(text))
				{
					text = HelperMethods.StringFormat(CultureInfo.CurrentCulture, ": {0}", text);
				}
			}
			throw new SQLiteException(sQLiteErrorCode, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "{0}{1}", "sqlite3session_diff", text));
		}
		finally
		{
			if (errMsg != IntPtr.Zero)
			{
				SQLiteMemory.FreeUntracked(errMsg);
				errMsg = IntPtr.Zero;
			}
		}
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteSession).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				if (xFilter != null)
				{
					xFilter = null;
				}
				if (streamManager != null)
				{
					streamManager.Dispose();
					streamManager = null;
				}
			}
			if (session != IntPtr.Zero)
			{
				UnsafeNativeMethods.sqlite3session_delete(session);
				session = IntPtr.Zero;
			}
			Unlock();
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
