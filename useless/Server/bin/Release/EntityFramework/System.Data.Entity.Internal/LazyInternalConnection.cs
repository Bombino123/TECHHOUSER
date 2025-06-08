using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class LazyInternalConnection : InternalConnection
{
	private readonly string _nameOrConnectionString;

	private DbConnectionStringOrigin _connectionStringOrigin;

	private string _connectionStringName;

	private readonly DbConnectionInfo _connectionInfo;

	private bool? _hasModel;

	public override DbConnection Connection
	{
		get
		{
			Initialize();
			return base.Connection;
		}
	}

	public override DbConnectionStringOrigin ConnectionStringOrigin
	{
		get
		{
			Initialize();
			return _connectionStringOrigin;
		}
	}

	public override string ConnectionStringName
	{
		get
		{
			Initialize();
			return _connectionStringName;
		}
	}

	public override string ConnectionKey
	{
		get
		{
			Initialize();
			return base.ConnectionKey;
		}
	}

	public override string OriginalConnectionString
	{
		get
		{
			Initialize();
			return base.OriginalConnectionString;
		}
	}

	public override string ProviderName
	{
		get
		{
			Initialize();
			return base.ProviderName;
		}
		set
		{
			base.ProviderName = value;
		}
	}

	public override bool ConnectionHasModel
	{
		get
		{
			if (!_hasModel.HasValue)
			{
				if (base.UnderlyingConnection == null)
				{
					string nameOrConnectionString = _nameOrConnectionString;
					string name;
					if (_connectionInfo != null)
					{
						nameOrConnectionString = _connectionInfo.GetConnectionString(AppConfig).ConnectionString;
					}
					else if (DbHelpers.TryGetConnectionName(_nameOrConnectionString, out name))
					{
						ConnectionStringSettings val = FindConnectionInConfig(name, AppConfig);
						if (val == null && DbHelpers.TreatAsConnectionString(_nameOrConnectionString))
						{
							throw Error.DbContext_ConnectionStringNotFound(name);
						}
						if (val != null)
						{
							nameOrConnectionString = val.ConnectionString;
						}
					}
					_hasModel = DbHelpers.IsFullEFConnectionString(nameOrConnectionString);
				}
				else
				{
					_hasModel = base.UnderlyingConnection is EntityConnection;
				}
			}
			return _hasModel.Value;
		}
	}

	internal bool IsInitialized => base.UnderlyingConnection != null;

	public LazyInternalConnection(string nameOrConnectionString)
		: this(null, nameOrConnectionString)
	{
	}

	public LazyInternalConnection(DbContext context, string nameOrConnectionString)
		: base((context == null) ? null : new DbInterceptionContext().WithDbContext(context))
	{
		_nameOrConnectionString = nameOrConnectionString;
		AppConfig = AppConfig.DefaultInstance;
	}

	public LazyInternalConnection(DbContext context, DbConnectionInfo connectionInfo)
		: base(new DbInterceptionContext().WithDbContext(context))
	{
		_connectionInfo = connectionInfo;
		AppConfig = AppConfig.DefaultInstance;
	}

	public override ObjectContext CreateObjectContextFromConnectionModel()
	{
		Initialize();
		return base.CreateObjectContextFromConnectionModel();
	}

	public override void Dispose()
	{
		if (base.UnderlyingConnection != null)
		{
			if (base.UnderlyingConnection is EntityConnection)
			{
				base.UnderlyingConnection.Dispose();
			}
			else
			{
				DbInterception.Dispatch.Connection.Dispose(base.UnderlyingConnection, base.InterceptionContext);
			}
			base.UnderlyingConnection = null;
		}
	}

	private void Initialize()
	{
		if (base.UnderlyingConnection != null)
		{
			return;
		}
		string name;
		if (_connectionInfo != null)
		{
			ConnectionStringSettings connectionString = _connectionInfo.GetConnectionString(AppConfig);
			InitializeFromConnectionStringSetting(connectionString);
			_connectionStringOrigin = DbConnectionStringOrigin.DbContextInfo;
			_connectionStringName = connectionString.Name;
		}
		else if (!DbHelpers.TryGetConnectionName(_nameOrConnectionString, out name) || !TryInitializeFromAppConfig(name, AppConfig))
		{
			if (name != null && DbHelpers.TreatAsConnectionString(_nameOrConnectionString))
			{
				throw Error.DbContext_ConnectionStringNotFound(name);
			}
			if (DbHelpers.IsFullEFConnectionString(_nameOrConnectionString))
			{
				base.UnderlyingConnection = new EntityConnection(_nameOrConnectionString);
			}
			else if (base.ProviderName != null)
			{
				CreateConnectionFromProviderName(base.ProviderName);
			}
			else
			{
				base.UnderlyingConnection = DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>().CreateConnection(name ?? _nameOrConnectionString);
				if (base.UnderlyingConnection == null)
				{
					throw Error.DbContext_ConnectionFactoryReturnedNullConnection();
				}
			}
			if (name != null)
			{
				_connectionStringOrigin = DbConnectionStringOrigin.Convention;
				_connectionStringName = name;
			}
			else
			{
				_connectionStringOrigin = DbConnectionStringOrigin.UserCode;
			}
		}
		OnConnectionInitialized();
	}

	private bool TryInitializeFromAppConfig(string name, AppConfig config)
	{
		ConnectionStringSettings val = FindConnectionInConfig(name, config);
		if (val != null)
		{
			InitializeFromConnectionStringSetting(val);
			_connectionStringOrigin = DbConnectionStringOrigin.Configuration;
			_connectionStringName = val.Name;
			return true;
		}
		return false;
	}

	private static ConnectionStringSettings FindConnectionInConfig(string name, AppConfig config)
	{
		List<string> list = new List<string> { name };
		int num = name.LastIndexOf('.');
		if (num >= 0 && num + 1 < name.Length)
		{
			list.Add(name.Substring(num + 1));
		}
		return (from c in list
			where config.GetConnectionString(c) != null
			select config.GetConnectionString(c)).FirstOrDefault();
	}

	private void InitializeFromConnectionStringSetting(ConnectionStringSettings appConfigConnection)
	{
		string providerName = appConfigConnection.ProviderName;
		if (string.IsNullOrWhiteSpace(providerName))
		{
			throw Error.DbContext_ProviderNameMissing(appConfigConnection.Name);
		}
		if (string.Equals(providerName, "System.Data.EntityClient", StringComparison.OrdinalIgnoreCase))
		{
			base.UnderlyingConnection = new EntityConnection(appConfigConnection.ConnectionString);
			return;
		}
		CreateConnectionFromProviderName(providerName);
		DbInterception.Dispatch.Connection.SetConnectionString(base.UnderlyingConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(appConfigConnection.ConnectionString));
	}

	private void CreateConnectionFromProviderName(string providerInvariantName)
	{
		DbProviderFactory service = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(providerInvariantName);
		base.UnderlyingConnection = service.CreateConnection();
		if (base.UnderlyingConnection == null)
		{
			throw Error.DbContext_ProviderReturnedNullConnection();
		}
	}
}
