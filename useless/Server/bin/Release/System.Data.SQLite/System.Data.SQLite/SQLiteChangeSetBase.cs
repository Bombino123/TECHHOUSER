using System.Globalization;

namespace System.Data.SQLite;

internal class SQLiteChangeSetBase : SQLiteConnectionLock
{
	private bool disposed;

	internal SQLiteChangeSetBase(SQLiteConnectionHandle handle, SQLiteConnectionFlags flags)
		: base(handle, flags, autoLock: true)
	{
	}

	private ISQLiteChangeSetMetadataItem CreateMetadataItem(IntPtr iterator)
	{
		return new SQLiteChangeSetMetadataItem(SQLiteChangeSetIterator.Attach(iterator));
	}

	protected UnsafeNativeMethods.xSessionFilter GetDelegate(SessionTableFilterCallback tableFilterCallback, object clientData)
	{
		if (tableFilterCallback == null)
		{
			return null;
		}
		return delegate(IntPtr context, IntPtr pTblName)
		{
			try
			{
				string name = SQLiteString.StringFromUtf8IntPtr(pTblName);
				return tableFilterCallback(clientData, name) ? 1 : 0;
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
		};
	}

	protected UnsafeNativeMethods.xSessionConflict GetDelegate(SessionConflictCallback conflictCallback, object clientData)
	{
		if (conflictCallback == null)
		{
			return null;
		}
		return delegate(IntPtr context, SQLiteChangeSetConflictType type, IntPtr iterator)
		{
			try
			{
				ISQLiteChangeSetMetadataItem iSQLiteChangeSetMetadataItem = CreateMetadataItem(iterator);
				if (iSQLiteChangeSetMetadataItem == null)
				{
					throw new SQLiteException("could not create metadata item");
				}
				return conflictCallback(clientData, type, iSQLiteChangeSetMetadataItem);
			}
			catch (Exception ex)
			{
				try
				{
					if (HelperMethods.LogCallbackExceptions(GetFlags()))
					{
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "xSessionConflict", ex));
					}
				}
				catch
				{
				}
			}
			return SQLiteChangeSetConflictResult.Abort;
		};
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteChangeSetBase).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposed)
			{
				Unlock();
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
