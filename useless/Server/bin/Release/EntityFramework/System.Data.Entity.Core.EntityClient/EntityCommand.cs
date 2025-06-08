using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.EntityClient;

public class EntityCommand : DbCommand
{
	internal class EntityDataReaderFactory
	{
		internal virtual EntityDataReader CreateEntityDataReader(EntityCommand entityCommand, DbDataReader storeDataReader, CommandBehavior behavior)
		{
			return new EntityDataReader(entityCommand, storeDataReader, behavior);
		}
	}

	private bool _designTimeVisible;

	private string _esqlCommandText;

	private EntityConnection _connection;

	private DbCommandTree _preparedCommandTree;

	private readonly EntityParameterCollection _parameters;

	private int? _commandTimeout;

	private CommandType _commandType;

	private EntityTransaction _transaction;

	private UpdateRowSource _updatedRowSource;

	private EntityCommandDefinition _commandDefinition;

	private bool _isCommandDefinitionBased;

	private DbCommandTree _commandTreeSetByUser;

	private DbDataReader _dataReader;

	private bool _enableQueryPlanCaching;

	private DbCommand _storeProviderCommand;

	private readonly EntityDataReaderFactory _entityDataReaderFactory;

	private readonly IDbDependencyResolver _dependencyResolver;

	private readonly DbInterceptionContext _interceptionContext;

	internal virtual DbInterceptionContext InterceptionContext => _interceptionContext;

	public new virtual EntityConnection Connection
	{
		get
		{
			return _connection;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			if (_connection != value)
			{
				if (_connection != null)
				{
					Unprepare();
				}
				_connection = value;
				_transaction = null;
			}
		}
	}

	protected override DbConnection DbConnection
	{
		get
		{
			return Connection;
		}
		set
		{
			Connection = (EntityConnection)value;
		}
	}

	public override string CommandText
	{
		get
		{
			if (_commandTreeSetByUser != null)
			{
				throw new InvalidOperationException(Strings.EntityClient_CannotGetCommandText);
			}
			return _esqlCommandText ?? "";
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			if (_commandTreeSetByUser != null)
			{
				throw new InvalidOperationException(Strings.EntityClient_CannotSetCommandText);
			}
			if (_esqlCommandText != value)
			{
				_esqlCommandText = value;
				Unprepare();
				_isCommandDefinitionBased = false;
			}
		}
	}

	public virtual DbCommandTree CommandTree
	{
		get
		{
			if (!string.IsNullOrEmpty(_esqlCommandText))
			{
				throw new InvalidOperationException(Strings.EntityClient_CannotGetCommandTree);
			}
			return _commandTreeSetByUser;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			if (!string.IsNullOrEmpty(_esqlCommandText))
			{
				throw new InvalidOperationException(Strings.EntityClient_CannotSetCommandTree);
			}
			if (CommandType.Text != CommandType)
			{
				throw new InvalidOperationException(Strings.ADP_InternalProviderError(1026));
			}
			if (_commandTreeSetByUser != value)
			{
				_commandTreeSetByUser = value;
				Unprepare();
				_isCommandDefinitionBased = false;
			}
		}
	}

	public override int CommandTimeout
	{
		get
		{
			if (_commandTimeout.HasValue)
			{
				return _commandTimeout.Value;
			}
			if (_connection != null && _connection.StoreProviderFactory != null)
			{
				DbCommand dbCommand = _connection.StoreProviderFactory.CreateCommand();
				if (dbCommand != null)
				{
					return dbCommand.CommandTimeout;
				}
			}
			return 0;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			_commandTimeout = value;
		}
	}

	public override CommandType CommandType
	{
		get
		{
			return _commandType;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			if (value != CommandType.Text && value != CommandType.StoredProcedure)
			{
				throw new NotSupportedException(Strings.EntityClient_UnsupportedCommandType);
			}
			_commandType = value;
		}
	}

	public new virtual EntityParameterCollection Parameters => _parameters;

	protected override DbParameterCollection DbParameterCollection => Parameters;

	public new virtual EntityTransaction Transaction
	{
		get
		{
			return _transaction;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			_transaction = value;
		}
	}

	protected override DbTransaction DbTransaction
	{
		get
		{
			return Transaction;
		}
		set
		{
			Transaction = (EntityTransaction)value;
		}
	}

	public override UpdateRowSource UpdatedRowSource
	{
		get
		{
			return _updatedRowSource;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			_updatedRowSource = value;
		}
	}

	public override bool DesignTimeVisible
	{
		get
		{
			return _designTimeVisible;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			_designTimeVisible = value;
			TypeDescriptor.Refresh(this);
		}
	}

	public virtual bool EnablePlanCaching
	{
		get
		{
			return _enableQueryPlanCaching;
		}
		set
		{
			ThrowIfDataReaderIsOpen();
			_enableQueryPlanCaching = value;
		}
	}

	internal event EventHandler OnDataReaderClosing;

	public EntityCommand()
		: this(new DbInterceptionContext())
	{
	}

	internal EntityCommand(DbInterceptionContext interceptionContext)
		: this(interceptionContext, new EntityDataReaderFactory())
	{
	}

	internal EntityCommand(DbInterceptionContext interceptionContext, EntityDataReaderFactory factory)
	{
		_designTimeVisible = true;
		_commandType = CommandType.Text;
		_updatedRowSource = UpdateRowSource.Both;
		_parameters = new EntityParameterCollection();
		_interceptionContext = interceptionContext;
		_enableQueryPlanCaching = true;
		_entityDataReaderFactory = factory ?? new EntityDataReaderFactory();
	}

	public EntityCommand(string statement)
		: this(statement, new DbInterceptionContext(), new EntityDataReaderFactory())
	{
	}

	internal EntityCommand(string statement, DbInterceptionContext context, EntityDataReaderFactory factory)
		: this(context, factory)
	{
		_esqlCommandText = statement;
	}

	public EntityCommand(string statement, EntityConnection connection, IDbDependencyResolver resolver)
		: this(statement, connection)
	{
		_dependencyResolver = resolver;
	}

	public EntityCommand(string statement, EntityConnection connection)
		: this(statement, connection, new EntityDataReaderFactory())
	{
	}

	internal EntityCommand(string statement, EntityConnection connection, EntityDataReaderFactory factory)
		: this(statement, new DbInterceptionContext(), factory)
	{
		_connection = connection;
	}

	public EntityCommand(string statement, EntityConnection connection, EntityTransaction transaction)
		: this(statement, connection, transaction, new EntityDataReaderFactory())
	{
	}

	internal EntityCommand(string statement, EntityConnection connection, EntityTransaction transaction, EntityDataReaderFactory factory)
		: this(statement, connection, factory)
	{
		_transaction = transaction;
	}

	internal EntityCommand(EntityCommandDefinition commandDefinition, DbInterceptionContext context, EntityDataReaderFactory factory = null)
		: this(context, factory)
	{
		_commandDefinition = commandDefinition;
		_parameters = new EntityParameterCollection();
		foreach (EntityParameter parameter in commandDefinition.Parameters)
		{
			_parameters.Add(parameter.Clone());
		}
		_parameters.ResetIsDirty();
		_isCommandDefinitionBased = true;
	}

	internal EntityCommand(EntityConnection connection, EntityCommandDefinition entityCommandDefinition, DbInterceptionContext context, EntityDataReaderFactory factory = null)
		: this(entityCommandDefinition, context, factory)
	{
		_connection = connection;
	}

	public override void Cancel()
	{
	}

	public new virtual EntityParameter CreateParameter()
	{
		return new EntityParameter();
	}

	protected override DbParameter CreateDbParameter()
	{
		return CreateParameter();
	}

	public new virtual EntityDataReader ExecuteReader()
	{
		return ExecuteReader(CommandBehavior.Default);
	}

	public new virtual EntityDataReader ExecuteReader(CommandBehavior behavior)
	{
		Prepare();
		return (EntityDataReader)(_dataReader = _entityDataReaderFactory.CreateEntityDataReader(this, _commandDefinition.Execute(this, behavior), behavior));
	}

	public new virtual Task<EntityDataReader> ExecuteReaderAsync()
	{
		return ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);
	}

	public new virtual Task<EntityDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
	{
		return ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
	}

	public new virtual Task<EntityDataReader> ExecuteReaderAsync(CommandBehavior behavior)
	{
		return ExecuteReaderAsync(behavior, CancellationToken.None);
	}

	public new virtual async Task<EntityDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Prepare();
		DbDataReader storeDataReader = await _commandDefinition.ExecuteAsync(this, behavior, cancellationToken).WithCurrentCulture();
		return (EntityDataReader)(_dataReader = _entityDataReaderFactory.CreateEntityDataReader(this, storeDataReader, behavior));
	}

	protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		return ExecuteReader(behavior);
	}

	protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	{
		return await ExecuteReaderAsync(behavior, cancellationToken).WithCurrentCulture();
	}

	public override int ExecuteNonQuery()
	{
		using EntityDataReader entityDataReader = ExecuteReader(CommandBehavior.SequentialAccess);
		CommandHelper.ConsumeReader(entityDataReader);
		return entityDataReader.RecordsAffected;
	}

	public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
	{
		using EntityDataReader reader = await ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).WithCurrentCulture();
		await CommandHelper.ConsumeReaderAsync(reader, cancellationToken).WithCurrentCulture();
		return reader.RecordsAffected;
	}

	public override object ExecuteScalar()
	{
		using EntityDataReader entityDataReader = ExecuteReader(CommandBehavior.SequentialAccess);
		object result = (entityDataReader.Read() ? entityDataReader.GetValue(0) : null);
		CommandHelper.ConsumeReader(entityDataReader);
		return result;
	}

	internal virtual void Unprepare()
	{
		_commandDefinition = null;
		_preparedCommandTree = null;
		_parameters.ResetIsDirty();
	}

	public override void Prepare()
	{
		ThrowIfDataReaderIsOpen();
		CheckIfReadyToPrepare();
		InnerPrepare();
	}

	private void InnerPrepare()
	{
		if (_parameters.IsDirty)
		{
			Unprepare();
		}
		_commandDefinition = GetCommandDefinition();
	}

	private DbCommandTree MakeCommandTree()
	{
		DbCommandTree result = null;
		if (_commandTreeSetByUser != null)
		{
			result = _commandTreeSetByUser;
		}
		else if (CommandType.Text == CommandType)
		{
			if (string.IsNullOrEmpty(_esqlCommandText))
			{
				if (_isCommandDefinitionBased)
				{
					throw new InvalidOperationException(Strings.EntityClient_CannotReprepareCommandDefinitionBasedCommand);
				}
				throw new InvalidOperationException(Strings.EntityClient_NoCommandText);
			}
			Perspective perspective = new ModelPerspective(_connection.GetMetadataWorkspace());
			Dictionary<string, TypeUsage> parameterTypeUsage = GetParameterTypeUsage();
			result = CqlQuery.Compile(_esqlCommandText, perspective, null, parameterTypeUsage.Select((KeyValuePair<string, TypeUsage> paramInfo) => paramInfo.Value.Parameter(paramInfo.Key))).CommandTree;
		}
		else if (CommandType.StoredProcedure == CommandType)
		{
			IEnumerable<KeyValuePair<string, TypeUsage>> parameterTypeUsage2 = GetParameterTypeUsage();
			EdmFunction edmFunction = DetermineFunctionImport();
			result = new DbFunctionCommandTree(Connection.GetMetadataWorkspace(), DataSpace.CSpace, edmFunction, null, parameterTypeUsage2);
		}
		return result;
	}

	private EdmFunction DetermineFunctionImport()
	{
		if (string.IsNullOrEmpty(CommandText) || string.IsNullOrEmpty(CommandText.Trim()))
		{
			throw new InvalidOperationException(Strings.EntityClient_FunctionImportEmptyCommandText);
		}
		string defaultContainerName = null;
		CommandHelper.ParseFunctionImportCommandText(CommandText, defaultContainerName, out var containerName, out var functionImportName);
		return CommandHelper.FindFunctionImport(_connection.GetMetadataWorkspace(), containerName, functionImportName);
	}

	internal virtual EntityCommandDefinition GetCommandDefinition()
	{
		EntityCommandDefinition entityCommandDefinition = _commandDefinition;
		if (entityCommandDefinition == null)
		{
			if (!TryGetEntityCommandDefinitionFromQueryCache(out entityCommandDefinition))
			{
				entityCommandDefinition = CreateCommandDefinition();
			}
			_commandDefinition = entityCommandDefinition;
		}
		return entityCommandDefinition;
	}

	internal virtual EntityTransaction ValidateAndGetEntityTransaction()
	{
		if (Transaction != null && Transaction != Connection.CurrentTransaction)
		{
			throw new InvalidOperationException(Strings.EntityClient_InvalidTransactionForCommand);
		}
		return Connection.CurrentTransaction;
	}

	[Browsable(false)]
	public virtual string ToTraceString()
	{
		CheckConnectionPresent();
		InnerPrepare();
		EntityCommandDefinition commandDefinition = _commandDefinition;
		if (commandDefinition != null)
		{
			return commandDefinition.ToTraceString();
		}
		return string.Empty;
	}

	private bool TryGetEntityCommandDefinitionFromQueryCache(out EntityCommandDefinition entityCommandDefinition)
	{
		entityCommandDefinition = null;
		if (!_enableQueryPlanCaching || string.IsNullOrEmpty(_esqlCommandText))
		{
			return false;
		}
		EntityClientCacheKey entityClientCacheKey = new EntityClientCacheKey(this);
		QueryCacheManager queryCacheManager = _connection.GetMetadataWorkspace().GetQueryCacheManager();
		if (!queryCacheManager.TryCacheLookup<EntityClientCacheKey, EntityCommandDefinition>(entityClientCacheKey, out entityCommandDefinition))
		{
			entityCommandDefinition = CreateCommandDefinition();
			QueryCacheEntry outQueryCacheEntry = null;
			if (queryCacheManager.TryLookupAndAdd(new QueryCacheEntry(entityClientCacheKey, entityCommandDefinition), out outQueryCacheEntry))
			{
				entityCommandDefinition = (EntityCommandDefinition)outQueryCacheEntry.GetTarget();
			}
		}
		return true;
	}

	private EntityCommandDefinition CreateCommandDefinition()
	{
		if (_preparedCommandTree == null)
		{
			_preparedCommandTree = MakeCommandTree();
		}
		if (!_preparedCommandTree.MetadataWorkspace.IsMetadataWorkspaceCSCompatible(Connection.GetMetadataWorkspace()))
		{
			throw new InvalidOperationException(Strings.EntityClient_CommandTreeMetadataIncompatible);
		}
		return EntityProviderServices.CreateCommandDefinition(_connection.StoreProviderFactory, _preparedCommandTree, _interceptionContext, _dependencyResolver);
	}

	private void CheckConnectionPresent()
	{
		if (_connection == null)
		{
			throw new InvalidOperationException(Strings.EntityClient_NoConnectionForCommand);
		}
	}

	private void CheckIfReadyToPrepare()
	{
		CheckConnectionPresent();
		if (_connection.StoreProviderFactory == null || _connection.StoreConnection == null)
		{
			throw Error.EntityClient_ConnectionStringNeededBeforeOperation();
		}
		if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
		{
			throw new InvalidOperationException(Strings.EntityClient_ExecutingOnClosedConnection((_connection.State == ConnectionState.Closed) ? Strings.EntityClient_ConnectionStateClosed : Strings.EntityClient_ConnectionStateBroken));
		}
	}

	private void ThrowIfDataReaderIsOpen()
	{
		if (_dataReader != null)
		{
			throw new InvalidOperationException(Strings.EntityClient_DataReaderIsStillOpen);
		}
	}

	internal virtual Dictionary<string, TypeUsage> GetParameterTypeUsage()
	{
		Dictionary<string, TypeUsage> dictionary = new Dictionary<string, TypeUsage>(_parameters.Count);
		foreach (EntityParameter parameter in _parameters)
		{
			string parameterName = parameter.ParameterName;
			if (string.IsNullOrEmpty(parameterName))
			{
				throw new InvalidOperationException(Strings.EntityClient_EmptyParameterName);
			}
			if (CommandType == CommandType.Text && parameter.Direction != ParameterDirection.Input)
			{
				throw new InvalidOperationException(Strings.EntityClient_InvalidParameterDirection(parameter.ParameterName));
			}
			if (parameter.EdmType == null && parameter.DbType == DbType.Object && (parameter.Value == null || parameter.Value is DBNull))
			{
				throw new InvalidOperationException(Strings.EntityClient_UnknownParameterType(parameterName));
			}
			TypeUsage typeUsage = null;
			typeUsage = parameter.GetTypeUsage();
			try
			{
				dictionary.Add(parameterName, typeUsage);
			}
			catch (ArgumentException innerException)
			{
				throw new InvalidOperationException(Strings.EntityClient_DuplicateParameterNames(parameter.ParameterName), innerException);
			}
		}
		return dictionary;
	}

	internal virtual void NotifyDataReaderClosing()
	{
		_dataReader = null;
		if (_storeProviderCommand != null)
		{
			CommandHelper.SetEntityParameterValues(this, _storeProviderCommand, _connection);
			_storeProviderCommand = null;
		}
		if (IsNotNullOnDataReaderClosingEvent())
		{
			InvokeOnDataReaderClosingEvent(this, new EventArgs());
		}
	}

	internal virtual void SetStoreProviderCommand(DbCommand storeProviderCommand)
	{
		_storeProviderCommand = storeProviderCommand;
	}

	internal virtual bool IsNotNullOnDataReaderClosingEvent()
	{
		return this.OnDataReaderClosing != null;
	}

	internal virtual void InvokeOnDataReaderClosingEvent(EntityCommand sender, EventArgs e)
	{
		this.OnDataReaderClosing(sender, e);
	}
}
