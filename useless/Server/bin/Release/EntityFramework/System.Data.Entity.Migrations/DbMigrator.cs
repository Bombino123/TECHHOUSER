using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Edm;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Resources;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations;

public class DbMigrator : MigratorBase
{
	public const string InitialDatabase = "0";

	private const string DefaultSchemaResourceKey = "DefaultSchema";

	private readonly Lazy<XDocument> _emptyModel;

	private readonly DbMigrationsConfiguration _configuration;

	private readonly XDocument _currentModel;

	private readonly DbProviderFactory _providerFactory;

	private readonly HistoryRepository _historyRepository;

	private readonly MigrationAssembly _migrationAssembly;

	private readonly DbContextInfo _usersContextInfo;

	private readonly EdmModelDiffer _modelDiffer;

	private readonly Lazy<ModificationCommandTreeGenerator> _modificationCommandTreeGenerator;

	private readonly DbContext _usersContext;

	private readonly Func<DbConnection, string, HistoryContext> _historyContextFactory;

	private readonly DbConnection _connection;

	private readonly bool _calledByCreateDatabase;

	private readonly DatabaseExistenceState _existenceState;

	private readonly string _providerManifestToken;

	private readonly string _targetDatabase;

	private readonly string _legacyContextKey;

	private readonly string _defaultSchema;

	private MigrationSqlGenerator _sqlGenerator;

	private bool _emptyMigrationNeeded;

	private bool _committedStatements;

	public override DbMigrationsConfiguration Configuration => _configuration;

	internal override string TargetDatabase => _targetDatabase;

	private MigrationSqlGenerator SqlGenerator => _sqlGenerator ?? (_sqlGenerator = _configuration.GetSqlGenerator(_usersContextInfo.ConnectionProviderName));

	internal DbMigrator(DbContext usersContext = null, DbProviderFactory providerFactory = null, MigrationAssembly migrationAssembly = null)
		: base(null)
	{
		_usersContext = usersContext;
		_providerFactory = providerFactory;
		_migrationAssembly = migrationAssembly;
		_usersContextInfo = new DbContextInfo(typeof(DbContext));
		_configuration = new DbMigrationsConfiguration();
		_calledByCreateDatabase = true;
	}

	public DbMigrator(DbMigrationsConfiguration configuration)
		: this(configuration, null, DatabaseExistenceState.Unknown, calledByCreateDatabase: false)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(configuration.ContextType, "configuration.ContextType");
	}

	public DbMigrator(DbMigrationsConfiguration configuration, DbContext context)
		: this(configuration, context, DatabaseExistenceState.Unknown, calledByCreateDatabase: false)
	{
	}

	internal DbMigrator(DbMigrationsConfiguration configuration, DbContext usersContext, DatabaseExistenceState existenceState, bool calledByCreateDatabase)
		: base(null)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(configuration.ContextType, "configuration.ContextType");
		_configuration = configuration;
		_calledByCreateDatabase = calledByCreateDatabase;
		_existenceState = existenceState;
		if (usersContext != null)
		{
			_usersContextInfo = new DbContextInfo(usersContext);
		}
		else
		{
			_usersContextInfo = ((configuration.TargetDatabase == null) ? new DbContextInfo(configuration.ContextType) : new DbContextInfo(configuration.ContextType, configuration.TargetDatabase));
			if (!_usersContextInfo.IsConstructible)
			{
				throw Error.ContextNotConstructible(configuration.ContextType);
			}
		}
		_modelDiffer = _configuration.ModelDiffer;
		DbContext dbContext = (_usersContext = usersContext ?? _usersContextInfo.CreateInstance());
		try
		{
			_migrationAssembly = new MigrationAssembly(_configuration.MigrationsAssembly, _configuration.MigrationsNamespace);
			_currentModel = dbContext.GetModel();
			_connection = dbContext.Database.Connection;
			_providerFactory = DbProviderServices.GetProviderFactory(_connection);
			_defaultSchema = dbContext.InternalContext.DefaultSchema ?? "dbo";
			_historyContextFactory = _configuration.GetHistoryContextFactory(_usersContextInfo.ConnectionProviderName);
			_historyRepository = new HistoryRepository(dbContext.InternalContext, _usersContextInfo.ConnectionString, _providerFactory, _configuration.ContextKey, _configuration.CommandTimeout, _historyContextFactory, new string[1] { _defaultSchema }.Concat(GetHistorySchemas()), _usersContext, _existenceState, (Exception e) => SqlGenerator.IsPermissionDeniedError(e));
			_providerManifestToken = ((dbContext.InternalContext.ModelProviderInfo != null) ? dbContext.InternalContext.ModelProviderInfo.ProviderManifestToken : DbConfiguration.DependencyResolver.GetService<IManifestTokenResolver>().ResolveManifestToken(_connection));
			DbModelBuilder modelBuilder = dbContext.InternalContext.CodeFirstModel.CachedModelBuilder;
			_modificationCommandTreeGenerator = new Lazy<ModificationCommandTreeGenerator>(() => new ModificationCommandTreeGenerator(modelBuilder.BuildDynamicUpdateModel(new DbProviderInfo(_usersContextInfo.ConnectionProviderName, _providerManifestToken)), CreateConnection()));
			DbInterceptionContext dbInterceptionContext = new DbInterceptionContext();
			dbInterceptionContext = dbInterceptionContext.WithDbContext(_usersContext);
			_targetDatabase = Strings.LoggingTargetDatabaseFormat(DbInterception.Dispatch.Connection.GetDataSource(_connection, dbInterceptionContext), DbInterception.Dispatch.Connection.GetDatabase(_connection, dbInterceptionContext), _usersContextInfo.ConnectionProviderName, (_usersContextInfo.ConnectionStringOrigin == DbConnectionStringOrigin.DbContextInfo) ? Strings.LoggingExplicit : _usersContextInfo.ConnectionStringOrigin.ToString());
			_legacyContextKey = dbContext.InternalContext.DefaultContextKey;
			_emptyModel = GetEmptyModel();
		}
		finally
		{
			if (usersContext == null)
			{
				_usersContext = null;
				_connection = null;
				dbContext.Dispose();
			}
		}
	}

	private Lazy<XDocument> GetEmptyModel()
	{
		return new Lazy<XDocument>((Func<XDocument>)(() => new DbModelBuilder().Build(new DbProviderInfo(_usersContextInfo.ConnectionProviderName, _providerManifestToken)).GetModel()));
	}

	private XDocument GetHistoryModel(string defaultSchema)
	{
		DbConnection dbConnection = null;
		try
		{
			dbConnection = CreateConnection();
			using HistoryContext context = _historyContextFactory(dbConnection, defaultSchema);
			return context.GetModel();
		}
		finally
		{
			if (dbConnection != null)
			{
				DbInterception.Dispatch.Connection.Dispose(dbConnection, new DbInterceptionContext());
			}
		}
	}

	private IEnumerable<string> GetHistorySchemas()
	{
		return from migrationId in _migrationAssembly.MigrationIds
			let migration = _migrationAssembly.GetMigration(migrationId)
			select GetDefaultSchema(migration);
	}

	public override IEnumerable<string> GetLocalMigrations()
	{
		return _migrationAssembly.MigrationIds;
	}

	public override IEnumerable<string> GetDatabaseMigrations()
	{
		return _historyRepository.GetMigrationsSince("0");
	}

	public override IEnumerable<string> GetPendingMigrations()
	{
		return _historyRepository.GetPendingMigrations(_migrationAssembly.MigrationIds);
	}

	internal ScaffoldedMigration ScaffoldInitialCreate(string @namespace)
	{
		string migrationId;
		string productVersion;
		XDocument lastModel = _historyRepository.GetLastModel(out migrationId, out productVersion, _legacyContextKey);
		if (lastModel == null || !migrationId.MigrationName().Equals(Strings.InitialCreate))
		{
			return null;
		}
		List<MigrationOperation> operations = _modelDiffer.Diff(_emptyModel.Value, lastModel, _modificationCommandTreeGenerator, SqlGenerator).ToList();
		ScaffoldedMigration scaffoldedMigration = _configuration.CodeGenerator.Generate(migrationId, operations, null, Convert.ToBase64String(new ModelCompressor().Compress(_currentModel)), @namespace, Strings.InitialCreate);
		scaffoldedMigration.MigrationId = migrationId;
		scaffoldedMigration.Directory = _configuration.MigrationsDirectory;
		scaffoldedMigration.Resources.Add("DefaultSchema", _defaultSchema);
		return scaffoldedMigration;
	}

	internal ScaffoldedMigration Scaffold(string migrationName, string @namespace, bool ignoreChanges)
	{
		string migrationId = null;
		bool flag = false;
		List<string> list = GetPendingMigrations().ToList();
		if (list.Any())
		{
			string text = list.Last();
			if (!text.EqualsIgnoreCase(migrationName) && !text.MigrationName().EqualsIgnoreCase(migrationName))
			{
				throw Error.MigrationsPendingException(list.Join());
			}
			flag = true;
			migrationId = text;
			migrationName = text.MigrationName();
		}
		XDocument sourceModel = null;
		CheckLegacyCompatibility(delegate
		{
			sourceModel = _currentModel;
		});
		string migrationId2 = null;
		string productVersion = null;
		sourceModel = sourceModel ?? _historyRepository.GetLastModel(out migrationId2, out productVersion) ?? _emptyModel.Value;
		IEnumerable<MigrationOperation> enumerable2;
		if (!ignoreChanges)
		{
			IEnumerable<MigrationOperation> enumerable = _modelDiffer.Diff(sourceModel, _currentModel, _modificationCommandTreeGenerator, SqlGenerator, productVersion).ToList();
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = Enumerable.Empty<MigrationOperation>();
		}
		IEnumerable<MigrationOperation> operations = enumerable2;
		if (!flag)
		{
			migrationName = _migrationAssembly.UniquifyName(migrationName);
			migrationId = MigrationAssembly.CreateMigrationId(migrationName);
		}
		ModelCompressor modelCompressor = new ModelCompressor();
		ScaffoldedMigration scaffoldedMigration = _configuration.CodeGenerator.Generate(migrationId, operations, (sourceModel == _emptyModel.Value || sourceModel == _currentModel || !migrationId2.IsAutomaticMigration()) ? null : Convert.ToBase64String(modelCompressor.Compress(sourceModel)), Convert.ToBase64String(modelCompressor.Compress(_currentModel)), @namespace, migrationName);
		scaffoldedMigration.MigrationId = migrationId;
		scaffoldedMigration.Directory = _configuration.MigrationsDirectory;
		scaffoldedMigration.IsRescaffold = flag;
		scaffoldedMigration.Resources.Add("DefaultSchema", _defaultSchema);
		return scaffoldedMigration;
	}

	private void CheckLegacyCompatibility(Action onCompatible)
	{
		if (_calledByCreateDatabase || _historyRepository.Exists())
		{
			return;
		}
		DbContext dbContext = _usersContext ?? _usersContextInfo.CreateInstance();
		try
		{
			bool flag;
			try
			{
				flag = dbContext.Database.CompatibleWithModel(throwIfNoMetadata: true);
			}
			catch
			{
				return;
			}
			if (!flag)
			{
				throw Error.MetadataOutOfDate();
			}
			onCompatible();
		}
		finally
		{
			if (_usersContext == null)
			{
				dbContext.Dispose();
			}
		}
	}

	public override void Update(string targetMigration)
	{
		base.EnsureDatabaseExists(delegate
		{
			UpdateInternal(targetMigration);
		});
	}

	private void UpdateInternal(string targetMigration)
	{
		IEnumerable<MigrationOperation> upgradeOperations = _historyRepository.GetUpgradeOperations();
		if (upgradeOperations.Any())
		{
			base.UpgradeHistory(upgradeOperations);
		}
		IEnumerable<string> enumerable = GetPendingMigrations();
		if (!enumerable.Any())
		{
			CheckLegacyCompatibility(delegate
			{
				ExecuteOperations(MigrationAssembly.CreateBootstrapMigrationId(), new VersionedModel(_currentModel), Enumerable.Empty<MigrationOperation>(), _modelDiffer.Diff(_emptyModel.Value, GetHistoryModel(_defaultSchema), _modificationCommandTreeGenerator, SqlGenerator), downgrading: false);
			});
		}
		string targetMigrationId = targetMigration;
		if (!string.IsNullOrWhiteSpace(targetMigrationId))
		{
			if (!targetMigrationId.IsValidMigrationId())
			{
				if (targetMigrationId == Strings.AutomaticMigration)
				{
					throw Error.AutoNotValidTarget(Strings.AutomaticMigration);
				}
				targetMigrationId = GetMigrationId(targetMigration);
			}
			if (enumerable.Any((string m) => m.EqualsIgnoreCase(targetMigrationId)))
			{
				enumerable = enumerable.Where((string m) => string.CompareOrdinal(m.ToLowerInvariant(), targetMigrationId.ToLowerInvariant()) <= 0);
			}
			else
			{
				enumerable = _historyRepository.GetMigrationsSince(targetMigrationId);
				if (enumerable.Any())
				{
					base.Downgrade(enumerable.Concat(new string[1] { targetMigrationId }));
					return;
				}
			}
		}
		base.Upgrade(enumerable, targetMigrationId, null);
	}

	internal override void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
	{
		IEnumerable<MigrationStatement> migrationStatements = SqlGenerator.Generate(upgradeOperations, _providerManifestToken);
		base.ExecuteStatements(migrationStatements);
	}

	internal override string GetMigrationId(string migration)
	{
		if (migration.IsValidMigrationId())
		{
			return migration;
		}
		string? obj = GetPendingMigrations().SingleOrDefault((string m) => m.MigrationName().EqualsIgnoreCase(migration)) ?? _historyRepository.GetMigrationId(migration);
		if (obj == null)
		{
			throw Error.MigrationNotFound(migration);
		}
		return obj;
	}

	internal override void Upgrade(IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
	{
		DbMigration lastMigration = null;
		if (lastMigrationId != null)
		{
			lastMigration = _migrationAssembly.GetMigration(lastMigrationId);
		}
		foreach (string pendingMigration in pendingMigrations)
		{
			DbMigration migration = _migrationAssembly.GetMigration(pendingMigration);
			base.ApplyMigration(migration, lastMigration);
			lastMigration = migration;
			_emptyMigrationNeeded = false;
			if (pendingMigration.EqualsIgnoreCase(targetMigrationId))
			{
				break;
			}
		}
		if (string.IsNullOrWhiteSpace(targetMigrationId) && ((_emptyMigrationNeeded && _configuration.AutomaticMigrationsEnabled) || IsModelOutOfDate(_currentModel, lastMigration)))
		{
			if (!_configuration.AutomaticMigrationsEnabled)
			{
				throw Error.AutomaticDisabledException();
			}
			base.AutoMigrate(MigrationAssembly.CreateMigrationId(_calledByCreateDatabase ? Strings.InitialCreate : Strings.AutomaticMigration), _calledByCreateDatabase ? new VersionedModel(_emptyModel.Value) : GetLastModel(lastMigration), new VersionedModel(_currentModel), downgrading: false);
		}
		if (!_calledByCreateDatabase && !IsModelOutOfDate(_currentModel, lastMigration))
		{
			base.SeedDatabase();
		}
	}

	internal override void SeedDatabase()
	{
		DbContext dbContext = _usersContext ?? _usersContextInfo.CreateInstance();
		if (_usersContext != null)
		{
			dbContext.InternalContext.UseTempObjectContext();
		}
		try
		{
			_configuration.OnSeed(dbContext);
			dbContext.SaveChanges();
		}
		finally
		{
			if (_usersContext == null)
			{
				dbContext.Dispose();
			}
			else
			{
				dbContext.InternalContext.DisposeTempObjectContext();
			}
		}
	}

	internal virtual bool IsModelOutOfDate(XDocument model, DbMigration lastMigration)
	{
		VersionedModel lastModel = GetLastModel(lastMigration);
		return _modelDiffer.Diff(lastModel.Model, model, null, null, lastModel.Version).Any();
	}

	private VersionedModel GetLastModel(DbMigration lastMigration, string currentMigrationId = null)
	{
		if (lastMigration != null)
		{
			return lastMigration.GetTargetModel();
		}
		string migrationId;
		string productVersion;
		XDocument lastModel = _historyRepository.GetLastModel(out migrationId, out productVersion);
		if (lastModel != null && (currentMigrationId == null || string.CompareOrdinal(migrationId, currentMigrationId) < 0))
		{
			return new VersionedModel(lastModel, productVersion);
		}
		return new VersionedModel(_emptyModel.Value);
	}

	internal override void Downgrade(IEnumerable<string> pendingMigrations)
	{
		for (int i = 0; i < pendingMigrations.Count() - 1; i++)
		{
			string migrationId = pendingMigrations.ElementAt(i);
			DbMigration migration = _migrationAssembly.GetMigration(migrationId);
			string text = pendingMigrations.ElementAt(i + 1);
			string productVersion = null;
			XDocument val = ((text != "0") ? _historyRepository.GetModel(text, out productVersion) : _emptyModel.Value);
			string productVersion2;
			XDocument model = _historyRepository.GetModel(migrationId, out productVersion2);
			if (migration == null)
			{
				base.AutoMigrate(migrationId, new VersionedModel(model), new VersionedModel(val, productVersion), downgrading: true);
			}
			else
			{
				base.RevertMigration(migrationId, migration, val);
			}
		}
	}

	internal override void RevertMigration(string migrationId, DbMigration migration, XDocument targetModel)
	{
		IEnumerable<MigrationOperation> systemOperations = Enumerable.Empty<MigrationOperation>();
		string defaultSchema = GetDefaultSchema(migration);
		XDocument historyModel = GetHistoryModel(defaultSchema);
		if (targetModel == _emptyModel.Value && !_historyRepository.IsShared())
		{
			systemOperations = _modelDiffer.Diff(historyModel, _emptyModel.Value);
		}
		else
		{
			string lastDefaultSchema = GetLastDefaultSchema(migrationId);
			if (!string.Equals(lastDefaultSchema, defaultSchema, StringComparison.Ordinal))
			{
				XDocument historyModel2 = GetHistoryModel(lastDefaultSchema);
				systemOperations = _modelDiffer.Diff(historyModel, historyModel2);
			}
		}
		migration.Down();
		ExecuteOperations(migrationId, new VersionedModel(targetModel), migration.Operations, systemOperations, downgrading: true);
	}

	internal override void ApplyMigration(DbMigration migration, DbMigration lastMigration)
	{
		IMigrationMetadata migrationMetadata = (IMigrationMetadata)migration;
		VersionedModel versionedModel = GetLastModel(lastMigration, migrationMetadata.Id);
		VersionedModel sourceModel = migration.GetSourceModel();
		VersionedModel targetModel = migration.GetTargetModel();
		if (sourceModel != null && IsModelOutOfDate(sourceModel.Model, lastMigration))
		{
			base.AutoMigrate(migrationMetadata.Id.ToAutomaticMigrationId(), versionedModel, sourceModel, downgrading: false);
			versionedModel = sourceModel;
		}
		string defaultSchema = GetDefaultSchema(migration);
		XDocument historyModel = GetHistoryModel(defaultSchema);
		IEnumerable<MigrationOperation> systemOperations = Enumerable.Empty<MigrationOperation>();
		if (versionedModel.Model == _emptyModel.Value && !base.HistoryExists())
		{
			systemOperations = _modelDiffer.Diff(_emptyModel.Value, historyModel);
		}
		else
		{
			string lastDefaultSchema = GetLastDefaultSchema(migrationMetadata.Id);
			if (!string.Equals(lastDefaultSchema, defaultSchema, StringComparison.Ordinal))
			{
				XDocument historyModel2 = GetHistoryModel(lastDefaultSchema);
				systemOperations = _modelDiffer.Diff(historyModel2, historyModel);
			}
		}
		migration.Up();
		ExecuteOperations(migrationMetadata.Id, targetModel, migration.Operations, systemOperations, downgrading: false);
	}

	private static string GetDefaultSchema(DbMigration migration)
	{
		try
		{
			string @string = new ResourceManager(migration.GetType()).GetString("DefaultSchema");
			return (!string.IsNullOrWhiteSpace(@string)) ? @string : "dbo";
		}
		catch (MissingManifestResourceException)
		{
			return "dbo";
		}
	}

	private string GetLastDefaultSchema(string migrationId)
	{
		string text = _migrationAssembly.MigrationIds.LastOrDefault((string m) => string.CompareOrdinal(m, migrationId) < 0);
		if (text != null)
		{
			return GetDefaultSchema(_migrationAssembly.GetMigration(text));
		}
		return "dbo";
	}

	internal override bool HistoryExists()
	{
		return _historyRepository.Exists();
	}

	internal override void AutoMigrate(string migrationId, VersionedModel sourceModel, VersionedModel targetModel, bool downgrading)
	{
		IEnumerable<MigrationOperation> systemOperations = Enumerable.Empty<MigrationOperation>();
		if (!_historyRepository.IsShared())
		{
			if (targetModel.Model == _emptyModel.Value)
			{
				systemOperations = _modelDiffer.Diff(GetHistoryModel("dbo"), _emptyModel.Value);
			}
			else if (sourceModel.Model == _emptyModel.Value)
			{
				systemOperations = _modelDiffer.Diff(_emptyModel.Value, _calledByCreateDatabase ? GetHistoryModel(_defaultSchema) : GetHistoryModel("dbo"));
			}
		}
		List<MigrationOperation> list = _modelDiffer.Diff(sourceModel.Model, targetModel.Model, (targetModel.Model == _currentModel) ? _modificationCommandTreeGenerator : null, SqlGenerator, sourceModel.Version, targetModel.Version).ToList();
		if (!_calledByCreateDatabase && targetModel.Model == _currentModel && !string.Equals(GetLastDefaultSchema(migrationId), _defaultSchema, StringComparison.Ordinal))
		{
			throw Error.UnableToMoveHistoryTableWithAuto();
		}
		if (!_configuration.AutomaticMigrationDataLossAllowed && list.Any((MigrationOperation o) => o.IsDestructiveChange))
		{
			throw Error.AutomaticDataLoss();
		}
		if (targetModel.Model != _currentModel && list.Any((MigrationOperation o) => o is ProcedureOperation))
		{
			throw Error.AutomaticStaleFunctions(migrationId);
		}
		ExecuteOperations(migrationId, targetModel, list, systemOperations, downgrading, auto: true);
	}

	private void ExecuteOperations(string migrationId, VersionedModel targetModel, IEnumerable<MigrationOperation> operations, IEnumerable<MigrationOperation> systemOperations, bool downgrading, bool auto = false)
	{
		FillInForeignKeyOperations(operations, targetModel.Model);
		List<AddForeignKeyOperation> second = (from ct in operations.OfType<CreateTableOperation>()
			from afk in operations.OfType<AddForeignKeyOperation>()
			where ct.Name.EqualsIgnoreCase(afk.DependentTable)
			select afk).ToList();
		List<MigrationOperation> list = operations.Except(second).Concat(second).Concat(systemOperations)
			.ToList();
		CreateTableOperation createTableOperation = systemOperations.OfType<CreateTableOperation>().FirstOrDefault();
		if (createTableOperation != null)
		{
			_historyRepository.CurrentSchema = DatabaseName.Parse(createTableOperation.Name).Schema;
		}
		MoveTableOperation moveTableOperation = systemOperations.OfType<MoveTableOperation>().FirstOrDefault();
		if (moveTableOperation != null)
		{
			_historyRepository.CurrentSchema = moveTableOperation.NewSchema;
			moveTableOperation.ContextKey = _configuration.ContextKey;
			moveTableOperation.IsSystem = true;
		}
		if (!downgrading)
		{
			list.Add(_historyRepository.CreateInsertOperation(migrationId, targetModel));
		}
		else if (!systemOperations.Any((MigrationOperation o) => o is DropTableOperation))
		{
			list.Add(_historyRepository.CreateDeleteOperation(migrationId));
		}
		IEnumerable<MigrationStatement> enumerable = base.GenerateStatements(list, migrationId);
		if (auto)
		{
			enumerable = enumerable.Distinct((MigrationStatement m1, MigrationStatement m2) => string.Equals(m1.Sql, m2.Sql, StringComparison.Ordinal));
		}
		base.ExecuteStatements(enumerable);
		_historyRepository.ResetExists();
	}

	internal override IEnumerable<DbQueryCommandTree> CreateDiscoveryQueryTrees()
	{
		return _historyRepository.CreateDiscoveryQueryTrees();
	}

	internal override IEnumerable<MigrationStatement> GenerateStatements(IList<MigrationOperation> operations, string migrationId)
	{
		return SqlGenerator.Generate(operations, _providerManifestToken);
	}

	internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
	{
		ExecuteStatements(migrationStatements, null);
	}

	internal void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements, DbTransaction existingTransaction)
	{
		DbConnection connection = null;
		try
		{
			if (existingTransaction != null)
			{
				DbInterceptionContext dbInterceptionContext = new DbInterceptionContext();
				dbInterceptionContext = dbInterceptionContext.WithDbContext(_usersContext);
				ExecuteStatementsWithinTransaction(migrationStatements, existingTransaction, dbInterceptionContext);
			}
			else
			{
				connection = CreateConnection();
				DbProviderServices.GetExecutionStrategy(connection).Execute(delegate
				{
					ExecuteStatementsInternal(migrationStatements, connection);
				});
			}
		}
		finally
		{
			if (connection != null)
			{
				DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
			}
		}
	}

	private void ExecuteStatementsInternal(IEnumerable<MigrationStatement> migrationStatements, DbConnection connection)
	{
		DbContext dbContext = _usersContext ?? _usersContextInfo.CreateInstance();
		DbInterceptionContext dbInterceptionContext = new DbInterceptionContext();
		dbInterceptionContext = dbInterceptionContext.WithDbContext(dbContext);
		TransactionHandler transactionHandler = null;
		try
		{
			if (DbInterception.Dispatch.Connection.GetState(connection, dbInterceptionContext) == ConnectionState.Broken)
			{
				DbInterception.Dispatch.Connection.Close(connection, dbInterceptionContext);
			}
			if (DbInterception.Dispatch.Connection.GetState(connection, dbInterceptionContext) == ConnectionState.Closed)
			{
				DbInterception.Dispatch.Connection.Open(connection, dbInterceptionContext);
			}
			if (!(dbContext is TransactionContext))
			{
				string name = DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(DbProviderServices.GetProviderFactory(connection)).Name;
				string dataSource = DbInterception.Dispatch.Connection.GetDataSource(connection, dbInterceptionContext);
				Func<TransactionHandler> service = DbConfiguration.DependencyResolver.GetService<Func<TransactionHandler>>(new ExecutionStrategyKey(name, dataSource));
				if (service != null)
				{
					transactionHandler = service();
					transactionHandler.Initialize(dbContext, connection);
				}
			}
			ExecuteStatementsInternal(migrationStatements, connection, dbInterceptionContext);
			_committedStatements = true;
		}
		finally
		{
			transactionHandler?.Dispose();
			if (_usersContext == null)
			{
				dbContext.Dispose();
			}
		}
	}

	private void ExecuteStatementsInternal(IEnumerable<MigrationStatement> migrationStatements, DbConnection connection, DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		foreach (MigrationStatement migrationStatement in migrationStatements)
		{
			base.ExecuteSql(migrationStatement, connection, transaction, interceptionContext);
		}
	}

	private void ExecuteStatementsInternal(IEnumerable<MigrationStatement> migrationStatements, DbConnection connection, DbInterceptionContext interceptionContext)
	{
		List<MigrationStatement> list = new List<MigrationStatement>();
		foreach (MigrationStatement item in migrationStatements.Where((MigrationStatement s) => !string.IsNullOrEmpty(s.Sql)))
		{
			if (!item.SuppressTransaction)
			{
				list.Add(item);
				continue;
			}
			if (list.Any())
			{
				ExecuteStatementsWithinNewTransaction(list, connection, interceptionContext);
				list.Clear();
			}
			base.ExecuteSql(item, connection, null, interceptionContext);
		}
		if (list.Any())
		{
			ExecuteStatementsWithinNewTransaction(list, connection, interceptionContext);
		}
	}

	private void ExecuteStatementsWithinTransaction(IEnumerable<MigrationStatement> migrationStatements, DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		DbConnection connection = DbInterception.Dispatch.Transaction.GetConnection(transaction, interceptionContext);
		ExecuteStatementsInternal(migrationStatements, connection, transaction, interceptionContext);
	}

	private void ExecuteStatementsWithinNewTransaction(IEnumerable<MigrationStatement> migrationStatements, DbConnection connection, DbInterceptionContext interceptionContext)
	{
		BeginTransactionInterceptionContext interceptionContext2 = new BeginTransactionInterceptionContext(interceptionContext).WithIsolationLevel(IsolationLevel.Serializable);
		DbTransaction dbTransaction = null;
		try
		{
			dbTransaction = DbInterception.Dispatch.Connection.BeginTransaction(connection, interceptionContext2);
			ExecuteStatementsWithinTransaction(migrationStatements, dbTransaction, interceptionContext);
			DbInterception.Dispatch.Transaction.Commit(dbTransaction, interceptionContext);
		}
		finally
		{
			if (dbTransaction != null)
			{
				DbInterception.Dispatch.Transaction.Dispose(dbTransaction, interceptionContext);
			}
		}
	}

	internal override void ExecuteSql(MigrationStatement migrationStatement, DbConnection connection, DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		if (string.IsNullOrWhiteSpace(migrationStatement.Sql))
		{
			return;
		}
		DbCommand command = connection.CreateCommand();
		using InterceptableDbCommand interceptableDbCommand = ConfigureCommand(command, migrationStatement.Sql, interceptionContext);
		if (transaction != null)
		{
			interceptableDbCommand.Transaction = transaction;
		}
		interceptableDbCommand.ExecuteNonQuery();
	}

	private InterceptableDbCommand ConfigureCommand(DbCommand command, string commandText, DbInterceptionContext interceptionContext)
	{
		command.CommandText = commandText;
		if (_configuration.CommandTimeout.HasValue)
		{
			command.CommandTimeout = _configuration.CommandTimeout.Value;
		}
		return new InterceptableDbCommand(command, interceptionContext);
	}

	private void FillInForeignKeyOperations(IEnumerable<MigrationOperation> operations, XDocument targetModel)
	{
		foreach (AddForeignKeyOperation foreignKeyOperation in from fk in operations.OfType<AddForeignKeyOperation>()
			where fk.PrincipalTable != null && !fk.PrincipalColumns.Any()
			select fk)
		{
			string principalTable = GetStandardizedTableName(foreignKeyOperation.PrincipalTable);
			string entitySetName = (from es in ((XContainer)(object)targetModel).Descendants(EdmXNames.Ssdl.EntitySetNames)
				where new DatabaseName(es.TableAttribute(), es.SchemaAttribute()).ToString().EqualsIgnoreCase(principalTable)
				select es.NameAttribute()).SingleOrDefault();
			if (entitySetName != null)
			{
				((XContainer)(object)((XContainer)(object)targetModel).Descendants(EdmXNames.Ssdl.EntityTypeNames).Single((XElement et) => et.NameAttribute().EqualsIgnoreCase(entitySetName))).Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(delegate(XElement pr)
				{
					foreignKeyOperation.PrincipalColumns.Add(pr.NameAttribute());
				});
				continue;
			}
			CreateTableOperation createTableOperation = operations.OfType<CreateTableOperation>().SingleOrDefault((CreateTableOperation ct) => GetStandardizedTableName(ct.Name).EqualsIgnoreCase(principalTable));
			if (createTableOperation != null && createTableOperation.PrimaryKey != null)
			{
				createTableOperation.PrimaryKey.Columns.Each(delegate(string c)
				{
					foreignKeyOperation.PrincipalColumns.Add(c);
				});
				continue;
			}
			throw Error.PartialFkOperation(foreignKeyOperation.DependentTable, foreignKeyOperation.DependentColumns.Join());
		}
	}

	private string GetStandardizedTableName(string tableName)
	{
		if (!string.IsNullOrWhiteSpace(DatabaseName.Parse(tableName).Schema))
		{
			return tableName;
		}
		return new DatabaseName(tableName, _defaultSchema).ToString();
	}

	internal override void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
	{
		bool flag = false;
		System.Data.Entity.Migrations.Utilities.DatabaseCreator databaseCreator = new System.Data.Entity.Migrations.Utilities.DatabaseCreator(_configuration.CommandTimeout);
		DbConnection dbConnection = null;
		try
		{
			dbConnection = CreateConnection();
			if (_existenceState == DatabaseExistenceState.DoesNotExist || (_existenceState == DatabaseExistenceState.Unknown && !databaseCreator.Exists(dbConnection)))
			{
				databaseCreator.Create(dbConnection);
				flag = true;
			}
		}
		finally
		{
			if (dbConnection != null)
			{
				DbInterception.Dispatch.Connection.Dispose(dbConnection, new DbInterceptionContext());
			}
		}
		_emptyMigrationNeeded = flag;
		try
		{
			_committedStatements = false;
			mustSucceedToKeepDatabase();
		}
		catch
		{
			if (flag && !_committedStatements)
			{
				DbConnection dbConnection2 = null;
				try
				{
					dbConnection2 = CreateConnection();
					databaseCreator.Delete(dbConnection2);
				}
				catch
				{
				}
				finally
				{
					if (dbConnection2 != null)
					{
						DbInterception.Dispatch.Connection.Dispose(dbConnection2, new DbInterceptionContext());
					}
				}
			}
			throw;
		}
	}

	private DbConnection CreateConnection()
	{
		DbConnection dbConnection = ((_connection == null) ? _providerFactory.CreateConnection() : DbProviderServices.GetProviderServices(_connection).CloneDbConnection(_connection, _providerFactory));
		DbConnectionPropertyInterceptionContext<string> dbConnectionPropertyInterceptionContext = new DbConnectionPropertyInterceptionContext<string>().WithValue(_usersContextInfo.ConnectionString);
		if (_usersContext != null)
		{
			dbConnectionPropertyInterceptionContext = dbConnectionPropertyInterceptionContext.WithDbContext(_usersContext);
		}
		DbInterception.Dispatch.Connection.SetConnectionString(dbConnection, dbConnectionPropertyInterceptionContext);
		return dbConnection;
	}
}
