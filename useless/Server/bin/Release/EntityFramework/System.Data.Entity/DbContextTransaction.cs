using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;

namespace System.Data.Entity;

public class DbContextTransaction : IDisposable
{
	private readonly EntityConnection _connection;

	private readonly EntityTransaction _entityTransaction;

	private bool _shouldCloseConnection;

	private bool _isDisposed;

	public DbTransaction UnderlyingTransaction => _entityTransaction.StoreTransaction;

	internal DbContextTransaction(EntityConnection connection)
	{
		_connection = connection;
		EnsureOpenConnection();
		_entityTransaction = _connection.BeginTransaction();
	}

	internal DbContextTransaction(EntityConnection connection, IsolationLevel isolationLevel)
	{
		_connection = connection;
		EnsureOpenConnection();
		_entityTransaction = _connection.BeginTransaction(isolationLevel);
	}

	internal DbContextTransaction(EntityTransaction transaction)
	{
		_connection = transaction.Connection;
		EnsureOpenConnection();
		_entityTransaction = transaction;
	}

	private void EnsureOpenConnection()
	{
		if (ConnectionState.Open != _connection.State)
		{
			_connection.Open();
			_shouldCloseConnection = true;
		}
	}

	public void Commit()
	{
		_entityTransaction.Commit();
	}

	public void Rollback()
	{
		_entityTransaction.Rollback();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_isDisposed)
		{
			_connection.ClearCurrentTransaction();
			_entityTransaction.Dispose();
			if (_shouldCloseConnection && _connection.State != 0)
			{
				_connection.Close();
			}
			_isDisposed = true;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
