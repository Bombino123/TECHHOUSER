using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal;

internal class LazyInternalContext : InternalContext
{
	private static readonly CreateDatabaseIfNotExists<DbContext> _defaultCodeFirstInitializer = new CreateDatabaseIfNotExists<DbContext>();

	private static readonly ConcurrentDictionary<IDbModelCacheKey, RetryLazy<LazyInternalContext, DbCompiledModel>> _cachedModels = new ConcurrentDictionary<IDbModelCacheKey, RetryLazy<LazyInternalContext, DbCompiledModel>>();

	private static readonly ConcurrentDictionary<Tuple<DbCompiledModel, string>, RetryAction<InternalContext>> InitializedDatabases = new ConcurrentDictionary<Tuple<DbCompiledModel, string>, RetryAction<InternalContext>>();

	private IInternalConnection _internalConnection;

	private bool _creatingModel;

	private ObjectContext _objectContext;

	private DbCompiledModel _model;

	private readonly bool _createdWithExistingModel;

	private bool _initialEnsureTransactionsForFunctionsAndCommands = true;

	private bool _initialLazyLoadingFlag = true;

	private bool _initialProxyCreationFlag = true;

	private bool _useDatabaseNullSemanticsFlag;

	private int? _commandTimeout;

	private bool _inDatabaseInitialization;

	private Action<DbModelBuilder> _onModelCreating;

	private readonly Func<DbContext, IDbModelCacheKey> _cacheKeyFactory;

	private readonly AttributeProvider _attributeProvider;

	private DbModel _modelBeingInitialized;

	private DbProviderInfo _modelProviderInfo;

	private bool _disableFilterOverProjectionSimplificationForCustomFunctions;

	public override ObjectContext ObjectContext
	{
		get
		{
			Initialize();
			return ObjectContextInUse;
		}
	}

	public override DbCompiledModel CodeFirstModel
	{
		get
		{
			InitializeContext();
			return _model;
		}
	}

	public override DbModel ModelBeingInitialized
	{
		get
		{
			InitializeContext();
			return _modelBeingInitialized;
		}
	}

	public virtual ObjectContext ObjectContextInUse => base.TempObjectContext ?? _objectContext;

	public override DbConnection Connection
	{
		get
		{
			CheckContextNotDisposed();
			if (base.TempObjectContext != null)
			{
				return ((EntityConnection)base.TempObjectContext.Connection).StoreConnection;
			}
			return _internalConnection.Connection;
		}
	}

	public override string OriginalConnectionString => _internalConnection.OriginalConnectionString;

	public override DbConnectionStringOrigin ConnectionStringOrigin
	{
		get
		{
			CheckContextNotDisposed();
			return _internalConnection.ConnectionStringOrigin;
		}
	}

	public override AppConfig AppConfig
	{
		get
		{
			return base.AppConfig;
		}
		set
		{
			base.AppConfig = value;
			_internalConnection.AppConfig = value;
		}
	}

	public override string ConnectionStringName
	{
		get
		{
			CheckContextNotDisposed();
			return _internalConnection.ConnectionStringName;
		}
	}

	public override DbProviderInfo ModelProviderInfo
	{
		get
		{
			CheckContextNotDisposed();
			return _modelProviderInfo;
		}
		set
		{
			CheckContextNotDisposed();
			_modelProviderInfo = value;
			_internalConnection.ProviderName = _modelProviderInfo.ProviderInvariantName;
		}
	}

	public override string ProviderName => _internalConnection.ProviderName;

	public override Action<DbModelBuilder> OnModelCreating
	{
		get
		{
			CheckContextNotDisposed();
			return _onModelCreating;
		}
		set
		{
			CheckContextNotDisposed();
			_onModelCreating = value;
		}
	}

	public override IDatabaseInitializer<DbContext> DefaultInitializer
	{
		get
		{
			if (_model == null)
			{
				return null;
			}
			return _defaultCodeFirstInitializer;
		}
	}

	public override bool EnsureTransactionsForFunctionsAndCommands
	{
		get
		{
			return ObjectContextInUse?.ContextOptions.EnsureTransactionsForFunctionsAndCommands ?? _initialEnsureTransactionsForFunctionsAndCommands;
		}
		set
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse != null)
			{
				objectContextInUse.ContextOptions.EnsureTransactionsForFunctionsAndCommands = value;
			}
			else
			{
				_initialEnsureTransactionsForFunctionsAndCommands = value;
			}
		}
	}

	public override bool LazyLoadingEnabled
	{
		get
		{
			return ObjectContextInUse?.ContextOptions.LazyLoadingEnabled ?? _initialLazyLoadingFlag;
		}
		set
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse != null)
			{
				objectContextInUse.ContextOptions.LazyLoadingEnabled = value;
			}
			else
			{
				_initialLazyLoadingFlag = value;
			}
		}
	}

	public override bool ProxyCreationEnabled
	{
		get
		{
			return ObjectContextInUse?.ContextOptions.ProxyCreationEnabled ?? _initialProxyCreationFlag;
		}
		set
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse != null)
			{
				objectContextInUse.ContextOptions.ProxyCreationEnabled = value;
			}
			else
			{
				_initialProxyCreationFlag = value;
			}
		}
	}

	public override bool UseDatabaseNullSemantics
	{
		get
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse == null)
			{
				return _useDatabaseNullSemanticsFlag;
			}
			return !objectContextInUse.ContextOptions.UseCSharpNullComparisonBehavior;
		}
		set
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse != null)
			{
				objectContextInUse.ContextOptions.UseCSharpNullComparisonBehavior = !value;
			}
			else
			{
				_useDatabaseNullSemanticsFlag = value;
			}
		}
	}

	public override bool DisableFilterOverProjectionSimplificationForCustomFunctions
	{
		get
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse == null)
			{
				return _disableFilterOverProjectionSimplificationForCustomFunctions;
			}
			return !objectContextInUse.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions;
		}
		set
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse != null)
			{
				objectContextInUse.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions = !value;
			}
			else
			{
				_disableFilterOverProjectionSimplificationForCustomFunctions = value;
			}
		}
	}

	public override int? CommandTimeout
	{
		get
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse == null)
			{
				return _commandTimeout;
			}
			return objectContextInUse.CommandTimeout;
		}
		set
		{
			ObjectContext objectContextInUse = ObjectContextInUse;
			if (objectContextInUse != null)
			{
				objectContextInUse.CommandTimeout = value;
			}
			else
			{
				_commandTimeout = value;
			}
		}
	}

	public override string DefaultSchema => CodeFirstModel.DefaultSchema;

	public LazyInternalContext(DbContext owner, IInternalConnection internalConnection, DbCompiledModel model, Func<DbContext, IDbModelCacheKey> cacheKeyFactory = null, AttributeProvider attributeProvider = null, Lazy<DbDispatchers> dispatchers = null, ObjectContext objectContext = null)
		: base(owner, dispatchers)
	{
		_internalConnection = internalConnection;
		_model = model;
		_cacheKeyFactory = cacheKeyFactory ?? new Func<DbContext, IDbModelCacheKey>(new DefaultModelCacheKeyFactory().Create);
		_attributeProvider = attributeProvider ?? new AttributeProvider();
		_objectContext = objectContext;
		_createdWithExistingModel = model != null;
		LoadContextConfigs();
	}

	public override ObjectContext GetObjectContextWithoutDatabaseInitialization()
	{
		InitializeContext();
		return ObjectContextInUse;
	}

	public override int SaveChanges()
	{
		if (ObjectContextInUse != null)
		{
			return base.SaveChanges();
		}
		return 0;
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (ObjectContextInUse != null)
		{
			return base.SaveChangesAsync(cancellationToken);
		}
		return Task.FromResult(0);
	}

	public override void DisposeContext(bool disposing)
	{
		if (base.IsDisposed)
		{
			return;
		}
		base.DisposeContext(disposing);
		if (disposing)
		{
			if (_objectContext != null)
			{
				_objectContext.Dispose();
			}
			_internalConnection.Dispose();
		}
	}

	public override void OverrideConnection(IInternalConnection connection)
	{
		connection.AppConfig = AppConfig;
		if (connection.ConnectionHasModel != _internalConnection.ConnectionHasModel)
		{
			throw _internalConnection.ConnectionHasModel ? Error.LazyInternalContext_CannotReplaceEfConnectionWithDbConnection() : Error.LazyInternalContext_CannotReplaceDbConnectionWithEfConnection();
		}
		_internalConnection.Dispose();
		_internalConnection = connection;
	}

	protected override void InitializeContext()
	{
		CheckContextNotDisposed();
		if (_objectContext != null)
		{
			return;
		}
		if (_creatingModel)
		{
			throw Error.DbContext_ContextUsedInModelCreating();
		}
		try
		{
			DbContextInfo currentInfo = DbContextInfo.CurrentInfo;
			if (currentInfo != null)
			{
				ApplyContextInfo(currentInfo);
			}
			_creatingModel = true;
			if (_createdWithExistingModel)
			{
				if (_internalConnection.ConnectionHasModel)
				{
					throw Error.DbContext_ConnectionHasModel();
				}
				_objectContext = _model.CreateObjectContext<ObjectContext>(_internalConnection.Connection);
			}
			else if (_internalConnection.ConnectionHasModel)
			{
				_objectContext = _internalConnection.CreateObjectContextFromConnectionModel();
			}
			else
			{
				IDbModelCacheKey key = _cacheKeyFactory(base.Owner);
				DbCompiledModel value = _cachedModels.GetOrAdd(key, (IDbModelCacheKey t) => new RetryLazy<LazyInternalContext, DbCompiledModel>(CreateModel)).GetValue(this);
				_objectContext = value.CreateObjectContext<ObjectContext>(_internalConnection.Connection);
				_model = value;
			}
			_objectContext.ContextOptions.EnsureTransactionsForFunctionsAndCommands = _initialEnsureTransactionsForFunctionsAndCommands;
			_objectContext.ContextOptions.LazyLoadingEnabled = _initialLazyLoadingFlag;
			_objectContext.ContextOptions.ProxyCreationEnabled = _initialProxyCreationFlag;
			_objectContext.ContextOptions.UseCSharpNullComparisonBehavior = !_useDatabaseNullSemanticsFlag;
			_objectContext.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions = _disableFilterOverProjectionSimplificationForCustomFunctions;
			_objectContext.CommandTimeout = _commandTimeout;
			_objectContext.ContextOptions.UseConsistentNullReferenceBehavior = true;
			_objectContext.InterceptionContext = _objectContext.InterceptionContext.WithDbContext(base.Owner);
			ResetDbSets();
			_objectContext.InitializeMappingViewCacheFactory(base.Owner);
		}
		finally
		{
			_creatingModel = false;
		}
	}

	public static DbCompiledModel CreateModel(LazyInternalContext internalContext)
	{
		Type type = internalContext.Owner.GetType();
		DbModelStore dbModelStore = null;
		if (!(internalContext.Owner is HistoryContext))
		{
			dbModelStore = DbConfiguration.DependencyResolver.GetService<DbModelStore>();
			if (dbModelStore != null)
			{
				DbCompiledModel dbCompiledModel = dbModelStore.TryLoad(type);
				if (dbCompiledModel != null)
				{
					return dbCompiledModel;
				}
			}
		}
		DbModelBuilder dbModelBuilder = internalContext.CreateModelBuilder();
		DbModel dbModel = (internalContext._modelBeingInitialized = ((internalContext._modelProviderInfo == null) ? dbModelBuilder.Build(internalContext._internalConnection.Connection) : dbModelBuilder.Build(internalContext._modelProviderInfo)));
		dbModelStore?.Save(type, dbModel);
		return dbModel.Compile();
	}

	public DbModelBuilder CreateModelBuilder()
	{
		DbModelBuilder dbModelBuilder = new DbModelBuilder(_attributeProvider.GetAttributes(base.Owner.GetType()).OfType<DbModelBuilderVersionAttribute>().FirstOrDefault()?.Version ?? DbModelBuilderVersion.Latest);
		string text = StripInvalidCharacters(base.Owner.GetType().Namespace);
		if (!string.IsNullOrWhiteSpace(text))
		{
			dbModelBuilder.Conventions.Add(new ModelNamespaceConvention(text));
		}
		string text2 = StripInvalidCharacters(base.Owner.GetType().Name);
		if (!string.IsNullOrWhiteSpace(text2))
		{
			dbModelBuilder.Conventions.Add(new ModelContainerConvention(text2));
		}
		new DbSetDiscoveryService(base.Owner).RegisterSets(dbModelBuilder);
		base.Owner.CallOnModelCreating(dbModelBuilder);
		if (OnModelCreating != null)
		{
			OnModelCreating(dbModelBuilder);
		}
		return dbModelBuilder;
	}

	private static string StripInvalidCharacters(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(value.Length);
		bool flag = true;
		foreach (char c in value)
		{
			if (c == '.')
			{
				if (!flag)
				{
					stringBuilder.Append(c);
				}
				continue;
			}
			switch (char.GetUnicodeCategory(c))
			{
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.LetterNumber:
				flag = false;
				stringBuilder.Append(c);
				break;
			case UnicodeCategory.NonSpacingMark:
			case UnicodeCategory.SpacingCombiningMark:
			case UnicodeCategory.DecimalDigitNumber:
			case UnicodeCategory.ConnectorPunctuation:
				if (!flag)
				{
					stringBuilder.Append(c);
				}
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public override void MarkDatabaseNotInitialized()
	{
		if (!base.InInitializationAction)
		{
			InitializedDatabases.TryRemove(Tuple.Create(_model, _internalConnection.ConnectionKey), out var _);
		}
	}

	public override void MarkDatabaseInitialized()
	{
		InitializeContext();
		InitializeDatabaseAction(delegate
		{
		});
	}

	protected override void InitializeDatabase()
	{
		InitializeDatabaseAction(delegate(InternalContext c)
		{
			c.PerformDatabaseInitialization();
		});
	}

	private void InitializeDatabaseAction(Action<InternalContext> action)
	{
		if (_inDatabaseInitialization || base.InitializerDisabled)
		{
			return;
		}
		try
		{
			_inDatabaseInitialization = true;
			InitializedDatabases.GetOrAdd(Tuple.Create(_model, _internalConnection.ConnectionKey), (Tuple<DbCompiledModel, string> t) => new RetryAction<InternalContext>(action)).PerformAction(this);
		}
		finally
		{
			_inDatabaseInitialization = false;
			_modelBeingInitialized = null;
		}
	}
}
