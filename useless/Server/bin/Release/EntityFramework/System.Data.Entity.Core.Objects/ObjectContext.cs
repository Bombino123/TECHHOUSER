using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace System.Data.Entity.Core.Objects;

public class ObjectContext : IDisposable, IObjectContextAdapter
{
	private class ParameterBinder
	{
		private readonly EntityParameter _entityParameter;

		private readonly ObjectParameter _objectParameter;

		internal ParameterBinder(EntityParameter entityParameter, ObjectParameter objectParameter)
		{
			_entityParameter = entityParameter;
			_objectParameter = objectParameter;
		}

		internal void OnDataReaderClosingHandler(object sender, EventArgs args)
		{
			if (_entityParameter.Value != DBNull.Value && _objectParameter.MappableType.IsEnum())
			{
				_objectParameter.Value = Enum.ToObject(_objectParameter.MappableType, _entityParameter.Value);
			}
			else
			{
				_objectParameter.Value = _entityParameter.Value;
			}
		}
	}

	private bool _disposed;

	private readonly IEntityAdapter _adapter;

	private EntityConnection _connection;

	private readonly MetadataWorkspace _workspace;

	private ObjectStateManager _objectStateManager;

	private ClrPerspective _perspective;

	private bool _contextOwnsConnection;

	private bool _openedConnection;

	private int _connectionRequestCount;

	private int? _queryTimeout;

	private Transaction _lastTransaction;

	private readonly bool _disallowSettingDefaultContainerName;

	private EventHandler _onSavingChanges;

	private ObjectMaterializedEventHandler _onObjectMaterialized;

	private ObjectQueryProvider _queryProvider;

	private readonly EntityWrapperFactory _entityWrapperFactory;

	private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;

	private readonly Translator _translator;

	private readonly ColumnMapFactory _columnMapFactory;

	private readonly ObjectContextOptions _options = new ObjectContextOptions();

	private const string UseLegacyPreserveChangesBehavior = "EntityFramework_UseLegacyPreserveChangesBehavior";

	private readonly ThrowingMonitor _asyncMonitor = new ThrowingMonitor();

	private DbInterceptionContext _interceptionContext;

	private static readonly ConcurrentDictionary<Type, bool> _contextTypesWithViewCacheInitialized = new ConcurrentDictionary<Type, bool>();

	private TransactionHandler _transactionHandler;

	public virtual DbConnection Connection
	{
		get
		{
			if (_connection == null)
			{
				throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
			}
			return _connection;
		}
	}

	public virtual string DefaultContainerName
	{
		get
		{
			EntityContainer defaultContainer = Perspective.GetDefaultContainer();
			if (defaultContainer == null)
			{
				return string.Empty;
			}
			return defaultContainer.Name;
		}
		set
		{
			if (!_disallowSettingDefaultContainerName)
			{
				Perspective.SetDefaultContainer(value);
				return;
			}
			throw new InvalidOperationException(Strings.ObjectContext_CannotSetDefaultContainerName);
		}
	}

	public virtual MetadataWorkspace MetadataWorkspace => _workspace;

	public virtual ObjectStateManager ObjectStateManager
	{
		get
		{
			if (_objectStateManager == null)
			{
				_objectStateManager = new ObjectStateManager(_workspace);
			}
			return _objectStateManager;
		}
	}

	internal bool ContextOwnsConnection
	{
		set
		{
			_contextOwnsConnection = value;
		}
	}

	internal ClrPerspective Perspective
	{
		get
		{
			if (_perspective == null)
			{
				_perspective = new ClrPerspective(MetadataWorkspace);
			}
			return _perspective;
		}
	}

	public virtual int? CommandTimeout
	{
		get
		{
			return _queryTimeout;
		}
		set
		{
			if (value.HasValue && value < 0)
			{
				throw new ArgumentException(Strings.ObjectContext_InvalidCommandTimeout, "value");
			}
			_queryTimeout = value;
		}
	}

	protected internal virtual IQueryProvider QueryProvider
	{
		get
		{
			if (_queryProvider == null)
			{
				_queryProvider = new ObjectQueryProvider(this);
			}
			return _queryProvider;
		}
	}

	internal bool InMaterialization { get; set; }

	internal ThrowingMonitor AsyncMonitor => _asyncMonitor;

	public virtual ObjectContextOptions ContextOptions => _options;

	internal CollectionColumnMap ColumnMapBuilder { get; set; }

	internal virtual EntityWrapperFactory EntityWrapperFactory => _entityWrapperFactory;

	ObjectContext IObjectContextAdapter.ObjectContext => this;

	public TransactionHandler TransactionHandler
	{
		get
		{
			EnsureTransactionHandlerRegistered();
			return _transactionHandler;
		}
	}

	public DbInterceptionContext InterceptionContext
	{
		get
		{
			return _interceptionContext;
		}
		internal set
		{
			_interceptionContext = value;
		}
	}

	internal bool OnMaterializedHasHandlers
	{
		get
		{
			if (_onObjectMaterialized != null)
			{
				return _onObjectMaterialized.GetInvocationList().Length != 0;
			}
			return false;
		}
	}

	internal bool IsDisposed => _disposed;

	public event EventHandler SavingChanges
	{
		add
		{
			_onSavingChanges = (EventHandler)Delegate.Combine(_onSavingChanges, value);
		}
		remove
		{
			_onSavingChanges = (EventHandler)Delegate.Remove(_onSavingChanges, value);
		}
	}

	public event ObjectMaterializedEventHandler ObjectMaterialized
	{
		add
		{
			_onObjectMaterialized = (ObjectMaterializedEventHandler)Delegate.Combine(_onObjectMaterialized, value);
		}
		remove
		{
			_onObjectMaterialized = (ObjectMaterializedEventHandler)Delegate.Remove(_onObjectMaterialized, value);
		}
	}

	public ObjectContext(EntityConnection connection)
		: this(connection, isConnectionConstructor: true, null)
	{
		_contextOwnsConnection = false;
	}

	public ObjectContext(EntityConnection connection, bool contextOwnsConnection)
		: this(connection, isConnectionConstructor: true, null)
	{
		_contextOwnsConnection = contextOwnsConnection;
	}

	public ObjectContext(string connectionString)
		: this(CreateEntityConnection(connectionString), isConnectionConstructor: false, null)
	{
		_contextOwnsConnection = true;
	}

	protected ObjectContext(string connectionString, string defaultContainerName)
		: this(connectionString)
	{
		DefaultContainerName = defaultContainerName;
		if (!string.IsNullOrEmpty(defaultContainerName))
		{
			_disallowSettingDefaultContainerName = true;
		}
	}

	protected ObjectContext(EntityConnection connection, string defaultContainerName)
		: this(connection)
	{
		DefaultContainerName = defaultContainerName;
		if (!string.IsNullOrEmpty(defaultContainerName))
		{
			_disallowSettingDefaultContainerName = true;
		}
	}

	internal ObjectContext(EntityConnection connection, bool isConnectionConstructor, ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory, Translator translator = null, ColumnMapFactory columnMapFactory = null)
	{
		Check.NotNull(connection, "connection");
		_interceptionContext = new DbInterceptionContext().WithObjectContext(this);
		_objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
		_translator = translator ?? new Translator();
		_columnMapFactory = columnMapFactory ?? new ColumnMapFactory();
		_adapter = new EntityAdapter(this);
		_connection = connection;
		_connection.AssociateContext(this);
		_connection.StateChange += ConnectionStateChange;
		_entityWrapperFactory = new EntityWrapperFactory();
		string connectionString = connection.ConnectionString;
		if (connectionString == null || connectionString.Trim().Length == 0)
		{
			throw isConnectionConstructor ? new ArgumentException(Strings.ObjectContext_InvalidConnection, "connection", null) : new ArgumentException(Strings.ObjectContext_InvalidConnectionString, "connectionString", null);
		}
		try
		{
			_workspace = RetrieveMetadataWorkspaceFromConnection();
		}
		catch (InvalidOperationException innerException)
		{
			throw isConnectionConstructor ? new ArgumentException(Strings.ObjectContext_InvalidConnection, "connection", innerException) : new ArgumentException(Strings.ObjectContext_InvalidConnectionString, "connectionString", innerException);
		}
		string? value = ConfigurationManager.AppSettings["EntityFramework_UseLegacyPreserveChangesBehavior"];
		bool result = false;
		if (bool.TryParse(value, out result))
		{
			ContextOptions.UseLegacyPreserveChangesBehavior = result;
		}
		InitializeMappingViewCacheFactory();
	}

	internal ObjectContext(ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null, Translator translator = null, ColumnMapFactory columnMapFactory = null, IEntityAdapter adapter = null)
	{
		_interceptionContext = new DbInterceptionContext().WithObjectContext(this);
		_objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
		_translator = translator ?? new Translator();
		_columnMapFactory = columnMapFactory ?? new ColumnMapFactory();
		_adapter = adapter ?? new EntityAdapter(this);
	}

	private void OnSavingChanges()
	{
		if (_onSavingChanges != null)
		{
			_onSavingChanges(this, new EventArgs());
		}
	}

	internal void OnObjectMaterialized(object entity)
	{
		if (_onObjectMaterialized != null)
		{
			_onObjectMaterialized(this, new ObjectMaterializedEventArgs(entity));
		}
	}

	public virtual void AcceptAllChanges()
	{
		if (ObjectStateManager.SomeEntryWithConceptualNullExists())
		{
			throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
		}
		foreach (ObjectStateEntry objectStateEntry in ObjectStateManager.GetObjectStateEntries(EntityState.Deleted))
		{
			objectStateEntry.AcceptChanges();
		}
		foreach (ObjectStateEntry objectStateEntry2 in ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
		{
			objectStateEntry2.AcceptChanges();
		}
	}

	private void VerifyRootForAdd(bool doAttach, string entitySetName, IEntityWrapper wrappedEntity, EntityEntry existingEntry, out EntitySet entitySet, out bool isNoOperation)
	{
		isNoOperation = false;
		EntitySet entitySet2 = null;
		if (doAttach)
		{
			if (!string.IsNullOrEmpty(entitySetName))
			{
				entitySet2 = GetEntitySetFromName(entitySetName);
			}
		}
		else
		{
			entitySet2 = GetEntitySetFromName(entitySetName);
		}
		EntitySet entitySet3 = null;
		EntityKey entityKey = ((existingEntry != null) ? existingEntry.EntityKey : wrappedEntity.GetEntityKeyFromEntity());
		if ((object)entityKey != null)
		{
			entitySet3 = entityKey.GetEntitySet(MetadataWorkspace);
			if (entitySet2 != null)
			{
				EntityUtil.ValidateEntitySetInKey(entityKey, entitySet2, "entitySetName");
			}
			entityKey.ValidateEntityKey(_workspace, entitySet3);
		}
		entitySet = entitySet3 ?? entitySet2;
		if (entitySet == null)
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntitySetNameOrEntityKeyRequired);
		}
		ValidateEntitySet(entitySet, wrappedEntity.IdentityType);
		if (doAttach && existingEntry == null)
		{
			if ((object)entityKey == null)
			{
				entityKey = ObjectStateManager.CreateEntityKey(entitySet, wrappedEntity.Entity);
			}
			existingEntry = ObjectStateManager.FindEntityEntry(entityKey);
		}
		if (existingEntry != null && (!doAttach || !existingEntry.IsKeyEntry))
		{
			if (existingEntry.Entity != wrappedEntity.Entity)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(wrappedEntity.IdentityType.FullName));
			}
			EntityState entityState = (doAttach ? EntityState.Unchanged : EntityState.Added);
			if (existingEntry.State != entityState)
			{
				throw doAttach ? new InvalidOperationException(Strings.ObjectContext_EntityAlreadyExistsInObjectStateManager) : new InvalidOperationException(Strings.ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(existingEntry.State));
			}
			isNoOperation = true;
		}
	}

	public virtual void AddObject(string entitySetName, object entity)
	{
		Check.NotNull(entity, "entity");
		EntityEntry existingEntry;
		IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContextGettingEntry(entity, this, out existingEntry);
		if (existingEntry == null)
		{
			MetadataWorkspace.ImplicitLoadAssemblyForType(entityWrapper.IdentityType, null);
		}
		VerifyRootForAdd(doAttach: false, entitySetName, entityWrapper, existingEntry, out var entitySet, out var isNoOperation);
		if (isNoOperation)
		{
			return;
		}
		System.Data.Entity.Core.Objects.Internal.TransactionManager transactionManager = ObjectStateManager.TransactionManager;
		transactionManager.BeginAddTracking();
		try
		{
			RelationshipManager relationshipManager = entityWrapper.RelationshipManager;
			bool flag = true;
			try
			{
				AddSingleObject(entitySet, entityWrapper, "entity");
				flag = false;
			}
			finally
			{
				if (flag && entityWrapper.Context == this)
				{
					EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(entityWrapper.Entity);
					if (entityEntry != null && entityEntry.EntityKey.IsTemporary)
					{
						relationshipManager.NodeVisited = true;
						RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(entityWrapper);
						RelatedEnd.RemoveEntityFromObjectStateManager(entityWrapper);
					}
				}
			}
			relationshipManager.AddRelatedEntitiesToObjectStateManager(doAttach: false);
		}
		finally
		{
			transactionManager.EndAddTracking();
		}
	}

	internal void AddSingleObject(EntitySet entitySet, IEntityWrapper wrappedEntity, string argumentName)
	{
		EntityKey entityKeyFromEntity = wrappedEntity.GetEntityKeyFromEntity();
		if ((object)entityKeyFromEntity != null)
		{
			EntityUtil.ValidateEntitySetInKey(entityKeyFromEntity, entitySet);
			entityKeyFromEntity.ValidateEntityKey(_workspace, entitySet);
		}
		VerifyContextForAddOrAttach(wrappedEntity);
		wrappedEntity.Context = this;
		EntityEntry entityEntry = ObjectStateManager.AddEntry(wrappedEntity, null, entitySet, argumentName, isAdded: true);
		ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);
		wrappedEntity.AttachContext(this, entitySet, MergeOption.AppendOnly);
		entityEntry.FixupFKValuesFromNonAddedReferences();
		ObjectStateManager.FixupReferencesByForeignKeys(entityEntry);
		wrappedEntity.TakeSnapshotOfRelationships(entityEntry);
	}

	public virtual void LoadProperty(object entity, string navigationProperty)
	{
		WrapEntityAndCheckContext(entity, "property").RelationshipManager.GetRelatedEnd(navigationProperty).Load();
	}

	public virtual void LoadProperty(object entity, string navigationProperty, MergeOption mergeOption)
	{
		WrapEntityAndCheckContext(entity, "property").RelationshipManager.GetRelatedEnd(navigationProperty).Load(mergeOption);
	}

	public virtual void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector)
	{
		bool removedConvert;
		string navigationProperty = ParsePropertySelectorExpression(selector, out removedConvert);
		WrapEntityAndCheckContext(entity, "property").RelationshipManager.GetRelatedEnd(navigationProperty, removedConvert).Load();
	}

	public virtual void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector, MergeOption mergeOption)
	{
		bool removedConvert;
		string navigationProperty = ParsePropertySelectorExpression(selector, out removedConvert);
		WrapEntityAndCheckContext(entity, "property").RelationshipManager.GetRelatedEnd(navigationProperty, removedConvert).Load(mergeOption);
	}

	private IEntityWrapper WrapEntityAndCheckContext(object entity, string refType)
	{
		IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(entity, this);
		if (entityWrapper.Context == null)
		{
			throw new InvalidOperationException(Strings.ObjectContext_CannotExplicitlyLoadDetachedRelationships(refType));
		}
		if (entityWrapper.Context != this)
		{
			throw new InvalidOperationException(Strings.ObjectContext_CannotLoadReferencesUsingDifferentContext(refType));
		}
		return entityWrapper;
	}

	internal static string ParsePropertySelectorExpression<TEntity>(Expression<Func<TEntity, object>> selector, out bool removedConvert)
	{
		Check.NotNull(selector, "selector");
		removedConvert = false;
		Expression expression = selector.Body;
		while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
		{
			removedConvert = true;
			expression = ((UnaryExpression)expression).Operand;
		}
		if (!(expression is MemberExpression memberExpression) || !memberExpression.Member.DeclaringType.IsAssignableFrom(typeof(TEntity)) || memberExpression.Expression.NodeType != ExpressionType.Parameter)
		{
			throw new ArgumentException(Strings.ObjectContext_SelectorExpressionMustBeMemberAccess);
		}
		return memberExpression.Member.Name;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[Obsolete("Use ApplyCurrentValues instead")]
	public virtual void ApplyPropertyChanges(string entitySetName, object changed)
	{
		Check.NotNull(changed, "changed");
		Check.NotEmpty(entitySetName, "entitySetName");
		ApplyCurrentValues(entitySetName, changed);
	}

	public virtual TEntity ApplyCurrentValues<TEntity>(string entitySetName, TEntity currentEntity) where TEntity : class
	{
		Check.NotNull(currentEntity, "currentEntity");
		Check.NotEmpty(entitySetName, "entitySetName");
		IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(currentEntity, this);
		MetadataWorkspace.ImplicitLoadAssemblyForType(entityWrapper.IdentityType, null);
		EntitySet entitySetFromName = GetEntitySetFromName(entitySetName);
		EntityKey entityKey = entityWrapper.EntityKey;
		if ((object)entityKey != null)
		{
			EntityUtil.ValidateEntitySetInKey(entityKey, entitySetFromName, "entitySetName");
			entityKey.ValidateEntityKey(_workspace, entitySetFromName);
		}
		else
		{
			entityKey = ObjectStateManager.CreateEntityKey(entitySetFromName, currentEntity);
		}
		EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(entityKey);
		if (entityEntry == null || entityEntry.IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_EntityNotTracked);
		}
		entityEntry.ApplyCurrentValuesInternal(entityWrapper);
		return (TEntity)entityEntry.Entity;
	}

	public virtual TEntity ApplyOriginalValues<TEntity>(string entitySetName, TEntity originalEntity) where TEntity : class
	{
		Check.NotNull(originalEntity, "originalEntity");
		Check.NotEmpty(entitySetName, "entitySetName");
		IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(originalEntity, this);
		MetadataWorkspace.ImplicitLoadAssemblyForType(entityWrapper.IdentityType, null);
		EntitySet entitySetFromName = GetEntitySetFromName(entitySetName);
		EntityKey entityKey = entityWrapper.EntityKey;
		if ((object)entityKey != null)
		{
			EntityUtil.ValidateEntitySetInKey(entityKey, entitySetFromName, "entitySetName");
			entityKey.ValidateEntityKey(_workspace, entitySetFromName);
		}
		else
		{
			entityKey = ObjectStateManager.CreateEntityKey(entitySetFromName, originalEntity);
		}
		EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(entityKey);
		if (entityEntry == null || entityEntry.IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntityNotTrackedOrHasTempKey);
		}
		if (entityEntry.State != EntityState.Modified && entityEntry.State != EntityState.Unchanged && entityEntry.State != EntityState.Deleted)
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted(entityEntry.State.ToString()));
		}
		if (entityEntry.WrappedEntity.IdentityType != entityWrapper.IdentityType)
		{
			throw new ArgumentException(Strings.ObjectContext_EntitiesHaveDifferentType(entityEntry.Entity.GetType().FullName, originalEntity.GetType().FullName));
		}
		entityEntry.CompareKeyProperties(originalEntity);
		entityEntry.UpdateOriginalValues(entityWrapper.Entity);
		return (TEntity)entityEntry.Entity;
	}

	public virtual void AttachTo(string entitySetName, object entity)
	{
		Check.NotNull(entity, "entity");
		EntityEntry existingEntry;
		IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContextGettingEntry(entity, this, out existingEntry);
		if (existingEntry == null)
		{
			MetadataWorkspace.ImplicitLoadAssemblyForType(entityWrapper.IdentityType, null);
		}
		VerifyRootForAdd(doAttach: true, entitySetName, entityWrapper, existingEntry, out var entitySet, out var isNoOperation);
		if (isNoOperation)
		{
			return;
		}
		System.Data.Entity.Core.Objects.Internal.TransactionManager transactionManager = ObjectStateManager.TransactionManager;
		transactionManager.BeginAttachTracking();
		try
		{
			ObjectStateManager.TransactionManager.OriginalMergeOption = entityWrapper.MergeOption;
			RelationshipManager relationshipManager = entityWrapper.RelationshipManager;
			bool flag = true;
			try
			{
				AttachSingleObject(entityWrapper, entitySet);
				flag = false;
			}
			finally
			{
				if (flag && entityWrapper.Context == this)
				{
					relationshipManager.NodeVisited = true;
					RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(entityWrapper);
					RelatedEnd.RemoveEntityFromObjectStateManager(entityWrapper);
				}
			}
			relationshipManager.AddRelatedEntitiesToObjectStateManager(doAttach: true);
		}
		finally
		{
			transactionManager.EndAttachTracking();
		}
	}

	public virtual void Attach(IEntityWithKey entity)
	{
		Check.NotNull(entity, "entity");
		if ((object)entity.EntityKey == null)
		{
			throw new InvalidOperationException(Strings.ObjectContext_CannotAttachEntityWithoutKey);
		}
		AttachTo(null, entity);
	}

	internal void AttachSingleObject(IEntityWrapper wrappedEntity, EntitySet entitySet)
	{
		RelationshipManager relationshipManager = wrappedEntity.RelationshipManager;
		EntityKey entityKey = wrappedEntity.GetEntityKeyFromEntity();
		if ((object)entityKey != null)
		{
			EntityUtil.ValidateEntitySetInKey(entityKey, entitySet);
			entityKey.ValidateEntityKey(_workspace, entitySet);
		}
		else
		{
			entityKey = ObjectStateManager.CreateEntityKey(entitySet, wrappedEntity.Entity);
		}
		if (entityKey.IsTemporary)
		{
			throw new InvalidOperationException(Strings.ObjectContext_CannotAttachEntityWithTemporaryKey);
		}
		if (wrappedEntity.EntityKey != entityKey)
		{
			wrappedEntity.EntityKey = entityKey;
		}
		EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(entityKey);
		if (entityEntry != null)
		{
			if (!entityEntry.IsKeyEntry)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(wrappedEntity.IdentityType.FullName));
			}
			ObjectStateManager.PromoteKeyEntryInitialization(this, entityEntry, wrappedEntity, replacingEntry: false);
			ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);
			wrappedEntity.TakeSnapshotOfRelationships(entityEntry);
			ObjectStateManager.PromoteKeyEntry(entityEntry, wrappedEntity, replacingEntry: false, setIsLoaded: false, keyEntryInitialized: true);
			ObjectStateManager.FixupReferencesByForeignKeys(entityEntry);
			relationshipManager.CheckReferentialConstraintProperties(entityEntry);
		}
		else
		{
			VerifyContextForAddOrAttach(wrappedEntity);
			wrappedEntity.Context = this;
			entityEntry = ObjectStateManager.AttachEntry(entityKey, wrappedEntity, entitySet);
			ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);
			wrappedEntity.AttachContext(this, entitySet, MergeOption.AppendOnly);
			ObjectStateManager.FixupReferencesByForeignKeys(entityEntry);
			wrappedEntity.TakeSnapshotOfRelationships(entityEntry);
			relationshipManager.CheckReferentialConstraintProperties(entityEntry);
		}
	}

	private void VerifyContextForAddOrAttach(IEntityWrapper wrappedEntity)
	{
		if (wrappedEntity.Context != null && wrappedEntity.Context != this && !wrappedEntity.Context.ObjectStateManager.IsDisposed && wrappedEntity.MergeOption != MergeOption.NoTracking)
		{
			throw new InvalidOperationException(Strings.Entity_EntityCantHaveMultipleChangeTrackers);
		}
	}

	public virtual EntityKey CreateEntityKey(string entitySetName, object entity)
	{
		Check.NotNull(entity, "entity");
		Check.NotEmpty(entitySetName, "entitySetName");
		MetadataWorkspace.ImplicitLoadAssemblyForType(EntityUtil.GetEntityIdentityType(entity.GetType()), null);
		EntitySet entitySetFromName = GetEntitySetFromName(entitySetName);
		return ObjectStateManager.CreateEntityKey(entitySetFromName, entity);
	}

	internal EntitySet GetEntitySetFromName(string entitySetName)
	{
		GetEntitySetName(entitySetName, "entitySetName", this, out var entityset, out var container);
		return GetEntitySet(entityset, container);
	}

	private void AddRefreshKey(object entityLike, Dictionary<EntityKey, EntityEntry> entities, Dictionary<EntitySet, List<EntityKey>> currentKeys)
	{
		if (entityLike == null)
		{
			throw new InvalidOperationException(Strings.ObjectContext_NthElementIsNull(entities.Count));
		}
		EntityKey entityKey = EntityWrapperFactory.WrapEntityUsingContext(entityLike, this).EntityKey;
		RefreshCheck(entities, entityKey);
		EntitySet entitySet = entityKey.GetEntitySet(MetadataWorkspace);
		List<EntityKey> value = null;
		if (!currentKeys.TryGetValue(entitySet, out value))
		{
			value = new List<EntityKey>();
			currentKeys.Add(entitySet, value);
		}
		value.Add(entityKey);
	}

	public virtual ObjectSet<TEntity> CreateObjectSet<TEntity>() where TEntity : class
	{
		return new ObjectSet<TEntity>(GetEntitySetForType(typeof(TEntity), "TEntity"), this);
	}

	public virtual ObjectSet<TEntity> CreateObjectSet<TEntity>(string entitySetName) where TEntity : class
	{
		return new ObjectSet<TEntity>(GetEntitySetForNameAndType(entitySetName, typeof(TEntity), "TEntity"), this);
	}

	private EntitySet GetEntitySetForType(Type entityCLRType, string exceptionParameterName)
	{
		EntitySet entitySet = null;
		EntityContainer defaultContainer = Perspective.GetDefaultContainer();
		if (defaultContainer == null)
		{
			foreach (EntityContainer item in MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace))
			{
				EntitySet entitySetFromContainer = GetEntitySetFromContainer(item, entityCLRType, exceptionParameterName);
				if (entitySetFromContainer != null)
				{
					if (entitySet != null)
					{
						throw new ArgumentException(Strings.ObjectContext_MultipleEntitySetsFoundInAllContainers(entityCLRType.FullName), exceptionParameterName);
					}
					entitySet = entitySetFromContainer;
				}
			}
		}
		else
		{
			entitySet = GetEntitySetFromContainer(defaultContainer, entityCLRType, exceptionParameterName);
		}
		if (entitySet == null)
		{
			throw new ArgumentException(Strings.ObjectContext_NoEntitySetFoundForType(entityCLRType.FullName), exceptionParameterName);
		}
		return entitySet;
	}

	private EntitySet GetEntitySetFromContainer(EntityContainer container, Type entityCLRType, string exceptionParameterName)
	{
		EdmType edmType = GetTypeUsage(entityCLRType).EdmType;
		EntitySet entitySet = null;
		foreach (EntitySetBase baseEntitySet in container.BaseEntitySets)
		{
			if (baseEntitySet.BuiltInTypeKind == BuiltInTypeKind.EntitySet && baseEntitySet.ElementType == edmType)
			{
				if (entitySet != null)
				{
					throw new ArgumentException(Strings.ObjectContext_MultipleEntitySetsFoundInSingleContainer(entityCLRType.FullName, container.Name), exceptionParameterName);
				}
				entitySet = (EntitySet)baseEntitySet;
			}
		}
		return entitySet;
	}

	private EntitySet GetEntitySetForNameAndType(string entitySetName, Type entityCLRType, string exceptionParameterName)
	{
		EntitySet entitySetFromName = GetEntitySetFromName(entitySetName);
		EdmType edmType = GetTypeUsage(entityCLRType).EdmType;
		if (entitySetFromName.ElementType != edmType)
		{
			throw new ArgumentException(Strings.ObjectContext_InvalidObjectSetTypeForEntitySet(entityCLRType.FullName, entitySetFromName.ElementType.FullName, entitySetName), exceptionParameterName);
		}
		return entitySetFromName;
	}

	internal virtual void EnsureConnection(bool shouldMonitorTransactions)
	{
		if (shouldMonitorTransactions)
		{
			EnsureTransactionHandlerRegistered();
		}
		if (Connection.State == ConnectionState.Broken)
		{
			Connection.Close();
		}
		if (Connection.State == ConnectionState.Closed)
		{
			Connection.Open();
			_openedConnection = true;
		}
		if (_openedConnection)
		{
			_connectionRequestCount++;
		}
		try
		{
			Transaction current = Transaction.Current;
			EnsureContextIsEnlistedInCurrentTransaction(current, delegate
			{
				Connection.Open();
				return true;
			}, defaultValue: false);
			_lastTransaction = current;
		}
		catch (Exception)
		{
			ReleaseConnection();
			throw;
		}
	}

	internal virtual async Task EnsureConnectionAsync(bool shouldMonitorTransactions, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (shouldMonitorTransactions)
		{
			EnsureTransactionHandlerRegistered();
		}
		if (Connection.State == ConnectionState.Broken)
		{
			Connection.Close();
		}
		if (Connection.State == ConnectionState.Closed)
		{
			await Connection.OpenAsync(cancellationToken).WithCurrentCulture();
			_openedConnection = true;
		}
		if (_openedConnection)
		{
			_connectionRequestCount++;
		}
		try
		{
			Transaction currentTransaction = Transaction.Current;
			await EnsureContextIsEnlistedInCurrentTransaction<Task<bool>>(currentTransaction, async delegate
			{
				await Connection.OpenAsync(cancellationToken).WithCurrentCulture();
				return true;
			}, Task.FromResult(result: false)).WithCurrentCulture();
			_lastTransaction = currentTransaction;
		}
		catch (Exception)
		{
			ReleaseConnection();
			throw;
		}
	}

	private void EnsureTransactionHandlerRegistered()
	{
		if (_transactionHandler == null && !InterceptionContext.DbContexts.Any((DbContext dbc) => dbc is TransactionContext))
		{
			StoreItemCollection storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
			string name = DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(storeItemCollection.ProviderFactory).Name;
			Func<TransactionHandler> service = DbConfiguration.DependencyResolver.GetService<Func<TransactionHandler>>(new ExecutionStrategyKey(name, Connection.DataSource));
			if (service != null)
			{
				_transactionHandler = service();
				_transactionHandler.Initialize(this);
			}
		}
	}

	private T EnsureContextIsEnlistedInCurrentTransaction<T>(Transaction currentTransaction, Func<T> openConnection, T defaultValue)
	{
		if (Connection.State != ConnectionState.Open)
		{
			throw new InvalidOperationException(Strings.BadConnectionWrapping);
		}
		if ((null != currentTransaction && !currentTransaction.Equals(_lastTransaction)) || (null != _lastTransaction && !_lastTransaction.Equals(currentTransaction)))
		{
			if (!_openedConnection)
			{
				if (currentTransaction != null)
				{
					Connection.EnlistTransaction(currentTransaction);
				}
			}
			else if (_connectionRequestCount > 1)
			{
				if (!(null == _lastTransaction))
				{
					Connection.Close();
					return openConnection();
				}
				Connection.EnlistTransaction(currentTransaction);
			}
		}
		return defaultValue;
	}

	private void ConnectionStateChange(object sender, StateChangeEventArgs e)
	{
		if (e.CurrentState == ConnectionState.Closed)
		{
			_connectionRequestCount = 0;
			_openedConnection = false;
		}
	}

	internal virtual void ReleaseConnection()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
		}
		if (_openedConnection)
		{
			if (_connectionRequestCount > 0)
			{
				_connectionRequestCount--;
			}
			if (_connectionRequestCount == 0)
			{
				Connection.Close();
				_openedConnection = false;
			}
		}
	}

	public virtual ObjectQuery<T> CreateQuery<T>(string queryString, params ObjectParameter[] parameters)
	{
		Check.NotNull(queryString, "queryString");
		Check.NotNull(parameters, "parameters");
		MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
		ObjectQuery<T> objectQuery = new ObjectQuery<T>(queryString, this, MergeOption.AppendOnly);
		foreach (ObjectParameter item in parameters)
		{
			objectQuery.Parameters.Add(item);
		}
		return objectQuery;
	}

	private static EntityConnection CreateEntityConnection(string connectionString)
	{
		Check.NotEmpty(connectionString, "connectionString");
		return new EntityConnection(connectionString);
	}

	private MetadataWorkspace RetrieveMetadataWorkspaceFromConnection()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
		}
		return _connection.GetMetadataWorkspace();
	}

	public virtual void DeleteObject(object entity)
	{
		DeleteObject(entity, null);
	}

	internal void DeleteObject(object entity, EntitySet expectedEntitySet)
	{
		EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(entity);
		if (entityEntry == null || entityEntry.Entity != entity)
		{
			throw new InvalidOperationException(Strings.ObjectContext_CannotDeleteEntityNotInObjectStateManager);
		}
		if (expectedEntitySet != null)
		{
			EntitySetBase entitySet = entityEntry.EntitySet;
			if (entitySet != expectedEntitySet)
			{
				throw new InvalidOperationException(Strings.ObjectContext_EntityNotInObjectSet_Delete(entitySet.EntityContainer.Name, entitySet.Name, expectedEntitySet.EntityContainer.Name, expectedEntitySet.Name));
			}
		}
		entityEntry.Delete();
	}

	public virtual void Detach(object entity)
	{
		Detach(entity, null);
	}

	internal void Detach(object entity, EntitySet expectedEntitySet)
	{
		EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(entity);
		if (entityEntry == null || entityEntry.Entity != entity || entityEntry.Entity == null)
		{
			throw new InvalidOperationException(Strings.ObjectContext_CannotDetachEntityNotInObjectStateManager);
		}
		if (expectedEntitySet != null)
		{
			EntitySetBase entitySet = entityEntry.EntitySet;
			if (entitySet != expectedEntitySet)
			{
				throw new InvalidOperationException(Strings.ObjectContext_EntityNotInObjectSet_Detach(entitySet.EntityContainer.Name, entitySet.Name, expectedEntitySet.EntityContainer.Name, expectedEntitySet.Name));
			}
		}
		entityEntry.Detach();
	}

	~ObjectContext()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		if (_transactionHandler != null)
		{
			_transactionHandler.Dispose();
		}
		if (disposing)
		{
			if (_connection != null)
			{
				_connection.StateChange -= ConnectionStateChange;
				if (_contextOwnsConnection)
				{
					_connection.Dispose();
				}
			}
			_connection = null;
			if (_objectStateManager != null)
			{
				_objectStateManager.Dispose();
			}
		}
		_disposed = true;
	}

	internal EntitySet GetEntitySet(string entitySetName, string entityContainerName)
	{
		EntityContainer entityContainer = null;
		if (string.IsNullOrEmpty(entityContainerName))
		{
			entityContainer = Perspective.GetDefaultContainer();
		}
		else if (!MetadataWorkspace.TryGetEntityContainer(entityContainerName, DataSpace.CSpace, out entityContainer))
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntityContainerNotFoundForName(entityContainerName));
		}
		EntitySet entitySet = null;
		if (!entityContainer.TryGetEntitySetByName(entitySetName, ignoreCase: false, out entitySet))
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntitySetNotFoundForName(TypeHelpers.GetFullName(entityContainer.Name, entitySetName)));
		}
		return entitySet;
	}

	private static void GetEntitySetName(string qualifiedName, string parameterName, ObjectContext context, out string entityset, out string container)
	{
		entityset = null;
		container = null;
		Check.NotEmpty(qualifiedName, parameterName);
		string[] array = qualifiedName.Split(new char[1] { '.' });
		if (array.Length > 2)
		{
			throw new ArgumentException(Strings.ObjectContext_QualfiedEntitySetName, parameterName);
		}
		if (array.Length == 1)
		{
			entityset = array[0];
		}
		else
		{
			container = array[0];
			entityset = array[1];
			if (container == null || container.Length == 0)
			{
				throw new ArgumentException(Strings.ObjectContext_QualfiedEntitySetName, parameterName);
			}
		}
		if (entityset == null || entityset.Length == 0)
		{
			throw new ArgumentException(Strings.ObjectContext_QualfiedEntitySetName, parameterName);
		}
		if (context != null && string.IsNullOrEmpty(container) && context.Perspective.GetDefaultContainer() == null)
		{
			throw new ArgumentException(Strings.ObjectContext_ContainerQualifiedEntitySetNameRequired, parameterName);
		}
	}

	private void ValidateEntitySet(EntitySet entitySet, Type entityType)
	{
		TypeUsage typeUsage = GetTypeUsage(entityType);
		if (!entitySet.ElementType.IsAssignableFrom(typeUsage.EdmType))
		{
			throw new ArgumentException(Strings.ObjectContext_InvalidEntitySetOnEntity(entitySet.Name, entityType), "entity");
		}
	}

	internal TypeUsage GetTypeUsage(Type entityCLRType)
	{
		MetadataWorkspace.ImplicitLoadAssemblyForType(entityCLRType, Assembly.GetCallingAssembly());
		TypeUsage outTypeUsage = null;
		if (!Perspective.TryGetType(entityCLRType, out outTypeUsage) || !TypeSemantics.IsEntityType(outTypeUsage))
		{
			throw new InvalidOperationException(Strings.ObjectContext_NoMappingForEntityType(entityCLRType.FullName));
		}
		return outTypeUsage;
	}

	public virtual object GetObjectByKey(EntityKey key)
	{
		Check.NotNull(key, "key");
		EntitySet entitySet = key.GetEntitySet(MetadataWorkspace);
		MetadataWorkspace.ImplicitLoadFromEntityType(entitySet.ElementType, Assembly.GetCallingAssembly());
		if (!TryGetObjectByKey(key, out var value))
		{
			throw new ObjectNotFoundException(Strings.ObjectContext_ObjectNotFound);
		}
		return value;
	}

	public virtual void Refresh(RefreshMode refreshMode, IEnumerable collection)
	{
		Check.NotNull(collection, "collection");
		EntityUtil.CheckArgumentRefreshMode(refreshMode);
		RefreshEntities(refreshMode, collection);
	}

	public virtual void Refresh(RefreshMode refreshMode, object entity)
	{
		Check.NotNull(entity, "entity");
		EntityUtil.CheckArgumentRefreshMode(refreshMode);
		RefreshEntities(refreshMode, new object[1] { entity });
	}

	public Task RefreshAsync(RefreshMode refreshMode, IEnumerable collection)
	{
		return RefreshAsync(refreshMode, collection, CancellationToken.None);
	}

	public virtual Task RefreshAsync(RefreshMode refreshMode, IEnumerable collection, CancellationToken cancellationToken)
	{
		Check.NotNull(collection, "collection");
		cancellationToken.ThrowIfCancellationRequested();
		AsyncMonitor.EnsureNotEntered();
		EntityUtil.CheckArgumentRefreshMode(refreshMode);
		return RefreshEntitiesAsync(refreshMode, collection, cancellationToken);
	}

	public Task RefreshAsync(RefreshMode refreshMode, object entity)
	{
		return RefreshAsync(refreshMode, entity, CancellationToken.None);
	}

	public virtual Task RefreshAsync(RefreshMode refreshMode, object entity, CancellationToken cancellationToken)
	{
		Check.NotNull(entity, "entity");
		cancellationToken.ThrowIfCancellationRequested();
		AsyncMonitor.EnsureNotEntered();
		EntityUtil.CheckArgumentRefreshMode(refreshMode);
		return RefreshEntitiesAsync(refreshMode, new object[1] { entity }, cancellationToken);
	}

	private void RefreshCheck(Dictionary<EntityKey, EntityEntry> entities, EntityKey key)
	{
		EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(key);
		if (entityEntry == null)
		{
			throw new InvalidOperationException(Strings.ObjectContext_NthElementNotInObjectStateManager(entities.Count));
		}
		if (EntityState.Added == entityEntry.State)
		{
			throw new InvalidOperationException(Strings.ObjectContext_NthElementInAddedState(entities.Count));
		}
		try
		{
			entities.Add(key, entityEntry);
		}
		catch (ArgumentException)
		{
			throw new InvalidOperationException(Strings.ObjectContext_NthElementIsDuplicate(entities.Count));
		}
	}

	private void RefreshEntities(RefreshMode refreshMode, IEnumerable collection)
	{
		AsyncMonitor.EnsureNotEntered();
		bool flag = false;
		try
		{
			Dictionary<EntityKey, EntityEntry> dictionary = new Dictionary<EntityKey, EntityEntry>(RefreshEntitiesSize(collection));
			Dictionary<EntitySet, List<EntityKey>> dictionary2 = new Dictionary<EntitySet, List<EntityKey>>();
			foreach (object item in collection)
			{
				AddRefreshKey(item, dictionary, dictionary2);
			}
			if (dictionary2.Count > 0)
			{
				EnsureConnection(shouldMonitorTransactions: false);
				flag = true;
				foreach (EntitySet key in dictionary2.Keys)
				{
					List<EntityKey> list = dictionary2[key];
					for (int num = 0; num < list.Count; num = BatchRefreshEntitiesByKey(refreshMode, dictionary, key, list, num))
					{
					}
				}
			}
			if (RefreshMode.StoreWins == refreshMode)
			{
				foreach (KeyValuePair<EntityKey, EntityEntry> item2 in dictionary)
				{
					if (EntityState.Detached != item2.Value.State)
					{
						ObjectStateManager.TransactionManager.BeginDetaching();
						try
						{
							item2.Value.Delete();
						}
						finally
						{
							ObjectStateManager.TransactionManager.EndDetaching();
						}
						item2.Value.AcceptChanges();
					}
				}
				return;
			}
			if (RefreshMode.ClientWins != refreshMode || 0 >= dictionary.Count)
			{
				return;
			}
			string value = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<EntityKey, EntityEntry> item3 in dictionary)
			{
				if (item3.Value.State == EntityState.Deleted)
				{
					item3.Value.AcceptChanges();
					continue;
				}
				stringBuilder.Append(value).Append(Environment.NewLine);
				stringBuilder.Append('\'').Append(item3.Value.WrappedEntity.IdentityType.FullName).Append('\'');
				value = ",";
			}
			if (stringBuilder.Length > 0)
			{
				throw new InvalidOperationException(Strings.ObjectContext_ClientEntityRemovedFromStore(stringBuilder.ToString()));
			}
		}
		finally
		{
			if (flag)
			{
				ReleaseConnection();
			}
		}
	}

	private int BatchRefreshEntitiesByKey(RefreshMode refreshMode, Dictionary<EntityKey, EntityEntry> trackedEntities, EntitySet targetSet, List<EntityKey> targetKeys, int startFrom)
	{
		Tuple<ObjectQueryExecutionPlan, int> queryPlanAndNextPosition = PrepareRefreshQuery(refreshMode, targetSet, targetKeys, startFrom);
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		ObjectResult<object> results = executionStrategy.Execute(() => ExecuteInTransaction(() => queryPlanAndNextPosition.Item1.Execute<object>(this, null), executionStrategy, startLocalTransaction: false, releaseConnectionOnSuccess: true));
		ProcessRefreshedEntities(trackedEntities, results);
		return queryPlanAndNextPosition.Item2;
	}

	private async Task RefreshEntitiesAsync(RefreshMode refreshMode, IEnumerable collection, CancellationToken cancellationToken)
	{
		AsyncMonitor.Enter();
		bool openedConnection = false;
		try
		{
			Dictionary<EntityKey, EntityEntry> entities = new Dictionary<EntityKey, EntityEntry>(RefreshEntitiesSize(collection));
			Dictionary<EntitySet, List<EntityKey>> refreshKeys = new Dictionary<EntitySet, List<EntityKey>>();
			foreach (object item in collection)
			{
				AddRefreshKey(item, entities, refreshKeys);
			}
			if (refreshKeys.Count > 0)
			{
				await EnsureConnectionAsync(shouldMonitorTransactions: false, cancellationToken).WithCurrentCulture();
				openedConnection = true;
				foreach (EntitySet targetSet in refreshKeys.Keys)
				{
					List<EntityKey> setKeys = refreshKeys[targetSet];
					for (int num = 0; num < setKeys.Count; num = await BatchRefreshEntitiesByKeyAsync(refreshMode, entities, targetSet, setKeys, num, cancellationToken).WithCurrentCulture())
					{
					}
				}
			}
			if (RefreshMode.StoreWins == refreshMode)
			{
				foreach (KeyValuePair<EntityKey, EntityEntry> item2 in entities)
				{
					if (EntityState.Detached != item2.Value.State)
					{
						ObjectStateManager.TransactionManager.BeginDetaching();
						try
						{
							item2.Value.Delete();
						}
						finally
						{
							ObjectStateManager.TransactionManager.EndDetaching();
						}
						item2.Value.AcceptChanges();
					}
				}
			}
			else
			{
				if (RefreshMode.ClientWins != refreshMode || 0 >= entities.Count)
				{
					return;
				}
				string value = string.Empty;
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<EntityKey, EntityEntry> item3 in entities)
				{
					if (item3.Value.State == EntityState.Deleted)
					{
						item3.Value.AcceptChanges();
						continue;
					}
					stringBuilder.Append(value).Append(Environment.NewLine);
					stringBuilder.Append('\'').Append(item3.Value.WrappedEntity.IdentityType.FullName).Append('\'');
					value = ",";
				}
				if (stringBuilder.Length > 0)
				{
					throw new InvalidOperationException(Strings.ObjectContext_ClientEntityRemovedFromStore(stringBuilder.ToString()));
				}
			}
		}
		finally
		{
			if (openedConnection)
			{
				ReleaseConnection();
			}
			AsyncMonitor.Exit();
		}
	}

	private async Task<int> BatchRefreshEntitiesByKeyAsync(RefreshMode refreshMode, Dictionary<EntityKey, EntityEntry> trackedEntities, EntitySet targetSet, List<EntityKey> targetKeys, int startFrom, CancellationToken cancellationToken)
	{
		Tuple<ObjectQueryExecutionPlan, int> queryPlanAndNextPosition = PrepareRefreshQuery(refreshMode, targetSet, targetKeys, startFrom);
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		ProcessRefreshedEntities(trackedEntities, await executionStrategy.ExecuteAsync(() => ExecuteInTransactionAsync(() => queryPlanAndNextPosition.Item1.ExecuteAsync<object>(this, null, cancellationToken), executionStrategy, startLocalTransaction: false, releaseConnectionOnSuccess: true, cancellationToken), cancellationToken).WithCurrentCulture());
		return queryPlanAndNextPosition.Item2;
	}

	internal virtual Tuple<ObjectQueryExecutionPlan, int> PrepareRefreshQuery(RefreshMode refreshMode, EntitySet targetSet, List<EntityKey> targetKeys, int startFrom)
	{
		DbExpressionBinding dbExpressionBinding = targetSet.Scan().BindAs("EntitySet");
		DbExpression refKey = dbExpressionBinding.Variable.GetEntityRef().GetRefKey();
		int num = Math.Min(250, targetKeys.Count - startFrom);
		DbExpression[] array = new DbExpression[num];
		for (int i = 0; i < num; i++)
		{
			DbExpression right = DbExpressionBuilder.NewRow(targetKeys[startFrom++].GetKeyValueExpressions(targetSet));
			array[i] = refKey.Equal(right);
		}
		DbExpression predicate = Helpers.BuildBalancedTreeInPlace(array, DbExpressionBuilder.Or);
		DbExpression query = dbExpressionBinding.Filter(predicate);
		DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(MetadataWorkspace, DataSpace.CSpace, query, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false);
		MergeOption mergeOption = ((RefreshMode.StoreWins == refreshMode) ? MergeOption.OverwriteChanges : MergeOption.PreserveChanges);
		return new Tuple<ObjectQueryExecutionPlan, int>(_objectQueryExecutionPlanFactory.Prepare(this, tree, typeof(object), mergeOption, streaming: false, null, null, DbExpressionBuilder.AliasGenerator), startFrom);
	}

	private void ProcessRefreshedEntities(Dictionary<EntityKey, EntityEntry> trackedEntities, ObjectResult<object> results)
	{
		foreach (object result in results)
		{
			EntityEntry entityEntry = ObjectStateManager.FindEntityEntry(result);
			if (entityEntry != null && entityEntry.State == EntityState.Modified)
			{
				entityEntry.SetModifiedAll();
			}
			EntityKey entityKey = EntityWrapperFactory.WrapEntityUsingContext(result, this).EntityKey;
			if ((object)entityKey == null)
			{
				throw Error.EntityKey_UnexpectedNull();
			}
			if (!trackedEntities.Remove(entityKey))
			{
				throw new InvalidOperationException(Strings.ObjectContext_StoreEntityNotPresentInClient);
			}
		}
	}

	private static int RefreshEntitiesSize(IEnumerable collection)
	{
		if (!(collection is ICollection collection2))
		{
			return 0;
		}
		return collection2.Count;
	}

	public virtual int SaveChanges()
	{
		return SaveChanges(SaveOptions.AcceptAllChangesAfterSave | SaveOptions.DetectChangesBeforeSave);
	}

	public virtual Task<int> SaveChangesAsync()
	{
		return SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave | SaveOptions.DetectChangesBeforeSave, CancellationToken.None);
	}

	public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
	{
		return SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave | SaveOptions.DetectChangesBeforeSave, cancellationToken);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[Obsolete("Use SaveChanges(SaveOptions options) instead.")]
	public virtual int SaveChanges(bool acceptChangesDuringSave)
	{
		return SaveChanges(acceptChangesDuringSave ? (SaveOptions.AcceptAllChangesAfterSave | SaveOptions.DetectChangesBeforeSave) : SaveOptions.DetectChangesBeforeSave);
	}

	public virtual int SaveChanges(SaveOptions options)
	{
		return SaveChangesInternal(options, executeInExistingTransaction: false);
	}

	internal int SaveChangesInternal(SaveOptions options, bool executeInExistingTransaction)
	{
		AsyncMonitor.EnsureNotEntered();
		PrepareToSaveChanges(options);
		int result = 0;
		if (ObjectStateManager.HasChanges())
		{
			if (executeInExistingTransaction)
			{
				result = SaveChangesToStore(options, null, startLocalTransaction: false);
			}
			else
			{
				IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
				result = executionStrategy.Execute(() => SaveChangesToStore(options, executionStrategy, startLocalTransaction: true));
			}
		}
		return result;
	}

	public virtual Task<int> SaveChangesAsync(SaveOptions options)
	{
		return SaveChangesAsync(options, CancellationToken.None);
	}

	public virtual Task<int> SaveChangesAsync(SaveOptions options, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AsyncMonitor.EnsureNotEntered();
		return SaveChangesInternalAsync(options, executeInExistingTransaction: false, cancellationToken);
	}

	internal async Task<int> SaveChangesInternalAsync(SaveOptions options, bool executeInExistingTransaction, CancellationToken cancellationToken)
	{
		AsyncMonitor.Enter();
		try
		{
			PrepareToSaveChanges(options);
			int result = 0;
			if (ObjectStateManager.HasChanges())
			{
				if (executeInExistingTransaction)
				{
					result = await SaveChangesToStoreAsync(options, null, startLocalTransaction: false, cancellationToken).WithCurrentCulture();
				}
				else
				{
					IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
					result = await executionStrategy.ExecuteAsync(() => SaveChangesToStoreAsync(options, executionStrategy, startLocalTransaction: true, cancellationToken), cancellationToken).WithCurrentCulture();
				}
			}
			return result;
		}
		finally
		{
			AsyncMonitor.Exit();
		}
	}

	private void PrepareToSaveChanges(SaveOptions options)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
		}
		OnSavingChanges();
		if ((SaveOptions.DetectChangesBeforeSave & options) != 0)
		{
			ObjectStateManager.DetectChanges();
		}
		if (ObjectStateManager.SomeEntryWithConceptualNullExists())
		{
			throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
		}
	}

	private int SaveChangesToStore(SaveOptions options, IDbExecutionStrategy executionStrategy, bool startLocalTransaction)
	{
		_adapter.AcceptChangesDuringUpdate = false;
		_adapter.Connection = Connection;
		_adapter.CommandTimeout = CommandTimeout;
		int result = ExecuteInTransaction(() => _adapter.Update(), executionStrategy, startLocalTransaction, releaseConnectionOnSuccess: true);
		if ((SaveOptions.AcceptAllChangesAfterSave & options) != 0)
		{
			try
			{
				AcceptAllChanges();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.ObjectContext_AcceptAllChangesFailure(ex.Message), ex);
			}
		}
		return result;
	}

	private async Task<int> SaveChangesToStoreAsync(SaveOptions options, IDbExecutionStrategy executionStrategy, bool startLocalTransaction, CancellationToken cancellationToken)
	{
		_adapter.AcceptChangesDuringUpdate = false;
		_adapter.Connection = Connection;
		_adapter.CommandTimeout = CommandTimeout;
		int result = await ExecuteInTransactionAsync(() => _adapter.UpdateAsync(cancellationToken), executionStrategy, startLocalTransaction, releaseConnectionOnSuccess: true, cancellationToken).WithCurrentCulture();
		if ((SaveOptions.AcceptAllChangesAfterSave & options) != 0)
		{
			try
			{
				AcceptAllChanges();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.ObjectContext_AcceptAllChangesFailure(ex.Message), ex);
			}
		}
		return result;
	}

	internal virtual T ExecuteInTransaction<T>(Func<T> func, IDbExecutionStrategy executionStrategy, bool startLocalTransaction, bool releaseConnectionOnSuccess)
	{
		EnsureConnection(startLocalTransaction);
		bool flag = false;
		EntityConnection entityConnection = (EntityConnection)Connection;
		if (entityConnection.CurrentTransaction == null && !entityConnection.EnlistedInUserTransaction && _lastTransaction == null)
		{
			flag = startLocalTransaction;
		}
		else if (executionStrategy != null && executionStrategy.RetriesOnFailure)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_ExistingTransaction(executionStrategy.GetType().Name));
		}
		DbTransaction dbTransaction = null;
		try
		{
			if (flag)
			{
				dbTransaction = entityConnection.BeginTransaction();
			}
			T result = func();
			dbTransaction?.Commit();
			if (releaseConnectionOnSuccess)
			{
				ReleaseConnection();
			}
			return result;
		}
		catch (Exception)
		{
			ReleaseConnection();
			throw;
		}
		finally
		{
			dbTransaction?.Dispose();
		}
	}

	internal virtual async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func, IDbExecutionStrategy executionStrategy, bool startLocalTransaction, bool releaseConnectionOnSuccess, CancellationToken cancellationToken)
	{
		await EnsureConnectionAsync(startLocalTransaction, cancellationToken).WithCurrentCulture();
		bool flag = false;
		EntityConnection entityConnection = (EntityConnection)Connection;
		if (entityConnection.CurrentTransaction == null && !entityConnection.EnlistedInUserTransaction && _lastTransaction == null)
		{
			flag = startLocalTransaction;
		}
		else if (executionStrategy.RetriesOnFailure)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_ExistingTransaction(executionStrategy.GetType().Name));
		}
		DbTransaction localTransaction = null;
		try
		{
			if (flag)
			{
				localTransaction = entityConnection.BeginTransaction();
			}
			T result = await func().WithCurrentCulture();
			localTransaction?.Commit();
			if (releaseConnectionOnSuccess)
			{
				ReleaseConnection();
			}
			return result;
		}
		catch (Exception)
		{
			ReleaseConnection();
			throw;
		}
		finally
		{
			localTransaction?.Dispose();
		}
	}

	public virtual void DetectChanges()
	{
		ObjectStateManager.DetectChanges();
	}

	public virtual bool TryGetObjectByKey(EntityKey key, out object value)
	{
		ObjectStateManager.TryGetEntityEntry(key, out var entry);
		if (entry != null && !entry.IsKeyEntry)
		{
			value = entry.Entity;
			return value != null;
		}
		if (key.IsTemporary)
		{
			value = null;
			return false;
		}
		EntitySet entitySet = key.GetEntitySet(MetadataWorkspace);
		key.ValidateEntityKey(_workspace, entitySet, isArgumentException: true, "key");
		MetadataWorkspace.ImplicitLoadFromEntityType(entitySet.ElementType, Assembly.GetCallingAssembly());
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("SELECT VALUE X FROM {0}.{1} AS X WHERE ", EntityUtil.QuoteIdentifier(entitySet.EntityContainer.Name), EntityUtil.QuoteIdentifier(entitySet.Name));
		EntityKeyMember[] entityKeyValues = key.EntityKeyValues;
		ReadOnlyMetadataCollection<EdmMember> keyMembers = entitySet.ElementType.KeyMembers;
		ObjectParameter[] array = new ObjectParameter[entityKeyValues.Length];
		for (int i = 0; i < entityKeyValues.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" AND ");
			}
			string text = string.Format(CultureInfo.InvariantCulture, "p{0}", new object[1] { i.ToString(CultureInfo.InvariantCulture) });
			stringBuilder.AppendFormat("X.{0} = @{1}", EntityUtil.QuoteIdentifier(entityKeyValues[i].Key), text);
			array[i] = new ObjectParameter(text, entityKeyValues[i].Value);
			EdmMember item = null;
			if (keyMembers.TryGetValue(entityKeyValues[i].Key, ignoreCase: true, out item))
			{
				array[i].TypeUsage = item.TypeUsage;
			}
		}
		object obj = null;
		foreach (object item2 in CreateQuery<object>(stringBuilder.ToString(), array).Execute(MergeOption.AppendOnly))
		{
			obj = item2;
		}
		value = obj;
		return value != null;
	}

	public ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, params ObjectParameter[] parameters)
	{
		Check.NotNull(parameters, "parameters");
		return ExecuteFunction<TElement>(functionName, MergeOption.AppendOnly, parameters);
	}

	public virtual ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, MergeOption mergeOption, params ObjectParameter[] parameters)
	{
		Check.NotNull(parameters, "parameters");
		Check.NotEmpty(functionName, "functionName");
		return ExecuteFunction<TElement>(functionName, new ExecutionOptions(mergeOption), parameters);
	}

	public virtual ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, ExecutionOptions executionOptions, params ObjectParameter[] parameters)
	{
		Check.NotNull(parameters, "parameters");
		Check.NotEmpty(functionName, "functionName");
		AsyncMonitor.EnsureNotEntered();
		EdmFunction functionImport;
		EntityCommand entityCommand = CreateEntityCommandForFunctionImport(functionName, out functionImport, parameters);
		int num = Math.Max(1, functionImport.ReturnParameters.Count);
		EdmType[] expectedEdmTypes = new EdmType[num];
		expectedEdmTypes[0] = MetadataHelper.GetAndCheckFunctionImportReturnType<TElement>(functionImport, 0, MetadataWorkspace);
		for (int i = 1; i < num; i++)
		{
			if (!MetadataHelper.TryGetFunctionImportReturnType<EdmType>(functionImport, i, out expectedEdmTypes[i]))
			{
				throw EntityUtil.ExecuteFunctionCalledWithNonReaderFunction(functionImport);
			}
		}
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		if (executionStrategy.RetriesOnFailure && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
		}
		if (!executionOptions.UserSpecifiedStreaming.HasValue)
		{
			executionOptions = new ExecutionOptions(executionOptions.MergeOption, !executionStrategy.RetriesOnFailure);
		}
		bool startLocalTransaction = !executionOptions.UserSpecifiedStreaming.Value && _options.EnsureTransactionsForFunctionsAndCommands;
		return executionStrategy.Execute(() => ExecuteInTransaction(() => CreateFunctionObjectResult<TElement>(entityCommand, functionImport.EntitySets, expectedEdmTypes, executionOptions), executionStrategy, startLocalTransaction, !executionOptions.UserSpecifiedStreaming.Value));
	}

	public virtual int ExecuteFunction(string functionName, params ObjectParameter[] parameters)
	{
		Check.NotNull(parameters, "parameters");
		Check.NotEmpty(functionName, "functionName");
		AsyncMonitor.EnsureNotEntered();
		EdmFunction functionImport;
		EntityCommand entityCommand = CreateEntityCommandForFunctionImport(functionName, out functionImport, parameters);
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		return executionStrategy.Execute(() => ExecuteInTransaction(() => ExecuteFunctionCommand(entityCommand), executionStrategy, _options.EnsureTransactionsForFunctionsAndCommands, releaseConnectionOnSuccess: true));
	}

	private static int ExecuteFunctionCommand(EntityCommand entityCommand)
	{
		entityCommand.Prepare();
		try
		{
			return entityCommand.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableEntityExceptionType())
			{
				throw new EntityCommandExecutionException(Strings.EntityClient_CommandExecutionFailed, ex);
			}
			throw;
		}
	}

	private EntityCommand CreateEntityCommandForFunctionImport(string functionName, out EdmFunction functionImport, params ObjectParameter[] parameters)
	{
		for (int i = 0; i < parameters.Length; i++)
		{
			ObjectParameter objectParameter = parameters[i];
			if (objectParameter == null)
			{
				throw new InvalidOperationException(Strings.ObjectContext_ExecuteFunctionCalledWithNullParameter(i));
			}
		}
		functionImport = MetadataHelper.GetFunctionImport(functionName, DefaultContainerName, MetadataWorkspace, out var containerName, out var functionImportName);
		EntityConnection connection = (EntityConnection)Connection;
		EntityCommand entityCommand = new EntityCommand(InterceptionContext);
		entityCommand.CommandType = CommandType.StoredProcedure;
		entityCommand.CommandText = containerName + "." + functionImportName;
		entityCommand.Connection = connection;
		if (CommandTimeout.HasValue)
		{
			entityCommand.CommandTimeout = CommandTimeout.Value;
		}
		PopulateFunctionImportEntityCommandParameters(parameters, functionImport, entityCommand);
		return entityCommand;
	}

	private ObjectResult<TElement> CreateFunctionObjectResult<TElement>(EntityCommand entityCommand, ReadOnlyCollection<EntitySet> entitySets, EdmType[] edmTypes, ExecutionOptions executionOptions)
	{
		EntityCommandDefinition commandDefinition = entityCommand.GetCommandDefinition();
		DbDataReader dbDataReader = null;
		try
		{
			dbDataReader = commandDefinition.ExecuteStoreCommands(entityCommand, (!executionOptions.UserSpecifiedStreaming.Value) ? CommandBehavior.SequentialAccess : CommandBehavior.Default);
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableEntityExceptionType())
			{
				throw new EntityCommandExecutionException(Strings.EntityClient_CommandExecutionFailed, ex);
			}
			throw;
		}
		ShaperFactory<TElement> shaperFactory = null;
		if (!executionOptions.UserSpecifiedStreaming.Value)
		{
			BufferedDataReader bufferedDataReader = null;
			try
			{
				StoreItemCollection storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
				DbProviderServices service = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);
				shaperFactory = _translator.TranslateColumnMap<TElement>(commandDefinition.CreateColumnMap(dbDataReader, 0), MetadataWorkspace, null, executionOptions.MergeOption, streaming: false, valueLayer: false);
				bufferedDataReader = new BufferedDataReader(dbDataReader);
				bufferedDataReader.Initialize(storeItemCollection.ProviderManifestToken, service, shaperFactory.ColumnTypes, shaperFactory.NullableColumns);
				dbDataReader = bufferedDataReader;
			}
			catch (Exception)
			{
				bufferedDataReader?.Dispose();
				throw;
			}
		}
		return MaterializedDataRecord(entityCommand, dbDataReader, 0, entitySets, edmTypes, shaperFactory, executionOptions.MergeOption, executionOptions.UserSpecifiedStreaming.Value);
	}

	internal ObjectResult<TElement> MaterializedDataRecord<TElement>(EntityCommand entityCommand, DbDataReader storeReader, int resultSetIndex, ReadOnlyCollection<EntitySet> entitySets, EdmType[] edmTypes, ShaperFactory<TElement> shaperFactory, MergeOption mergeOption, bool streaming)
	{
		EntityCommandDefinition commandDefinition = entityCommand.GetCommandDefinition();
		try
		{
			bool flag = edmTypes.Length <= resultSetIndex + 1;
			EntitySet singleEntitySet = ((entitySets.Count > resultSetIndex) ? entitySets[resultSetIndex] : null);
			if (shaperFactory == null)
			{
				shaperFactory = _translator.TranslateColumnMap<TElement>(commandDefinition.CreateColumnMap(storeReader, resultSetIndex), MetadataWorkspace, null, mergeOption, streaming, valueLayer: false);
			}
			Shaper<TElement> shaper = shaperFactory.Create(storeReader, this, MetadataWorkspace, mergeOption, flag, streaming);
			bool onReaderDisposeHasRun = false;
			Action<object, EventArgs> action = delegate
			{
				if (!onReaderDisposeHasRun)
				{
					onReaderDisposeHasRun = true;
					CommandHelper.ConsumeReader(storeReader);
					entityCommand.NotifyDataReaderClosing();
				}
			};
			NextResultGenerator nextResultGenerator;
			if (flag)
			{
				shaper.OnDone += action.Invoke;
				nextResultGenerator = null;
			}
			else
			{
				nextResultGenerator = new NextResultGenerator(this, entityCommand, edmTypes, entitySets, mergeOption, streaming, resultSetIndex + 1);
			}
			return new ObjectResult<TElement>(shaper, singleEntitySet, TypeUsage.Create(edmTypes[resultSetIndex]), readerOwned: true, streaming, nextResultGenerator, action);
		}
		catch
		{
			ReleaseConnection();
			storeReader.Dispose();
			throw;
		}
	}

	private void PopulateFunctionImportEntityCommandParameters(ObjectParameter[] parameters, EdmFunction functionImport, EntityCommand command)
	{
		for (int i = 0; i < parameters.Length; i++)
		{
			ObjectParameter objectParameter = parameters[i];
			EntityParameter entityParameter = new EntityParameter();
			FunctionParameter functionParameter = FindParameterMetadata(functionImport, parameters, i);
			if (functionParameter != null)
			{
				entityParameter.Direction = MetadataHelper.ParameterModeToParameterDirection(functionParameter.Mode);
				entityParameter.ParameterName = functionParameter.Name;
			}
			else
			{
				entityParameter.ParameterName = objectParameter.Name;
			}
			entityParameter.Value = objectParameter.Value ?? DBNull.Value;
			if (DBNull.Value == entityParameter.Value || entityParameter.Direction != ParameterDirection.Input)
			{
				TypeUsage typeUsage;
				if (functionParameter != null)
				{
					typeUsage = functionParameter.TypeUsage;
				}
				else if (objectParameter.TypeUsage == null)
				{
					if (!Perspective.TryGetTypeByName(objectParameter.MappableType.FullNameWithNesting(), ignoreCase: false, out typeUsage))
					{
						MetadataWorkspace.ImplicitLoadAssemblyForType(objectParameter.MappableType, null);
						Perspective.TryGetTypeByName(objectParameter.MappableType.FullNameWithNesting(), ignoreCase: false, out typeUsage);
					}
				}
				else
				{
					typeUsage = objectParameter.TypeUsage;
				}
				EntityCommandDefinition.PopulateParameterFromTypeUsage(entityParameter, typeUsage, entityParameter.Direction != ParameterDirection.Input);
			}
			if (entityParameter.Direction != ParameterDirection.Input)
			{
				ParameterBinder @object = new ParameterBinder(entityParameter, objectParameter);
				command.OnDataReaderClosing += @object.OnDataReaderClosingHandler;
			}
			command.Parameters.Add(entityParameter);
		}
	}

	private static FunctionParameter FindParameterMetadata(EdmFunction functionImport, ObjectParameter[] parameters, int ordinal)
	{
		string name = parameters[ordinal].Name;
		if (!functionImport.Parameters.TryGetValue(name, ignoreCase: false, out var item))
		{
			int num = 0;
			for (int i = 0; i < parameters.Length; i++)
			{
				if (num >= 2)
				{
					break;
				}
				if (StringComparer.OrdinalIgnoreCase.Equals(parameters[i].Name, name))
				{
					num++;
				}
			}
			if (num == 1)
			{
				functionImport.Parameters.TryGetValue(name, ignoreCase: true, out item);
			}
		}
		return item;
	}

	public virtual void CreateProxyTypes(IEnumerable<Type> types)
	{
		ObjectItemCollection ospaceItems = (ObjectItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
		EntityProxyFactory.TryCreateProxyTypes(from entityType in types.Select(delegate(Type type)
			{
				MetadataWorkspace.ImplicitLoadAssemblyForType(type, null);
				ospaceItems.TryGetItem<EntityType>(type.FullNameWithNesting(), out var item);
				return item;
			})
			where entityType != null
			select entityType, MetadataWorkspace);
	}

	public static IEnumerable<Type> GetKnownProxyTypes()
	{
		return EntityProxyFactory.GetKnownProxyTypes();
	}

	public static Type GetObjectType(Type type)
	{
		Check.NotNull(type, "type");
		if (!EntityProxyFactory.IsProxyType(type))
		{
			return type;
		}
		return type.BaseType();
	}

	public virtual T CreateObject<T>() where T : class
	{
		T val = null;
		Type typeFromHandle = typeof(T);
		MetadataWorkspace.ImplicitLoadAssemblyForType(typeFromHandle, null);
		ClrEntityType item = MetadataWorkspace.GetItem<ClrEntityType>(typeFromHandle.FullNameWithNesting(), DataSpace.OSpace);
		EntityProxyTypeInfo entityProxyTypeInfo = null;
		if (ContextOptions.ProxyCreationEnabled && (entityProxyTypeInfo = EntityProxyFactory.GetProxyType(item, MetadataWorkspace)) != null)
		{
			val = (T)entityProxyTypeInfo.CreateProxyObject();
			IEntityWrapper entityWrapper = EntityWrapperFactory.CreateNewWrapper(val, null);
			entityWrapper.InitializingProxyRelatedEnds = true;
			try
			{
				entityWrapper.AttachContext(this, null, MergeOption.NoTracking);
				entityProxyTypeInfo.SetEntityWrapper(entityWrapper);
				if (entityProxyTypeInfo.InitializeEntityCollections != null)
				{
					entityProxyTypeInfo.InitializeEntityCollections.Invoke(null, new object[1] { entityWrapper });
				}
			}
			finally
			{
				entityWrapper.InitializingProxyRelatedEnds = false;
				entityWrapper.DetachContext();
			}
		}
		else
		{
			val = DelegateFactory.GetConstructorDelegateForType(item)() as T;
		}
		return val;
	}

	public virtual int ExecuteStoreCommand(string commandText, params object[] parameters)
	{
		return ExecuteStoreCommand((!_options.EnsureTransactionsForFunctionsAndCommands) ? TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction, commandText, parameters);
	}

	public virtual int ExecuteStoreCommand(TransactionalBehavior transactionalBehavior, string commandText, params object[] parameters)
	{
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		AsyncMonitor.EnsureNotEntered();
		return executionStrategy.Execute(() => ExecuteInTransaction(() => ExecuteStoreCommandInternal(commandText, parameters), executionStrategy, transactionalBehavior != TransactionalBehavior.DoNotEnsureTransaction, releaseConnectionOnSuccess: true));
	}

	private int ExecuteStoreCommandInternal(string commandText, object[] parameters)
	{
		DbCommand dbCommand = CreateStoreCommand(commandText, parameters);
		try
		{
			return dbCommand.ExecuteNonQuery();
		}
		finally
		{
			dbCommand.Parameters.Clear();
			dbCommand.Dispose();
		}
	}

	public Task<int> ExecuteStoreCommandAsync(string commandText, params object[] parameters)
	{
		return ExecuteStoreCommandAsync((!_options.EnsureTransactionsForFunctionsAndCommands) ? TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction, commandText, CancellationToken.None, parameters);
	}

	public Task<int> ExecuteStoreCommandAsync(TransactionalBehavior transactionalBehavior, string commandText, params object[] parameters)
	{
		return ExecuteStoreCommandAsync(transactionalBehavior, commandText, CancellationToken.None, parameters);
	}

	public virtual Task<int> ExecuteStoreCommandAsync(string commandText, CancellationToken cancellationToken, params object[] parameters)
	{
		return ExecuteStoreCommandAsync((!_options.EnsureTransactionsForFunctionsAndCommands) ? TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction, commandText, cancellationToken, parameters);
	}

	public virtual Task<int> ExecuteStoreCommandAsync(TransactionalBehavior transactionalBehavior, string commandText, CancellationToken cancellationToken, params object[] parameters)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AsyncMonitor.EnsureNotEntered();
		return ExecuteStoreCommandInternalAsync(transactionalBehavior, commandText, cancellationToken, parameters);
	}

	private async Task<int> ExecuteStoreCommandInternalAsync(TransactionalBehavior transactionalBehavior, string commandText, CancellationToken cancellationToken, params object[] parameters)
	{
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		AsyncMonitor.Enter();
		try
		{
			return await executionStrategy.ExecuteAsync(() => ExecuteInTransactionAsync(() => ExecuteStoreCommandInternalAsync(commandText, cancellationToken, parameters), executionStrategy, transactionalBehavior != TransactionalBehavior.DoNotEnsureTransaction, releaseConnectionOnSuccess: true, cancellationToken), cancellationToken).WithCurrentCulture();
		}
		finally
		{
			AsyncMonitor.Exit();
		}
	}

	private async Task<int> ExecuteStoreCommandInternalAsync(string commandText, CancellationToken cancellationToken, object[] parameters)
	{
		DbCommand command = CreateStoreCommand(commandText, parameters);
		try
		{
			return await command.ExecuteNonQueryAsync(cancellationToken).WithCurrentCulture();
		}
		finally
		{
			command.Parameters.Clear();
			command.Dispose();
		}
	}

	public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, params object[] parameters)
	{
		return ExecuteStoreQueryReliably<TElement>(commandText, null, ExecutionOptions.Default, parameters);
	}

	public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, ExecutionOptions executionOptions, params object[] parameters)
	{
		return ExecuteStoreQueryReliably<TElement>(commandText, null, executionOptions, parameters);
	}

	public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, string entitySetName, MergeOption mergeOption, params object[] parameters)
	{
		Check.NotEmpty(entitySetName, "entitySetName");
		return ExecuteStoreQueryReliably<TElement>(commandText, entitySetName, new ExecutionOptions(mergeOption), parameters);
	}

	public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
	{
		Check.NotEmpty(entitySetName, "entitySetName");
		return ExecuteStoreQueryReliably<TElement>(commandText, entitySetName, executionOptions, parameters);
	}

	private ObjectResult<TElement> ExecuteStoreQueryReliably<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
	{
		AsyncMonitor.EnsureNotEntered();
		MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		if (executionStrategy.RetriesOnFailure && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
		}
		if (!executionOptions.UserSpecifiedStreaming.HasValue)
		{
			executionOptions = new ExecutionOptions(executionOptions.MergeOption, !executionStrategy.RetriesOnFailure);
		}
		return executionStrategy.Execute(() => ExecuteInTransaction(() => ExecuteStoreQueryInternal<TElement>(commandText, entitySetName, executionOptions, parameters), executionStrategy, startLocalTransaction: false, !executionOptions.UserSpecifiedStreaming.Value));
	}

	private ObjectResult<TElement> ExecuteStoreQueryInternal<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
	{
		DbDataReader dbDataReader = null;
		DbCommand dbCommand = null;
		ShaperFactory<TElement> shaperFactory;
		EntitySet entitySet;
		TypeUsage edmType;
		try
		{
			dbCommand = CreateStoreCommand(commandText, parameters);
			dbDataReader = dbCommand.ExecuteReader((!executionOptions.UserSpecifiedStreaming.Value) ? CommandBehavior.SequentialAccess : CommandBehavior.Default);
			shaperFactory = InternalTranslate<TElement>(dbDataReader, entitySetName, executionOptions.MergeOption, executionOptions.UserSpecifiedStreaming.Value, out entitySet, out edmType);
		}
		catch
		{
			dbDataReader?.Dispose();
			if (dbCommand != null)
			{
				dbCommand.Parameters.Clear();
				dbCommand.Dispose();
			}
			throw;
		}
		if (!executionOptions.UserSpecifiedStreaming.Value)
		{
			BufferedDataReader bufferedDataReader = null;
			try
			{
				StoreItemCollection storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
				DbProviderServices service = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);
				bufferedDataReader = new BufferedDataReader(dbDataReader);
				bufferedDataReader.Initialize(storeItemCollection.ProviderManifestToken, service, shaperFactory.ColumnTypes, shaperFactory.NullableColumns);
				dbDataReader = bufferedDataReader;
			}
			catch
			{
				bufferedDataReader?.Dispose();
				throw;
			}
		}
		return ShapeResult(dbDataReader, executionOptions.MergeOption, readerOwned: true, executionOptions.UserSpecifiedStreaming.Value, shaperFactory, entitySet, edmType, dbCommand);
	}

	public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, params object[] parameters)
	{
		return ExecuteStoreQueryAsync<TElement>(commandText, CancellationToken.None, parameters);
	}

	public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, CancellationToken cancellationToken, params object[] parameters)
	{
		AsyncMonitor.EnsureNotEntered();
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		return ExecuteStoreQueryReliablyAsync<TElement>(commandText, null, ExecutionOptions.Default, cancellationToken, executionStrategy, parameters);
	}

	public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, ExecutionOptions executionOptions, params object[] parameters)
	{
		return ExecuteStoreQueryAsync<TElement>(commandText, executionOptions, CancellationToken.None, parameters);
	}

	public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, ExecutionOptions executionOptions, CancellationToken cancellationToken, params object[] parameters)
	{
		AsyncMonitor.EnsureNotEntered();
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		if (executionStrategy.RetriesOnFailure && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
		}
		return ExecuteStoreQueryReliablyAsync<TElement>(commandText, null, executionOptions, cancellationToken, executionStrategy, parameters);
	}

	public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
	{
		return ExecuteStoreQueryAsync<TElement>(commandText, entitySetName, executionOptions, CancellationToken.None, parameters);
	}

	public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, CancellationToken cancellationToken, params object[] parameters)
	{
		Check.NotEmpty(entitySetName, "entitySetName");
		AsyncMonitor.EnsureNotEntered();
		IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
		if (executionStrategy.RetriesOnFailure && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
		}
		return ExecuteStoreQueryReliablyAsync<TElement>(commandText, entitySetName, executionOptions, cancellationToken, executionStrategy, parameters);
	}

	private async Task<ObjectResult<TElement>> ExecuteStoreQueryReliablyAsync<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, CancellationToken cancellationToken, IDbExecutionStrategy executionStrategy, params object[] parameters)
	{
		if (executionOptions.MergeOption != MergeOption.NoTracking)
		{
			AsyncMonitor.Enter();
		}
		try
		{
			MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());
			if (!executionOptions.UserSpecifiedStreaming.HasValue)
			{
				executionOptions = new ExecutionOptions(executionOptions.MergeOption, !executionStrategy.RetriesOnFailure);
			}
			return await executionStrategy.ExecuteAsync(() => ExecuteInTransactionAsync(() => ExecuteStoreQueryInternalAsync<TElement>(commandText, entitySetName, executionOptions, cancellationToken, parameters), executionStrategy, startLocalTransaction: false, !executionOptions.UserSpecifiedStreaming.Value, cancellationToken), cancellationToken).WithCurrentCulture();
		}
		finally
		{
			if (executionOptions.MergeOption != MergeOption.NoTracking)
			{
				AsyncMonitor.Exit();
			}
		}
	}

	private async Task<ObjectResult<TElement>> ExecuteStoreQueryInternalAsync<TElement>(string commandText, string entitySetName, ExecutionOptions executionOptions, CancellationToken cancellationToken, params object[] parameters)
	{
		DbDataReader reader = null;
		DbCommand command = null;
		ShaperFactory<TElement> shaperFactory;
		EntitySet entitySet;
		TypeUsage edmType;
		try
		{
			command = CreateStoreCommand(commandText, parameters);
			reader = await command.ExecuteReaderAsync((!executionOptions.UserSpecifiedStreaming.Value) ? CommandBehavior.SequentialAccess : CommandBehavior.Default, cancellationToken).WithCurrentCulture();
			shaperFactory = InternalTranslate<TElement>(reader, entitySetName, executionOptions.MergeOption, executionOptions.UserSpecifiedStreaming.Value, out entitySet, out edmType);
		}
		catch
		{
			reader?.Dispose();
			if (command != null)
			{
				command.Parameters.Clear();
				command.Dispose();
			}
			throw;
		}
		if (!executionOptions.UserSpecifiedStreaming.Value)
		{
			BufferedDataReader bufferedReader = null;
			try
			{
				StoreItemCollection storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
				DbProviderServices service = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);
				bufferedReader = new BufferedDataReader(reader);
				await bufferedReader.InitializeAsync(storeItemCollection.ProviderManifestToken, service, shaperFactory.ColumnTypes, shaperFactory.NullableColumns, cancellationToken).WithCurrentCulture();
				reader = bufferedReader;
			}
			catch
			{
				bufferedReader?.Dispose();
				throw;
			}
		}
		return ShapeResult(reader, executionOptions.MergeOption, readerOwned: true, executionOptions.UserSpecifiedStreaming.Value, shaperFactory, entitySet, edmType, command);
	}

	public virtual ObjectResult<TElement> Translate<TElement>(DbDataReader reader)
	{
		MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());
		EntitySet entitySet;
		TypeUsage edmType;
		ShaperFactory<TElement> shaperFactory = InternalTranslate<TElement>(reader, null, MergeOption.AppendOnly, streaming: false, out entitySet, out edmType);
		return ShapeResult(reader, MergeOption.AppendOnly, readerOwned: false, streaming: false, shaperFactory, entitySet, edmType);
	}

	public virtual ObjectResult<TEntity> Translate<TEntity>(DbDataReader reader, string entitySetName, MergeOption mergeOption)
	{
		Check.NotEmpty(entitySetName, "entitySetName");
		MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TEntity), Assembly.GetCallingAssembly());
		EntitySet entitySet;
		TypeUsage edmType;
		ShaperFactory<TEntity> shaperFactory = InternalTranslate<TEntity>(reader, entitySetName, mergeOption, streaming: false, out entitySet, out edmType);
		return ShapeResult(reader, mergeOption, readerOwned: false, streaming: false, shaperFactory, entitySet, edmType);
	}

	private ShaperFactory<TElement> InternalTranslate<TElement>(DbDataReader reader, string entitySetName, MergeOption mergeOption, bool streaming, out EntitySet entitySet, out TypeUsage edmType)
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		entitySet = null;
		if (!string.IsNullOrEmpty(entitySetName))
		{
			entitySet = GetEntitySetFromName(entitySetName);
		}
		Type type = Nullable.GetUnderlyingType(typeof(TElement)) ?? typeof(TElement);
		CollectionColumnMap collectionColumnMap;
		if (MetadataWorkspace.TryDetermineCSpaceModelType<TElement>(out var modelEdmType) || (type.IsEnum() && MetadataWorkspace.TryDetermineCSpaceModelType(type.GetEnumUnderlyingType(), out modelEdmType)))
		{
			if (entitySet != null && !entitySet.ElementType.IsAssignableFrom(modelEdmType))
			{
				throw new InvalidOperationException(Strings.ObjectContext_InvalidEntitySetForStoreQuery(entitySet.EntityContainer.Name, entitySet.Name, typeof(TElement)));
			}
			collectionColumnMap = _columnMapFactory.CreateColumnMapFromReaderAndType(reader, modelEdmType, entitySet, null);
		}
		else
		{
			collectionColumnMap = _columnMapFactory.CreateColumnMapFromReaderAndClrType(reader, typeof(TElement), MetadataWorkspace);
		}
		edmType = collectionColumnMap.Type;
		return _translator.TranslateColumnMap<TElement>(collectionColumnMap, MetadataWorkspace, null, mergeOption, streaming, valueLayer: false);
	}

	private ObjectResult<TElement> ShapeResult<TElement>(DbDataReader reader, MergeOption mergeOption, bool readerOwned, bool streaming, ShaperFactory<TElement> shaperFactory, EntitySet entitySet, TypeUsage edmType, DbCommand command = null)
	{
		return new ObjectResult<TElement>(shaperFactory.Create(reader, this, MetadataWorkspace, mergeOption, readerOwned, streaming), entitySet, MetadataHelper.GetElementType(edmType), readerOwned, streaming, command);
	}

	private DbCommand CreateStoreCommand(string commandText, params object[] parameters)
	{
		DbCommand dbCommand = ((EntityConnection)Connection).StoreConnection.CreateCommand();
		dbCommand.CommandText = commandText;
		if (CommandTimeout.HasValue)
		{
			dbCommand.CommandTimeout = CommandTimeout.Value;
		}
		EntityTransaction currentTransaction = ((EntityConnection)Connection).CurrentTransaction;
		if (currentTransaction != null)
		{
			dbCommand.Transaction = currentTransaction.StoreTransaction;
		}
		if (parameters != null && parameters.Length != 0)
		{
			DbParameter[] array = new DbParameter[parameters.Length];
			if (parameters.All((object p) => p is DbParameter))
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					array[i] = (DbParameter)parameters[i];
				}
			}
			else
			{
				if (parameters.Any((object p) => p is DbParameter))
				{
					throw new InvalidOperationException(Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues);
				}
				string[] array2 = new string[parameters.Length];
				string[] array3 = new string[parameters.Length];
				for (int j = 0; j < parameters.Length; j++)
				{
					array2[j] = string.Format(CultureInfo.InvariantCulture, "p{0}", new object[1] { j });
					array[j] = dbCommand.CreateParameter();
					array[j].ParameterName = array2[j];
					array[j].Value = parameters[j] ?? DBNull.Value;
					array3[j] = "@" + array2[j];
				}
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string commandText2 = dbCommand.CommandText;
				object[] args = array3;
				dbCommand.CommandText = string.Format(invariantCulture, commandText2, args);
			}
			dbCommand.Parameters.AddRange(array);
		}
		return new InterceptableDbCommand(dbCommand, InterceptionContext);
	}

	public virtual void CreateDatabase()
	{
		DbConnection storeConnection = ((EntityConnection)Connection).StoreConnection;
		GetStoreItemCollection().ProviderFactory.GetProviderServices().CreateDatabase(storeConnection, CommandTimeout, GetStoreItemCollection());
	}

	public virtual void DeleteDatabase()
	{
		DbConnection storeConnection = ((EntityConnection)Connection).StoreConnection;
		GetStoreItemCollection().ProviderFactory.GetProviderServices().DeleteDatabase(storeConnection, CommandTimeout, GetStoreItemCollection());
	}

	public virtual bool DatabaseExists()
	{
		DbConnection storeConnection = ((EntityConnection)Connection).StoreConnection;
		DbProviderServices providerServices = GetStoreItemCollection().ProviderFactory.GetProviderServices();
		try
		{
			return providerServices.DatabaseExists(storeConnection, CommandTimeout, GetStoreItemCollection());
		}
		catch (Exception)
		{
			if (Connection.State == ConnectionState.Open)
			{
				return true;
			}
			try
			{
				Connection.Open();
				return true;
			}
			catch (EntityException)
			{
				return false;
			}
			finally
			{
				Connection.Close();
			}
		}
	}

	private StoreItemCollection GetStoreItemCollection()
	{
		return (StoreItemCollection)((EntityConnection)Connection).GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
	}

	public virtual string CreateDatabaseScript()
	{
		DbProviderServices providerServices = GetStoreItemCollection().ProviderFactory.GetProviderServices();
		string providerManifestToken = GetStoreItemCollection().ProviderManifestToken;
		return providerServices.CreateDatabaseScript(providerManifestToken, GetStoreItemCollection());
	}

	internal void InitializeMappingViewCacheFactory(DbContext owner = null)
	{
		StorageMappingItemCollection itemCollection = (StorageMappingItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
		if (itemCollection == null)
		{
			return;
		}
		Type key = ((owner != null) ? owner.GetType() : GetType());
		_contextTypesWithViewCacheInitialized.GetOrAdd(key, delegate(Type t)
		{
			IEnumerable<DbMappingViewCacheTypeAttribute> source = from a in t.Assembly().GetCustomAttributes<DbMappingViewCacheTypeAttribute>()
				where a.ContextType == t
				select a;
			int num = source.Count();
			if (num > 1)
			{
				throw new InvalidOperationException(Strings.DbMappingViewCacheTypeAttribute_MultipleInstancesWithSameContextType(t));
			}
			if (num == 1)
			{
				itemCollection.MappingViewCacheFactory = new DefaultDbMappingViewCacheFactory(source.First().CacheType);
			}
			return true;
		});
	}
}
