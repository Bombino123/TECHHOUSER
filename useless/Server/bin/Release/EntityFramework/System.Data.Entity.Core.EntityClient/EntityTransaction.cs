using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.EntityClient;

public class EntityTransaction : DbTransaction
{
	private readonly EntityConnection _connection;

	private readonly DbTransaction _storeTransaction;

	public new virtual EntityConnection Connection => (EntityConnection)DbConnection;

	protected override DbConnection DbConnection
	{
		get
		{
			if (((_storeTransaction != null) ? DbInterception.Dispatch.Transaction.GetConnection(_storeTransaction, InterceptionContext) : null) == null)
			{
				return null;
			}
			return _connection;
		}
	}

	public override IsolationLevel IsolationLevel
	{
		get
		{
			if (_storeTransaction == null)
			{
				return (IsolationLevel)0;
			}
			return DbInterception.Dispatch.Transaction.GetIsolationLevel(_storeTransaction, InterceptionContext);
		}
	}

	public virtual DbTransaction StoreTransaction => _storeTransaction;

	private DbInterceptionContext InterceptionContext => DbInterceptionContext.Combine(_connection.AssociatedContexts.Select((ObjectContext c) => c.InterceptionContext));

	internal EntityTransaction()
	{
	}

	internal EntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
	{
		_connection = connection;
		_storeTransaction = storeTransaction;
	}

	public override void Commit()
	{
		try
		{
			if (_storeTransaction != null)
			{
				DbInterception.Dispatch.Transaction.Commit(_storeTransaction, InterceptionContext);
			}
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType() && !(ex is CommitFailedException))
			{
				throw new EntityException(Strings.EntityClient_ProviderSpecificError("Commit"), ex);
			}
			throw;
		}
		ClearCurrentTransaction();
	}

	public override void Rollback()
	{
		try
		{
			if (_storeTransaction != null)
			{
				DbInterception.Dispatch.Transaction.Rollback(_storeTransaction, InterceptionContext);
			}
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityException(Strings.EntityClient_ProviderSpecificError("Rollback"), ex);
			}
			throw;
		}
		ClearCurrentTransaction();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ClearCurrentTransaction();
			if (_storeTransaction != null)
			{
				DbInterception.Dispatch.Transaction.Dispose(_storeTransaction, InterceptionContext);
			}
		}
		base.Dispose(disposing);
	}

	private void ClearCurrentTransaction()
	{
		if (_connection != null && _connection.CurrentTransaction == this)
		{
			_connection.ClearCurrentTransaction();
		}
	}
}
