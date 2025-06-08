namespace System.Data.SQLite;

public sealed class SQLiteBlob : IDisposable
{
	internal SQLiteBase _sql;

	internal SQLiteBlobHandle _sqlite_blob;

	private bool disposed;

	private SQLiteBlob(SQLiteBase sqlbase, SQLiteBlobHandle blob)
	{
		_sql = sqlbase;
		_sqlite_blob = blob;
	}

	public static SQLiteBlob Create(SQLiteDataReader dataReader, int i, bool readOnly)
	{
		if (dataReader == null)
		{
			throw new ArgumentNullException("dataReader");
		}
		long? rowId = dataReader.GetRowId(i);
		if (!rowId.HasValue)
		{
			throw new InvalidOperationException("No RowId is available");
		}
		return Create(SQLiteDataReader.GetConnection(dataReader), dataReader.GetDatabaseName(i), dataReader.GetTableName(i), dataReader.GetName(i), rowId.Value, readOnly);
	}

	public static SQLiteBlob Create(SQLiteConnection connection, string databaseName, string tableName, string columnName, long rowId, bool readOnly)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		if (!(connection._sql is SQLite3 { _sql: var sql } sQLite))
		{
			throw new InvalidOperationException("Connection has no wrapper");
		}
		if (sql == null)
		{
			throw new InvalidOperationException("Connection has an invalid handle.");
		}
		SQLiteBlobHandle sQLiteBlobHandle = null;
		try
		{
		}
		finally
		{
			IntPtr ptrBlob = IntPtr.Zero;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_open(sql, SQLiteConvert.ToUTF8(databaseName), SQLiteConvert.ToUTF8(tableName), SQLiteConvert.ToUTF8(columnName), rowId, (!readOnly) ? 1 : 0, ref ptrBlob);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, null);
			}
			sQLiteBlobHandle = new SQLiteBlobHandle(sql, ptrBlob);
		}
		SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteBlobHandle, null, new object[6]
		{
			typeof(SQLiteBlob),
			databaseName,
			tableName,
			columnName,
			rowId,
			readOnly
		}));
		return new SQLiteBlob(sQLite, sQLiteBlobHandle);
	}

	private void CheckOpen()
	{
		if (_sqlite_blob == IntPtr.Zero)
		{
			throw new InvalidOperationException("Blob is not open");
		}
	}

	private void VerifyParameters(byte[] buffer, int count, int offset)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentException("Negative offset not allowed.");
		}
		if (count < 0)
		{
			throw new ArgumentException("Negative count not allowed.");
		}
		if (count > buffer.Length)
		{
			throw new ArgumentException("Buffer is too small.");
		}
	}

	public void Reopen(long rowId)
	{
		CheckDisposed();
		CheckOpen();
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_reopen(_sqlite_blob, rowId);
		if (sQLiteErrorCode != 0)
		{
			Dispose();
			throw new SQLiteException(sQLiteErrorCode, null);
		}
	}

	public int GetCount()
	{
		CheckDisposed();
		CheckOpen();
		return UnsafeNativeMethods.sqlite3_blob_bytes(_sqlite_blob);
	}

	public void Read(byte[] buffer, int count, int offset)
	{
		CheckDisposed();
		CheckOpen();
		VerifyParameters(buffer, count, offset);
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_read(_sqlite_blob, buffer, count, offset);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, null);
		}
	}

	public void Write(byte[] buffer, int count, int offset)
	{
		CheckDisposed();
		CheckOpen();
		VerifyParameters(buffer, count, offset);
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_write(_sqlite_blob, buffer, count, offset);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, null);
		}
	}

	public void Close()
	{
		Dispose();
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
			throw new ObjectDisposedException(typeof(SQLiteBlob).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			if (_sqlite_blob != null)
			{
				_sqlite_blob.Dispose();
				_sqlite_blob = null;
			}
			_sql = null;
		}
		disposed = true;
	}

	~SQLiteBlob()
	{
		Dispose(disposing: false);
	}
}
