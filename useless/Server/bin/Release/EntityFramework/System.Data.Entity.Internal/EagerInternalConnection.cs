using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;

namespace System.Data.Entity.Internal;

internal class EagerInternalConnection : InternalConnection
{
	private readonly bool _connectionOwned;

	public override DbConnectionStringOrigin ConnectionStringOrigin => DbConnectionStringOrigin.UserCode;

	public EagerInternalConnection(DbContext context, DbConnection existingConnection, bool connectionOwned)
		: base(new DbInterceptionContext().WithDbContext(context))
	{
		base.UnderlyingConnection = existingConnection;
		_connectionOwned = connectionOwned;
		OnConnectionInitialized();
	}

	public override void Dispose()
	{
		if (_connectionOwned)
		{
			if (base.UnderlyingConnection is EntityConnection)
			{
				base.UnderlyingConnection.Dispose();
			}
			else
			{
				DbInterception.Dispatch.Connection.Dispose(base.UnderlyingConnection, base.InterceptionContext);
			}
		}
	}
}
