using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
public class EntityCollection<TEntity> : RelatedEnd, ICollection<TEntity>, IEnumerable<TEntity>, IEnumerable, IListSource where TEntity : class
{
	private HashSet<TEntity> _relatedEntities;

	[NonSerialized]
	private CollectionChangeEventHandler _onAssociationChangedforObjectView;

	[NonSerialized]
	private Dictionary<TEntity, IEntityWrapper> _wrappedRelatedEntities;

	private Dictionary<TEntity, IEntityWrapper> WrappedRelatedEntities
	{
		get
		{
			if (_wrappedRelatedEntities == null)
			{
				_wrappedRelatedEntities = new Dictionary<TEntity, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
			}
			return _wrappedRelatedEntities;
		}
	}

	public int Count
	{
		get
		{
			DeferredLoad();
			return CountInternal;
		}
	}

	internal int CountInternal
	{
		get
		{
			if (_wrappedRelatedEntities == null)
			{
				return 0;
			}
			return _wrappedRelatedEntities.Count;
		}
	}

	public bool IsReadOnly => false;

	bool IListSource.ContainsListCollection => false;

	internal override event CollectionChangeEventHandler AssociationChangedForObjectView
	{
		add
		{
			_onAssociationChangedforObjectView = (CollectionChangeEventHandler)Delegate.Combine(_onAssociationChangedforObjectView, value);
		}
		remove
		{
			_onAssociationChangedforObjectView = (CollectionChangeEventHandler)Delegate.Remove(_onAssociationChangedforObjectView, value);
		}
	}

	public EntityCollection()
	{
	}

	internal EntityCollection(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
		: base(wrappedOwner, navigation, relationshipFixer)
	{
	}

	internal override void OnAssociationChanged(CollectionChangeAction collectionChangeAction, object entity)
	{
		if (!_suppressEvents)
		{
			if (_onAssociationChangedforObjectView != null)
			{
				_onAssociationChangedforObjectView(this, new CollectionChangeEventArgs(collectionChangeAction, entity));
			}
			if (_onAssociationChanged != null)
			{
				_onAssociationChanged(this, new CollectionChangeEventArgs(collectionChangeAction, entity));
			}
		}
	}

	IList IListSource.GetList()
	{
		EntityType entityType = null;
		if (WrappedOwner.Entity != null)
		{
			EntitySet entitySet = null;
			if (RelationshipSet != null)
			{
				entitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[ToEndMember.Name].EntitySet;
				EntityType entityType2 = (EntityType)((RefType)ToEndMember.TypeUsage.EdmType).ElementType;
				EntityType elementType = entitySet.ElementType;
				entityType = ((!entityType2.IsAssignableFrom(elementType)) ? entityType2 : elementType);
			}
		}
		return ObjectViewFactory.CreateViewForEntityCollection(entityType, this);
	}

	public override void Load(MergeOption mergeOption)
	{
		CheckOwnerNull();
		Load(null, mergeOption);
	}

	public override Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken)
	{
		CheckOwnerNull();
		cancellationToken.ThrowIfCancellationRequested();
		return LoadAsync(null, mergeOption, cancellationToken);
	}

	public void Attach(IEnumerable<TEntity> entities)
	{
		Check.NotNull(entities, "entities");
		CheckOwnerNull();
		IList<IEntityWrapper> list = new List<IEntityWrapper>();
		foreach (TEntity entity in entities)
		{
			list.Add(EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext));
		}
		Attach(list, allowCollection: true);
	}

	public void Attach(TEntity entity)
	{
		Check.NotNull(entity, "entity");
		Attach(new IEntityWrapper[1] { EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext) }, allowCollection: false);
	}

	internal virtual void Load(List<IEntityWrapper> collection, MergeOption mergeOption)
	{
		bool hasResults;
		ObjectQuery<TEntity> objectQuery = ValidateLoad<TEntity>(mergeOption, "EntityCollection", out hasResults);
		_suppressEvents = true;
		try
		{
			if (collection == null)
			{
				IEnumerable<TEntity> collection2 = ((!hasResults) ? Enumerable.Empty<TEntity>() : objectQuery.Execute(objectQuery.MergeOption));
				Merge(collection2, mergeOption, setIsLoaded: true);
			}
			else
			{
				Merge<TEntity>(collection, mergeOption, setIsLoaded: true);
			}
		}
		finally
		{
			_suppressEvents = false;
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	internal virtual async Task LoadAsync(List<IEntityWrapper> collection, MergeOption mergeOption, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool hasResults;
		ObjectQuery<TEntity> objectQuery = ValidateLoad<TEntity>(mergeOption, "EntityCollection", out hasResults);
		_suppressEvents = true;
		try
		{
			if (collection == null)
			{
				IEnumerable<TEntity> collection2 = ((!hasResults) ? Enumerable.Empty<TEntity>() : (await (await objectQuery.ExecuteAsync(objectQuery.MergeOption, cancellationToken).WithCurrentCulture()).ToListAsync(cancellationToken).WithCurrentCulture()));
				Merge(collection2, mergeOption, setIsLoaded: true);
			}
			else
			{
				Merge<TEntity>(collection, mergeOption, setIsLoaded: true);
			}
		}
		finally
		{
			_suppressEvents = false;
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	public void Add(TEntity item)
	{
		Check.NotNull(item, "item");
		Add(EntityWrapperFactory.WrapEntityUsingContext(item, ObjectContext));
	}

	internal override void DisconnectedAdd(IEntityWrapper wrappedEntity)
	{
		if (wrappedEntity.Context != null && wrappedEntity.MergeOption != MergeOption.NoTracking)
		{
			throw new InvalidOperationException(Strings.RelatedEnd_UnableToAddEntity);
		}
		VerifyType(wrappedEntity);
		AddToCache(wrappedEntity, applyConstraints: false);
		OnAssociationChanged(CollectionChangeAction.Add, wrappedEntity.Entity);
	}

	internal override bool DisconnectedRemove(IEntityWrapper wrappedEntity)
	{
		if (wrappedEntity.Context != null && wrappedEntity.MergeOption != MergeOption.NoTracking)
		{
			throw new InvalidOperationException(Strings.RelatedEnd_UnableToRemoveEntity);
		}
		bool result = RemoveFromCache(wrappedEntity, resetIsLoaded: false, preserveForeignKey: false);
		OnAssociationChanged(CollectionChangeAction.Remove, wrappedEntity.Entity);
		return result;
	}

	public bool Remove(TEntity item)
	{
		Check.NotNull(item, "item");
		DeferredLoad();
		return RemoveInternal(item);
	}

	internal bool RemoveInternal(TEntity entity)
	{
		return Remove(EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext), preserveForeignKey: false);
	}

	internal override void Include(bool addRelationshipAsUnchanged, bool doAttach)
	{
		if (_wrappedRelatedEntities == null || ObjectContext == null)
		{
			return;
		}
		foreach (IEntityWrapper item in new List<IEntityWrapper>(_wrappedRelatedEntities.Values))
		{
			IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(item.Entity, WrappedOwner.Context);
			if (entityWrapper != item)
			{
				_wrappedRelatedEntities[(TEntity)entityWrapper.Entity] = entityWrapper;
			}
			IncludeEntity(entityWrapper, addRelationshipAsUnchanged, doAttach);
		}
	}

	internal override void Exclude()
	{
		if (_wrappedRelatedEntities == null || ObjectContext == null)
		{
			return;
		}
		if (!base.IsForeignKey)
		{
			foreach (IEntityWrapper value in _wrappedRelatedEntities.Values)
			{
				ExcludeEntity(value);
			}
			return;
		}
		TransactionManager transactionManager = ObjectContext.ObjectStateManager.TransactionManager;
		foreach (IEntityWrapper item in new List<IEntityWrapper>(_wrappedRelatedEntities.Values))
		{
			EntityReference entityReference = GetOtherEndOfRelationship(item) as EntityReference;
			bool flag = transactionManager.PopulatedEntityReferences.Contains(entityReference);
			bool flag2 = transactionManager.AlignedEntityReferences.Contains(entityReference);
			if (flag || flag2)
			{
				entityReference.Remove(entityReference.CachedValue, flag, deleteEntity: false, deleteOwner: false, applyReferentialConstraints: false, preserveForeignKey: true);
				if (flag)
				{
					transactionManager.PopulatedEntityReferences.Remove(entityReference);
				}
				else
				{
					transactionManager.AlignedEntityReferences.Remove(entityReference);
				}
			}
			else
			{
				ExcludeEntity(item);
			}
		}
	}

	internal override void ClearCollectionOrRef(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete)
	{
		if (_wrappedRelatedEntities == null)
		{
			return;
		}
		foreach (IEntityWrapper item in new List<IEntityWrapper>(_wrappedRelatedEntities.Values))
		{
			if (wrappedEntity.Entity == item.Entity && navigation.Equals(base.RelationshipNavigation))
			{
				Remove(item, doFixup: false, deleteEntity: false, deleteOwner: false, applyReferentialConstraints: false, preserveForeignKey: false);
			}
			else
			{
				Remove(item, doFixup: true, doCascadeDelete, deleteOwner: false, applyReferentialConstraints: false, preserveForeignKey: false);
			}
		}
	}

	internal override void ClearWrappedValues()
	{
		if (_wrappedRelatedEntities != null)
		{
			_wrappedRelatedEntities.Clear();
		}
		if (_relatedEntities != null)
		{
			_relatedEntities.Clear();
		}
	}

	internal override bool CanSetEntityType(IEntityWrapper wrappedEntity)
	{
		return wrappedEntity.Entity is TEntity;
	}

	internal override void VerifyType(IEntityWrapper wrappedEntity)
	{
		if (!CanSetEntityType(wrappedEntity))
		{
			throw new InvalidOperationException(Strings.RelatedEnd_InvalidContainedType_Collection(wrappedEntity.Entity.GetType().FullName, typeof(TEntity).FullName));
		}
	}

	internal override bool RemoveFromLocalCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey)
	{
		if (_wrappedRelatedEntities != null && _wrappedRelatedEntities.Remove((TEntity)wrappedEntity.Entity))
		{
			if (resetIsLoaded)
			{
				_isLoaded = false;
			}
			return true;
		}
		return false;
	}

	internal override bool RemoveFromObjectCache(IEntityWrapper wrappedEntity)
	{
		if (base.TargetAccessor.HasProperty)
		{
			return WrappedOwner.CollectionRemove(this, wrappedEntity.Entity);
		}
		return false;
	}

	internal override void RetrieveReferentialConstraintProperties(Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited)
	{
	}

	internal override bool IsEmpty()
	{
		if (_wrappedRelatedEntities != null)
		{
			return _wrappedRelatedEntities.Count == 0;
		}
		return true;
	}

	internal override void VerifyMultiplicityConstraintsForAdd(bool applyConstraints)
	{
	}

	internal override void OnRelatedEndClear()
	{
		_isLoaded = false;
	}

	internal override bool ContainsEntity(IEntityWrapper wrappedEntity)
	{
		if (_wrappedRelatedEntities != null)
		{
			return _wrappedRelatedEntities.ContainsKey((TEntity)wrappedEntity.Entity);
		}
		return false;
	}

	public new IEnumerator<TEntity> GetEnumerator()
	{
		DeferredLoad();
		return WrappedRelatedEntities.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		DeferredLoad();
		return WrappedRelatedEntities.Keys.GetEnumerator();
	}

	internal override IEnumerable GetInternalEnumerable()
	{
		return WrappedRelatedEntities.Keys;
	}

	internal override IEnumerable<IEntityWrapper> GetWrappedEntities()
	{
		return WrappedRelatedEntities.Values;
	}

	public void Clear()
	{
		DeferredLoad();
		if (WrappedOwner.Entity != null)
		{
			bool flag = CountInternal > 0;
			if (_wrappedRelatedEntities != null)
			{
				List<IEntityWrapper> list = new List<IEntityWrapper>(_wrappedRelatedEntities.Values);
				try
				{
					_suppressEvents = true;
					foreach (IEntityWrapper item in list)
					{
						Remove(item, preserveForeignKey: false);
						if (base.UsingNoTracking)
						{
							GetOtherEndOfRelationship(item).OnRelatedEndClear();
						}
					}
				}
				finally
				{
					_suppressEvents = false;
				}
				if (base.UsingNoTracking)
				{
					_isLoaded = false;
				}
			}
			if (flag)
			{
				OnAssociationChanged(CollectionChangeAction.Refresh, null);
			}
		}
		else if (_wrappedRelatedEntities != null)
		{
			_wrappedRelatedEntities.Clear();
		}
	}

	public bool Contains(TEntity item)
	{
		DeferredLoad();
		if (_wrappedRelatedEntities != null)
		{
			return _wrappedRelatedEntities.ContainsKey(item);
		}
		return false;
	}

	public void CopyTo(TEntity[] array, int arrayIndex)
	{
		DeferredLoad();
		WrappedRelatedEntities.Keys.CopyTo(array, arrayIndex);
	}

	internal virtual void BulkDeleteAll(List<object> list)
	{
		if (list.Count <= 0)
		{
			return;
		}
		_suppressEvents = true;
		try
		{
			foreach (object item in list)
			{
				RemoveInternal(item as TEntity);
			}
		}
		finally
		{
			_suppressEvents = false;
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	internal override bool CheckIfNavigationPropertyContainsEntity(IEntityWrapper wrapper)
	{
		if (!base.TargetAccessor.HasProperty)
		{
			return false;
		}
		bool state = DisableLazyLoading();
		try
		{
			object navigationPropertyValue = WrappedOwner.GetNavigationPropertyValue(this);
			if (navigationPropertyValue != null)
			{
				if (!(navigationPropertyValue is IEnumerable<TEntity> source))
				{
					throw new EntityException(Strings.ObjectStateEntry_UnableToEnumerateCollection(base.TargetAccessor.PropertyName, WrappedOwner.Entity.GetType().FullName));
				}
				HashSet<TEntity> hashSet = navigationPropertyValue as HashSet<TEntity>;
				if (!wrapper.OverridesEqualsOrGetHashCode || (hashSet != null && hashSet.Comparer is ObjectReferenceEqualityComparer))
				{
					return source.Contains((TEntity)wrapper.Entity);
				}
				return source.Any((TEntity o) => o == wrapper.Entity);
			}
		}
		finally
		{
			ResetLazyLoading(state);
		}
		return false;
	}

	internal override void VerifyNavigationPropertyForAdd(IEntityWrapper wrapper)
	{
	}

	[OnSerializing]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void OnSerializing(StreamingContext context)
	{
		if (!(WrappedOwner.Entity is IEntityWithRelationships))
		{
			throw new InvalidOperationException(Strings.RelatedEnd_CannotSerialize("EntityCollection"));
		}
		_relatedEntities = ((_wrappedRelatedEntities == null) ? null : new HashSet<TEntity>(_wrappedRelatedEntities.Keys, ObjectReferenceEqualityComparer.Default));
	}

	[OnDeserialized]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void OnCollectionDeserialized(StreamingContext context)
	{
		if (_relatedEntities == null)
		{
			return;
		}
		_relatedEntities.OnDeserialization(null);
		_wrappedRelatedEntities = new Dictionary<TEntity, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
		foreach (TEntity relatedEntity in _relatedEntities)
		{
			_wrappedRelatedEntities.Add(relatedEntity, EntityWrapperFactory.WrapEntityUsingContext(relatedEntity, ObjectContext));
		}
	}

	public ObjectQuery<TEntity> CreateSourceQuery()
	{
		CheckOwnerNull();
		bool hasResults;
		return CreateSourceQuery<TEntity>(base.DefaultMergeOption, out hasResults);
	}

	internal override IEnumerable CreateSourceQueryInternal()
	{
		return CreateSourceQuery();
	}

	internal override void AddToLocalCache(IEntityWrapper wrappedEntity, bool applyConstraints)
	{
		WrappedRelatedEntities[(TEntity)wrappedEntity.Entity] = wrappedEntity;
	}

	internal override void AddToObjectCache(IEntityWrapper wrappedEntity)
	{
		if (base.TargetAccessor.HasProperty)
		{
			WrappedOwner.CollectionAdd(this, wrappedEntity.Entity);
		}
	}
}
