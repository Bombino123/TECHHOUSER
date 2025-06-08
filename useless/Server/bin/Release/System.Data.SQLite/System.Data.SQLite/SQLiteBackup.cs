namespace System.Data.SQLite;

internal sealed class SQLiteBackup : IDisposable
{
	internal SQLiteBase _sql;

	internal SQLiteBackupHandle _sqlite_backup;

	internal IntPtr _destDb;

	internal byte[] _zDestName;

	internal IntPtr _sourceDb;

	internal byte[] _zSourceName;

	internal SQLiteErrorCode _stepResult;

	private bool disposed;

	internal SQLiteBackup(SQLiteBase sqlbase, SQLiteBackupHandle backup, IntPtr destDb, byte[] zDestName, IntPtr sourceDb, byte[] zSourceName)
	{
		_sql = sqlbase;
		_sqlite_backup = backup;
		_destDb = destDb;
		_zDestName = zDestName;
		_sourceDb = sourceDb;
		_zSourceName = zSourceName;
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
			throw new ObjectDisposedException(typeof(SQLiteBackup).Name);
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
			if (_sqlite_backup != null)
			{
				_sqlite_backup.Dispose();
				_sqlite_backup = null;
			}
			_zSourceName = null;
			_sourceDb = IntPtr.Zero;
			_zDestName = null;
			_destDb = IntPtr.Zero;
			_sql = null;
		}
		disposed = true;
	}

	~SQLiteBackup()
	{
		Dispose(disposing: false);
	}
}
