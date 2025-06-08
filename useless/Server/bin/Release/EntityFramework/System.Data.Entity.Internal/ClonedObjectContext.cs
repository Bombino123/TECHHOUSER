using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal.MockingProxies;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal class ClonedObjectContext : IDisposable
{
	private ObjectContextProxy _objectContext;

	private readonly bool _connectionCloned;

	private readonly EntityConnectionProxy _clonedEntityConnection;

	public virtual ObjectContextProxy ObjectContext => _objectContext;

	public virtual DbConnection Connection => _objectContext.Connection.StoreConnection;

	protected ClonedObjectContext()
	{
	}

	public ClonedObjectContext(ObjectContextProxy objectContext, DbConnection connection, string connectionString, bool transferLoadedAssemblies = true)
	{
		if (connection == null || connection.State != ConnectionState.Open)
		{
			connection = connection ?? objectContext.Connection.StoreConnection;
			connection = DbProviderServices.GetProviderServices(connection).CloneDbConnection(connection);
			DbInterception.Dispatch.Connection.SetConnectionString(connection, new DbConnectionPropertyInterceptionContext<string>().WithValue(connectionString));
			_connectionCloned = true;
		}
		_clonedEntityConnection = objectContext.Connection.CreateNew(connection);
		_objectContext = objectContext.CreateNew(_clonedEntityConnection);
		_objectContext.CopyContextOptions(objectContext);
		if (!string.IsNullOrWhiteSpace(objectContext.DefaultContainerName))
		{
			_objectContext.DefaultContainerName = objectContext.DefaultContainerName;
		}
		if (transferLoadedAssemblies)
		{
			TransferLoadedAssemblies(objectContext);
		}
	}

	private void TransferLoadedAssemblies(ObjectContextProxy source)
	{
		IEnumerable<GlobalItem> objectItemCollection = source.GetObjectItemCollection();
		foreach (Assembly item in (from i in objectItemCollection
			where i is EntityType || i is ComplexType
			select source.GetClrType((StructuralType)i).Assembly()).Union(from i in objectItemCollection.OfType<EnumType>()
			select source.GetClrType(i).Assembly()).Distinct())
		{
			_objectContext.LoadFromAssembly(item);
		}
	}

	public void Dispose()
	{
		if (_objectContext != null)
		{
			ObjectContextProxy objectContext = _objectContext;
			DbConnection connection = Connection;
			_objectContext = null;
			objectContext.Dispose();
			_clonedEntityConnection.Dispose();
			if (_connectionCloned)
			{
				DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
			}
		}
	}
}
