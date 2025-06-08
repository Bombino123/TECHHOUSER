using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity;

public class Database
{
	private static readonly Lazy<IDbConnectionFactory> _defaultDefaultConnectionFactory = new Lazy<IDbConnectionFactory>(() => AppConfig.DefaultInstance.TryGetDefaultConnectionFactory() ?? new LocalDbConnectionFactory(), isThreadSafe: true);

	private static volatile Lazy<IDbConnectionFactory> _defaultConnectionFactory = _defaultDefaultConnectionFactory;

	private readonly InternalContext _internalContext;

	private EntityTransaction _entityTransaction;

	private DbContextTransaction _dbContextTransaction;

	public DbContextTransaction CurrentTransaction
	{
		get
		{
			EntityTransaction currentTransaction = ((EntityConnection)_internalContext.ObjectContext.Connection).CurrentTransaction;
			if (_dbContextTransaction == null || _entityTransaction != currentTransaction)
			{
				_entityTransaction = currentTransaction;
				if (currentTransaction != null)
				{
					_dbContextTransaction = new DbContextTransaction(currentTransaction);
				}
				else
				{
					_dbContextTransaction = null;
				}
			}
			return _dbContextTransaction;
		}
	}

	public DbConnection Connection => _internalContext.Connection;

	[Obsolete("The default connection factory should be set in the config file or using the DbConfiguration class. (See http://go.microsoft.com/fwlink/?LinkId=260883)")]
	public static IDbConnectionFactory DefaultConnectionFactory
	{
		get
		{
			return DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>();
		}
		set
		{
			Check.NotNull(value, "value");
			_defaultConnectionFactory = new Lazy<IDbConnectionFactory>(() => value, isThreadSafe: true);
		}
	}

	internal static IDbConnectionFactory SetDefaultConnectionFactory => _defaultConnectionFactory.Value;

	internal static bool DefaultConnectionFactoryChanged => _defaultConnectionFactory != _defaultDefaultConnectionFactory;

	public int? CommandTimeout
	{
		get
		{
			return _internalContext.CommandTimeout;
		}
		set
		{
			if (value.HasValue && value < 0)
			{
				throw new ArgumentException(Strings.ObjectContext_InvalidCommandTimeout);
			}
			_internalContext.CommandTimeout = value;
		}
	}

	public Action<string> Log
	{
		get
		{
			return _internalContext.Log;
		}
		set
		{
			_internalContext.Log = value;
		}
	}

	internal Database(InternalContext internalContext)
	{
		_internalContext = internalContext;
	}

	public void UseTransaction(DbTransaction transaction)
	{
		_entityTransaction = ((EntityConnection)_internalContext.GetObjectContextWithoutDatabaseInitialization().Connection).UseStoreTransaction(transaction);
		_dbContextTransaction = null;
	}

	public DbContextTransaction BeginTransaction()
	{
		EntityConnection entityConnection = (EntityConnection)_internalContext.ObjectContext.Connection;
		_dbContextTransaction = new DbContextTransaction(entityConnection);
		_entityTransaction = entityConnection.CurrentTransaction;
		return _dbContextTransaction;
	}

	public DbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
	{
		EntityConnection entityConnection = (EntityConnection)_internalContext.ObjectContext.Connection;
		_dbContextTransaction = new DbContextTransaction(entityConnection, isolationLevel);
		_entityTransaction = entityConnection.CurrentTransaction;
		return _dbContextTransaction;
	}

	public static void SetInitializer<TContext>(IDatabaseInitializer<TContext> strategy) where TContext : DbContext
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
		InternalConfiguration.Instance.RootResolver.DatabaseInitializerResolver.SetInitializer(typeof(TContext), strategy ?? new NullDatabaseInitializer<TContext>());
	}

	public void Initialize(bool force)
	{
		if (force)
		{
			_internalContext.MarkDatabaseInitialized();
			_internalContext.PerformDatabaseInitialization();
		}
		else
		{
			_internalContext.Initialize();
		}
	}

	public bool CompatibleWithModel(bool throwIfNoMetadata)
	{
		return CompatibleWithModel(throwIfNoMetadata, DatabaseExistenceState.Unknown);
	}

	internal bool CompatibleWithModel(bool throwIfNoMetadata, DatabaseExistenceState existenceState)
	{
		return _internalContext.CompatibleWithModel(throwIfNoMetadata, existenceState);
	}

	public void Create()
	{
		Create(DatabaseExistenceState.Unknown);
	}

	internal void Create(DatabaseExistenceState existenceState)
	{
		if (existenceState == DatabaseExistenceState.Unknown)
		{
			if (_internalContext.DatabaseOperations.Exists(_internalContext.Connection, _internalContext.CommandTimeout, new Lazy<StoreItemCollection>(CreateStoreItemCollection)))
			{
				DbInterceptionContext dbInterceptionContext = new DbInterceptionContext();
				dbInterceptionContext = dbInterceptionContext.WithDbContext(_internalContext.Owner);
				throw Error.Database_DatabaseAlreadyExists(DbInterception.Dispatch.Connection.GetDatabase(_internalContext.Connection, dbInterceptionContext));
			}
			existenceState = DatabaseExistenceState.DoesNotExist;
		}
		using ClonedObjectContext clonedObjectContext = _internalContext.CreateObjectContextForDdlOps();
		_internalContext.CreateDatabase(clonedObjectContext.ObjectContext, existenceState);
	}

	public bool CreateIfNotExists()
	{
		if (_internalContext.DatabaseOperations.Exists(_internalContext.Connection, _internalContext.CommandTimeout, new Lazy<StoreItemCollection>(CreateStoreItemCollection)))
		{
			return false;
		}
		using (ClonedObjectContext clonedObjectContext = _internalContext.CreateObjectContextForDdlOps())
		{
			_internalContext.CreateDatabase(clonedObjectContext.ObjectContext, DatabaseExistenceState.DoesNotExist);
		}
		return true;
	}

	public bool Exists()
	{
		return _internalContext.DatabaseOperations.Exists(_internalContext.Connection, _internalContext.CommandTimeout, new Lazy<StoreItemCollection>(CreateStoreItemCollection));
	}

	public bool Delete()
	{
		if (!_internalContext.DatabaseOperations.Exists(_internalContext.Connection, _internalContext.CommandTimeout, new Lazy<StoreItemCollection>(CreateStoreItemCollection)))
		{
			return false;
		}
		using (ClonedObjectContext clonedObjectContext = _internalContext.CreateObjectContextForDdlOps())
		{
			_internalContext.DatabaseOperations.Delete(clonedObjectContext.ObjectContext);
			_internalContext.MarkDatabaseNotInitialized();
		}
		return true;
	}

	public static bool Exists(string nameOrConnectionString)
	{
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		using LazyInternalConnection lazyInternalConnection = new LazyInternalConnection(nameOrConnectionString);
		return new DatabaseOperations().Exists(lazyInternalConnection.Connection, null, new Lazy<StoreItemCollection>(() => new StoreItemCollection()));
	}

	public static bool Delete(string nameOrConnectionString)
	{
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		if (!Exists(nameOrConnectionString))
		{
			return false;
		}
		using (LazyInternalConnection lazyInternalConnection = new LazyInternalConnection(nameOrConnectionString))
		{
			using ObjectContext objectContext = CreateEmptyObjectContext(lazyInternalConnection.Connection);
			new DatabaseOperations().Delete(objectContext);
		}
		return true;
	}

	public static bool Exists(DbConnection existingConnection)
	{
		Check.NotNull(existingConnection, "existingConnection");
		return new DatabaseOperations().Exists(existingConnection, null, new Lazy<StoreItemCollection>(() => new StoreItemCollection()));
	}

	public static bool Delete(DbConnection existingConnection)
	{
		Check.NotNull(existingConnection, "existingConnection");
		if (!Exists(existingConnection))
		{
			return false;
		}
		using (ObjectContext objectContext = CreateEmptyObjectContext(existingConnection))
		{
			new DatabaseOperations().Delete(objectContext);
		}
		return true;
	}

	internal static void ResetDefaultConnectionFactory()
	{
		_defaultConnectionFactory = _defaultDefaultConnectionFactory;
	}

	private static ObjectContext CreateEmptyObjectContext(DbConnection connection)
	{
		return new DbModelBuilder().Build(connection).Compile().CreateObjectContext<ObjectContext>(connection);
	}

	public DbRawSqlQuery<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
	{
		Check.NotEmpty(sql, "sql");
		Check.NotNull(parameters, "parameters");
		return new DbRawSqlQuery<TElement>(new InternalSqlNonSetQuery(_internalContext, typeof(TElement), sql, parameters));
	}

	public DbRawSqlQuery SqlQuery(Type elementType, string sql, params object[] parameters)
	{
		Check.NotNull(elementType, "elementType");
		Check.NotEmpty(sql, "sql");
		Check.NotNull(parameters, "parameters");
		return new DbRawSqlQuery(new InternalSqlNonSetQuery(_internalContext, elementType, sql, parameters));
	}

	public int ExecuteSqlCommand(string sql, params object[] parameters)
	{
		return ExecuteSqlCommand((!_internalContext.EnsureTransactionsForFunctionsAndCommands) ? TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction, sql, parameters);
	}

	public int ExecuteSqlCommand(TransactionalBehavior transactionalBehavior, string sql, params object[] parameters)
	{
		Check.NotEmpty(sql, "sql");
		Check.NotNull(parameters, "parameters");
		return _internalContext.ExecuteSqlCommand(transactionalBehavior, sql, parameters);
	}

	public Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
	{
		return ExecuteSqlCommandAsync(sql, CancellationToken.None, parameters);
	}

	public Task<int> ExecuteSqlCommandAsync(TransactionalBehavior transactionalBehavior, string sql, params object[] parameters)
	{
		return ExecuteSqlCommandAsync(transactionalBehavior, sql, CancellationToken.None, parameters);
	}

	public Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken, params object[] parameters)
	{
		return ExecuteSqlCommandAsync((!_internalContext.EnsureTransactionsForFunctionsAndCommands) ? TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction, sql, cancellationToken, parameters);
	}

	public Task<int> ExecuteSqlCommandAsync(TransactionalBehavior transactionalBehavior, string sql, CancellationToken cancellationToken, params object[] parameters)
	{
		Check.NotEmpty(sql, "sql");
		Check.NotNull(parameters, "parameters");
		cancellationToken.ThrowIfCancellationRequested();
		return _internalContext.ExecuteSqlCommandAsync(transactionalBehavior, sql, cancellationToken, parameters);
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

	private StoreItemCollection CreateStoreItemCollection()
	{
		using ClonedObjectContext clonedObjectContext = _internalContext.CreateObjectContextForDdlOps();
		return (StoreItemCollection)((EntityConnection)clonedObjectContext.ObjectContext.Connection).GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
	}
}
