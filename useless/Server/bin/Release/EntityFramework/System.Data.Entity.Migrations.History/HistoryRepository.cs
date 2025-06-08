using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Migrations.Edm;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.History;

internal class HistoryRepository : RepositoryBase
{
	private class ParameterInliner : DefaultExpressionVisitor
	{
		private readonly DbParameterCollection _parameters;

		public ParameterInliner(DbParameterCollection parameters)
		{
			_parameters = parameters;
		}

		public override DbExpression Visit(DbParameterReferenceExpression expression)
		{
			return DbExpressionBuilder.Constant(_parameters[expression.ParameterName].Value);
		}

		public override DbExpression Visit(DbOrExpression expression)
		{
			return expression.Left.Accept(this);
		}

		public override DbExpression Visit(DbAndExpression expression)
		{
			if (expression.Right is DbNotExpression)
			{
				return expression.Left.Accept(this);
			}
			return base.Visit(expression);
		}
	}

	private static readonly string _productVersion = typeof(HistoryRepository).Assembly().GetInformationalVersion();

	public static readonly PropertyInfo MigrationIdProperty = typeof(HistoryRow).GetDeclaredProperty("MigrationId");

	public static readonly PropertyInfo ContextKeyProperty = typeof(HistoryRow).GetDeclaredProperty("ContextKey");

	private readonly string _contextKey;

	private readonly int? _commandTimeout;

	private readonly IEnumerable<string> _schemas;

	private readonly Func<DbConnection, string, HistoryContext> _historyContextFactory;

	private readonly DbContext _contextForInterception;

	private readonly int _contextKeyMaxLength;

	private readonly int _migrationIdMaxLength;

	private readonly DatabaseExistenceState _initialExistence;

	private readonly Func<Exception, bool> _permissionDeniedDetector;

	private readonly DbTransaction _existingTransaction;

	private string _currentSchema;

	private bool? _exists;

	private bool _contextKeyColumnExists;

	public int ContextKeyMaxLength => _contextKeyMaxLength;

	public int MigrationIdMaxLength => _migrationIdMaxLength;

	public string CurrentSchema
	{
		get
		{
			return _currentSchema;
		}
		set
		{
			_currentSchema = value;
		}
	}

	public HistoryRepository(InternalContext usersContext, string connectionString, DbProviderFactory providerFactory, string contextKey, int? commandTimeout, Func<DbConnection, string, HistoryContext> historyContextFactory, IEnumerable<string> schemas = null, DbContext contextForInterception = null, DatabaseExistenceState initialExistence = DatabaseExistenceState.Unknown, Func<Exception, bool> permissionDeniedDetector = null)
		: base(usersContext, connectionString, providerFactory)
	{
		_initialExistence = initialExistence;
		_permissionDeniedDetector = permissionDeniedDetector;
		_commandTimeout = commandTimeout;
		_existingTransaction = usersContext.TryGetCurrentStoreTransaction();
		_schemas = new string[1] { "dbo" }.Concat(schemas ?? Enumerable.Empty<string>()).Distinct();
		_contextForInterception = contextForInterception;
		_historyContextFactory = historyContextFactory;
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext historyContext = CreateContext(connection);
			EntityType entityType = ((IObjectContextAdapter)historyContext).ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.CSpace).Single((EntityType et) => et.GetClrType() == typeof(HistoryRow));
			int? maxLength = entityType.Properties.Single((EdmProperty p) => p.GetClrPropertyInfo().IsSameAs(MigrationIdProperty)).MaxLength;
			_migrationIdMaxLength = (maxLength.HasValue ? maxLength.Value : 150);
			maxLength = entityType.Properties.Single((EdmProperty p) => p.GetClrPropertyInfo().IsSameAs(ContextKeyProperty)).MaxLength;
			_contextKeyMaxLength = (maxLength.HasValue ? maxLength.Value : 300);
		}
		finally
		{
			DisposeConnection(connection);
		}
		_contextKey = contextKey.RestrictTo(_contextKeyMaxLength);
	}

	public virtual XDocument GetLastModel(out string migrationId, out string productVersion, string contextKey = null)
	{
		migrationId = null;
		productVersion = null;
		if (!Exists(contextKey))
		{
			return null;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext context = CreateContext(connection);
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
				var anon = (from h in CreateHistoryQuery(context, contextKey)
					orderby h.MigrationId descending
					select h into s
					select new { s.MigrationId, s.Model, s.ProductVersion }).FirstOrDefault();
				if (anon == null)
				{
					return null;
				}
				migrationId = anon.MigrationId;
				productVersion = anon.ProductVersion;
				return new ModelCompressor().Decompress(anon.Model);
			}
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual XDocument GetModel(string migrationId, out string productVersion)
	{
		productVersion = null;
		if (!Exists())
		{
			return null;
		}
		migrationId = migrationId.RestrictTo(_migrationIdMaxLength);
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext context = CreateContext(connection);
			var anon = (from h in CreateHistoryQuery(context)
				where h.MigrationId == migrationId
				select new { h.Model, h.ProductVersion }).SingleOrDefault();
			if (anon == null)
			{
				return null;
			}
			productVersion = anon.ProductVersion;
			return new ModelCompressor().Decompress(anon.Model);
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual IEnumerable<string> GetPendingMigrations(IEnumerable<string> localMigrations)
	{
		if (!Exists())
		{
			return localMigrations;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext context = CreateContext(connection);
			List<string> list;
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
				list = (from h in CreateHistoryQuery(context)
					select h.MigrationId).ToList();
			}
			localMigrations = localMigrations.Select((string m) => m.RestrictTo(_migrationIdMaxLength)).ToArray();
			IEnumerable<string> source = localMigrations.Except(list);
			string text = list.FirstOrDefault();
			string text2 = localMigrations.FirstOrDefault();
			if (text != text2 && text != null && text.MigrationName() == Strings.InitialCreate && text2 != null && text2.MigrationName() == Strings.InitialCreate)
			{
				source = source.Skip(1);
			}
			return source.ToList();
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual IEnumerable<string> GetMigrationsSince(string migrationId)
	{
		bool flag = Exists();
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext context = CreateContext(connection);
			IQueryable<HistoryRow> source = CreateHistoryQuery(context);
			migrationId = migrationId.RestrictTo(_migrationIdMaxLength);
			if (migrationId != "0")
			{
				if (!flag || !source.Any((HistoryRow h) => h.MigrationId == migrationId))
				{
					throw Error.MigrationNotFound(migrationId);
				}
				source = source.Where((HistoryRow h) => string.Compare(h.MigrationId, migrationId, StringComparison.Ordinal) > 0);
			}
			else if (!flag)
			{
				return Enumerable.Empty<string>();
			}
			return (from h in source
				orderby h.MigrationId descending
				select h.MigrationId).ToList();
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual string GetMigrationId(string migrationName)
	{
		if (!Exists())
		{
			return null;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext context = CreateContext(connection);
			List<string> source = (from h in CreateHistoryQuery(context)
				select h.MigrationId into m
				where m.Substring(16) == migrationName
				select m).ToList();
			if (!source.Any())
			{
				return null;
			}
			if (source.Count() == 1)
			{
				return source.Single();
			}
			throw Error.AmbiguousMigrationName(migrationName);
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	private IQueryable<HistoryRow> CreateHistoryQuery(HistoryContext context, string contextKey = null)
	{
		IQueryable<HistoryRow> queryable = context.History;
		contextKey = ((!string.IsNullOrWhiteSpace(contextKey)) ? contextKey.RestrictTo(_contextKeyMaxLength) : _contextKey);
		if (_contextKeyColumnExists)
		{
			queryable = queryable.Where((HistoryRow h) => h.ContextKey == contextKey);
		}
		return queryable;
	}

	public virtual bool IsShared()
	{
		if (!Exists() || !_contextKeyColumnExists)
		{
			return false;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext historyContext = CreateContext(connection);
			return historyContext.History.Any((HistoryRow hr) => hr.ContextKey != _contextKey);
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual bool HasMigrations()
	{
		if (!Exists())
		{
			return false;
		}
		if (!_contextKeyColumnExists)
		{
			return true;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext historyContext = CreateContext(connection);
			return historyContext.History.Count((HistoryRow hr) => hr.ContextKey == _contextKey) > 0;
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual bool Exists(string contextKey = null)
	{
		if (!_exists.HasValue)
		{
			_exists = QueryExists(contextKey ?? _contextKey);
		}
		return _exists.Value;
	}

	private bool QueryExists(string contextKey)
	{
		if (_initialExistence == DatabaseExistenceState.DoesNotExist)
		{
			return false;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			if (_initialExistence == DatabaseExistenceState.Unknown)
			{
				using HistoryContext historyContext = CreateContext(connection);
				if (!historyContext.Database.Exists())
				{
					return false;
				}
			}
			foreach (string item in _schemas.Reverse())
			{
				using HistoryContext historyContext2 = CreateContext(connection, item);
				_currentSchema = item;
				_contextKeyColumnExists = true;
				try
				{
					using (new TransactionScope(TransactionScopeOption.Suppress))
					{
						contextKey = contextKey.RestrictTo(_contextKeyMaxLength);
						if (historyContext2.History.Count((HistoryRow hr) => hr.ContextKey == contextKey) > 0)
						{
							return true;
						}
					}
				}
				catch (EntityException ex)
				{
					if (_permissionDeniedDetector != null && _permissionDeniedDetector(ex.InnerException))
					{
						throw;
					}
					_contextKeyColumnExists = false;
				}
				if (_contextKeyColumnExists)
				{
					continue;
				}
				try
				{
					using (new TransactionScope(TransactionScopeOption.Suppress))
					{
						historyContext2.History.Count();
					}
				}
				catch (EntityException ex2)
				{
					if (_permissionDeniedDetector != null && _permissionDeniedDetector(ex2.InnerException))
					{
						throw;
					}
					_currentSchema = null;
				}
			}
		}
		finally
		{
			DisposeConnection(connection);
		}
		return !string.IsNullOrWhiteSpace(_currentSchema);
	}

	public virtual void ResetExists()
	{
		_exists = null;
	}

	public virtual IEnumerable<MigrationOperation> GetUpgradeOperations()
	{
		if (!Exists())
		{
			yield break;
		}
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			string tableName = "dbo.__MigrationHistory";
			if (connection.GetProviderInfo(out var _).IsSqlCe())
			{
				tableName = "__MigrationHistory";
			}
			using (LegacyHistoryContext legacyHistoryContext = new LegacyHistoryContext(connection))
			{
				bool flag = false;
				try
				{
					InjectInterceptionContext(legacyHistoryContext);
					using (new TransactionScope(TransactionScopeOption.Suppress))
					{
						legacyHistoryContext.History.Select((LegacyHistoryRow h) => h.CreatedOn).FirstOrDefault();
					}
					flag = true;
				}
				catch (EntityException)
				{
				}
				if (flag)
				{
					yield return new DropColumnOperation(tableName, "CreatedOn");
				}
			}
			using HistoryContext context = CreateContext(connection);
			if (!_contextKeyColumnExists)
			{
				if (_historyContextFactory != HistoryContext.DefaultFactory)
				{
					throw Error.UnableToUpgradeHistoryWhenCustomFactory();
				}
				yield return new AddColumnOperation(tableName, new ColumnModel(PrimitiveTypeKind.String)
				{
					MaxLength = _contextKeyMaxLength,
					Name = "ContextKey",
					IsNullable = false,
					DefaultValue = _contextKey
				});
				XDocument model = new DbModelBuilder().Build(connection).GetModel();
				CreateTableOperation createTableOperation = (CreateTableOperation)new EdmModelDiffer().Diff(model, context.GetModel()).Single();
				DropPrimaryKeyOperation dropPrimaryKeyOperation = new DropPrimaryKeyOperation
				{
					Table = tableName,
					CreateTableOperation = createTableOperation
				};
				dropPrimaryKeyOperation.Columns.Add("MigrationId");
				yield return dropPrimaryKeyOperation;
				yield return new AlterColumnOperation(tableName, new ColumnModel(PrimitiveTypeKind.String)
				{
					MaxLength = _migrationIdMaxLength,
					Name = "MigrationId",
					IsNullable = false
				}, isDestructiveChange: false);
				AddPrimaryKeyOperation addPrimaryKeyOperation = new AddPrimaryKeyOperation
				{
					Table = tableName
				};
				addPrimaryKeyOperation.Columns.Add("MigrationId");
				addPrimaryKeyOperation.Columns.Add("ContextKey");
				yield return addPrimaryKeyOperation;
			}
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual MigrationOperation CreateInsertOperation(string migrationId, VersionedModel versionedModel)
	{
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext historyContext = CreateContext(connection);
			historyContext.History.Add(new HistoryRow
			{
				MigrationId = migrationId.RestrictTo(_migrationIdMaxLength),
				ContextKey = _contextKey,
				Model = new ModelCompressor().Compress(versionedModel.Model),
				ProductVersion = (versionedModel.Version ?? _productVersion)
			});
			using CommandTracer commandTracer = new CommandTracer(historyContext);
			historyContext.SaveChanges();
			return new HistoryOperation(commandTracer.CommandTrees.OfType<DbModificationCommandTree>().ToList());
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual MigrationOperation CreateDeleteOperation(string migrationId)
	{
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext historyContext = CreateContext(connection);
			HistoryRow entity = new HistoryRow
			{
				MigrationId = migrationId.RestrictTo(_migrationIdMaxLength),
				ContextKey = _contextKey
			};
			historyContext.History.Attach(entity);
			historyContext.History.Remove(entity);
			using CommandTracer commandTracer = new CommandTracer(historyContext);
			historyContext.SaveChanges();
			return new HistoryOperation(commandTracer.CommandTrees.OfType<DbModificationCommandTree>().ToList());
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual IEnumerable<DbQueryCommandTree> CreateDiscoveryQueryTrees()
	{
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			foreach (string schema in _schemas)
			{
				using HistoryContext context = CreateContext(connection, schema);
				IOrderedQueryable<string> orderedQueryable = from h in context.History
					where h.ContextKey == _contextKey
					select h into s
					select s.MigrationId into s
					orderby s descending
					select s;
				if (orderedQueryable is DbQuery<string> dbQuery)
				{
					dbQuery.InternalQuery.ObjectQuery.EnablePlanCaching = false;
				}
				using CommandTracer commandTracer = new CommandTracer(context);
				orderedQueryable.First();
				DbQueryCommandTree dbQueryCommandTree = commandTracer.CommandTrees.OfType<DbQueryCommandTree>().Single((DbQueryCommandTree t) => t.DataSpace == DataSpace.SSpace);
				yield return new DbQueryCommandTree(dbQueryCommandTree.MetadataWorkspace, dbQueryCommandTree.DataSpace, dbQueryCommandTree.Query.Accept(new ParameterInliner(commandTracer.DbCommands.Single().Parameters)));
			}
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public virtual void BootstrapUsingEFProviderDdl(VersionedModel versionedModel)
	{
		DbConnection connection = null;
		try
		{
			connection = CreateConnection();
			using HistoryContext historyContext = CreateContext(connection);
			historyContext.Database.ExecuteSqlCommand(((IObjectContextAdapter)historyContext).ObjectContext.CreateDatabaseScript());
			historyContext.History.Add(new HistoryRow
			{
				MigrationId = MigrationAssembly.CreateMigrationId(Strings.InitialCreate).RestrictTo(_migrationIdMaxLength),
				ContextKey = _contextKey,
				Model = new ModelCompressor().Compress(versionedModel.Model),
				ProductVersion = (versionedModel.Version ?? _productVersion)
			});
			historyContext.SaveChanges();
		}
		finally
		{
			DisposeConnection(connection);
		}
	}

	public HistoryContext CreateContext(DbConnection connection, string schema = null)
	{
		HistoryContext historyContext = _historyContextFactory(connection, schema ?? CurrentSchema);
		historyContext.Database.CommandTimeout = _commandTimeout;
		if (_existingTransaction != null && _existingTransaction.Connection == connection)
		{
			historyContext.Database.UseTransaction(_existingTransaction);
		}
		InjectInterceptionContext(historyContext);
		return historyContext;
	}

	private void InjectInterceptionContext(DbContext context)
	{
		if (_contextForInterception != null)
		{
			ObjectContext objectContext = context.InternalContext.ObjectContext;
			objectContext.InterceptionContext = objectContext.InterceptionContext.WithDbContext(_contextForInterception);
		}
	}
}
