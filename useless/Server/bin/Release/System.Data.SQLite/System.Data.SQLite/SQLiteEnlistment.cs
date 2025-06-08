using System.Globalization;
using System.Transactions;

namespace System.Data.SQLite;

internal sealed class SQLiteEnlistment : IDisposable, IEnlistmentNotification
{
	internal SQLiteTransaction _transaction;

	internal Transaction _scope;

	internal bool _disposeConnection;

	private bool disposed;

	internal SQLiteEnlistment(SQLiteConnection cnn, Transaction scope, IsolationLevel defaultIsolationLevel, bool throwOnUnavailable, bool throwOnUnsupported)
	{
		_transaction = cnn.BeginTransaction(GetSystemDataIsolationLevel(cnn, scope, defaultIsolationLevel, throwOnUnavailable, throwOnUnsupported));
		_scope = scope;
		_scope.EnlistVolatile(this, EnlistmentOptions.None);
	}

	private IsolationLevel GetSystemDataIsolationLevel(SQLiteConnection connection, Transaction transaction, IsolationLevel defaultIsolationLevel, bool throwOnUnavailable, bool throwOnUnsupported)
	{
		if (transaction == null)
		{
			if (connection != null)
			{
				return connection.GetDefaultIsolationLevel();
			}
			if (throwOnUnavailable)
			{
				throw new InvalidOperationException("isolation level is unavailable");
			}
			return defaultIsolationLevel;
		}
		System.Transactions.IsolationLevel isolationLevel = transaction.IsolationLevel;
		switch (isolationLevel)
		{
		case System.Transactions.IsolationLevel.Unspecified:
			return IsolationLevel.Unspecified;
		case System.Transactions.IsolationLevel.Chaos:
			return IsolationLevel.Chaos;
		case System.Transactions.IsolationLevel.ReadUncommitted:
			return IsolationLevel.ReadUncommitted;
		case System.Transactions.IsolationLevel.ReadCommitted:
			return IsolationLevel.ReadCommitted;
		case System.Transactions.IsolationLevel.RepeatableRead:
			return IsolationLevel.RepeatableRead;
		case System.Transactions.IsolationLevel.Serializable:
			return IsolationLevel.Serializable;
		case System.Transactions.IsolationLevel.Snapshot:
			return IsolationLevel.Snapshot;
		default:
			if (throwOnUnsupported)
			{
				throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "unsupported isolation level {0}", isolationLevel));
			}
			return defaultIsolationLevel;
		}
	}

	private void Cleanup(SQLiteConnection cnn)
	{
		if (_disposeConnection)
		{
			cnn?.Dispose();
		}
		_transaction = null;
		_scope = null;
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
			throw new ObjectDisposedException(typeof(SQLiteEnlistment).Name);
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
			if (_transaction != null)
			{
				_transaction.Dispose();
				_transaction = null;
			}
			if (_scope != null)
			{
				_scope = null;
			}
		}
		disposed = true;
	}

	~SQLiteEnlistment()
	{
		Dispose(disposing: false);
	}

	public void Commit(Enlistment enlistment)
	{
		CheckDisposed();
		SQLiteConnection sQLiteConnection = null;
		try
		{
			while (true)
			{
				sQLiteConnection = _transaction.Connection;
				if (sQLiteConnection == null)
				{
					break;
				}
				lock (sQLiteConnection._enlistmentSyncRoot)
				{
					if (sQLiteConnection != _transaction.Connection)
					{
						continue;
					}
					sQLiteConnection._enlistment = null;
					_transaction.IsValid(throwError: true);
					sQLiteConnection._transactionLevel = 1;
					_transaction.Commit();
					break;
				}
			}
			enlistment.Done();
		}
		finally
		{
			Cleanup(sQLiteConnection);
		}
	}

	public void InDoubt(Enlistment enlistment)
	{
		CheckDisposed();
		enlistment.Done();
	}

	public void Prepare(PreparingEnlistment preparingEnlistment)
	{
		CheckDisposed();
		if (!_transaction.IsValid(throwError: false))
		{
			preparingEnlistment.ForceRollback();
		}
		else
		{
			preparingEnlistment.Prepared();
		}
	}

	public void Rollback(Enlistment enlistment)
	{
		CheckDisposed();
		SQLiteConnection sQLiteConnection = null;
		try
		{
			while (true)
			{
				sQLiteConnection = _transaction.Connection;
				if (sQLiteConnection == null)
				{
					break;
				}
				lock (sQLiteConnection._enlistmentSyncRoot)
				{
					if (sQLiteConnection != _transaction.Connection)
					{
						continue;
					}
					sQLiteConnection._enlistment = null;
					_transaction.Rollback();
					break;
				}
			}
			enlistment.Done();
		}
		finally
		{
			Cleanup(sQLiteConnection);
		}
	}
}
