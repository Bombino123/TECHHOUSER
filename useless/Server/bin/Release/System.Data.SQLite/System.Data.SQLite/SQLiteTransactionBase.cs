using System.Data.Common;

namespace System.Data.SQLite;

public abstract class SQLiteTransactionBase : DbTransaction
{
	internal SQLiteConnection _cnn;

	internal int _version;

	private IsolationLevel _level;

	private bool disposed;

	public override IsolationLevel IsolationLevel
	{
		get
		{
			CheckDisposed();
			return _level;
		}
	}

	public new SQLiteConnection Connection
	{
		get
		{
			CheckDisposed();
			return _cnn;
		}
	}

	protected override DbConnection DbConnection => Connection;

	internal SQLiteTransactionBase(SQLiteConnection connection, bool deferredLock)
	{
		_cnn = connection;
		_version = _cnn._version;
		_level = (deferredLock ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable);
		Begin(deferredLock);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteTransactionBase).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing && IsValid(throwError: false))
			{
				IssueRollback(throwError: false);
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}

	public override void Rollback()
	{
		CheckDisposed();
		IsValid(throwError: true);
		IssueRollback(throwError: true);
	}

	protected abstract void Begin(bool deferredLock);

	protected abstract void IssueRollback(bool throwError);

	internal bool IsValid(bool throwError)
	{
		if (_cnn == null)
		{
			if (throwError)
			{
				throw new ArgumentNullException("No connection associated with this transaction");
			}
			return false;
		}
		if (_cnn._version != _version)
		{
			if (throwError)
			{
				throw new SQLiteException("The connection was closed and re-opened, changes were already rolled back");
			}
			return false;
		}
		if (_cnn.State != ConnectionState.Open)
		{
			if (throwError)
			{
				throw new SQLiteException("Connection was closed");
			}
			return false;
		}
		if (_cnn._transactionLevel == 0 || _cnn._sql.AutoCommit)
		{
			_cnn._transactionLevel = 0;
			if (throwError)
			{
				throw new SQLiteException("No transaction is active on this connection");
			}
			return false;
		}
		return true;
	}
}
