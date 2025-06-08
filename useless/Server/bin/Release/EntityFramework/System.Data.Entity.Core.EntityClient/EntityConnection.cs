using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace System.Data.Entity.Core.EntityClient;

public class EntityConnection : DbConnection
{
	private const string EntityClientProviderName = "System.Data.EntityClient";

	private const string ProviderInvariantName = "provider";

	private const string ProviderConnectionString = "provider connection string";

	private const string ReaderPrefix = "reader://";

	private readonly object _connectionStringLock = new object();

	private static readonly DbConnectionOptions _emptyConnectionOptions = new DbConnectionOptions(string.Empty, new string[0]);

	private DbConnectionOptions _userConnectionOptions;

	private DbConnectionOptions _effectiveConnectionOptions;

	private ConnectionState _entityClientConnectionState;

	private DbProviderFactory _providerFactory;

	private DbConnection _storeConnection;

	private readonly bool _entityConnectionOwnsStoreConnection = true;

	private MetadataWorkspace _metadataWorkspace;

	private EntityTransaction _currentTransaction;

	private Transaction _enlistedTransaction;

	private bool _initialized;

	private ConnectionState? _fakeConnectionState;

	private readonly List<ObjectContext> _associatedContexts = new List<ObjectContext>();

	public override string ConnectionString
	{
		get
		{
			if (_userConnectionOptions == null)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}={3}{4};{1}={5};{2}=\"{6}\";", "metadata", "provider", "provider connection string", "reader://", _metadataWorkspace.MetadataWorkspaceId, _storeConnection.GetProviderInvariantName(), DbInterception.Dispatch.Connection.GetConnectionString(_storeConnection, InterceptionContext));
			}
			string usersConnectionString = _userConnectionOptions.UsersConnectionString;
			if (_userConnectionOptions == _effectiveConnectionOptions && _storeConnection != null)
			{
				string text = null;
				try
				{
					text = DbInterception.Dispatch.Connection.GetConnectionString(_storeConnection, InterceptionContext);
				}
				catch (Exception ex)
				{
					if (ex.IsCatchableExceptionType())
					{
						throw new EntityException(Strings.EntityClient_ProviderSpecificError("ConnectionString"), ex);
					}
					throw;
				}
				string text2 = _userConnectionOptions["provider connection string"];
				if (text != text2 && (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2)))
				{
					return new EntityConnectionStringBuilder(usersConnectionString)
					{
						ProviderConnectionString = text
					}.ConnectionString;
				}
			}
			return usersConnectionString;
		}
		set
		{
			if (_initialized)
			{
				throw new InvalidOperationException(Strings.EntityClient_SettingsCannotBeChangedOnOpenConnection);
			}
			ChangeConnectionString(value);
		}
	}

	internal IEnumerable<ObjectContext> AssociatedContexts => _associatedContexts;

	internal DbInterceptionContext InterceptionContext => DbInterceptionContext.Combine(AssociatedContexts.Select((ObjectContext c) => c.InterceptionContext));

	public override int ConnectionTimeout
	{
		get
		{
			if (_storeConnection == null)
			{
				return 0;
			}
			try
			{
				return DbInterception.Dispatch.Connection.GetConnectionTimeout(_storeConnection, InterceptionContext);
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityException(Strings.EntityClient_ProviderSpecificError("ConnectionTimeout"), ex);
				}
				throw;
			}
		}
	}

	public override string Database => string.Empty;

	public override ConnectionState State => _fakeConnectionState ?? _entityClientConnectionState;

	public override string DataSource
	{
		get
		{
			if (_storeConnection == null)
			{
				return string.Empty;
			}
			try
			{
				return DbInterception.Dispatch.Connection.GetDataSource(_storeConnection, InterceptionContext);
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityException(Strings.EntityClient_ProviderSpecificError("DataSource"), ex);
				}
				throw;
			}
		}
	}

	public override string ServerVersion
	{
		get
		{
			if (_storeConnection == null)
			{
				throw Error.EntityClient_ConnectionStringNeededBeforeOperation();
			}
			if (State != ConnectionState.Open)
			{
				throw Error.EntityClient_ConnectionNotOpen();
			}
			try
			{
				return DbInterception.Dispatch.Connection.GetServerVersion(_storeConnection, InterceptionContext);
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityException(Strings.EntityClient_ProviderSpecificError("ServerVersion"), ex);
				}
				throw;
			}
		}
	}

	protected override DbProviderFactory DbProviderFactory => EntityProviderFactory.Instance;

	internal virtual DbProviderFactory StoreProviderFactory => _providerFactory;

	public virtual DbConnection StoreConnection => _storeConnection;

	public virtual EntityTransaction CurrentTransaction
	{
		get
		{
			if (_currentTransaction != null && (DbInterception.Dispatch.Transaction.GetConnection(_currentTransaction.StoreTransaction, InterceptionContext) == null || State == ConnectionState.Closed))
			{
				ClearCurrentTransaction();
			}
			return _currentTransaction;
		}
	}

	internal virtual bool EnlistedInUserTransaction
	{
		get
		{
			try
			{
				return _enlistedTransaction != null && _enlistedTransaction.TransactionInformation.Status == TransactionStatus.Active;
			}
			catch (ObjectDisposedException)
			{
				_enlistedTransaction = null;
				return false;
			}
		}
	}

	public EntityConnection()
		: this(string.Empty)
	{
	}

	public EntityConnection(string connectionString)
	{
		ChangeConnectionString(connectionString);
	}

	public EntityConnection(MetadataWorkspace workspace, DbConnection connection)
		: this(Check.NotNull(workspace, "workspace"), Check.NotNull(connection, "connection"), skipInitialization: false, entityConnectionOwnsStoreConnection: false)
	{
	}

	public EntityConnection(MetadataWorkspace workspace, DbConnection connection, bool entityConnectionOwnsStoreConnection)
		: this(Check.NotNull(workspace, "workspace"), Check.NotNull(connection, "connection"), skipInitialization: false, entityConnectionOwnsStoreConnection)
	{
	}

	internal EntityConnection(MetadataWorkspace workspace, DbConnection connection, bool skipInitialization, bool entityConnectionOwnsStoreConnection)
	{
		if (!skipInitialization)
		{
			if (!workspace.IsItemCollectionAlreadyRegistered(DataSpace.CSpace))
			{
				throw new ArgumentException(Strings.EntityClient_ItemCollectionsNotRegisteredInWorkspace("EdmItemCollection"));
			}
			if (!workspace.IsItemCollectionAlreadyRegistered(DataSpace.SSpace))
			{
				throw new ArgumentException(Strings.EntityClient_ItemCollectionsNotRegisteredInWorkspace("StoreItemCollection"));
			}
			if (!workspace.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace))
			{
				throw new ArgumentException(Strings.EntityClient_ItemCollectionsNotRegisteredInWorkspace("StorageMappingItemCollection"));
			}
			if (connection.GetProviderFactory() == null)
			{
				throw new ProviderIncompatibleException(Strings.EntityClient_DbConnectionHasNoProvider(connection));
			}
			StoreItemCollection storeItemCollection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
			_providerFactory = storeItemCollection.ProviderFactory;
			_initialized = true;
		}
		_metadataWorkspace = workspace;
		_storeConnection = connection;
		_entityConnectionOwnsStoreConnection = entityConnectionOwnsStoreConnection;
		if (_storeConnection != null)
		{
			_entityClientConnectionState = DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext);
		}
		SubscribeToStoreConnectionStateChangeEvents();
	}

	private void SubscribeToStoreConnectionStateChangeEvents()
	{
		if (_storeConnection != null)
		{
			_storeConnection.StateChange += StoreConnectionStateChangeHandler;
		}
	}

	private void UnsubscribeFromStoreConnectionStateChangeEvents()
	{
		if (_storeConnection != null)
		{
			_storeConnection.StateChange -= StoreConnectionStateChangeHandler;
		}
	}

	internal virtual void StoreConnectionStateChangeHandler(object sender, StateChangeEventArgs stateChange)
	{
		ConnectionState currentState = stateChange.CurrentState;
		if (_entityClientConnectionState != currentState)
		{
			ConnectionState entityClientConnectionState = _entityClientConnectionState;
			_entityClientConnectionState = stateChange.CurrentState;
			OnStateChange(new StateChangeEventArgs(entityClientConnectionState, currentState));
		}
	}

	internal virtual void AssociateContext(ObjectContext context)
	{
		if (_associatedContexts.Count != 0)
		{
			ObjectContext[] array = _associatedContexts.ToArray();
			foreach (ObjectContext objectContext in array)
			{
				if (context == objectContext || objectContext.IsDisposed)
				{
					_associatedContexts.Remove(objectContext);
				}
			}
		}
		_associatedContexts.Add(context);
	}

	public virtual MetadataWorkspace GetMetadataWorkspace()
	{
		if (_metadataWorkspace != null)
		{
			return _metadataWorkspace;
		}
		_metadataWorkspace = MetadataCache.Instance.GetMetadataWorkspace(_effectiveConnectionOptions);
		_initialized = true;
		return _metadataWorkspace;
	}

	public override void Open()
	{
		_fakeConnectionState = null;
		if (!DbInterception.Dispatch.CancelableEntityConnection.Opening(this, InterceptionContext))
		{
			_fakeConnectionState = ConnectionState.Open;
			return;
		}
		if (_storeConnection == null)
		{
			throw Error.EntityClient_ConnectionStringNeededBeforeOperation();
		}
		if (State == ConnectionState.Broken)
		{
			throw Error.EntityClient_CannotOpenBrokenConnection();
		}
		if (DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) != ConnectionState.Open)
		{
			MetadataWorkspace metadataWorkspace = GetMetadataWorkspace();
			try
			{
				DbProviderServices.GetExecutionStrategy(_storeConnection, metadataWorkspace).Execute(delegate
				{
					DbInterception.Dispatch.Connection.Open(_storeConnection, InterceptionContext);
				});
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityException(Strings.EntityClient_ProviderSpecificError("Open"), ex);
				}
				throw;
			}
			ClearTransactions();
		}
		if (_storeConnection != null && DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) == ConnectionState.Open)
		{
			return;
		}
		throw Error.EntityClient_ConnectionNotOpen();
	}

	public override async Task OpenAsync(CancellationToken cancellationToken)
	{
		if (_storeConnection == null)
		{
			throw Error.EntityClient_ConnectionStringNeededBeforeOperation();
		}
		if (State == ConnectionState.Broken)
		{
			throw Error.EntityClient_CannotOpenBrokenConnection();
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) != ConnectionState.Open)
		{
			MetadataWorkspace metadataWorkspace = GetMetadataWorkspace();
			try
			{
				await DbProviderServices.GetExecutionStrategy(_storeConnection, metadataWorkspace).ExecuteAsync(() => DbInterception.Dispatch.Connection.OpenAsync(_storeConnection, InterceptionContext, cancellationToken), cancellationToken).WithCurrentCulture();
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityException(Strings.EntityClient_ProviderSpecificError("Open"), ex);
				}
				throw;
			}
			ClearTransactions();
		}
		if (_storeConnection == null || DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) != ConnectionState.Open)
		{
			throw Error.EntityClient_ConnectionNotOpen();
		}
	}

	public new virtual EntityCommand CreateCommand()
	{
		return new EntityCommand(null, this);
	}

	protected override DbCommand CreateDbCommand()
	{
		return CreateCommand();
	}

	public override void Close()
	{
		_fakeConnectionState = null;
		if (_storeConnection != null)
		{
			StoreCloseHelper();
		}
	}

	public override void ChangeDatabase(string databaseName)
	{
		throw new NotSupportedException();
	}

	public new virtual EntityTransaction BeginTransaction()
	{
		return base.BeginTransaction() as EntityTransaction;
	}

	public new virtual EntityTransaction BeginTransaction(IsolationLevel isolationLevel)
	{
		return base.BeginTransaction(isolationLevel) as EntityTransaction;
	}

	protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
	{
		if (_fakeConnectionState.HasValue)
		{
			return new EntityTransaction();
		}
		if (CurrentTransaction != null)
		{
			throw new InvalidOperationException(Strings.EntityClient_TransactionAlreadyStarted);
		}
		if (_storeConnection == null)
		{
			throw Error.EntityClient_ConnectionStringNeededBeforeOperation();
		}
		if (State != ConnectionState.Open)
		{
			throw Error.EntityClient_ConnectionNotOpen();
		}
		BeginTransactionInterceptionContext interceptionContext = new BeginTransactionInterceptionContext(InterceptionContext);
		if (isolationLevel != IsolationLevel.Unspecified)
		{
			interceptionContext = interceptionContext.WithIsolationLevel(isolationLevel);
		}
		DbTransaction dbTransaction = null;
		try
		{
			dbTransaction = DbProviderServices.GetExecutionStrategy(_storeConnection, GetMetadataWorkspace()).Execute(delegate
			{
				if (DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) == ConnectionState.Broken)
				{
					DbInterception.Dispatch.Connection.Close(_storeConnection, interceptionContext);
				}
				if (DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) == ConnectionState.Closed)
				{
					DbInterception.Dispatch.Connection.Open(_storeConnection, interceptionContext);
				}
				return DbInterception.Dispatch.Connection.BeginTransaction(_storeConnection, interceptionContext);
			});
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityException(Strings.EntityClient_ErrorInBeginningTransaction, ex);
			}
			throw;
		}
		if (dbTransaction == null)
		{
			throw new ProviderIncompatibleException(Strings.EntityClient_ReturnedNullOnProviderMethod("BeginTransaction", _storeConnection.GetType().Name));
		}
		_currentTransaction = new EntityTransaction(this, dbTransaction);
		return _currentTransaction;
	}

	internal virtual EntityTransaction UseStoreTransaction(DbTransaction storeTransaction)
	{
		if (storeTransaction == null)
		{
			ClearCurrentTransaction();
		}
		else
		{
			if (CurrentTransaction != null)
			{
				throw new InvalidOperationException(Strings.DbContext_TransactionAlreadyStarted);
			}
			if (EnlistedInUserTransaction)
			{
				throw new InvalidOperationException(Strings.DbContext_TransactionAlreadyEnlistedInUserTransaction);
			}
			if ((DbInterception.Dispatch.Transaction.GetConnection(storeTransaction, InterceptionContext) ?? throw new InvalidOperationException(Strings.DbContext_InvalidTransactionNoConnection)) != StoreConnection)
			{
				throw new InvalidOperationException(Strings.DbContext_InvalidTransactionForConnection);
			}
			_currentTransaction = new EntityTransaction(this, storeTransaction);
		}
		return _currentTransaction;
	}

	public override void EnlistTransaction(Transaction transaction)
	{
		if (_storeConnection == null)
		{
			throw Error.EntityClient_ConnectionStringNeededBeforeOperation();
		}
		if (State != ConnectionState.Open)
		{
			throw Error.EntityClient_ConnectionNotOpen();
		}
		try
		{
			EnlistTransactionInterceptionContext enlistTransactionInterceptionContext = new EnlistTransactionInterceptionContext(InterceptionContext);
			enlistTransactionInterceptionContext = enlistTransactionInterceptionContext.WithTransaction(transaction);
			DbInterception.Dispatch.Connection.EnlistTransaction(_storeConnection, enlistTransactionInterceptionContext);
			if (transaction != null && !EnlistedInUserTransaction)
			{
				transaction.TransactionCompleted += EnlistedTransactionCompleted;
			}
			_enlistedTransaction = transaction;
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityException(Strings.EntityClient_ProviderSpecificError("EnlistTransaction"), ex);
			}
			throw;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ClearTransactions();
			if (_storeConnection != null)
			{
				if (_entityConnectionOwnsStoreConnection)
				{
					StoreCloseHelper();
				}
				UnsubscribeFromStoreConnectionStateChangeEvents();
				if (_entityConnectionOwnsStoreConnection)
				{
					DbInterception.Dispatch.Connection.Dispose(_storeConnection, InterceptionContext);
				}
				_storeConnection = null;
			}
			_entityClientConnectionState = ConnectionState.Closed;
			ChangeConnectionString(string.Empty);
		}
		base.Dispose(disposing);
	}

	internal virtual void ClearCurrentTransaction()
	{
		_currentTransaction = null;
	}

	private void ChangeConnectionString(string newConnectionString)
	{
		DbConnectionOptions dbConnectionOptions = _emptyConnectionOptions;
		if (!string.IsNullOrEmpty(newConnectionString))
		{
			dbConnectionOptions = new DbConnectionOptions(newConnectionString, EntityConnectionStringBuilder.ValidKeywords);
		}
		DbProviderFactory dbProviderFactory = null;
		DbConnection dbConnection = null;
		DbConnectionOptions dbConnectionOptions2 = dbConnectionOptions;
		if (!dbConnectionOptions.IsEmpty)
		{
			string text = dbConnectionOptions["name"];
			if (!string.IsNullOrEmpty(text))
			{
				if (1 < dbConnectionOptions.Parsetable.Count)
				{
					throw new ArgumentException(Strings.EntityClient_ExtraParametersWithNamedConnection);
				}
				ConnectionStringSettings val = ConfigurationManager.ConnectionStrings[text];
				if (val == null || val.ProviderName != "System.Data.EntityClient")
				{
					throw new ArgumentException(Strings.EntityClient_InvalidNamedConnection);
				}
				dbConnectionOptions2 = new DbConnectionOptions(val.ConnectionString, EntityConnectionStringBuilder.ValidKeywords);
				if (!string.IsNullOrEmpty(dbConnectionOptions2["name"]))
				{
					throw new ArgumentException(Strings.EntityClient_NestedNamedConnection(text));
				}
			}
			ValidateValueForTheKeyword(dbConnectionOptions2, "metadata");
			string key = ValidateValueForTheKeyword(dbConnectionOptions2, "provider");
			dbProviderFactory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(key);
			dbConnection = GetStoreConnection(dbProviderFactory);
			try
			{
				string text2 = dbConnectionOptions2["provider connection string"];
				if (text2 != null)
				{
					DbInterception.Dispatch.Connection.SetConnectionString(dbConnection, new DbConnectionPropertyInterceptionContext<string>(InterceptionContext).WithValue(text2));
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityException(Strings.EntityClient_ProviderSpecificError("ConnectionString"), ex);
				}
				throw;
			}
		}
		lock (_connectionStringLock)
		{
			_providerFactory = dbProviderFactory;
			_metadataWorkspace = null;
			ClearTransactions();
			UnsubscribeFromStoreConnectionStateChangeEvents();
			_storeConnection = dbConnection;
			SubscribeToStoreConnectionStateChangeEvents();
			_userConnectionOptions = dbConnectionOptions;
			_effectiveConnectionOptions = dbConnectionOptions2;
		}
	}

	private static string ValidateValueForTheKeyword(DbConnectionOptions effectiveConnectionOptions, string keywordName)
	{
		string text = effectiveConnectionOptions[keywordName];
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Trim();
		}
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentException(Strings.EntityClient_ConnectionStringMissingInfo(keywordName));
		}
		return text;
	}

	private void ClearTransactions()
	{
		ClearCurrentTransaction();
		ClearEnlistedTransaction();
	}

	private void ClearEnlistedTransaction()
	{
		if (EnlistedInUserTransaction)
		{
			_enlistedTransaction.TransactionCompleted -= EnlistedTransactionCompleted;
		}
		_enlistedTransaction = null;
	}

	private void EnlistedTransactionCompleted(object sender, TransactionEventArgs e)
	{
		e.Transaction.TransactionCompleted -= EnlistedTransactionCompleted;
	}

	private void StoreCloseHelper()
	{
		try
		{
			if (_storeConnection != null && DbInterception.Dispatch.Connection.GetState(_storeConnection, InterceptionContext) != 0)
			{
				DbInterception.Dispatch.Connection.Close(_storeConnection, InterceptionContext);
			}
			ClearTransactions();
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityException(Strings.EntityClient_ErrorInClosingConnection, ex);
			}
			throw;
		}
	}

	private static DbConnection GetStoreConnection(DbProviderFactory factory)
	{
		return factory.CreateConnection() ?? throw new ProviderIncompatibleException(Strings.EntityClient_ReturnedNullOnProviderMethod("CreateConnection", factory.GetType().Name));
	}
}
