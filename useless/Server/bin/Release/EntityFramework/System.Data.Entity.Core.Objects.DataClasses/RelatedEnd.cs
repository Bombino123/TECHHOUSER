using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
[DataContract]
public abstract class RelatedEnd : IRelatedEnd
{
	private const string _entityKeyParamName = "EntityKeyValue";

	[Obsolete]
	private IEntityWithRelationships _owner;

	private RelationshipNavigation _navigation;

	private IRelationshipFixer _relationshipFixer;

	internal bool _isLoaded;

	[NonSerialized]
	private RelationshipSet _relationshipSet;

	[NonSerialized]
	private ObjectContext _context;

	[NonSerialized]
	private bool _usingNoTracking;

	[NonSerialized]
	private RelationshipType _relationMetadata;

	[NonSerialized]
	private RelationshipEndMember _fromEndMember;

	[NonSerialized]
	private RelationshipEndMember _toEndMember;

	[NonSerialized]
	private string _sourceQuery;

	[NonSerialized]
	private IEnumerable<EdmMember> _sourceQueryParamProperties;

	[NonSerialized]
	internal bool _suppressEvents;

	[NonSerialized]
	internal CollectionChangeEventHandler _onAssociationChanged;

	[NonSerialized]
	private IEntityWrapper _wrappedOwner;

	[NonSerialized]
	private EntityWrapperFactory _entityWrapperFactory;

	[NonSerialized]
	private NavigationProperty navigationPropertyCache;

	internal bool IsForeignKey => ((AssociationType)_relationMetadata).IsForeignKey;

	internal RelationshipNavigation RelationshipNavigation => _navigation;

	[SoapIgnore]
	[XmlIgnore]
	public string RelationshipName
	{
		get
		{
			CheckOwnerNull();
			return _navigation.RelationshipName;
		}
	}

	[SoapIgnore]
	[XmlIgnore]
	public virtual string SourceRoleName
	{
		get
		{
			CheckOwnerNull();
			return _navigation.From;
		}
	}

	[SoapIgnore]
	[XmlIgnore]
	public virtual string TargetRoleName
	{
		get
		{
			CheckOwnerNull();
			return _navigation.To;
		}
	}

	internal virtual IEntityWrapper WrappedOwner => _wrappedOwner;

	internal virtual ObjectContext ObjectContext => _context;

	internal virtual EntityWrapperFactory EntityWrapperFactory
	{
		get
		{
			if (_entityWrapperFactory == null)
			{
				_entityWrapperFactory = new EntityWrapperFactory();
			}
			return _entityWrapperFactory;
		}
	}

	[SoapIgnore]
	[XmlIgnore]
	public virtual RelationshipSet RelationshipSet
	{
		get
		{
			CheckOwnerNull();
			return _relationshipSet;
		}
	}

	internal virtual RelationshipType RelationMetadata => _relationMetadata;

	internal virtual RelationshipEndMember ToEndMember => _toEndMember;

	internal bool UsingNoTracking => _usingNoTracking;

	internal MergeOption DefaultMergeOption
	{
		get
		{
			if (!UsingNoTracking)
			{
				return MergeOption.AppendOnly;
			}
			return MergeOption.NoTracking;
		}
	}

	internal virtual RelationshipEndMember FromEndMember => _fromEndMember;

	[SoapIgnore]
	[XmlIgnore]
	public bool IsLoaded
	{
		get
		{
			CheckOwnerNull();
			return _isLoaded;
		}
		set
		{
			CheckOwnerNull();
			_isLoaded = value;
		}
	}

	internal virtual bool CanDeferredLoad => true;

	internal NavigationProperty NavigationProperty
	{
		get
		{
			if (navigationPropertyCache == null && _wrappedOwner.Context != null && TargetAccessor.HasProperty)
			{
				string propertyName = TargetAccessor.PropertyName;
				if (!_wrappedOwner.Context.MetadataWorkspace.GetItem<EntityType>(_wrappedOwner.IdentityType.FullNameWithNesting(), DataSpace.OSpace).NavigationProperties.TryGetValue(propertyName, ignoreCase: false, out var item))
				{
					throw Error.RelationshipManager_NavigationPropertyNotFound(propertyName);
				}
				navigationPropertyCache = item;
			}
			return navigationPropertyCache;
		}
	}

	internal NavigationPropertyAccessor TargetAccessor
	{
		get
		{
			if (_wrappedOwner.Entity != null)
			{
				EnsureRelationshipNavigationAccessorsInitialized();
				return RelationshipNavigation.ToPropertyAccessor;
			}
			return NavigationPropertyAccessor.NoNavigationProperty;
		}
	}

	public event CollectionChangeEventHandler AssociationChanged
	{
		add
		{
			CheckOwnerNull();
			_onAssociationChanged = (CollectionChangeEventHandler)Delegate.Combine(_onAssociationChanged, value);
		}
		remove
		{
			CheckOwnerNull();
			_onAssociationChanged = (CollectionChangeEventHandler)Delegate.Remove(_onAssociationChanged, value);
		}
	}

	internal virtual event CollectionChangeEventHandler AssociationChangedForObjectView
	{
		add
		{
		}
		remove
		{
		}
	}

	internal RelatedEnd()
	{
		_wrappedOwner = NullEntityWrapper.NullWrapper;
	}

	internal RelatedEnd(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
	{
		InitializeRelatedEnd(wrappedOwner, navigation, relationshipFixer);
	}

	IEnumerable IRelatedEnd.CreateSourceQuery()
	{
		CheckOwnerNull();
		return CreateSourceQueryInternal();
	}

	internal ObjectQuery<TEntity> CreateSourceQuery<TEntity>(MergeOption mergeOption, out bool hasResults)
	{
		if (_context == null)
		{
			hasResults = false;
			return null;
		}
		EntityEntry entityEntry = _context.ObjectStateManager.FindEntityEntry(_wrappedOwner.Entity);
		EntityState entityState;
		if (entityEntry == null)
		{
			if (!UsingNoTracking)
			{
				throw Error.Collections_InvalidEntityStateSource();
			}
			entityState = EntityState.Detached;
		}
		else
		{
			entityState = entityEntry.State;
		}
		if (entityState == EntityState.Added && (!IsForeignKey || !IsDependentEndOfReferentialConstraint(checkIdentifying: false)))
		{
			throw Error.Collections_InvalidEntityStateSource();
		}
		if ((entityState != EntityState.Detached || !UsingNoTracking) && entityState != EntityState.Modified && entityState != EntityState.Unchanged && entityState != EntityState.Deleted && entityState != EntityState.Added)
		{
			hasResults = false;
			return null;
		}
		if (_sourceQuery == null)
		{
			_sourceQuery = GenerateQueryText();
		}
		ObjectQuery<TEntity> objectQuery = new ObjectQuery<TEntity>(_sourceQuery, _context, mergeOption);
		hasResults = AddQueryParameters(objectQuery);
		objectQuery.Parameters.SetReadOnly(isReadOnly: true);
		return objectQuery;
	}

	private string GenerateQueryText()
	{
		EntityKey entityKey = _wrappedOwner.EntityKey;
		if (entityKey == null)
		{
			throw Error.EntityKey_UnexpectedNull();
		}
		AssociationType associationType = (AssociationType)_relationMetadata;
		EntitySet entitySet = ((AssociationSet)_relationshipSet).AssociationSetEnds[_toEndMember.Name].EntitySet;
		EntityType entityType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)_toEndMember);
		bool ofTypeRequired = false;
		if (!entitySet.ElementType.EdmEquals(entityType) && !TypeSemantics.IsSubTypeOf(entitySet.ElementType, entityType))
		{
			ofTypeRequired = true;
			entityType = (EntityType)ObjectContext.MetadataWorkspace.GetOSpaceTypeUsage(TypeUsage.Create(entityType)).EdmType;
		}
		StringBuilder stringBuilder;
		if (associationType.IsForeignKey)
		{
			ReferentialConstraint referentialConstraint = associationType.ReferentialConstraints[0];
			ReadOnlyMetadataCollection<EdmProperty> fromProperties = referentialConstraint.FromProperties;
			ReadOnlyMetadataCollection<EdmProperty> toProperties = referentialConstraint.ToProperties;
			if (!referentialConstraint.ToRole.EdmEquals(_toEndMember))
			{
				stringBuilder = new StringBuilder("SELECT VALUE P FROM ");
				AppendEntitySet(stringBuilder, entitySet, entityType, ofTypeRequired);
				stringBuilder.Append(" AS P WHERE ");
				AliasGenerator aliasGenerator = new AliasGenerator("EntityKeyValue");
				_sourceQueryParamProperties = toProperties;
				for (int i = 0; i < fromProperties.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(" AND ");
					}
					stringBuilder.Append("P.[");
					stringBuilder.Append(fromProperties[i].Name);
					stringBuilder.Append("] = @");
					stringBuilder.Append(aliasGenerator.Next());
				}
				return stringBuilder.ToString();
			}
			stringBuilder = new StringBuilder("SELECT VALUE D FROM ");
			AppendEntitySet(stringBuilder, entitySet, entityType, ofTypeRequired);
			stringBuilder.Append(" AS D WHERE ");
			AliasGenerator aliasGenerator2 = new AliasGenerator("EntityKeyValue");
			_sourceQueryParamProperties = fromProperties;
			for (int j = 0; j < toProperties.Count; j++)
			{
				if (j > 0)
				{
					stringBuilder.Append(" AND ");
				}
				stringBuilder.Append("D.[");
				stringBuilder.Append(toProperties[j].Name);
				stringBuilder.Append("] = @");
				stringBuilder.Append(aliasGenerator2.Next());
			}
		}
		else
		{
			stringBuilder = new StringBuilder("SELECT VALUE [TargetEntity] FROM (SELECT VALUE x FROM ");
			stringBuilder.Append("[");
			stringBuilder.Append(_relationshipSet.EntityContainer.Name);
			stringBuilder.Append("].[");
			stringBuilder.Append(_relationshipSet.Name);
			stringBuilder.Append("] AS x WHERE Key(x.[");
			stringBuilder.Append(_fromEndMember.Name);
			stringBuilder.Append("]) = ");
			AppendKeyParameterRow(stringBuilder, entityKey.GetEntitySet(ObjectContext.MetadataWorkspace).ElementType.KeyMembers);
			stringBuilder.Append(") AS [AssociationEntry] INNER JOIN ");
			AppendEntitySet(stringBuilder, entitySet, entityType, ofTypeRequired);
			stringBuilder.Append(" AS [TargetEntity] ON Key([AssociationEntry].[");
			stringBuilder.Append(_toEndMember.Name);
			stringBuilder.Append("]) = Key(Ref([TargetEntity]))");
		}
		return stringBuilder.ToString();
	}

	private bool AddQueryParameters<TEntity>(ObjectQuery<TEntity> query)
	{
		EntityKey entityKey = _wrappedOwner.EntityKey;
		if (entityKey == null)
		{
			throw Error.EntityKey_UnexpectedNull();
		}
		bool result = true;
		AliasGenerator aliasGenerator = new AliasGenerator("EntityKeyValue");
		foreach (EdmMember parameterMember in _sourceQueryParamProperties ?? entityKey.GetEntitySet(ObjectContext.MetadataWorkspace).ElementType.KeyMembers)
		{
			object obj = ((_sourceQueryParamProperties == null) ? _wrappedOwner.EntityKey.EntityKeyValues.Single((EntityKeyMember ekv) => ekv.Key == parameterMember.Name).Value : ((!CachedForeignKeyIsConceptualNull()) ? GetCurrentValueFromEntity(parameterMember) : null));
			ObjectParameter objectParameter;
			if (obj == null)
			{
				EdmType edmType = parameterMember.TypeUsage.EdmType;
				Type type = (Helper.IsPrimitiveType(edmType) ? ((PrimitiveType)edmType).ClrEquivalentType : ObjectContext.MetadataWorkspace.GetObjectSpaceType((EnumType)edmType).ClrType);
				objectParameter = new ObjectParameter(aliasGenerator.Next(), type);
				result = false;
			}
			else
			{
				objectParameter = new ObjectParameter(aliasGenerator.Next(), obj);
			}
			objectParameter.TypeUsage = Helper.GetModelTypeUsage(parameterMember);
			query.Parameters.Add(objectParameter);
		}
		return result;
	}

	private object GetCurrentValueFromEntity(EdmMember member)
	{
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = _context.ObjectStateManager.GetOrAddStateManagerTypeMetadata(member.DeclaringType);
		return orAddStateManagerTypeMetadata.Member(orAddStateManagerTypeMetadata.GetOrdinalforCLayerMemberName(member.Name)).GetValue(_wrappedOwner.Entity);
	}

	private static void AppendKeyParameterRow(StringBuilder sourceBuilder, IList<EdmMember> keyMembers)
	{
		sourceBuilder.Append("ROW(");
		AliasGenerator aliasGenerator = new AliasGenerator("EntityKeyValue");
		int count = keyMembers.Count;
		for (int i = 0; i < count; i++)
		{
			string value = aliasGenerator.Next();
			sourceBuilder.Append("@");
			sourceBuilder.Append(value);
			sourceBuilder.Append(" AS ");
			sourceBuilder.Append(value);
			if (i < count - 1)
			{
				sourceBuilder.Append(",");
			}
		}
		sourceBuilder.Append(")");
	}

	private static void AppendEntitySet(StringBuilder sourceBuilder, EntitySet targetEntitySet, EntityType targetEntityType, bool ofTypeRequired)
	{
		if (ofTypeRequired)
		{
			sourceBuilder.Append("OfType(");
		}
		sourceBuilder.Append("[");
		sourceBuilder.Append(targetEntitySet.EntityContainer.Name);
		sourceBuilder.Append("].[");
		sourceBuilder.Append(targetEntitySet.Name);
		sourceBuilder.Append("]");
		if (ofTypeRequired)
		{
			sourceBuilder.Append(", [");
			if (!string.IsNullOrEmpty(targetEntityType.NamespaceName))
			{
				sourceBuilder.Append(targetEntityType.NamespaceName);
				sourceBuilder.Append("].[");
			}
			sourceBuilder.Append(targetEntityType.Name);
			sourceBuilder.Append("])");
		}
	}

	internal virtual ObjectQuery<TEntity> ValidateLoad<TEntity>(MergeOption mergeOption, string relatedEndName, out bool hasResults)
	{
		ObjectQuery<TEntity> objectQuery = CreateSourceQuery<TEntity>(mergeOption, out hasResults);
		if (objectQuery == null)
		{
			throw Error.RelatedEnd_RelatedEndNotAttachedToContext(relatedEndName);
		}
		EntityEntry entityEntry = ObjectContext.ObjectStateManager.FindEntityEntry(_wrappedOwner.Entity);
		if (entityEntry != null && entityEntry.State == EntityState.Deleted)
		{
			throw Error.Collections_InvalidEntityStateLoad(relatedEndName);
		}
		if (UsingNoTracking != (mergeOption == MergeOption.NoTracking))
		{
			throw Error.RelatedEnd_MismatchedMergeOptionOnLoad(mergeOption);
		}
		if (UsingNoTracking)
		{
			if (IsLoaded)
			{
				throw Error.RelatedEnd_LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd();
			}
			if (!IsEmpty())
			{
				throw Error.RelatedEnd_LoadCalledOnNonEmptyNoTrackedRelatedEnd();
			}
		}
		return objectQuery;
	}

	public void Load()
	{
		Load(DefaultMergeOption);
	}

	public Task LoadAsync(CancellationToken cancellationToken)
	{
		return LoadAsync(DefaultMergeOption, cancellationToken);
	}

	public abstract void Load(MergeOption mergeOption);

	public abstract Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken);

	internal void DeferredLoad()
	{
		if (_wrappedOwner != null && _wrappedOwner != NullEntityWrapper.NullWrapper && !IsLoaded && _context != null && _context.ContextOptions.LazyLoadingEnabled && !_context.InMaterialization && CanDeferredLoad && (UsingNoTracking || (_wrappedOwner.ObjectStateEntry != null && (_wrappedOwner.ObjectStateEntry.State == EntityState.Unchanged || _wrappedOwner.ObjectStateEntry.State == EntityState.Modified || (_wrappedOwner.ObjectStateEntry.State == EntityState.Added && IsForeignKey && IsDependentEndOfReferentialConstraint(checkIdentifying: false))))))
		{
			_context.ContextOptions.LazyLoadingEnabled = false;
			try
			{
				Load();
			}
			finally
			{
				_context.ContextOptions.LazyLoadingEnabled = true;
			}
		}
	}

	internal virtual void Merge<TEntity>(IEnumerable<TEntity> collection, MergeOption mergeOption, bool setIsLoaded)
	{
		List<IEntityWrapper> list = collection as List<IEntityWrapper>;
		if (list == null)
		{
			list = new List<IEntityWrapper>();
			EntitySet entitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[TargetRoleName].EntitySet;
			foreach (TEntity item in collection)
			{
				IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(item, ObjectContext);
				if (mergeOption == MergeOption.NoTracking)
				{
					EntityWrapperFactory.UpdateNoTrackingWrapper(entityWrapper, ObjectContext, entitySet);
				}
				list.Add(entityWrapper);
			}
		}
		Merge<TEntity>(list, mergeOption, setIsLoaded);
	}

	internal virtual void Merge<TEntity>(List<IEntityWrapper> collection, MergeOption mergeOption, bool setIsLoaded)
	{
		if (WrappedOwner.EntityKey == null)
		{
			throw Error.EntityKey_UnexpectedNull();
		}
		ObjectContext.ObjectStateManager.UpdateRelationships(ObjectContext, mergeOption, (AssociationSet)RelationshipSet, (AssociationEndMember)FromEndMember, WrappedOwner, (AssociationEndMember)ToEndMember, collection, setIsLoaded);
		if (setIsLoaded)
		{
			_isLoaded = true;
		}
	}

	void IRelatedEnd.Attach(IEntityWithRelationships entity)
	{
		Check.NotNull(entity, "entity");
		((IRelatedEnd)this).Attach((object)entity);
	}

	void IRelatedEnd.Attach(object entity)
	{
		Check.NotNull(entity, "entity");
		CheckOwnerNull();
		Attach(new IEntityWrapper[1] { EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext) }, allowCollection: false);
	}

	internal void Attach(IEnumerable<IEntityWrapper> wrappedEntities, bool allowCollection)
	{
		CheckOwnerNull();
		ValidateOwnerForAttach();
		int num = 0;
		List<IEntityWrapper> list = new List<IEntityWrapper>();
		foreach (IEntityWrapper wrappedEntity in wrappedEntities)
		{
			ValidateEntityForAttach(wrappedEntity, num++, allowCollection);
			list.Add(wrappedEntity);
		}
		_suppressEvents = true;
		try
		{
			Merge(list, MergeOption.OverwriteChanges, setIsLoaded: false);
			ReferentialConstraint referentialConstraint = ((AssociationType)RelationMetadata).ReferentialConstraints.FirstOrDefault();
			if (referentialConstraint != null)
			{
				ObjectStateManager objectStateManager = ObjectContext.ObjectStateManager;
				EntityEntry entityEntry = objectStateManager.FindEntityEntry(_wrappedOwner.Entity);
				if (IsDependentEndOfReferentialConstraint(checkIdentifying: false))
				{
					if (!VerifyRIConstraintsWithRelatedEntry(referentialConstraint, entityEntry.GetCurrentEntityValue, list[0].ObjectStateEntry.EntityKey))
					{
						throw new InvalidOperationException(referentialConstraint.BuildConstraintExceptionMessage());
					}
				}
				else
				{
					foreach (IEntityWrapper item in list)
					{
						RelatedEnd otherEndOfRelationship = GetOtherEndOfRelationship(item);
						if (otherEndOfRelationship.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
						{
							EntityEntry @object = objectStateManager.FindEntityEntry(otherEndOfRelationship.WrappedOwner.Entity);
							if (!VerifyRIConstraintsWithRelatedEntry(referentialConstraint, @object.GetCurrentEntityValue, entityEntry.EntityKey))
							{
								throw new InvalidOperationException(referentialConstraint.BuildConstraintExceptionMessage());
							}
						}
					}
				}
			}
		}
		finally
		{
			_suppressEvents = false;
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	internal void ValidateOwnerForAttach()
	{
		if (ObjectContext == null || UsingNoTracking)
		{
			throw Error.RelatedEnd_InvalidOwnerStateForAttach();
		}
		EntityEntry entityEntry = ObjectContext.ObjectStateManager.GetEntityEntry(_wrappedOwner.Entity);
		if (entityEntry.State != EntityState.Modified && entityEntry.State != EntityState.Unchanged)
		{
			throw Error.RelatedEnd_InvalidOwnerStateForAttach();
		}
	}

	internal void ValidateEntityForAttach(IEntityWrapper wrappedEntity, int index, bool allowCollection)
	{
		if (wrappedEntity == null || wrappedEntity.Entity == null)
		{
			if (allowCollection)
			{
				throw Error.RelatedEnd_InvalidNthElementNullForAttach(index);
			}
			throw new ArgumentNullException("wrappedEntity");
		}
		VerifyType(wrappedEntity);
		EntityEntry entityEntry = ObjectContext.ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
		if (entityEntry == null || entityEntry.Entity != wrappedEntity.Entity)
		{
			if (allowCollection)
			{
				throw Error.RelatedEnd_InvalidNthElementContextForAttach(index);
			}
			throw Error.RelatedEnd_InvalidEntityContextForAttach();
		}
		if (entityEntry.State != EntityState.Unchanged && entityEntry.State != EntityState.Modified)
		{
			if (allowCollection)
			{
				throw Error.RelatedEnd_InvalidNthElementStateForAttach(index);
			}
			throw Error.RelatedEnd_InvalidEntityStateForAttach();
		}
	}

	internal abstract IEnumerable CreateSourceQueryInternal();

	void IRelatedEnd.Add(IEntityWithRelationships entity)
	{
		Check.NotNull(entity, "entity");
		((IRelatedEnd)this).Add((object)entity);
	}

	void IRelatedEnd.Add(object entity)
	{
		Check.NotNull(entity, "entity");
		Add(EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext));
	}

	internal void Add(IEntityWrapper wrappedEntity)
	{
		if (_wrappedOwner.Entity != null)
		{
			Add(wrappedEntity, applyConstraints: true);
		}
		else
		{
			DisconnectedAdd(wrappedEntity);
		}
	}

	bool IRelatedEnd.Remove(IEntityWithRelationships entity)
	{
		Check.NotNull(entity, "entity");
		return ((IRelatedEnd)this).Remove((object)entity);
	}

	bool IRelatedEnd.Remove(object entity)
	{
		Check.NotNull(entity, "entity");
		DeferredLoad();
		return Remove(EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext), preserveForeignKey: false);
	}

	internal bool Remove(IEntityWrapper wrappedEntity, bool preserveForeignKey)
	{
		if (_wrappedOwner.Entity != null)
		{
			if (ContainsEntity(wrappedEntity))
			{
				Remove(wrappedEntity, doFixup: true, deleteEntity: false, deleteOwner: false, applyReferentialConstraints: true, preserveForeignKey);
				return true;
			}
			return false;
		}
		return DisconnectedRemove(wrappedEntity);
	}

	internal abstract void DisconnectedAdd(IEntityWrapper wrappedEntity);

	internal abstract bool DisconnectedRemove(IEntityWrapper wrappedEntity);

	internal void Add(IEntityWrapper wrappedEntity, bool applyConstraints)
	{
		if (_context != null && !UsingNoTracking)
		{
			ValidateStateForAdd(_wrappedOwner);
			ValidateStateForAdd(wrappedEntity);
		}
		Add(wrappedEntity, applyConstraints, addRelationshipAsUnchanged: false, relationshipAlreadyExists: false, allowModifyingOtherEndOfRelationship: true, forceForeignKeyChanges: true);
	}

	internal void CheckRelationEntitySet(EntitySet set)
	{
		if (((AssociationSet)_relationshipSet).AssociationSetEnds[_navigation.To] != null && ((AssociationSet)_relationshipSet).AssociationSetEnds[_navigation.To].EntitySet != set)
		{
			throw Error.RelatedEnd_EntitySetIsNotValidForRelationship(set.EntityContainer.Name, set.Name, _navigation.To, _relationshipSet.EntityContainer.Name, _relationshipSet.Name);
		}
	}

	internal void ValidateStateForAdd(IEntityWrapper wrappedEntity)
	{
		EntityEntry entityEntry = ObjectContext.ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
		if (entityEntry != null && entityEntry.State == EntityState.Deleted)
		{
			throw Error.RelatedEnd_UnableToAddRelationshipWithDeletedEntity();
		}
	}

	internal void Add(IEntityWrapper wrappedTarget, bool applyConstraints, bool addRelationshipAsUnchanged, bool relationshipAlreadyExists, bool allowModifyingOtherEndOfRelationship, bool forceForeignKeyChanges)
	{
		if (VerifyEntityForAdd(wrappedTarget, relationshipAlreadyExists))
		{
			EntityKey entityKey = wrappedTarget.EntityKey;
			if (entityKey != null && ObjectContext != null)
			{
				CheckRelationEntitySet(entityKey.GetEntitySet(ObjectContext.MetadataWorkspace));
			}
			RelatedEnd otherEndOfRelationship = GetOtherEndOfRelationship(wrappedTarget);
			ValidateContextsAreCompatible(otherEndOfRelationship);
			otherEndOfRelationship.VerifyEntityForAdd(_wrappedOwner, relationshipAlreadyExists);
			otherEndOfRelationship.VerifyMultiplicityConstraintsForAdd(!allowModifyingOtherEndOfRelationship);
			if (CheckIfNavigationPropertyContainsEntity(wrappedTarget))
			{
				AddToLocalCache(wrappedTarget, applyConstraints);
			}
			else
			{
				AddToCache(wrappedTarget, applyConstraints);
			}
			if (otherEndOfRelationship.CheckIfNavigationPropertyContainsEntity(WrappedOwner))
			{
				otherEndOfRelationship.AddToLocalCache(_wrappedOwner, applyConstraints: false);
			}
			else
			{
				otherEndOfRelationship.AddToCache(_wrappedOwner, applyConstraints: false);
			}
			SynchronizeContexts(otherEndOfRelationship, relationshipAlreadyExists, addRelationshipAsUnchanged);
			if (ObjectContext != null && IsForeignKey && !ObjectContext.ObjectStateManager.TransactionManager.IsGraphUpdate && !UpdateDependentEndForeignKey(otherEndOfRelationship, forceForeignKeyChanges))
			{
				otherEndOfRelationship.UpdateDependentEndForeignKey(this, forceForeignKeyChanges);
			}
			otherEndOfRelationship.OnAssociationChanged(CollectionChangeAction.Add, _wrappedOwner.Entity);
			OnAssociationChanged(CollectionChangeAction.Add, wrappedTarget.Entity);
		}
	}

	internal virtual void AddToNavigationPropertyIfCompatible(RelatedEnd otherRelatedEnd)
	{
		AddToNavigationProperty(otherRelatedEnd.WrappedOwner);
	}

	internal virtual bool CachedForeignKeyIsConceptualNull()
	{
		return false;
	}

	internal virtual bool UpdateDependentEndForeignKey(RelatedEnd targetRelatedEnd, bool forceForeignKeyChanges)
	{
		return false;
	}

	internal virtual void VerifyDetachedKeyMatches(EntityKey entityKey)
	{
	}

	private void ValidateContextsAreCompatible(RelatedEnd targetRelatedEnd)
	{
		if (ObjectContext == targetRelatedEnd.ObjectContext && ObjectContext != null)
		{
			if (UsingNoTracking != targetRelatedEnd.UsingNoTracking)
			{
				throw Error.RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities(UsingNoTracking ? _navigation.From : _navigation.To);
			}
		}
		else if (ObjectContext != null && targetRelatedEnd.ObjectContext != null)
		{
			if (!UsingNoTracking || !targetRelatedEnd.UsingNoTracking)
			{
				throw Error.RelatedEnd_CannotCreateRelationshipEntitiesInDifferentContexts();
			}
			targetRelatedEnd.WrappedOwner.ResetContext(ObjectContext, GetTargetEntitySetFromRelationshipSet(), MergeOption.NoTracking);
		}
		else if ((_context == null || UsingNoTracking) && targetRelatedEnd.ObjectContext != null && !targetRelatedEnd.UsingNoTracking)
		{
			targetRelatedEnd.ValidateStateForAdd(targetRelatedEnd.WrappedOwner);
		}
	}

	private void SynchronizeContexts(RelatedEnd targetRelatedEnd, bool relationshipAlreadyExists, bool addRelationshipAsUnchanged)
	{
		RelatedEnd relatedEnd = null;
		IEntityWrapper entityWrapper = null;
		IEntityWrapper wrappedOwner = targetRelatedEnd.WrappedOwner;
		if (ObjectContext == targetRelatedEnd.ObjectContext && ObjectContext != null)
		{
			if (!IsForeignKey && !relationshipAlreadyExists && !UsingNoTracking)
			{
				if (!ObjectContext.ObjectStateManager.TransactionManager.IsLocalPublicAPI && WrappedOwner.EntityKey != null && !WrappedOwner.EntityKey.IsTemporary && IsDependentEndOfReferentialConstraint(checkIdentifying: false))
				{
					addRelationshipAsUnchanged = true;
				}
				AddRelationshipToObjectStateManager(wrappedOwner, addRelationshipAsUnchanged, doAttach: false);
			}
			if (wrappedOwner.RequiresRelationshipChangeTracking && (ObjectContext.ObjectStateManager.TransactionManager.IsAddTracking || ObjectContext.ObjectStateManager.TransactionManager.IsAttachTracking || ObjectContext.ObjectStateManager.TransactionManager.IsDetectChanges))
			{
				AddToNavigationProperty(wrappedOwner);
				targetRelatedEnd.AddToNavigationProperty(_wrappedOwner);
			}
		}
		else
		{
			if (ObjectContext == null && targetRelatedEnd.ObjectContext == null)
			{
				return;
			}
			if (ObjectContext == null)
			{
				relatedEnd = targetRelatedEnd;
				entityWrapper = _wrappedOwner;
			}
			else
			{
				relatedEnd = this;
				entityWrapper = wrappedOwner;
			}
			if (relatedEnd.UsingNoTracking)
			{
				return;
			}
			TransactionManager transactionManager = relatedEnd.WrappedOwner.Context.ObjectStateManager.TransactionManager;
			transactionManager.BeginAddTracking();
			try
			{
				bool flag = true;
				try
				{
					if (transactionManager.TrackProcessedEntities)
					{
						if (!transactionManager.WrappedEntities.ContainsKey(entityWrapper.Entity))
						{
							transactionManager.WrappedEntities.Add(entityWrapper.Entity, entityWrapper);
						}
						transactionManager.ProcessedEntities.Add(relatedEnd.WrappedOwner);
					}
					relatedEnd.AddGraphToObjectStateManager(entityWrapper, relationshipAlreadyExists, addRelationshipAsUnchanged, doAttach: false);
					if (entityWrapper.RequiresRelationshipChangeTracking && TargetAccessor.HasProperty)
					{
						targetRelatedEnd.AddToNavigationProperty(_wrappedOwner);
					}
					flag = false;
				}
				finally
				{
					if (flag)
					{
						relatedEnd.WrappedOwner.Context.ObjectStateManager.DegradePromotedRelationships();
						relatedEnd.FixupOtherEndOfRelationshipForRemove(entityWrapper, preserveForeignKey: false);
						relatedEnd.RemoveFromCache(entityWrapper, resetIsLoaded: false, preserveForeignKey: false);
						entityWrapper.RelationshipManager.NodeVisited = true;
						RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(entityWrapper);
						RemoveEntityFromObjectStateManager(entityWrapper);
					}
				}
			}
			finally
			{
				transactionManager.EndAddTracking();
			}
		}
	}

	private void AddGraphToObjectStateManager(IEntityWrapper wrappedEntity, bool relationshipAlreadyExists, bool addRelationshipAsUnchanged, bool doAttach)
	{
		AddEntityToObjectStateManager(wrappedEntity, doAttach);
		if (!relationshipAlreadyExists && ObjectContext != null && wrappedEntity.Context != null)
		{
			if (!IsForeignKey)
			{
				AddRelationshipToObjectStateManager(wrappedEntity, addRelationshipAsUnchanged, doAttach);
			}
			if (wrappedEntity.RequiresRelationshipChangeTracking || WrappedOwner.RequiresRelationshipChangeTracking)
			{
				UpdateSnapshotOfRelationships(wrappedEntity);
				if (doAttach)
				{
					EntityEntry entityEntry = _context.ObjectStateManager.GetEntityEntry(wrappedEntity.Entity);
					wrappedEntity.RelationshipManager.CheckReferentialConstraintProperties(entityEntry);
				}
			}
		}
		WalkObjectGraphToIncludeAllRelatedEntities(wrappedEntity, addRelationshipAsUnchanged, doAttach);
	}

	private void UpdateSnapshotOfRelationships(IEntityWrapper wrappedEntity)
	{
		RelatedEnd otherEndOfRelationship = GetOtherEndOfRelationship(wrappedEntity);
		if (!otherEndOfRelationship.ContainsEntity(WrappedOwner))
		{
			otherEndOfRelationship.AddToLocalCache(WrappedOwner, applyConstraints: false);
		}
	}

	internal void Remove(IEntityWrapper wrappedEntity, bool doFixup, bool deleteEntity, bool deleteOwner, bool applyReferentialConstraints, bool preserveForeignKey)
	{
		if (wrappedEntity.RequiresRelationshipChangeTracking && doFixup && TargetAccessor.HasProperty && !CheckIfNavigationPropertyContainsEntity(wrappedEntity))
		{
			GetOtherEndOfRelationship(wrappedEntity).RemoveFromNavigationProperty(WrappedOwner);
		}
		if (!ContainsEntity(wrappedEntity))
		{
			return;
		}
		if (_context != null && doFixup && applyReferentialConstraints && IsDependentEndOfReferentialConstraint(checkIdentifying: false))
		{
			GetOtherEndOfRelationship(wrappedEntity).Remove(_wrappedOwner, doFixup, deleteEntity, deleteOwner, applyReferentialConstraints, preserveForeignKey);
			return;
		}
		bool num = RemoveFromCache(wrappedEntity, resetIsLoaded: false, preserveForeignKey);
		if (!UsingNoTracking && ObjectContext != null && !IsForeignKey)
		{
			MarkRelationshipAsDeletedInObjectStateManager(wrappedEntity, _wrappedOwner, _relationshipSet, _navigation);
		}
		if (doFixup)
		{
			FixupOtherEndOfRelationshipForRemove(wrappedEntity, preserveForeignKey);
			if ((_context == null || !_context.ObjectStateManager.TransactionManager.IsLocalPublicAPI) && _context != null && (deleteEntity || (deleteOwner && CheckCascadeDeleteFlag(_fromEndMember)) || (applyReferentialConstraints && IsPrincipalEndOfReferentialConstraint())) && wrappedEntity.Entity != _context.ObjectStateManager.TransactionManager.EntityBeingReparented && _context.ObjectStateManager.EntityInvokingFKSetter != wrappedEntity.Entity)
			{
				EnsureRelationshipNavigationAccessorsInitialized();
				RemoveEntityFromRelatedEnds(wrappedEntity, _wrappedOwner, _navigation.Reverse);
				MarkEntityAsDeletedInObjectStateManager(wrappedEntity);
			}
		}
		if (num)
		{
			OnAssociationChanged(CollectionChangeAction.Remove, wrappedEntity.Entity);
		}
	}

	internal bool IsDependentEndOfReferentialConstraint(bool checkIdentifying)
	{
		if (_relationMetadata != null)
		{
			foreach (ReferentialConstraint referentialConstraint in ((AssociationType)RelationMetadata).ReferentialConstraints)
			{
				if (referentialConstraint.ToRole == FromEndMember)
				{
					if (checkIdentifying)
					{
						return CheckIfAllPropertiesAreKeyProperties(referentialConstraint.ToRole.GetEntityType().KeyMemberNames, referentialConstraint.ToProperties);
					}
					return true;
				}
			}
		}
		return false;
	}

	internal bool IsPrincipalEndOfReferentialConstraint()
	{
		if (_relationMetadata != null)
		{
			foreach (ReferentialConstraint referentialConstraint in ((AssociationType)_relationMetadata).ReferentialConstraints)
			{
				if (referentialConstraint.FromRole == _fromEndMember)
				{
					return CheckIfAllPropertiesAreKeyProperties(referentialConstraint.ToRole.GetEntityType().KeyMemberNames, referentialConstraint.ToProperties);
				}
			}
		}
		return false;
	}

	internal static bool CheckIfAllPropertiesAreKeyProperties(string[] keyMemberNames, ReadOnlyMetadataCollection<EdmProperty> toProperties)
	{
		foreach (EdmProperty toProperty in toProperties)
		{
			bool flag = false;
			for (int i = 0; i < keyMemberNames.Length; i++)
			{
				if (keyMemberNames[i] == toProperty.Name)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	internal void IncludeEntity(IEntityWrapper wrappedEntity, bool addRelationshipAsUnchanged, bool doAttach)
	{
		EntityEntry entityEntry = _context.ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
		if (entityEntry != null && entityEntry.State == EntityState.Deleted)
		{
			throw Error.RelatedEnd_UnableToAddRelationshipWithDeletedEntity();
		}
		if (wrappedEntity.RequiresRelationshipChangeTracking || WrappedOwner.RequiresRelationshipChangeTracking)
		{
			RelatedEnd otherEndOfRelationship = GetOtherEndOfRelationship(wrappedEntity);
			ObjectContext.GetTypeUsage(otherEndOfRelationship.WrappedOwner.IdentityType);
			otherEndOfRelationship.AddToNavigationPropertyIfCompatible(this);
		}
		if (entityEntry == null)
		{
			AddGraphToObjectStateManager(wrappedEntity, relationshipAlreadyExists: false, addRelationshipAsUnchanged, doAttach);
		}
		else
		{
			if (FindRelationshipEntryInObjectStateManager(wrappedEntity) != null)
			{
				return;
			}
			VerifyDetachedKeyMatches(wrappedEntity.EntityKey);
			if (ObjectContext == null || wrappedEntity.Context == null)
			{
				return;
			}
			if (!IsForeignKey)
			{
				if (entityEntry.State == EntityState.Added)
				{
					AddRelationshipToObjectStateManager(wrappedEntity, addRelationshipAsUnchanged, doAttach: false);
				}
				else
				{
					AddRelationshipToObjectStateManager(wrappedEntity, addRelationshipAsUnchanged, doAttach);
				}
			}
			if (wrappedEntity.RequiresRelationshipChangeTracking || WrappedOwner.RequiresRelationshipChangeTracking)
			{
				UpdateSnapshotOfRelationships(wrappedEntity);
				if (doAttach && entityEntry.State != EntityState.Added)
				{
					EntityEntry entityEntry2 = ObjectContext.ObjectStateManager.GetEntityEntry(wrappedEntity.Entity);
					wrappedEntity.RelationshipManager.CheckReferentialConstraintProperties(entityEntry2);
				}
			}
		}
	}

	internal void MarkForeignKeyPropertiesModified()
	{
		ReferentialConstraint referentialConstraint = ((AssociationType)RelationMetadata).ReferentialConstraints[0];
		EntityEntry objectStateEntry = WrappedOwner.ObjectStateEntry;
		if (objectStateEntry.State != EntityState.Unchanged && objectStateEntry.State != EntityState.Modified)
		{
			return;
		}
		foreach (EdmProperty toProperty in referentialConstraint.ToProperties)
		{
			objectStateEntry.SetModifiedProperty(toProperty.Name);
		}
	}

	internal abstract bool CheckIfNavigationPropertyContainsEntity(IEntityWrapper wrapper);

	internal abstract void VerifyNavigationPropertyForAdd(IEntityWrapper wrapper);

	internal void AddToNavigationProperty(IEntityWrapper wrapper)
	{
		if (TargetAccessor.HasProperty && !CheckIfNavigationPropertyContainsEntity(wrapper))
		{
			TransactionManager transactionManager = wrapper.Context.ObjectStateManager.TransactionManager;
			if (transactionManager.IsAddTracking || transactionManager.IsAttachTracking)
			{
				wrapper.Context.ObjectStateManager.TrackPromotedRelationship(this, wrapper);
			}
			AddToObjectCache(wrapper);
		}
	}

	internal void RemoveFromNavigationProperty(IEntityWrapper wrapper)
	{
		if (TargetAccessor.HasProperty && CheckIfNavigationPropertyContainsEntity(wrapper))
		{
			RemoveFromObjectCache(wrapper);
		}
	}

	internal void ExcludeEntity(IEntityWrapper wrappedEntity)
	{
		if (_context.ObjectStateManager.TransactionManager.TrackProcessedEntities && (_context.ObjectStateManager.TransactionManager.IsAttachTracking || _context.ObjectStateManager.TransactionManager.IsAddTracking) && !_context.ObjectStateManager.TransactionManager.ProcessedEntities.Contains(wrappedEntity))
		{
			return;
		}
		EntityEntry entityEntry = _context.ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
		if (entityEntry != null && entityEntry.State != EntityState.Deleted && !wrappedEntity.RelationshipManager.NodeVisited)
		{
			wrappedEntity.RelationshipManager.NodeVisited = true;
			RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(wrappedEntity);
			if (!IsForeignKey)
			{
				RemoveRelationshipFromObjectStateManager(wrappedEntity, _wrappedOwner, _relationshipSet, _navigation);
			}
			RemoveEntityFromObjectStateManager(wrappedEntity);
		}
		else if (!IsForeignKey && FindRelationshipEntryInObjectStateManager(wrappedEntity) != null)
		{
			RemoveRelationshipFromObjectStateManager(wrappedEntity, _wrappedOwner, _relationshipSet, _navigation);
		}
	}

	internal RelationshipEntry FindRelationshipEntryInObjectStateManager(IEntityWrapper wrappedEntity)
	{
		EntityKey entityKey = wrappedEntity.EntityKey;
		EntityKey entityKey2 = _wrappedOwner.EntityKey;
		return _context.ObjectStateManager.FindRelationship(_relationshipSet, new KeyValuePair<string, EntityKey>(_navigation.From, entityKey2), new KeyValuePair<string, EntityKey>(_navigation.To, entityKey));
	}

	internal void Clear(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete)
	{
		ClearCollectionOrRef(wrappedEntity, navigation, doCascadeDelete);
	}

	internal void CheckReferentialConstraintProperties(EntityEntry ownerEntry)
	{
		foreach (ReferentialConstraint referentialConstraint in ((AssociationType)RelationMetadata).ReferentialConstraints)
		{
			if (referentialConstraint.ToRole == FromEndMember)
			{
				if (!CheckReferentialConstraintPrincipalProperty(ownerEntry, referentialConstraint))
				{
					throw new InvalidOperationException(referentialConstraint.BuildConstraintExceptionMessage());
				}
			}
			else if (referentialConstraint.FromRole == FromEndMember && !CheckReferentialConstraintDependentProperty(ownerEntry, referentialConstraint))
			{
				throw new InvalidOperationException(referentialConstraint.BuildConstraintExceptionMessage());
			}
		}
	}

	internal virtual bool CheckReferentialConstraintPrincipalProperty(EntityEntry ownerEntry, ReferentialConstraint constraint)
	{
		return false;
	}

	internal virtual bool CheckReferentialConstraintDependentProperty(EntityEntry ownerEntry, ReferentialConstraint constraint)
	{
		if (!IsEmpty())
		{
			foreach (IEntityWrapper wrappedEntity in GetWrappedEntities())
			{
				EntityEntry objectStateEntry = wrappedEntity.ObjectStateEntry;
				if (objectStateEntry != null && objectStateEntry.State != EntityState.Added && objectStateEntry.State != EntityState.Deleted && objectStateEntry.State != EntityState.Detached && !VerifyRIConstraintsWithRelatedEntry(constraint, objectStateEntry.GetCurrentEntityValue, ownerEntry.EntityKey))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal static bool VerifyRIConstraintsWithRelatedEntry(ReferentialConstraint constraint, Func<string, object> getDependentPropertyValue, EntityKey principalKey)
	{
		for (int i = 0; i < constraint.FromProperties.Count; i++)
		{
			string name = constraint.FromProperties[i].Name;
			string name2 = constraint.ToProperties[i].Name;
			object x = principalKey.FindValueByName(name);
			object y = getDependentPropertyValue(name2);
			if (!ByValueEqualityComparer.Default.Equals(x, y))
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerator GetEnumerator()
	{
		DeferredLoad();
		return GetInternalEnumerable().GetEnumerator();
	}

	internal void RemoveAll()
	{
		List<IEntityWrapper> list = null;
		bool flag = false;
		try
		{
			_suppressEvents = true;
			foreach (IEntityWrapper wrappedEntity in GetWrappedEntities())
			{
				if (list == null)
				{
					list = new List<IEntityWrapper>();
				}
				list.Add(wrappedEntity);
			}
			if (flag = list != null && list.Count > 0)
			{
				foreach (IEntityWrapper item in list)
				{
					Remove(item, doFixup: true, deleteEntity: false, deleteOwner: true, applyReferentialConstraints: true, preserveForeignKey: false);
				}
			}
		}
		finally
		{
			_suppressEvents = false;
		}
		if (flag)
		{
			OnAssociationChanged(CollectionChangeAction.Refresh, null);
		}
	}

	internal virtual void DetachAll(EntityState ownerEntityState)
	{
		List<IEntityWrapper> list = new List<IEntityWrapper>();
		foreach (IEntityWrapper wrappedEntity in GetWrappedEntities())
		{
			list.Add(wrappedEntity);
		}
		bool flag = ownerEntityState == EntityState.Added || _fromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many;
		foreach (IEntityWrapper item in list)
		{
			if (!ContainsEntity(item))
			{
				return;
			}
			if (flag)
			{
				DetachRelationshipFromObjectStateManager(item, _wrappedOwner, _relationshipSet, _navigation);
			}
			RelatedEnd otherEndOfRelationship = GetOtherEndOfRelationship(item);
			otherEndOfRelationship.RemoveFromCache(_wrappedOwner, resetIsLoaded: true, preserveForeignKey: false);
			otherEndOfRelationship.OnAssociationChanged(CollectionChangeAction.Remove, _wrappedOwner.Entity);
		}
		foreach (IEntityWrapper item2 in list)
		{
			GetOtherEndOfRelationship(item2);
			RemoveFromCache(item2, resetIsLoaded: false, preserveForeignKey: false);
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	internal void AddToCache(IEntityWrapper wrappedEntity, bool applyConstraints)
	{
		AddToLocalCache(wrappedEntity, applyConstraints);
		AddToObjectCache(wrappedEntity);
	}

	internal abstract void AddToLocalCache(IEntityWrapper wrappedEntity, bool applyConstraints);

	internal abstract void AddToObjectCache(IEntityWrapper wrappedEntity);

	internal bool RemoveFromCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey)
	{
		bool result = RemoveFromLocalCache(wrappedEntity, resetIsLoaded, preserveForeignKey);
		RemoveFromObjectCache(wrappedEntity);
		return result;
	}

	internal abstract bool RemoveFromLocalCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey);

	internal abstract bool RemoveFromObjectCache(IEntityWrapper wrappedEntity);

	internal virtual bool VerifyEntityForAdd(IEntityWrapper wrappedEntity, bool relationshipAlreadyExists)
	{
		if (relationshipAlreadyExists && ContainsEntity(wrappedEntity))
		{
			return false;
		}
		VerifyType(wrappedEntity);
		return true;
	}

	internal abstract void VerifyType(IEntityWrapper wrappedEntity);

	internal abstract bool CanSetEntityType(IEntityWrapper wrappedEntity);

	internal abstract void Include(bool addRelationshipAsUnchanged, bool doAttach);

	internal abstract void Exclude();

	internal abstract void ClearCollectionOrRef(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete);

	internal abstract bool ContainsEntity(IEntityWrapper wrappedEntity);

	internal abstract IEnumerable GetInternalEnumerable();

	internal abstract IEnumerable<IEntityWrapper> GetWrappedEntities();

	internal abstract void RetrieveReferentialConstraintProperties(Dictionary<string, KeyValuePair<object, IntBox>> keyValues, HashSet<object> visited);

	internal abstract bool IsEmpty();

	internal abstract void OnRelatedEndClear();

	internal abstract void ClearWrappedValues();

	internal abstract void VerifyMultiplicityConstraintsForAdd(bool applyConstraints);

	internal virtual void OnAssociationChanged(CollectionChangeAction collectionChangeAction, object entity)
	{
		if (!_suppressEvents && _onAssociationChanged != null)
		{
			_onAssociationChanged(this, new CollectionChangeEventArgs(collectionChangeAction, entity));
		}
	}

	internal virtual void AddEntityToObjectStateManager(IEntityWrapper wrappedEntity, bool doAttach)
	{
		EntitySet targetEntitySetFromRelationshipSet = GetTargetEntitySetFromRelationshipSet();
		if (!doAttach)
		{
			_context.AddSingleObject(targetEntitySetFromRelationshipSet, wrappedEntity, "entity");
		}
		else
		{
			_context.AttachSingleObject(wrappedEntity, targetEntitySetFromRelationshipSet);
		}
	}

	internal EntitySet GetTargetEntitySetFromRelationshipSet()
	{
		AssociationSet obj = (AssociationSet)_relationshipSet;
		AssociationEndMember associationEndMember = (AssociationEndMember)ToEndMember;
		return obj.AssociationSetEnds[associationEndMember.Name].EntitySet;
	}

	private RelationshipEntry AddRelationshipToObjectStateManager(IEntityWrapper wrappedEntity, bool addRelationshipAsUnchanged, bool doAttach)
	{
		EntityKey entityKey = _wrappedOwner.EntityKey;
		EntityKey entityKey2 = wrappedEntity.EntityKey;
		if ((object)entityKey == null)
		{
			throw Error.EntityKey_UnexpectedNull();
		}
		if ((object)entityKey2 == null)
		{
			throw Error.EntityKey_UnexpectedNull();
		}
		return ObjectContext.ObjectStateManager.AddRelation(new RelationshipWrapper((AssociationSet)_relationshipSet, new KeyValuePair<string, EntityKey>(_navigation.From, entityKey), new KeyValuePair<string, EntityKey>(_navigation.To, entityKey2)), (addRelationshipAsUnchanged || doAttach) ? EntityState.Unchanged : EntityState.Added);
	}

	private static void WalkObjectGraphToIncludeAllRelatedEntities(IEntityWrapper wrappedEntity, bool addRelationshipAsUnchanged, bool doAttach)
	{
		foreach (RelatedEnd relationship in wrappedEntity.RelationshipManager.Relationships)
		{
			relationship.Include(addRelationshipAsUnchanged, doAttach);
		}
	}

	internal static void RemoveEntityFromObjectStateManager(IEntityWrapper wrappedEntity)
	{
		if (wrappedEntity.Context != null && wrappedEntity.Context.ObjectStateManager.TransactionManager.IsAttachTracking && wrappedEntity.Context.ObjectStateManager.TransactionManager.PromotedKeyEntries.TryGetValue(wrappedEntity.Entity, out var value))
		{
			value.DegradeEntry();
			return;
		}
		value = MarkEntityAsDeletedInObjectStateManager(wrappedEntity);
		if (value != null && value.State != EntityState.Detached)
		{
			value.AcceptChanges();
		}
	}

	private static void RemoveRelationshipFromObjectStateManager(IEntityWrapper wrappedEntity, IEntityWrapper wrappedOwner, RelationshipSet relationshipSet, RelationshipNavigation navigation)
	{
		RelationshipEntry relationshipEntry = MarkRelationshipAsDeletedInObjectStateManager(wrappedEntity, wrappedOwner, relationshipSet, navigation);
		if (relationshipEntry != null && relationshipEntry.State != EntityState.Detached)
		{
			relationshipEntry.AcceptChanges();
		}
	}

	private void FixupOtherEndOfRelationshipForRemove(IEntityWrapper wrappedEntity, bool preserveForeignKey)
	{
		RelatedEnd otherEndOfRelationship = GetOtherEndOfRelationship(wrappedEntity);
		otherEndOfRelationship.Remove(_wrappedOwner, doFixup: false, deleteEntity: false, deleteOwner: false, applyReferentialConstraints: false, preserveForeignKey);
		otherEndOfRelationship.RemoveFromNavigationProperty(_wrappedOwner);
	}

	private static EntityEntry MarkEntityAsDeletedInObjectStateManager(IEntityWrapper wrappedEntity)
	{
		EntityEntry entityEntry = null;
		if (wrappedEntity.Context != null)
		{
			entityEntry = wrappedEntity.Context.ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
			entityEntry?.Delete(doFixup: false);
		}
		return entityEntry;
	}

	private static RelationshipEntry MarkRelationshipAsDeletedInObjectStateManager(IEntityWrapper wrappedEntity, IEntityWrapper wrappedOwner, RelationshipSet relationshipSet, RelationshipNavigation navigation)
	{
		RelationshipEntry result = null;
		if (wrappedOwner.Context != null && wrappedEntity.Context != null && relationshipSet != null)
		{
			EntityKey entityKey = wrappedOwner.EntityKey;
			EntityKey entityKey2 = wrappedEntity.EntityKey;
			result = wrappedEntity.Context.ObjectStateManager.DeleteRelationship(relationshipSet, new KeyValuePair<string, EntityKey>(navigation.From, entityKey), new KeyValuePair<string, EntityKey>(navigation.To, entityKey2));
		}
		return result;
	}

	private static void DetachRelationshipFromObjectStateManager(IEntityWrapper wrappedEntity, IEntityWrapper wrappedOwner, RelationshipSet relationshipSet, RelationshipNavigation navigation)
	{
		if (wrappedOwner.Context != null && wrappedEntity.Context != null && relationshipSet != null)
		{
			EntityKey entityKey = wrappedOwner.EntityKey;
			EntityKey entityKey2 = wrappedEntity.EntityKey;
			wrappedEntity.Context.ObjectStateManager.FindRelationship(relationshipSet, new KeyValuePair<string, EntityKey>(navigation.From, entityKey), new KeyValuePair<string, EntityKey>(navigation.To, entityKey2))?.DetachRelationshipEntry();
		}
	}

	private static void RemoveEntityFromRelatedEnds(IEntityWrapper wrappedEntity1, IEntityWrapper wrappedEntity2, RelationshipNavigation navigation)
	{
		foreach (RelatedEnd relationship in wrappedEntity1.RelationshipManager.Relationships)
		{
			bool flag = false;
			flag = CheckCascadeDeleteFlag(relationship.FromEndMember) || relationship.IsPrincipalEndOfReferentialConstraint();
			relationship.Clear(wrappedEntity2, navigation, flag);
		}
	}

	private static bool CheckCascadeDeleteFlag(RelationshipEndMember relationEndProperty)
	{
		if (relationEndProperty != null)
		{
			return relationEndProperty.DeleteBehavior == OperationAction.Cascade;
		}
		return false;
	}

	internal void AttachContext(ObjectContext context, MergeOption mergeOption)
	{
		if (!_wrappedOwner.InitializingProxyRelatedEnds)
		{
			EntitySet entitySet = (_wrappedOwner.EntityKey ?? throw Error.EntityKey_UnexpectedNull()).GetEntitySet(context.MetadataWorkspace);
			AttachContext(context, entitySet, mergeOption);
		}
	}

	internal void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		_wrappedOwner.RelationshipManager.NodeVisited = false;
		if (_context == context && _usingNoTracking == (mergeOption == MergeOption.NoTracking))
		{
			return;
		}
		bool flag = true;
		try
		{
			_sourceQuery = null;
			_context = context;
			_entityWrapperFactory = context.EntityWrapperFactory;
			_usingNoTracking = mergeOption == MergeOption.NoTracking;
			FindRelationshipSet(_context, entitySet, out var relationshipType, out var relationshipSet);
			if (relationshipSet != null)
			{
				_relationshipSet = relationshipSet;
				_relationMetadata = (RelationshipType)relationshipType;
				bool flag2 = false;
				bool flag3 = false;
				foreach (AssociationEndMember associationEndMember in ((AssociationType)_relationMetadata).AssociationEndMembers)
				{
					if (associationEndMember.Name == _navigation.From)
					{
						flag2 = true;
						_fromEndMember = associationEndMember;
					}
					if (associationEndMember.Name == _navigation.To)
					{
						flag3 = true;
						_toEndMember = associationEndMember;
					}
				}
				if (!(flag2 && flag3))
				{
					throw Error.RelatedEnd_RelatedEndNotFound();
				}
				ValidateDetachedEntityKey();
				flag = false;
				return;
			}
			foreach (EntitySetBase baseEntitySet in entitySet.EntityContainer.BaseEntitySets)
			{
				if (baseEntitySet is AssociationSet associationSet && associationSet.ElementType == relationshipType && associationSet.AssociationSetEnds[_navigation.From].EntitySet != entitySet && associationSet.AssociationSetEnds[_navigation.From].EntitySet.ElementType == entitySet.ElementType)
				{
					throw Error.RelatedEnd_EntitySetIsNotValidForRelationship(entitySet.EntityContainer.Name, entitySet.Name, _navigation.From, baseEntitySet.EntityContainer.Name, baseEntitySet.Name);
				}
			}
			throw Error.Collections_NoRelationshipSetMatched(_navigation.RelationshipName);
		}
		finally
		{
			if (flag)
			{
				DetachContext();
			}
		}
	}

	internal virtual void ValidateDetachedEntityKey()
	{
	}

	internal void FindRelationshipSet(ObjectContext context, EntitySet entitySet, out EdmType relationshipType, out RelationshipSet relationshipSet)
	{
		if (_navigation.AssociationType == null || _navigation.AssociationType.Index < 0)
		{
			FindRelationshipSet(context, _navigation, entitySet, out relationshipType, out relationshipSet);
			return;
		}
		MetadataOptimization metadataOptimization = context.MetadataWorkspace.MetadataOptimization;
		relationshipSet = metadataOptimization.FindCSpaceAssociationSet((AssociationType)(relationshipType = metadataOptimization.GetCSpaceAssociationType(_navigation.AssociationType)), _navigation.From, entitySet);
	}

	internal static void FindRelationshipSet(ObjectContext context, RelationshipNavigation navigation, EntitySet entitySet, out EdmType relationshipType, out RelationshipSet relationshipSet)
	{
		relationshipType = context.MetadataWorkspace.GetItem<EdmType>(navigation.RelationshipName, DataSpace.CSpace);
		if (relationshipType == null)
		{
			throw Error.Collections_NoRelationshipSetMatched(navigation.RelationshipName);
		}
		foreach (AssociationSet associationSet in entitySet.AssociationSets)
		{
			if (associationSet.ElementType == relationshipType && associationSet.AssociationSetEnds[navigation.From].EntitySet == entitySet)
			{
				relationshipSet = associationSet;
				return;
			}
		}
		relationshipSet = null;
	}

	internal void DetachContext()
	{
		if (_context != null && ObjectContext.ObjectStateManager.TransactionManager.IsAttachTracking && ObjectContext.ObjectStateManager.TransactionManager.OriginalMergeOption == MergeOption.NoTracking)
		{
			_usingNoTracking = true;
			return;
		}
		_sourceQuery = null;
		_context = null;
		_relationshipSet = null;
		_fromEndMember = null;
		_toEndMember = null;
		_relationMetadata = null;
		_isLoaded = false;
	}

	internal RelatedEnd GetOtherEndOfRelationship(IEntityWrapper wrappedEntity)
	{
		EnsureRelationshipNavigationAccessorsInitialized();
		return wrappedEntity.RelationshipManager.GetRelatedEnd(_navigation.Reverse, _relationshipFixer);
	}

	internal virtual void CheckOwnerNull()
	{
		if (_wrappedOwner.Entity == null)
		{
			throw Error.RelatedEnd_OwnerIsNull();
		}
	}

	internal void InitializeRelatedEnd(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
	{
		SetWrappedOwner(wrappedOwner);
		_navigation = navigation;
		_relationshipFixer = relationshipFixer;
	}

	internal void SetWrappedOwner(IEntityWrapper wrappedOwner)
	{
		_wrappedOwner = ((wrappedOwner != null) ? wrappedOwner : NullEntityWrapper.NullWrapper);
		_owner = wrappedOwner.Entity as IEntityWithRelationships;
	}

	internal static bool IsValidEntityKeyType(EntityKey entityKey)
	{
		if (!entityKey.IsTemporary && (object)EntityKey.EntityNotValidKey != entityKey)
		{
			return (object)EntityKey.NoEntitySetKey != entityKey;
		}
		return false;
	}

	[OnDeserialized]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void OnDeserialized(StreamingContext context)
	{
		_wrappedOwner = EntityWrapperFactory.WrapEntityUsingContext(_owner, ObjectContext);
	}

	private void EnsureRelationshipNavigationAccessorsInitialized()
	{
		if (!RelationshipNavigation.IsInitialized)
		{
			NavigationPropertyAccessor navigationPropertyAccessor = null;
			NavigationPropertyAccessor navigationPropertyAccessor2 = null;
			string relationshipName = _navigation.RelationshipName;
			string from = _navigation.From;
			string to = _navigation.To;
			AssociationType obj = (RelationMetadata as AssociationType) ?? _wrappedOwner.RelationshipManager.GetRelationshipType(relationshipName);
			if (obj.AssociationEndMembers.TryGetValue(from, ignoreCase: false, out var item))
			{
				navigationPropertyAccessor2 = MetadataHelper.GetNavigationPropertyAccessor(MetadataHelper.GetEntityTypeForEnd(item), relationshipName, from, to);
			}
			if (obj.AssociationEndMembers.TryGetValue(to, ignoreCase: false, out var item2))
			{
				navigationPropertyAccessor = MetadataHelper.GetNavigationPropertyAccessor(MetadataHelper.GetEntityTypeForEnd(item2), relationshipName, to, from);
			}
			if (navigationPropertyAccessor == null || navigationPropertyAccessor2 == null)
			{
				throw RelationshipManager.UnableToGetMetadata(WrappedOwner, relationshipName);
			}
			RelationshipNavigation.InitializeAccessors(navigationPropertyAccessor, navigationPropertyAccessor2);
		}
	}

	internal bool DisableLazyLoading()
	{
		if (_context == null)
		{
			return false;
		}
		bool lazyLoadingEnabled = _context.ContextOptions.LazyLoadingEnabled;
		_context.ContextOptions.LazyLoadingEnabled = false;
		return lazyLoadingEnabled;
	}

	internal void ResetLazyLoading(bool state)
	{
		if (_context != null)
		{
			_context.ContextOptions.LazyLoadingEnabled = state;
		}
	}
}
