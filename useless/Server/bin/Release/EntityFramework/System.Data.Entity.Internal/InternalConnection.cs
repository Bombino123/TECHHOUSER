using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Internal;

internal abstract class InternalConnection : IInternalConnection, IDisposable
{
	private string _key;

	private string _providerName;

	private string _originalConnectionString;

	private string _originalDatabaseName;

	private string _originalDataSource;

	protected DbInterceptionContext InterceptionContext { get; private set; }

	public virtual DbConnection Connection
	{
		get
		{
			if (!(UnderlyingConnection is EntityConnection entityConnection))
			{
				return UnderlyingConnection;
			}
			return entityConnection.StoreConnection;
		}
	}

	public virtual string ConnectionKey => _key ?? (_key = string.Format(CultureInfo.InvariantCulture, "{0};{1}", new object[2]
	{
		UnderlyingConnection.GetType(),
		OriginalConnectionString
	}));

	public virtual bool ConnectionHasModel => UnderlyingConnection is EntityConnection;

	public abstract DbConnectionStringOrigin ConnectionStringOrigin { get; }

	public virtual AppConfig AppConfig { get; set; }

	public virtual string ProviderName
	{
		get
		{
			return _providerName ?? (_providerName = ((UnderlyingConnection == null) ? null : Connection.GetProviderInvariantName()));
		}
		set
		{
			_providerName = value;
		}
	}

	public virtual string ConnectionStringName => null;

	public virtual string OriginalConnectionString
	{
		get
		{
			string b = ((UnderlyingConnection is EntityConnection) ? UnderlyingConnection.Database : DbInterception.Dispatch.Connection.GetDatabase(UnderlyingConnection, InterceptionContext));
			string b2 = ((UnderlyingConnection is EntityConnection) ? UnderlyingConnection.DataSource : DbInterception.Dispatch.Connection.GetDataSource(UnderlyingConnection, InterceptionContext));
			if (!string.Equals(_originalDatabaseName, b, StringComparison.OrdinalIgnoreCase) || !string.Equals(_originalDataSource, b2, StringComparison.OrdinalIgnoreCase))
			{
				OnConnectionInitialized();
			}
			return _originalConnectionString;
		}
	}

	protected DbConnection UnderlyingConnection { get; set; }

	public InternalConnection(DbInterceptionContext interceptionContext)
	{
		InterceptionContext = interceptionContext ?? new DbInterceptionContext();
	}

	public virtual ObjectContext CreateObjectContextFromConnectionModel()
	{
		ObjectContext objectContext = new ObjectContext((EntityConnection)UnderlyingConnection);
		ReadOnlyCollection<EntityContainer> items = objectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
		if (items.Count == 1)
		{
			objectContext.DefaultContainerName = items.Single().Name;
		}
		return objectContext;
	}

	public abstract void Dispose();

	protected void OnConnectionInitialized()
	{
		_originalConnectionString = GetStoreConnectionString(UnderlyingConnection);
		try
		{
			_originalDatabaseName = ((UnderlyingConnection is EntityConnection) ? UnderlyingConnection.Database : DbInterception.Dispatch.Connection.GetDatabase(UnderlyingConnection, InterceptionContext));
		}
		catch (NotImplementedException)
		{
		}
		try
		{
			_originalDataSource = ((UnderlyingConnection is EntityConnection) ? UnderlyingConnection.DataSource : DbInterception.Dispatch.Connection.GetDataSource(UnderlyingConnection, InterceptionContext));
		}
		catch (NotImplementedException)
		{
		}
	}

	public static string GetStoreConnectionString(DbConnection connection)
	{
		if (connection is EntityConnection entityConnection)
		{
			connection = entityConnection.StoreConnection;
			return (connection != null) ? DbInterception.Dispatch.Connection.GetConnectionString(connection, new DbInterceptionContext()) : null;
		}
		return DbInterception.Dispatch.Connection.GetConnectionString(connection, new DbInterceptionContext());
	}
}
