using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal.Linq;

internal class InternalSet<TEntity> : InternalQuery<TEntity>, IInternalSet<TEntity>, IInternalSet, IInternalQuery, IInternalQuery<TEntity> where TEntity : class
{
	private DbLocalView<TEntity> _localView;

	private EntitySet _entitySet;

	private string _entitySetName;

	private string _quotedEntitySetName;

	private Type _baseType;

	public ObservableCollection<TEntity> Local
	{
		get
		{
			InternalContext.DetectChanges();
			return _localView ?? (_localView = new DbLocalView<TEntity>(InternalContext));
		}
	}

	public override ObjectQuery<TEntity> ObjectQuery
	{
		get
		{
			Initialize();
			return base.ObjectQuery;
		}
	}

	public string EntitySetName
	{
		get
		{
			Initialize();
			return _entitySetName;
		}
	}

	public string QuotedEntitySetName
	{
		get
		{
			Initialize();
			return _quotedEntitySetName;
		}
	}

	public EntitySet EntitySet
	{
		get
		{
			Initialize();
			return _entitySet;
		}
	}

	public Type EntitySetBaseType
	{
		get
		{
			Initialize();
			return _baseType;
		}
	}

	public override InternalContext InternalContext
	{
		get
		{
			Initialize();
			return base.InternalContext;
		}
	}

	public override Expression Expression
	{
		get
		{
			Initialize();
			return base.Expression;
		}
	}

	public override ObjectQueryProvider ObjectQueryProvider
	{
		get
		{
			Initialize();
			return base.ObjectQueryProvider;
		}
	}

	public InternalSet(InternalContext internalContext)
		: base(internalContext)
	{
	}

	public override void ResetQuery()
	{
		_entitySet = null;
		_localView = null;
		base.ResetQuery();
	}

	public TEntity Find(params object[] keyValues)
	{
		InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();
		InternalContext.DetectChanges();
		WrappedEntityKey key = new WrappedEntityKey(EntitySet, EntitySetName, keyValues, "keyValues");
		object obj = FindInStateManager(key) ?? FindInStore(key, "keyValues");
		if (obj != null && !(obj is TEntity))
		{
			throw Error.DbSet_WrongEntityTypeFound(obj.GetType().Name, typeof(TEntity).Name);
		}
		return (TEntity)obj;
	}

	public Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
	{
		cancellationToken.ThrowIfCancellationRequested();
		InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();
		return FindInternalAsync(cancellationToken, keyValues);
	}

	private async Task<TEntity> FindInternalAsync(CancellationToken cancellationToken, params object[] keyValues)
	{
		InternalContext.DetectChanges();
		WrappedEntityKey key = new WrappedEntityKey(EntitySet, EntitySetName, keyValues, "keyValues");
		object obj = FindInStateManager(key);
		if (obj == null)
		{
			obj = await FindInStoreAsync(key, "keyValues", cancellationToken).WithCurrentCulture();
		}
		object obj2 = obj;
		if (obj2 != null && !(obj2 is TEntity))
		{
			throw Error.DbSet_WrongEntityTypeFound(obj2.GetType().Name, typeof(TEntity).Name);
		}
		return (TEntity)obj2;
	}

	private object FindInStateManager(WrappedEntityKey key)
	{
		if (!key.HasNullValues && InternalContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(key.EntityKey, out var entry))
		{
			return entry.Entity;
		}
		object obj = null;
		foreach (ObjectStateEntry item in from e in InternalContext.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added)
			where !e.IsRelationship && e.Entity != null && EntitySetBaseType.IsAssignableFrom(e.Entity.GetType())
			select e)
		{
			bool flag = true;
			foreach (KeyValuePair<string, object> keyValuePair in key.KeyValuePairs)
			{
				int ordinal = item.CurrentValues.GetOrdinal(keyValuePair.Key);
				if (!DbHelpers.KeyValuesEqual(keyValuePair.Value, item.CurrentValues.GetValue(ordinal)))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (obj != null)
				{
					throw Error.DbSet_MultipleAddedEntitiesFound();
				}
				obj = item.Entity;
			}
		}
		return obj;
	}

	private object FindInStore(WrappedEntityKey key, string keyValuesParamName)
	{
		if (key.HasNullValues)
		{
			return null;
		}
		try
		{
			return BuildFindQuery(key).SingleOrDefault();
		}
		catch (EntitySqlException innerException)
		{
			throw new ArgumentException(Strings.DbSet_WrongKeyValueType, keyValuesParamName, innerException);
		}
	}

	private async Task<object> FindInStoreAsync(WrappedEntityKey key, string keyValuesParamName, CancellationToken cancellationToken)
	{
		if (key.HasNullValues)
		{
			return null;
		}
		try
		{
			return await IDbAsyncEnumerableExtensions.SingleOrDefaultAsync(BuildFindQuery(key), cancellationToken).WithCurrentCulture();
		}
		catch (EntitySqlException innerException)
		{
			throw new ArgumentException(Strings.DbSet_WrongKeyValueType, keyValuesParamName, innerException);
		}
	}

	private ObjectQuery<TEntity> BuildFindQuery(WrappedEntityKey key)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("SELECT VALUE X FROM {0} AS X WHERE ", QuotedEntitySetName);
		EntityKeyMember[] entityKeyValues = key.EntityKey.EntityKeyValues;
		ObjectParameter[] array = new ObjectParameter[entityKeyValues.Length];
		for (int i = 0; i < entityKeyValues.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" AND ");
			}
			string text = string.Format(CultureInfo.InvariantCulture, "p{0}", new object[1] { i.ToString(CultureInfo.InvariantCulture) });
			stringBuilder.AppendFormat("X.{0} = @{1}", DbHelpers.QuoteIdentifier(entityKeyValues[i].Key), text);
			array[i] = new ObjectParameter(text, entityKeyValues[i].Value);
		}
		return InternalContext.ObjectContext.CreateQuery<TEntity>(stringBuilder.ToString(), array);
	}

	public virtual void Attach(object entity)
	{
		ActOnSet(delegate
		{
			InternalContext.ObjectContext.AttachTo(EntitySetName, entity);
		}, EntityState.Unchanged, entity, "Attach");
	}

	public virtual void Add(object entity)
	{
		ActOnSet(delegate
		{
			InternalContext.ObjectContext.AddObject(EntitySetName, entity);
		}, EntityState.Added, entity, "Add");
	}

	public virtual void AddRange(IEnumerable entities)
	{
		InternalContext.DetectChanges();
		ActOnSet(delegate(object entity)
		{
			InternalContext.ObjectContext.AddObject(EntitySetName, entity);
		}, EntityState.Added, entities, "AddRange");
	}

	public virtual void Remove(object entity)
	{
		if (!(entity is TEntity))
		{
			throw Error.DbSet_BadTypeForAddAttachRemove("Remove", entity.GetType().Name, typeof(TEntity).Name);
		}
		InternalContext.DetectChanges();
		InternalContext.ObjectContext.DeleteObject(entity);
	}

	public virtual void RemoveRange(IEnumerable entities)
	{
		List<object> list = entities.Cast<object>().ToList();
		InternalContext.DetectChanges();
		foreach (object item in list)
		{
			Check.NotNull(item, "entity");
			if (!(item is TEntity))
			{
				throw Error.DbSet_BadTypeForAddAttachRemove("RemoveRange", item.GetType().Name, typeof(TEntity).Name);
			}
			InternalContext.ObjectContext.DeleteObject(item);
		}
	}

	private void ActOnSet(Action action, EntityState newState, object entity, string methodName)
	{
		if (!(entity is TEntity))
		{
			throw Error.DbSet_BadTypeForAddAttachRemove(methodName, entity.GetType().Name, typeof(TEntity).Name);
		}
		InternalContext.DetectChanges();
		if (InternalContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out var entry))
		{
			entry.ChangeState(newState);
		}
		else
		{
			action();
		}
	}

	private void ActOnSet(Action<object> action, EntityState newState, IEnumerable entities, string methodName)
	{
		foreach (object entity in entities)
		{
			Check.NotNull(entity, "entity");
			if (!(entity is TEntity))
			{
				throw Error.DbSet_BadTypeForAddAttachRemove(methodName, entity.GetType().Name, typeof(TEntity).Name);
			}
			if (InternalContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out var entry))
			{
				entry.ChangeState(newState);
			}
			else
			{
				action(entity);
			}
		}
	}

	public TEntity Create()
	{
		return InternalContext.CreateObject<TEntity>();
	}

	public TEntity Create(Type derivedEntityType)
	{
		if (!typeof(TEntity).IsAssignableFrom(derivedEntityType))
		{
			throw Error.DbSet_BadTypeForCreate(derivedEntityType.Name, typeof(TEntity).Name);
		}
		return (TEntity)InternalContext.CreateObject(ObjectContextTypeCache.GetObjectType(derivedEntityType));
	}

	public virtual void Initialize()
	{
		if (_entitySet == null)
		{
			EntitySetTypePair entitySetAndBaseTypeForType = base.InternalContext.GetEntitySetAndBaseTypeForType(typeof(TEntity));
			if (_entitySet == null)
			{
				InitializeUnderlyingTypes(entitySetAndBaseTypeForType);
			}
		}
	}

	public virtual void TryInitialize()
	{
		if (_entitySet == null)
		{
			EntitySetTypePair entitySetTypePair = base.InternalContext.TryGetEntitySetAndBaseTypeForType(typeof(TEntity));
			if (entitySetTypePair != null)
			{
				InitializeUnderlyingTypes(entitySetTypePair);
			}
		}
	}

	private void InitializeUnderlyingTypes(EntitySetTypePair pair)
	{
		_entitySet = pair.EntitySet;
		_baseType = pair.BaseType;
		_entitySetName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
		{
			_entitySet.EntityContainer.Name,
			_entitySet.Name
		});
		_quotedEntitySetName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
		{
			DbHelpers.QuoteIdentifier(_entitySet.EntityContainer.Name),
			DbHelpers.QuoteIdentifier(_entitySet.Name)
		});
		InitializeQuery(CreateObjectQuery(asNoTracking: false));
	}

	private ObjectQuery<TEntity> CreateObjectQuery(bool asNoTracking, bool? streaming = null, IDbExecutionStrategy executionStrategy = null)
	{
		ObjectQuery<TEntity> objectQuery = InternalContext.ObjectContext.CreateQuery<TEntity>(_quotedEntitySetName, new ObjectParameter[0]);
		if (_baseType != typeof(TEntity))
		{
			objectQuery = objectQuery.OfType<TEntity>();
		}
		if (asNoTracking)
		{
			objectQuery.MergeOption = MergeOption.NoTracking;
		}
		if (streaming.HasValue)
		{
			objectQuery.Streaming = streaming.Value;
		}
		objectQuery.ExecutionStrategy = executionStrategy;
		return objectQuery;
	}

	public override string ToString()
	{
		Initialize();
		return base.ToString();
	}

	public override string ToTraceString()
	{
		Initialize();
		return base.ToTraceString();
	}

	public override IInternalQuery<TEntity> Include(string path)
	{
		Initialize();
		return base.Include(path);
	}

	public override IInternalQuery<TEntity> AsNoTracking()
	{
		Initialize();
		return new InternalQuery<TEntity>(InternalContext, CreateObjectQuery(asNoTracking: true));
	}

	public override IInternalQuery<TEntity> AsStreaming()
	{
		Initialize();
		return new InternalQuery<TEntity>(InternalContext, CreateObjectQuery(asNoTracking: false, true));
	}

	public override IInternalQuery<TEntity> WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
	{
		Initialize();
		return new InternalQuery<TEntity>(InternalContext, CreateObjectQuery(asNoTracking: false, false, executionStrategy));
	}

	public IEnumerator ExecuteSqlQuery(string sql, bool asNoTracking, bool? streaming, object[] parameters)
	{
		InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();
		Initialize();
		MergeOption mergeOption = (asNoTracking ? MergeOption.NoTracking : MergeOption.AppendOnly);
		return new LazyEnumerator<TEntity>(() => InternalContext.ObjectContext.ExecuteStoreQuery<TEntity>(sql, EntitySetName, new ExecutionOptions(mergeOption, streaming), parameters));
	}

	public IDbAsyncEnumerator ExecuteSqlQueryAsync(string sql, bool asNoTracking, bool? streaming, object[] parameters)
	{
		InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();
		Initialize();
		MergeOption mergeOption = (asNoTracking ? MergeOption.NoTracking : MergeOption.AppendOnly);
		return new LazyAsyncEnumerator<TEntity>((CancellationToken cancellationToken) => InternalContext.ObjectContext.ExecuteStoreQueryAsync<TEntity>(sql, EntitySetName, new ExecutionOptions(mergeOption, streaming), cancellationToken, parameters));
	}

	public override IEnumerator<TEntity> GetEnumerator()
	{
		Initialize();
		return base.GetEnumerator();
	}

	public override IDbAsyncEnumerator<TEntity> GetAsyncEnumerator()
	{
		Initialize();
		return base.GetAsyncEnumerator();
	}
}
