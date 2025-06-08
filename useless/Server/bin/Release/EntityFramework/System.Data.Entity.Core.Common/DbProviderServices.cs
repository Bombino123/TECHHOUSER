using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Transactions;
using System.Xml;

namespace System.Data.Entity.Core.Common;

public abstract class DbProviderServices : IDbDependencyResolver
{
	private readonly Lazy<IDbDependencyResolver> _resolver;

	private readonly Lazy<DbCommandTreeDispatcher> _treeDispatcher;

	private static readonly ConcurrentDictionary<DbProviderInfo, DbSpatialServices> _spatialServices = new ConcurrentDictionary<DbProviderInfo, DbSpatialServices>();

	private static readonly ConcurrentDictionary<ExecutionStrategyKey, Func<IDbExecutionStrategy>> _executionStrategyFactories = new ConcurrentDictionary<ExecutionStrategyKey, Func<IDbExecutionStrategy>>();

	private readonly ResolverChain _resolvers = new ResolverChain();

	protected DbProviderServices()
		: this(() => DbConfiguration.DependencyResolver)
	{
	}

	internal DbProviderServices(Func<IDbDependencyResolver> resolver)
		: this(resolver, new Lazy<DbCommandTreeDispatcher>(() => DbInterception.Dispatch.CommandTree))
	{
	}

	internal DbProviderServices(Func<IDbDependencyResolver> resolver, Lazy<DbCommandTreeDispatcher> treeDispatcher)
	{
		Check.NotNull(resolver, "resolver");
		_resolver = new Lazy<IDbDependencyResolver>(resolver);
		_treeDispatcher = treeDispatcher;
	}

	public virtual void RegisterInfoMessageHandler(DbConnection connection, Action<string> handler)
	{
	}

	public DbCommandDefinition CreateCommandDefinition(DbCommandTree commandTree)
	{
		Check.NotNull(commandTree, "commandTree");
		return CreateCommandDefinition(commandTree, new DbInterceptionContext());
	}

	internal DbCommandDefinition CreateCommandDefinition(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
	{
		ValidateDataSpace(commandTree);
		StoreItemCollection storeItemCollection = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
		commandTree = _treeDispatcher.Value.Created(commandTree, interceptionContext);
		return CreateDbCommandDefinition(storeItemCollection.ProviderManifest, commandTree, interceptionContext);
	}

	internal virtual DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree, DbInterceptionContext interceptionContext)
	{
		return CreateDbCommandDefinition(providerManifest, commandTree);
	}

	public DbCommandDefinition CreateCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
	{
		Check.NotNull(providerManifest, "providerManifest");
		Check.NotNull(commandTree, "commandTree");
		try
		{
			return CreateDbCommandDefinition(providerManifest, commandTree);
		}
		catch (ProviderIncompatibleException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			if (ex2.IsCatchableExceptionType())
			{
				throw new ProviderIncompatibleException(Strings.ProviderDidNotCreateACommandDefinition, ex2);
			}
			throw;
		}
	}

	protected abstract DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree);

	internal virtual void ValidateDataSpace(DbCommandTree commandTree)
	{
		if (commandTree.DataSpace != DataSpace.SSpace)
		{
			throw new ProviderIncompatibleException(Strings.ProviderRequiresStoreCommandTree);
		}
	}

	internal virtual DbCommand CreateCommand(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
	{
		return CreateCommandDefinition(commandTree, interceptionContext).CreateCommand();
	}

	public virtual DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
	{
		return new DbCommandDefinition(prototype, CloneDbCommand);
	}

	protected virtual DbCommand CloneDbCommand(DbCommand fromDbCommand)
	{
		Check.NotNull(fromDbCommand, "fromDbCommand");
		if (!(fromDbCommand is ICloneable cloneable))
		{
			throw new ProviderIncompatibleException(Strings.EntityClient_CannotCloneStoreProvider);
		}
		return (DbCommand)cloneable.Clone();
	}

	public virtual DbConnection CloneDbConnection(DbConnection connection)
	{
		return CloneDbConnection(connection, GetProviderFactory(connection));
	}

	public virtual DbConnection CloneDbConnection(DbConnection connection, DbProviderFactory factory)
	{
		return factory.CreateConnection();
	}

	public string GetProviderManifestToken(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		try
		{
			string dbProviderManifestToken;
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
				dbProviderManifestToken = GetDbProviderManifestToken(connection);
			}
			if (dbProviderManifestToken == null)
			{
				throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifestToken);
			}
			return dbProviderManifestToken;
		}
		catch (ProviderIncompatibleException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			if (ex2.IsCatchableExceptionType())
			{
				throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifestToken, ex2);
			}
			throw;
		}
	}

	protected abstract string GetDbProviderManifestToken(DbConnection connection);

	public DbProviderManifest GetProviderManifest(string manifestToken)
	{
		Check.NotNull(manifestToken, "manifestToken");
		try
		{
			return GetDbProviderManifest(manifestToken) ?? throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifest);
		}
		catch (ProviderIncompatibleException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			if (ex2.IsCatchableExceptionType())
			{
				throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifest, ex2);
			}
			throw;
		}
	}

	protected abstract DbProviderManifest GetDbProviderManifest(string manifestToken);

	public static IDbExecutionStrategy GetExecutionStrategy(DbConnection connection)
	{
		return GetExecutionStrategy(connection, GetProviderFactory(connection));
	}

	internal static IDbExecutionStrategy GetExecutionStrategy(DbConnection connection, MetadataWorkspace metadataWorkspace)
	{
		StoreItemCollection storeItemCollection = (StoreItemCollection)metadataWorkspace.GetItemCollection(DataSpace.SSpace);
		return GetExecutionStrategy(connection, storeItemCollection.ProviderFactory);
	}

	protected static IDbExecutionStrategy GetExecutionStrategy(DbConnection connection, string providerInvariantName)
	{
		return GetExecutionStrategy(connection, GetProviderFactory(connection), providerInvariantName);
	}

	private static IDbExecutionStrategy GetExecutionStrategy(DbConnection connection, DbProviderFactory providerFactory, string providerInvariantName = null)
	{
		if (connection is EntityConnection entityConnection)
		{
			connection = entityConnection.StoreConnection;
		}
		string dataSource = DbInterception.Dispatch.Connection.GetDataSource(connection, new DbInterceptionContext());
		ExecutionStrategyKey key = new ExecutionStrategyKey(providerFactory.GetType().FullName, dataSource);
		return _executionStrategyFactories.GetOrAdd(key, (ExecutionStrategyKey k) => DbConfiguration.DependencyResolver.GetService<Func<IDbExecutionStrategy>>(new ExecutionStrategyKey(providerInvariantName ?? DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(providerFactory).Name, dataSource)))();
	}

	public DbSpatialDataReader GetSpatialDataReader(DbDataReader fromReader, string manifestToken)
	{
		try
		{
			return GetDbSpatialDataReader(fromReader, manifestToken);
		}
		catch (ProviderIncompatibleException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			if (ex2.IsCatchableExceptionType())
			{
				throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices, ex2);
			}
			throw;
		}
	}

	[Obsolete("Use GetSpatialServices(DbProviderInfo) or DbConfiguration to ensure the configured spatial services are used. See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.")]
	public DbSpatialServices GetSpatialServices(string manifestToken)
	{
		try
		{
			return DbGetSpatialServices(manifestToken);
		}
		catch (ProviderIncompatibleException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices, innerException);
		}
	}

	internal static DbSpatialServices GetSpatialServices(IDbDependencyResolver resolver, EntityConnection connection)
	{
		StoreItemCollection storeItemCollection = (StoreItemCollection)connection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
		DbProviderInfo key = new DbProviderInfo(storeItemCollection.ProviderInvariantName, storeItemCollection.ProviderManifestToken);
		return GetSpatialServices(resolver, key, () => GetProviderServices(connection.StoreConnection));
	}

	public DbSpatialServices GetSpatialServices(DbProviderInfo key)
	{
		return GetSpatialServices(_resolver.Value, key, () => this);
	}

	private static DbSpatialServices GetSpatialServices(IDbDependencyResolver resolver, DbProviderInfo key, Func<DbProviderServices> providerServices)
	{
		return _spatialServices.GetOrAdd(key, (DbProviderInfo k) => resolver.GetService<DbSpatialServices>(k) ?? providerServices().GetSpatialServices(k.ProviderManifestToken) ?? resolver.GetService<DbSpatialServices>()) ?? throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
	}

	protected virtual DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string manifestToken)
	{
		Check.NotNull(fromReader, "fromReader");
		return null;
	}

	[Obsolete("Return DbSpatialServices from the GetService method. See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.")]
	protected virtual DbSpatialServices DbGetSpatialServices(string manifestToken)
	{
		return null;
	}

	public void SetParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
	{
		Check.NotNull(parameter, "parameter");
		Check.NotNull(parameterType, "parameterType");
		SetDbParameterValue(parameter, parameterType, value);
	}

	protected virtual void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
	{
		Check.NotNull(parameter, "parameter");
		Check.NotNull(parameterType, "parameterType");
		parameter.Value = value;
	}

	public static DbProviderServices GetProviderServices(DbConnection connection)
	{
		return GetProviderFactory(connection).GetProviderServices();
	}

	public static DbProviderFactory GetProviderFactory(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		return connection.GetProviderFactory() ?? throw new ProviderIncompatibleException(Strings.EntityClient_ReturnedNullOnProviderMethod("get_ProviderFactory", connection.GetType().ToString()));
	}

	public static XmlReader GetConceptualSchemaDefinition(string csdlName)
	{
		Check.NotEmpty(csdlName, "csdlName");
		return GetXmlResource("System.Data.Resources.DbProviderServices." + csdlName + ".csdl");
	}

	internal static XmlReader GetXmlResource(string resourceName)
	{
		return XmlReader.Create(typeof(DbProviderServices).Assembly().GetManifestResourceStream(resourceName) ?? throw Error.InvalidResourceName(resourceName));
	}

	public string CreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(providerManifestToken, "providerManifestToken");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		return DbCreateDatabaseScript(providerManifestToken, storeItemCollection);
	}

	protected virtual string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(providerManifestToken, "providerManifestToken");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportCreateDatabaseScript);
	}

	public void CreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		DbCreateDatabase(connection, commandTimeout, storeItemCollection);
	}

	protected virtual void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportCreateDatabase);
	}

	public bool DatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		using (new TransactionScope(TransactionScopeOption.Suppress))
		{
			return DbDatabaseExists(connection, commandTimeout, storeItemCollection);
		}
	}

	public bool DatabaseExists(DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		using (new TransactionScope(TransactionScopeOption.Suppress))
		{
			return DbDatabaseExists(connection, commandTimeout, storeItemCollection);
		}
	}

	protected virtual bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportDatabaseExists);
	}

	protected virtual bool DbDatabaseExists(DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		return DbDatabaseExists(connection, commandTimeout, storeItemCollection.Value);
	}

	public void DeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		DbDeleteDatabase(connection, commandTimeout, storeItemCollection);
	}

	protected virtual void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportDeleteDatabase);
	}

	public static string ExpandDataDirectory(string path)
	{
		if (string.IsNullOrEmpty(path) || !path.StartsWith("|datadirectory|", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		object data = AppDomain.CurrentDomain.GetData("DataDirectory");
		string text = data as string;
		if (data != null && text == null)
		{
			throw new InvalidOperationException(Strings.ADP_InvalidDataDirectory);
		}
		if (text == string.Empty)
		{
			text = AppDomain.CurrentDomain.BaseDirectory;
		}
		if (text == null)
		{
			text = string.Empty;
		}
		path = path.Substring("|datadirectory|".Length);
		if (path.StartsWith("\\", StringComparison.Ordinal))
		{
			path = path.Substring(1);
		}
		path = (text.EndsWith("\\", StringComparison.Ordinal) ? text : (text + "\\")) + path;
		if (text.Contains(".."))
		{
			throw new ArgumentException(Strings.ExpandingDataDirectoryFailed);
		}
		return path;
	}

	protected void AddDependencyResolver(IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		_resolvers.Add(resolver);
	}

	public virtual object GetService(Type type, object key)
	{
		return _resolvers.GetService(type, key);
	}

	public virtual IEnumerable<object> GetServices(Type type, object key)
	{
		return _resolvers.GetServices(type, key);
	}
}
