using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure.Interception;

namespace System.Data.Entity.Internal.MockingProxies;

internal class EntityConnectionProxy
{
	private readonly EntityConnection _entityConnection;

	public virtual DbConnection StoreConnection => _entityConnection.StoreConnection;

	protected EntityConnectionProxy()
	{
	}

	public EntityConnectionProxy(EntityConnection entityConnection)
	{
		_entityConnection = entityConnection;
	}

	public static implicit operator EntityConnection(EntityConnectionProxy proxy)
	{
		return proxy._entityConnection;
	}

	public virtual void Dispose()
	{
		_entityConnection.Dispose();
	}

	public virtual EntityConnectionProxy CreateNew(DbConnection storeConnection)
	{
		EntityConnection entityConnection = new EntityConnection(_entityConnection.GetMetadataWorkspace(), storeConnection);
		EntityTransaction currentTransaction = _entityConnection.CurrentTransaction;
		if (currentTransaction != null && DbInterception.Dispatch.Transaction.GetConnection(currentTransaction.StoreTransaction, _entityConnection.InterceptionContext) == storeConnection)
		{
			entityConnection.UseStoreTransaction(currentTransaction.StoreTransaction);
		}
		return new EntityConnectionProxy(entityConnection);
	}
}
