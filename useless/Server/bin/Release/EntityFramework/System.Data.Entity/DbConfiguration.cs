using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity;

public class DbConfiguration
{
	private readonly InternalConfiguration _internalConfiguration;

	public static IDbDependencyResolver DependencyResolver => InternalConfiguration.Instance.DependencyResolver;

	internal virtual InternalConfiguration InternalConfiguration => _internalConfiguration;

	public static event EventHandler<DbConfigurationLoadedEventArgs> Loaded
	{
		add
		{
			Check.NotNull(value, "value");
			DbConfigurationManager.Instance.AddLoadedHandler(value);
		}
		remove
		{
			Check.NotNull(value, "value");
			DbConfigurationManager.Instance.RemoveLoadedHandler(value);
		}
	}

	protected internal DbConfiguration()
		: this(new InternalConfiguration())
	{
		_internalConfiguration.Owner = this;
	}

	internal DbConfiguration(InternalConfiguration internalConfiguration)
	{
		_internalConfiguration = internalConfiguration;
		_internalConfiguration.Owner = this;
	}

	public static void SetConfiguration(DbConfiguration configuration)
	{
		Check.NotNull(configuration, "configuration");
		InternalConfiguration.Instance = configuration.InternalConfiguration;
	}

	public static void LoadConfiguration(Type contextType)
	{
		Check.NotNull(contextType, "contextType");
		if (!typeof(DbContext).IsAssignableFrom(contextType))
		{
			throw new ArgumentException(Strings.BadContextTypeForDiscovery(contextType.Name));
		}
		DbConfigurationManager.Instance.EnsureLoadedForContext(contextType);
	}

	public static void LoadConfiguration(Assembly assemblyHint)
	{
		Check.NotNull(assemblyHint, "assemblyHint");
		DbConfigurationManager.Instance.EnsureLoadedForAssembly(assemblyHint, null);
	}

	protected internal void AddDependencyResolver(IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		_internalConfiguration.CheckNotLocked("AddDependencyResolver");
		_internalConfiguration.AddDependencyResolver(resolver);
	}

	protected internal void AddDefaultResolver(IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		_internalConfiguration.CheckNotLocked("AddDefaultResolver");
		_internalConfiguration.AddDefaultResolver(resolver);
	}

	protected internal void SetProviderServices(string providerInvariantName, DbProviderServices provider)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(provider, "provider");
		_internalConfiguration.CheckNotLocked("SetProviderServices");
		_internalConfiguration.RegisterSingleton(provider, providerInvariantName);
		AddDefaultResolver(provider);
	}

	protected internal void SetProviderFactory(string providerInvariantName, DbProviderFactory providerFactory)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(providerFactory, "providerFactory");
		_internalConfiguration.CheckNotLocked("SetProviderFactory");
		_internalConfiguration.RegisterSingleton(providerFactory, providerInvariantName);
		_internalConfiguration.AddDependencyResolver(new InvariantNameResolver(providerFactory, providerInvariantName));
	}

	protected internal void SetExecutionStrategy(string providerInvariantName, Func<IDbExecutionStrategy> getExecutionStrategy)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(getExecutionStrategy, "getExecutionStrategy");
		_internalConfiguration.CheckNotLocked("SetExecutionStrategy");
		_internalConfiguration.AddDependencyResolver(new ExecutionStrategyResolver<IDbExecutionStrategy>(providerInvariantName, null, getExecutionStrategy));
	}

	protected internal void SetExecutionStrategy(string providerInvariantName, Func<IDbExecutionStrategy> getExecutionStrategy, string serverName)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotEmpty(serverName, "serverName");
		Check.NotNull(getExecutionStrategy, "getExecutionStrategy");
		_internalConfiguration.CheckNotLocked("SetExecutionStrategy");
		_internalConfiguration.AddDependencyResolver(new ExecutionStrategyResolver<IDbExecutionStrategy>(providerInvariantName, serverName, getExecutionStrategy));
	}

	protected internal void SetDefaultTransactionHandler(Func<TransactionHandler> transactionHandlerFactory)
	{
		Check.NotNull(transactionHandlerFactory, "transactionHandlerFactory");
		_internalConfiguration.CheckNotLocked("SetTransactionHandler");
		_internalConfiguration.AddDependencyResolver(new TransactionHandlerResolver(transactionHandlerFactory, null, null));
	}

	protected internal void SetTransactionHandler(string providerInvariantName, Func<TransactionHandler> transactionHandlerFactory)
	{
		Check.NotNull(transactionHandlerFactory, "transactionHandlerFactory");
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		_internalConfiguration.CheckNotLocked("SetTransactionHandler");
		_internalConfiguration.AddDependencyResolver(new TransactionHandlerResolver(transactionHandlerFactory, providerInvariantName, null));
	}

	protected internal void SetTransactionHandler(string providerInvariantName, Func<TransactionHandler> transactionHandlerFactory, string serverName)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(transactionHandlerFactory, "transactionHandlerFactory");
		Check.NotEmpty(serverName, "serverName");
		_internalConfiguration.CheckNotLocked("SetTransactionHandler");
		_internalConfiguration.AddDependencyResolver(new TransactionHandlerResolver(transactionHandlerFactory, providerInvariantName, serverName));
	}

	protected internal void SetDefaultConnectionFactory(IDbConnectionFactory connectionFactory)
	{
		Check.NotNull(connectionFactory, "connectionFactory");
		_internalConfiguration.CheckNotLocked("SetDefaultConnectionFactory");
		_internalConfiguration.RegisterSingleton(connectionFactory);
	}

	protected internal void SetPluralizationService(IPluralizationService pluralizationService)
	{
		Check.NotNull(pluralizationService, "pluralizationService");
		_internalConfiguration.CheckNotLocked("SetPluralizationService");
		_internalConfiguration.RegisterSingleton(pluralizationService);
	}

	protected internal void SetDatabaseInitializer<TContext>(IDatabaseInitializer<TContext> initializer) where TContext : DbContext
	{
		_internalConfiguration.CheckNotLocked("SetDatabaseInitializer");
		_internalConfiguration.RegisterSingleton(initializer ?? new NullDatabaseInitializer<TContext>());
	}

	protected internal void SetMigrationSqlGenerator(string providerInvariantName, Func<MigrationSqlGenerator> sqlGenerator)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(sqlGenerator, "sqlGenerator");
		_internalConfiguration.CheckNotLocked("SetMigrationSqlGenerator");
		_internalConfiguration.RegisterSingleton(sqlGenerator, providerInvariantName);
	}

	protected internal void SetManifestTokenResolver(IManifestTokenResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		_internalConfiguration.CheckNotLocked("SetManifestTokenResolver");
		_internalConfiguration.RegisterSingleton(resolver);
	}

	protected internal void SetMetadataAnnotationSerializer(string annotationName, Func<IMetadataAnnotationSerializer> serializerFactory)
	{
		Check.NotEmpty(annotationName, "annotationName");
		Check.NotNull(serializerFactory, "serializerFactory");
		_internalConfiguration.CheckNotLocked("SetMetadataAnnotationSerializer");
		_internalConfiguration.RegisterSingleton(serializerFactory, annotationName);
	}

	protected internal void SetProviderFactoryResolver(IDbProviderFactoryResolver providerFactoryResolver)
	{
		Check.NotNull(providerFactoryResolver, "providerFactoryResolver");
		_internalConfiguration.CheckNotLocked("SetProviderFactoryResolver");
		_internalConfiguration.RegisterSingleton(providerFactoryResolver);
	}

	protected internal void SetModelCacheKey(Func<DbContext, IDbModelCacheKey> keyFactory)
	{
		Check.NotNull(keyFactory, "keyFactory");
		_internalConfiguration.CheckNotLocked("SetModelCacheKey");
		_internalConfiguration.RegisterSingleton(keyFactory);
	}

	protected internal void SetDefaultHistoryContext(Func<DbConnection, string, HistoryContext> factory)
	{
		Check.NotNull(factory, "factory");
		_internalConfiguration.CheckNotLocked("SetDefaultHistoryContext");
		_internalConfiguration.RegisterSingleton(factory);
	}

	protected internal void SetHistoryContext(string providerInvariantName, Func<DbConnection, string, HistoryContext> factory)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(factory, "factory");
		_internalConfiguration.CheckNotLocked("SetHistoryContext");
		_internalConfiguration.RegisterSingleton(factory, providerInvariantName);
	}

	protected internal void SetDefaultSpatialServices(DbSpatialServices spatialProvider)
	{
		Check.NotNull(spatialProvider, "spatialProvider");
		_internalConfiguration.CheckNotLocked("SetDefaultSpatialServices");
		_internalConfiguration.RegisterSingleton(spatialProvider);
	}

	protected internal void SetSpatialServices(DbProviderInfo key, DbSpatialServices spatialProvider)
	{
		Check.NotNull(key, "key");
		Check.NotNull(spatialProvider, "spatialProvider");
		_internalConfiguration.CheckNotLocked("SetSpatialServices");
		_internalConfiguration.RegisterSingleton(spatialProvider, key);
	}

	protected internal void SetSpatialServices(string providerInvariantName, DbSpatialServices spatialProvider)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(spatialProvider, "spatialProvider");
		_internalConfiguration.CheckNotLocked("SetSpatialServices");
		RegisterSpatialServices(providerInvariantName, spatialProvider);
	}

	private void RegisterSpatialServices(string providerInvariantName, DbSpatialServices spatialProvider)
	{
		_internalConfiguration.RegisterSingleton(spatialProvider, (object k) => k is DbProviderInfo dbProviderInfo && dbProviderInfo.ProviderInvariantName == providerInvariantName);
	}

	protected internal void SetDatabaseLogFormatter(Func<DbContext, Action<string>, DatabaseLogFormatter> logFormatterFactory)
	{
		Check.NotNull(logFormatterFactory, "logFormatterFactory");
		_internalConfiguration.CheckNotLocked("SetDatabaseLogFormatter");
		_internalConfiguration.RegisterSingleton(logFormatterFactory);
	}

	protected internal void AddInterceptor(IDbInterceptor interceptor)
	{
		Check.NotNull(interceptor, "interceptor");
		_internalConfiguration.CheckNotLocked("AddInterceptor");
		_internalConfiguration.RegisterSingleton(interceptor);
	}

	protected internal void SetContextFactory(Type contextType, Func<DbContext> factory)
	{
		Check.NotNull(contextType, "contextType");
		Check.NotNull(factory, "factory");
		if (!typeof(DbContext).IsAssignableFrom(contextType))
		{
			throw new ArgumentException(Strings.ContextFactoryContextType(contextType.FullName));
		}
		_internalConfiguration.CheckNotLocked("SetContextFactory");
		_internalConfiguration.RegisterSingleton(factory, contextType);
	}

	protected internal void SetContextFactory<TContext>(Func<TContext> factory) where TContext : DbContext
	{
		Check.NotNull(factory, "factory");
		SetContextFactory(typeof(TContext), factory);
	}

	protected internal void SetModelStore(DbModelStore modelStore)
	{
		Check.NotNull(modelStore, "modelStore");
		_internalConfiguration.CheckNotLocked("SetModelStore");
		_internalConfiguration.RegisterSingleton(modelStore);
	}

	protected internal void SetTableExistenceChecker(string providerInvariantName, TableExistenceChecker tableExistenceChecker)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(tableExistenceChecker, "tableExistenceChecker");
		_internalConfiguration.CheckNotLocked("SetTableExistenceChecker");
		_internalConfiguration.RegisterSingleton(tableExistenceChecker, providerInvariantName);
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected new object MemberwiseClone()
	{
		return base.MemberwiseClone();
	}
}
