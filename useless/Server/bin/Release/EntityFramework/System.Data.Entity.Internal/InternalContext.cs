using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal.Linq;
using System.Data.Entity.Internal.MockingProxies;
using System.Data.Entity.Internal.Validation;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace System.Data.Entity.Internal;

internal abstract class InternalContext : IDisposable
{
	public static readonly MethodInfo CreateObjectAsObjectMethod = typeof(InternalContext).GetOnlyDeclaredMethod("CreateObjectAsObject");

	private static readonly ConcurrentDictionary<Type, Func<InternalContext, object>> _entityFactories = new ConcurrentDictionary<Type, Func<InternalContext, object>>();

	public static readonly MethodInfo ExecuteSqlQueryAsIEnumeratorMethod = typeof(InternalContext).GetOnlyDeclaredMethod("ExecuteSqlQueryAsIEnumerator");

	public static readonly MethodInfo ExecuteSqlQueryAsIDbAsyncEnumeratorMethod = typeof(InternalContext).GetOnlyDeclaredMethod("ExecuteSqlQueryAsIDbAsyncEnumerator");

	private static readonly ConcurrentDictionary<Type, Func<InternalContext, string, bool?, object[], IEnumerator>> _queryExecutors = new ConcurrentDictionary<Type, Func<InternalContext, string, bool?, object[], IEnumerator>>();

	private static readonly ConcurrentDictionary<Type, Func<InternalContext, string, bool?, object[], IDbAsyncEnumerator>> _asyncQueryExecutors = new ConcurrentDictionary<Type, Func<InternalContext, string, bool?, object[], IDbAsyncEnumerator>>();

	private static readonly ConcurrentDictionary<Type, Func<InternalContext, IInternalSet, IInternalSetAdapter>> _setFactories = new ConcurrentDictionary<Type, Func<InternalContext, IInternalSet, IInternalSetAdapter>>();

	public static readonly MethodInfo CreateInitializationActionMethod = typeof(InternalContext).GetOnlyDeclaredMethod("CreateInitializationAction");

	private AppConfig _appConfig = AppConfig.DefaultInstance;

	private readonly DbContext _owner;

	private ClonedObjectContext _tempObjectContext;

	private int _tempObjectContextCount;

	private readonly Dictionary<Type, IInternalSetAdapter> _genericSets = new Dictionary<Type, IInternalSetAdapter>();

	private readonly Dictionary<Type, IInternalSetAdapter> _nonGenericSets = new Dictionary<Type, IInternalSetAdapter>();

	private readonly ValidationProvider _validationProvider = new ValidationProvider(null, DbConfiguration.DependencyResolver.GetService<AttributeProvider>());

	private bool _oSpaceLoadingForced;

	private DbProviderFactory _providerFactory;

	private readonly Lazy<DbDispatchers> _dispatchers;

	private DatabaseLogFormatter _logFormatter;

	private Func<DbMigrationsConfiguration> _migrationsConfiguration;

	private bool? _migrationsConfigurationDiscovered;

	private DbContextInfo _contextInfo;

	private string _defaultContextKey;

	public DbContext Owner => _owner;

	public abstract ObjectContext ObjectContext { get; }

	protected ObjectContext TempObjectContext => (_tempObjectContext == null) ? null : _tempObjectContext.ObjectContext;

	public virtual DbCompiledModel CodeFirstModel => null;

	public virtual DbModel ModelBeingInitialized => null;

	protected bool InInitializationAction { get; set; }

	public abstract IDatabaseInitializer<DbContext> DefaultInitializer { get; }

	public abstract bool EnsureTransactionsForFunctionsAndCommands { get; set; }

	public abstract bool LazyLoadingEnabled { get; set; }

	public abstract bool ProxyCreationEnabled { get; set; }

	public abstract bool UseDatabaseNullSemantics { get; set; }

	public abstract bool DisableFilterOverProjectionSimplificationForCustomFunctions { get; set; }

	public abstract int? CommandTimeout { get; set; }

	public bool AutoDetectChangesEnabled { get; set; }

	public bool ValidateOnSaveEnabled { get; set; }

	public bool IsDisposed { get; private set; }

	public abstract DbConnection Connection { get; }

	public abstract string OriginalConnectionString { get; }

	public abstract DbConnectionStringOrigin ConnectionStringOrigin { get; }

	public virtual AppConfig AppConfig
	{
		get
		{
			CheckContextNotDisposed();
			return _appConfig;
		}
		set
		{
			CheckContextNotDisposed();
			_appConfig = value;
		}
	}

	public virtual DbProviderInfo ModelProviderInfo
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual string ConnectionStringName => null;

	public virtual string ProviderName => Connection.GetProviderInvariantName();

	public DbProviderFactory ProviderFactory => _providerFactory ?? (_providerFactory = DbProviderServices.GetProviderFactory(Connection));

	public virtual Action<DbModelBuilder> OnModelCreating
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool InitializerDisabled { get; set; }

	public virtual DatabaseOperations DatabaseOperations => new DatabaseOperations();

	public virtual ValidationProvider ValidationProvider => _validationProvider;

	public virtual string DefaultSchema => null;

	public string DefaultContextKey
	{
		get
		{
			return _defaultContextKey ?? OwnerShortTypeName;
		}
		set
		{
			_defaultContextKey = value;
		}
	}

	public DbMigrationsConfiguration MigrationsConfiguration
	{
		get
		{
			DiscoverMigrationsConfiguration();
			return _migrationsConfiguration();
		}
	}

	public Func<DbConnection, string, HistoryContext> HistoryContextFactory
	{
		get
		{
			DiscoverMigrationsConfiguration();
			return _migrationsConfiguration().GetHistoryContextFactory(ProviderName);
		}
	}

	public virtual bool MigrationsConfigurationDiscovered
	{
		get
		{
			DiscoverMigrationsConfiguration();
			return _migrationsConfigurationDiscovered.Value;
		}
	}

	internal virtual string OwnerShortTypeName => Owner.GetType().ToString();

	public virtual Action<string> Log
	{
		get
		{
			if (_logFormatter == null)
			{
				return null;
			}
			return _logFormatter.WriteAction;
		}
		set
		{
			if (_logFormatter == null || _logFormatter.WriteAction != value)
			{
				if (_logFormatter != null)
				{
					_dispatchers.Value.RemoveInterceptor(_logFormatter);
					_logFormatter = null;
				}
				if (value != null)
				{
					_logFormatter = DbConfiguration.DependencyResolver.GetService<Func<DbContext, Action<string>, DatabaseLogFormatter>>()(Owner, value);
					_dispatchers.Value.AddInterceptor(_logFormatter);
				}
			}
		}
	}

	public event EventHandler<EventArgs> OnDisposing;

	protected InternalContext(DbContext owner, Lazy<DbDispatchers> dispatchers = null)
	{
		_owner = owner;
		_dispatchers = dispatchers ?? new Lazy<DbDispatchers>(() => DbInterception.Dispatch);
		AutoDetectChangesEnabled = true;
		ValidateOnSaveEnabled = true;
	}

	protected InternalContext()
	{
	}

	public abstract ObjectContext GetObjectContextWithoutDatabaseInitialization();

	public virtual ClonedObjectContext CreateObjectContextForDdlOps()
	{
		InitializeContext();
		return new ClonedObjectContext(new ObjectContextProxy(GetObjectContextWithoutDatabaseInitialization()), Connection, OriginalConnectionString, transferLoadedAssemblies: false);
	}

	public virtual void UseTempObjectContext()
	{
		_tempObjectContextCount++;
		if (_tempObjectContext == null)
		{
			_tempObjectContext = new ClonedObjectContext(new ObjectContextProxy(GetObjectContextWithoutDatabaseInitialization()), Connection, OriginalConnectionString);
			ResetDbSets();
		}
	}

	public virtual void DisposeTempObjectContext()
	{
		if (_tempObjectContextCount > 0 && --_tempObjectContextCount == 0 && _tempObjectContext != null)
		{
			_tempObjectContext.Dispose();
			_tempObjectContext = null;
			ResetDbSets();
		}
	}

	public virtual void CreateDatabase(ObjectContext objectContext, DatabaseExistenceState existenceState)
	{
		new DatabaseCreator().CreateDatabase(this, (DbMigrationsConfiguration config, DbContext context) => new DbMigrator(config, context, existenceState, calledByCreateDatabase: true), objectContext);
	}

	public virtual bool CompatibleWithModel(bool throwIfNoMetadata, DatabaseExistenceState existenceState)
	{
		return new ModelCompatibilityChecker().CompatibleWithModel(this, new ModelHashCalculator(), throwIfNoMetadata, existenceState);
	}

	public virtual bool ModelMatches(VersionedModel model)
	{
		return !new EdmModelDiffer().Diff(model.Model, Owner.GetModel(), null, null, model.Version).Any();
	}

	public virtual string QueryForModelHash()
	{
		return new EdmMetadataRepository(this, OriginalConnectionString, ProviderFactory).QueryForModelHash((DbConnection c) => new EdmMetadataContext(c));
	}

	public virtual VersionedModel QueryForModel(DatabaseExistenceState existenceState)
	{
		string migrationId;
		string productVersion;
		XDocument lastModel = CreateHistoryRepository(existenceState).GetLastModel(out migrationId, out productVersion);
		if (lastModel == null)
		{
			return null;
		}
		return new VersionedModel(lastModel, productVersion);
	}

	public virtual void SaveMetadataToDatabase()
	{
		if (CodeFirstModel != null)
		{
			PerformInitializationAction(delegate
			{
				CreateHistoryRepository().BootstrapUsingEFProviderDdl(new VersionedModel(Owner.GetModel()));
			});
		}
	}

	public virtual bool HasHistoryTableEntry()
	{
		return CreateHistoryRepository().HasMigrations();
	}

	private HistoryRepository CreateHistoryRepository(DatabaseExistenceState existenceState = DatabaseExistenceState.Unknown)
	{
		DiscoverMigrationsConfiguration();
		string originalConnectionString = OriginalConnectionString;
		DbProviderFactory providerFactory = ProviderFactory;
		string contextKey = _migrationsConfiguration().ContextKey;
		int? commandTimeout = CommandTimeout;
		Func<DbConnection, string, HistoryContext> historyContextFactory = HistoryContextFactory;
		IEnumerable<string> schemas;
		if (DefaultSchema == null)
		{
			schemas = Enumerable.Empty<string>();
		}
		else
		{
			IEnumerable<string> enumerable = new string[1] { DefaultSchema };
			schemas = enumerable;
		}
		return new HistoryRepository(this, originalConnectionString, providerFactory, contextKey, commandTimeout, historyContextFactory, schemas, Owner, existenceState);
	}

	public virtual DbTransaction TryGetCurrentStoreTransaction()
	{
		return ((EntityConnection)GetObjectContextWithoutDatabaseInitialization().Connection).CurrentTransaction?.StoreTransaction;
	}

	public void PerformInitializationAction(Action action)
	{
		if (InInitializationAction)
		{
			action();
			return;
		}
		try
		{
			InInitializationAction = true;
			action();
		}
		catch (DataException innerException)
		{
			throw new DataException(Strings.Database_InitializationException, innerException);
		}
		finally
		{
			InInitializationAction = false;
		}
	}

	public virtual void RegisterObjectStateManagerChangedEvent(CollectionChangeEventHandler handler)
	{
		ObjectContext.ObjectStateManager.ObjectStateManagerChanged += handler;
	}

	public virtual bool EntityInContextAndNotDeleted(object entity)
	{
		if (ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out var entry))
		{
			return entry.State != EntityState.Deleted;
		}
		return false;
	}

	public virtual int SaveChanges()
	{
		try
		{
			if (ValidateOnSaveEnabled)
			{
				IEnumerable<DbEntityValidationResult> validationErrors = Owner.GetValidationErrors();
				if (validationErrors.Any())
				{
					throw new DbEntityValidationException(Strings.DbEntityValidationException_ValidationFailed, validationErrors);
				}
			}
			bool flag = AutoDetectChangesEnabled && !ValidateOnSaveEnabled;
			SaveOptions options = SaveOptions.AcceptAllChangesAfterSave | (flag ? SaveOptions.DetectChangesBeforeSave : SaveOptions.None);
			return ObjectContext.SaveChanges(options);
		}
		catch (UpdateException updateException)
		{
			throw WrapUpdateException(updateException);
		}
	}

	public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (ValidateOnSaveEnabled)
		{
			IEnumerable<DbEntityValidationResult> validationErrors = Owner.GetValidationErrors();
			if (validationErrors.Any())
			{
				throw new DbEntityValidationException(Strings.DbEntityValidationException_ValidationFailed, validationErrors);
			}
		}
		TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
		bool flag = AutoDetectChangesEnabled && !ValidateOnSaveEnabled;
		SaveOptions options = SaveOptions.AcceptAllChangesAfterSave | (flag ? SaveOptions.DetectChangesBeforeSave : SaveOptions.None);
		ObjectContext.SaveChangesAsync(options, cancellationToken).ContinueWith(delegate(Task<int> t)
		{
			if (t.IsFaulted)
			{
				IEnumerable<Exception> exceptions = t.Exception.InnerExceptions.Select((Exception ex) => (ex is UpdateException updateException) ? WrapUpdateException(updateException) : ex);
				tcs.TrySetException(exceptions);
			}
			else if (t.IsCanceled)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetResult(t.Result);
			}
		}, TaskContinuationOptions.ExecuteSynchronously);
		return tcs.Task;
	}

	public void Initialize()
	{
		Debugger.NotifyOfCrossThreadDependency();
		InitializeContext();
		InitializeDatabase();
	}

	protected abstract void InitializeContext();

	public abstract void MarkDatabaseNotInitialized();

	protected abstract void InitializeDatabase();

	public abstract void MarkDatabaseInitialized();

	public void PerformDatabaseInitialization()
	{
		object obj = DbConfiguration.DependencyResolver.GetService(typeof(IDatabaseInitializer<>).MakeGenericType(Owner.GetType())) ?? DefaultInitializer ?? new NullDatabaseInitializer<DbContext>();
		Action action = (Action)CreateInitializationActionMethod.MakeGenericMethod(Owner.GetType()).Invoke(this, new object[1] { obj });
		bool autoDetectChangesEnabled = AutoDetectChangesEnabled;
		bool validateOnSaveEnabled = ValidateOnSaveEnabled;
		try
		{
			if (!(Owner is TransactionContext))
			{
				UseTempObjectContext();
			}
			PerformInitializationAction(action);
		}
		finally
		{
			if (!(Owner is TransactionContext))
			{
				DisposeTempObjectContext();
			}
			AutoDetectChangesEnabled = autoDetectChangesEnabled;
			ValidateOnSaveEnabled = validateOnSaveEnabled;
		}
	}

	private Action CreateInitializationAction<TContext>(IDatabaseInitializer<TContext> initializer) where TContext : DbContext
	{
		return delegate
		{
			initializer.InitializeDatabase((TContext)Owner);
		};
	}

	protected void LoadContextConfigs()
	{
		int? num = AppConfig.ContextConfigs.TryGetCommandTimeout(Owner.GetType());
		if (num.HasValue)
		{
			CommandTimeout = num.Value;
		}
	}

	~InternalContext()
	{
		DisposeContext(disposing: false);
	}

	public void Dispose()
	{
		DisposeContext(disposing: true);
		GC.SuppressFinalize(this);
	}

	public virtual void DisposeContext(bool disposing)
	{
		if (!IsDisposed)
		{
			if (disposing && this.OnDisposing != null)
			{
				this.OnDisposing(this, new EventArgs());
				this.OnDisposing = null;
			}
			if (_tempObjectContext != null)
			{
				_tempObjectContext.Dispose();
			}
			Log = null;
			IsDisposed = true;
		}
	}

	public virtual void DetectChanges(bool force = false)
	{
		if (AutoDetectChangesEnabled || force)
		{
			ObjectContext.DetectChanges();
		}
	}

	public virtual IDbSet<TEntity> Set<TEntity>() where TEntity : class
	{
		if (typeof(TEntity) != ObjectContextTypeCache.GetObjectType(typeof(TEntity)))
		{
			throw Error.CannotCallGenericSetWithProxyType();
		}
		if (!_genericSets.TryGetValue(typeof(TEntity), out var value))
		{
			IInternalSet internalSet2;
			if (!_nonGenericSets.TryGetValue(typeof(TEntity), out value))
			{
				IInternalSet internalSet = new InternalSet<TEntity>(this);
				internalSet2 = internalSet;
			}
			else
			{
				internalSet2 = value.InternalSet;
			}
			value = new DbSet<TEntity>((InternalSet<TEntity>)internalSet2);
			_genericSets.Add(typeof(TEntity), value);
		}
		return (IDbSet<TEntity>)value;
	}

	public virtual IInternalSetAdapter Set(Type entityType)
	{
		entityType = ObjectContextTypeCache.GetObjectType(entityType);
		if (!_nonGenericSets.TryGetValue(entityType, out var value))
		{
			value = CreateInternalSet(entityType, _genericSets.TryGetValue(entityType, out value) ? value.InternalSet : null);
			_nonGenericSets.Add(entityType, value);
		}
		return value;
	}

	private IInternalSetAdapter CreateInternalSet(Type entityType, IInternalSet internalSet)
	{
		if (!_setFactories.TryGetValue(entityType, out var value))
		{
			if (entityType.IsValueType())
			{
				throw Error.DbSet_EntityTypeNotInModel(entityType.Name);
			}
			MethodInfo declaredMethod = typeof(InternalDbSet<>).MakeGenericType(entityType).GetDeclaredMethod("Create", typeof(InternalContext), typeof(IInternalSet));
			value = (Func<InternalContext, IInternalSet, IInternalSetAdapter>)Delegate.CreateDelegate(typeof(Func<InternalContext, IInternalSet, IInternalSetAdapter>), declaredMethod);
			_setFactories.TryAdd(entityType, value);
		}
		return value(this, internalSet);
	}

	public virtual EntitySetTypePair GetEntitySetAndBaseTypeForType(Type entityType)
	{
		Initialize();
		UpdateEntitySetMappingsForType(entityType);
		return GetEntitySetMappingForType(entityType);
	}

	public virtual EntitySetTypePair TryGetEntitySetAndBaseTypeForType(Type entityType)
	{
		Initialize();
		if (!TryUpdateEntitySetMappingsForType(entityType))
		{
			return null;
		}
		return GetEntitySetMappingForType(entityType);
	}

	public virtual bool IsEntityTypeMapped(Type entityType)
	{
		Initialize();
		return TryUpdateEntitySetMappingsForType(entityType);
	}

	public virtual IEnumerable<TEntity> GetLocalEntities<TEntity>()
	{
		return from e in ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Unchanged | EntityState.Added | EntityState.Modified)
			where e.Entity is TEntity
			select (TEntity)e.Entity;
	}

	public virtual IEnumerator<TElement> ExecuteSqlQuery<TElement>(string sql, bool? streaming, object[] parameters)
	{
		ObjectContext.AsyncMonitor.EnsureNotEntered();
		return new LazyEnumerator<TElement>(delegate
		{
			Initialize();
			return ObjectContext.ExecuteStoreQuery<TElement>(sql, new ExecutionOptions(MergeOption.AppendOnly, streaming), parameters);
		});
	}

	public virtual IDbAsyncEnumerator<TElement> ExecuteSqlQueryAsync<TElement>(string sql, bool? streaming, object[] parameters)
	{
		ObjectContext.AsyncMonitor.EnsureNotEntered();
		return new LazyAsyncEnumerator<TElement>(delegate(CancellationToken cancellationToken)
		{
			Initialize();
			return ObjectContext.ExecuteStoreQueryAsync<TElement>(sql, new ExecutionOptions(MergeOption.AppendOnly, streaming), cancellationToken, parameters);
		});
	}

	public virtual IEnumerator ExecuteSqlQuery(Type elementType, string sql, bool? streaming, object[] parameters)
	{
		if (!_queryExecutors.TryGetValue(elementType, out var value))
		{
			MethodInfo method = ExecuteSqlQueryAsIEnumeratorMethod.MakeGenericMethod(elementType);
			value = (Func<InternalContext, string, bool?, object[], IEnumerator>)Delegate.CreateDelegate(typeof(Func<InternalContext, string, bool?, object[], IEnumerator>), method);
			_queryExecutors.TryAdd(elementType, value);
		}
		return value(this, sql, streaming, parameters);
	}

	private IEnumerator ExecuteSqlQueryAsIEnumerator<TElement>(string sql, bool? streaming, object[] parameters)
	{
		return ExecuteSqlQuery<TElement>(sql, streaming, parameters);
	}

	public virtual IDbAsyncEnumerator ExecuteSqlQueryAsync(Type elementType, string sql, bool? streaming, object[] parameters)
	{
		if (!_asyncQueryExecutors.TryGetValue(elementType, out var value))
		{
			MethodInfo method = ExecuteSqlQueryAsIDbAsyncEnumeratorMethod.MakeGenericMethod(elementType);
			value = (Func<InternalContext, string, bool?, object[], IDbAsyncEnumerator>)Delegate.CreateDelegate(typeof(Func<InternalContext, string, bool?, object[], IDbAsyncEnumerator>), method);
			_asyncQueryExecutors.TryAdd(elementType, value);
		}
		return value(this, sql, streaming, parameters);
	}

	private IDbAsyncEnumerator ExecuteSqlQueryAsIDbAsyncEnumerator<TElement>(string sql, bool? streaming, object[] parameters)
	{
		return ExecuteSqlQueryAsync<TElement>(sql, streaming, parameters);
	}

	public virtual int ExecuteSqlCommand(TransactionalBehavior transactionalBehavior, string sql, object[] parameters)
	{
		Initialize();
		return ObjectContext.ExecuteStoreCommand(transactionalBehavior, sql, parameters);
	}

	public virtual Task<int> ExecuteSqlCommandAsync(TransactionalBehavior transactionalBehavior, string sql, CancellationToken cancellationToken, object[] parameters)
	{
		Initialize();
		return ObjectContext.ExecuteStoreCommandAsync(transactionalBehavior, sql, cancellationToken, parameters);
	}

	public virtual IEntityStateEntry GetStateEntry(object entity)
	{
		DetectChanges();
		if (!ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out var entry))
		{
			return null;
		}
		return new StateEntryAdapter(entry);
	}

	public virtual IEnumerable<IEntityStateEntry> GetStateEntries()
	{
		return GetStateEntries((ObjectStateEntry e) => e.Entity != null);
	}

	public virtual IEnumerable<IEntityStateEntry> GetStateEntries<TEntity>() where TEntity : class
	{
		return GetStateEntries((ObjectStateEntry e) => e.Entity is TEntity);
	}

	private IEnumerable<IEntityStateEntry> GetStateEntries(Func<ObjectStateEntry, bool> predicate)
	{
		DetectChanges();
		return from e in ObjectContext.ObjectStateManager.GetObjectStateEntries(~EntityState.Detached).Where(predicate)
			select new StateEntryAdapter(e);
	}

	public virtual DbUpdateException WrapUpdateException(UpdateException updateException)
	{
		if (updateException.StateEntries != null && updateException.StateEntries.Any((ObjectStateEntry e) => e.Entity == null))
		{
			return new DbUpdateException(this, updateException, involvesIndependentAssociations: true);
		}
		if (!(updateException is OptimisticConcurrencyException innerException))
		{
			return new DbUpdateException(this, updateException, involvesIndependentAssociations: false);
		}
		return new DbUpdateConcurrencyException(this, innerException);
	}

	public virtual TEntity CreateObject<TEntity>() where TEntity : class
	{
		return ObjectContext.CreateObject<TEntity>();
	}

	public virtual object CreateObject(Type type)
	{
		if (!_entityFactories.TryGetValue(type, out var value))
		{
			MethodInfo method = CreateObjectAsObjectMethod.MakeGenericMethod(type);
			value = (Func<InternalContext, object>)Delegate.CreateDelegate(typeof(Func<InternalContext, object>), method);
			_entityFactories.TryAdd(type, value);
		}
		return value(this);
	}

	private object CreateObjectAsObject<TEntity>() where TEntity : class
	{
		return CreateObject<TEntity>();
	}

	public abstract void OverrideConnection(IInternalConnection connection);

	protected void CheckContextNotDisposed()
	{
		if (IsDisposed)
		{
			throw Error.DbContext_Disposed();
		}
	}

	protected void ResetDbSets()
	{
		foreach (IInternalSetAdapter item in _genericSets.Values.Union(_nonGenericSets.Values))
		{
			item.InternalSet.ResetQuery();
		}
	}

	public void ForceOSpaceLoadingForKnownEntityTypes()
	{
		if (_oSpaceLoadingForced)
		{
			return;
		}
		_oSpaceLoadingForced = true;
		Initialize();
		foreach (IInternalSetAdapter item in _genericSets.Values.Union(_nonGenericSets.Values))
		{
			item.InternalSet.TryInitialize();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryUpdateEntitySetMappingsForType(Type entityType)
	{
		return GetObjectContextWithoutDatabaseInitialization().MetadataWorkspace.MetadataOptimization.TryUpdateEntitySetMappingsForType(entityType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private EntitySetTypePair GetEntitySetMappingForType(Type entityType)
	{
		return GetObjectContextWithoutDatabaseInitialization().MetadataWorkspace.MetadataOptimization.EntitySetMappingCache[entityType];
	}

	private void UpdateEntitySetMappingsForType(Type entityType)
	{
		if (!TryUpdateEntitySetMappingsForType(entityType))
		{
			if (IsComplexType(entityType))
			{
				throw Error.DbSet_DbSetUsedWithComplexType(entityType.Name);
			}
			if (IsPocoTypeInNonPocoAssembly(entityType))
			{
				throw Error.DbSet_PocoAndNonPocoMixedInSameAssembly(entityType.Name);
			}
			throw Error.DbSet_EntityTypeNotInModel(entityType.Name);
		}
	}

	private static bool IsPocoTypeInNonPocoAssembly(Type entityType)
	{
		if (entityType.Assembly().GetCustomAttributes<EdmSchemaAttribute>().Any())
		{
			return !entityType.GetCustomAttributes<EdmEntityTypeAttribute>(inherit: true).Any();
		}
		return false;
	}

	private bool IsComplexType(Type clrType)
	{
		MetadataWorkspace metadataWorkspace = GetObjectContextWithoutDatabaseInitialization().MetadataWorkspace;
		ObjectItemCollection objectItemCollection = (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);
		return metadataWorkspace.GetItems<ComplexType>(DataSpace.OSpace).Any((ComplexType t) => objectItemCollection.GetClrType(t) == clrType);
	}

	public void ApplyContextInfo(DbContextInfo info)
	{
		if (_contextInfo == null)
		{
			InitializerDisabled = true;
			_contextInfo = info;
			_contextInfo.ConfigureContext(Owner);
		}
	}

	private void DiscoverMigrationsConfiguration()
	{
		if (_migrationsConfigurationDiscovered.HasValue)
		{
			return;
		}
		Type contextType = Owner.GetType();
		DbMigrationsConfiguration discoveredConfig = new MigrationsConfigurationFinder(new TypeFinder(contextType.Assembly)).FindMigrationsConfiguration(contextType, null);
		if (discoveredConfig != null)
		{
			_migrationsConfiguration = () => discoveredConfig;
			_migrationsConfigurationDiscovered = true;
			return;
		}
		_migrationsConfiguration = () => new Lazy<DbMigrationsConfiguration>(() => new DbMigrationsConfiguration
		{
			ContextType = contextType,
			AutomaticMigrationsEnabled = true,
			MigrationsAssembly = contextType.Assembly,
			MigrationsNamespace = contextType.Namespace,
			ContextKey = DefaultContextKey,
			TargetDatabase = new DbConnectionInfo(OriginalConnectionString, ProviderName),
			CommandTimeout = CommandTimeout
		}).Value;
		_migrationsConfigurationDiscovered = false;
	}
}
