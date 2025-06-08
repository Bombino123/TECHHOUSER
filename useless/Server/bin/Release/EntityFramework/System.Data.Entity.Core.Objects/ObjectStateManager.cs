using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Objects;

public class ObjectStateManager : IEntityStateManager
{
	private const int InitialListSize = 16;

	private Dictionary<EntityKey, EntityEntry> _addedEntityStore;

	private Dictionary<EntityKey, EntityEntry> _modifiedEntityStore;

	private Dictionary<EntityKey, EntityEntry> _deletedEntityStore;

	private Dictionary<EntityKey, EntityEntry> _unchangedEntityStore;

	private Dictionary<object, EntityEntry> _keylessEntityStore;

	private Dictionary<RelationshipWrapper, RelationshipEntry> _addedRelationshipStore;

	private Dictionary<RelationshipWrapper, RelationshipEntry> _deletedRelationshipStore;

	private Dictionary<RelationshipWrapper, RelationshipEntry> _unchangedRelationshipStore;

	private readonly Dictionary<EdmType, StateManagerTypeMetadata> _metadataStore;

	private readonly Dictionary<EntitySetQualifiedType, StateManagerTypeMetadata> _metadataMapping;

	private readonly MetadataWorkspace _metadataWorkspace;

	private CollectionChangeEventHandler onObjectStateManagerChangedDelegate;

	private CollectionChangeEventHandler onEntityDeletedDelegate;

	private bool _inRelationshipFixup;

	private bool _isDisposed;

	private ComplexTypeMaterializer _complexTypeMaterializer;

	private readonly Dictionary<EntityKey, HashSet<Tuple<EntityReference, EntityEntry>>> _danglingForeignKeys = new Dictionary<EntityKey, HashSet<Tuple<EntityReference, EntityEntry>>>();

	private HashSet<EntityEntry> _entriesWithConceptualNulls;

	private readonly EntityWrapperFactory _entityWrapperFactory;

	private bool _detectChangesNeeded;

	internal virtual object ChangingObject { get; set; }

	internal virtual string ChangingEntityMember { get; set; }

	internal virtual string ChangingMember { get; set; }

	internal virtual EntityState ChangingState { get; set; }

	internal virtual bool SaveOriginalValues { get; set; }

	internal virtual object ChangingOldValue { get; set; }

	internal virtual bool InRelationshipFixup => _inRelationshipFixup;

	internal virtual ComplexTypeMaterializer ComplexTypeMaterializer
	{
		get
		{
			if (_complexTypeMaterializer == null)
			{
				_complexTypeMaterializer = new ComplexTypeMaterializer(MetadataWorkspace);
			}
			return _complexTypeMaterializer;
		}
	}

	internal virtual TransactionManager TransactionManager { get; private set; }

	internal virtual EntityWrapperFactory EntityWrapperFactory => _entityWrapperFactory;

	public virtual MetadataWorkspace MetadataWorkspace => _metadataWorkspace;

	internal virtual bool IsDisposed => _isDisposed;

	internal virtual object EntityInvokingFKSetter { get; set; }

	public event CollectionChangeEventHandler ObjectStateManagerChanged
	{
		add
		{
			onObjectStateManagerChangedDelegate = (CollectionChangeEventHandler)Delegate.Combine(onObjectStateManagerChangedDelegate, value);
		}
		remove
		{
			onObjectStateManagerChangedDelegate = (CollectionChangeEventHandler)Delegate.Remove(onObjectStateManagerChangedDelegate, value);
		}
	}

	internal event CollectionChangeEventHandler EntityDeleted
	{
		add
		{
			onEntityDeletedDelegate = (CollectionChangeEventHandler)Delegate.Combine(onEntityDeletedDelegate, value);
		}
		remove
		{
			onEntityDeletedDelegate = (CollectionChangeEventHandler)Delegate.Remove(onEntityDeletedDelegate, value);
		}
	}

	internal ObjectStateManager()
	{
	}

	public ObjectStateManager(MetadataWorkspace metadataWorkspace)
	{
		Check.NotNull(metadataWorkspace, "metadataWorkspace");
		_metadataWorkspace = metadataWorkspace;
		_metadataStore = new Dictionary<EdmType, StateManagerTypeMetadata>();
		_metadataMapping = new Dictionary<EntitySetQualifiedType, StateManagerTypeMetadata>(EntitySetQualifiedType.EqualityComparer);
		_isDisposed = false;
		_entityWrapperFactory = new EntityWrapperFactory();
		TransactionManager = new TransactionManager();
	}

	internal virtual void OnObjectStateManagerChanged(CollectionChangeAction action, object entity)
	{
		if (onObjectStateManagerChangedDelegate != null)
		{
			onObjectStateManagerChangedDelegate(this, new CollectionChangeEventArgs(action, entity));
		}
	}

	private void OnEntityDeleted(CollectionChangeAction action, object entity)
	{
		if (onEntityDeletedDelegate != null)
		{
			onEntityDeletedDelegate(this, new CollectionChangeEventArgs(action, entity));
		}
	}

	internal virtual EntityEntry AddKeyEntry(EntityKey entityKey, EntitySet entitySet)
	{
		if (FindEntityEntry(entityKey) != null)
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(entitySet.ElementType.Name));
		}
		return InternalAddEntityEntry(entityKey, entitySet);
	}

	internal EntityEntry GetOrAddKeyEntry(EntityKey entityKey, EntitySet entitySet)
	{
		if (TryGetEntityEntry(entityKey, out var entry))
		{
			return entry;
		}
		return InternalAddEntityEntry(entityKey, entitySet);
	}

	private EntityEntry InternalAddEntityEntry(EntityKey entityKey, EntitySet entitySet)
	{
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = GetOrAddStateManagerTypeMetadata(entitySet.ElementType);
		EntityEntry entityEntry = new EntityEntry(entityKey, entitySet, this, orAddStateManagerTypeMetadata);
		AddEntityEntryToDictionary(entityEntry, entityEntry.State);
		return entityEntry;
	}

	private void ValidateProxyType(IEntityWrapper wrappedEntity)
	{
		Type identityType = wrappedEntity.IdentityType;
		Type type = wrappedEntity.Entity.GetType();
		if (identityType != type)
		{
			EntityProxyTypeInfo proxyType = EntityProxyFactory.GetProxyType(MetadataWorkspace.GetItem<ClrEntityType>(identityType.FullNameWithNesting(), DataSpace.OSpace), MetadataWorkspace);
			if (proxyType == null || proxyType.ProxyType != type)
			{
				throw new InvalidOperationException(Strings.EntityProxyTypeInfo_DuplicateOSpaceType(identityType.FullName));
			}
		}
	}

	internal virtual EntityEntry AddEntry(IEntityWrapper wrappedObject, EntityKey passedKey, EntitySet entitySet, string argumentName, bool isAdded)
	{
		EntityKey entityKey = passedKey;
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = GetOrAddStateManagerTypeMetadata(wrappedObject.IdentityType, entitySet);
		ValidateProxyType(wrappedObject);
		EdmType edmType = orAddStateManagerTypeMetadata.CdmMetadata.EdmType;
		if (isAdded && !entitySet.ElementType.IsAssignableFrom(edmType))
		{
			throw new ArgumentException(Strings.ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType(wrappedObject.Entity.GetType().Name, TypeHelpers.GetFullName(entitySet.EntityContainer.Name, entitySet.Name)), argumentName);
		}
		EntityKey entityKey2 = null;
		entityKey2 = ((!isAdded) ? wrappedObject.EntityKey : wrappedObject.GetEntityKeyFromEntity());
		if ((object)entityKey2 != null)
		{
			entityKey = entityKey2;
			if ((object)entityKey == null)
			{
				throw new InvalidOperationException(Strings.EntityKey_UnexpectedNull);
			}
			if (wrappedObject.EntityKey != entityKey)
			{
				throw new InvalidOperationException(Strings.EntityKey_DoesntMatchKeyOnEntity(wrappedObject.Entity.GetType().FullName));
			}
		}
		if ((object)entityKey != null && !entityKey.IsTemporary && !isAdded)
		{
			CheckKeyMatchesEntity(wrappedObject, entityKey, entitySet, forAttach: false);
		}
		EntityEntry entityEntry;
		if (isAdded && ((entityKey2 == null && (entityEntry = FindEntityEntry(wrappedObject.Entity)) != null) || (entityKey2 != null && (entityEntry = FindEntityEntry(entityKey2)) != null)))
		{
			if (entityEntry.Entity != wrappedObject.Entity)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(wrappedObject.IdentityType.FullName));
			}
			if (entityEntry.State != EntityState.Added)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(entityEntry.State));
			}
			return null;
		}
		if ((object)entityKey == null || (isAdded && !entityKey.IsTemporary))
		{
			entityKey = (wrappedObject.EntityKey = new EntityKey(entitySet));
		}
		if (!wrappedObject.OwnsRelationshipManager)
		{
			wrappedObject.RelationshipManager.ClearRelatedEndWrappers();
		}
		EntityEntry entityEntry2 = new EntityEntry(wrappedObject, entityKey, entitySet, this, orAddStateManagerTypeMetadata, isAdded ? EntityState.Added : EntityState.Unchanged);
		entityEntry2.AttachObjectStateManagerToEntity();
		AddEntityEntryToDictionary(entityEntry2, entityEntry2.State);
		OnObjectStateManagerChanged(CollectionChangeAction.Add, entityEntry2.Entity);
		if (!isAdded)
		{
			FixupReferencesByForeignKeys(entityEntry2);
		}
		return entityEntry2;
	}

	internal virtual void FixupReferencesByForeignKeys(EntityEntry newEntry, bool replaceAddedRefs = false)
	{
		if (!((EntitySet)newEntry.EntitySet).HasForeignKeyRelationships)
		{
			return;
		}
		newEntry.FixupReferencesByForeignKeys(replaceAddedRefs);
		foreach (EntityEntry item in GetNonFixedupEntriesContainingForeignKey(newEntry.EntityKey))
		{
			item.FixupReferencesByForeignKeys(replaceAddedRefs: false, newEntry.EntitySet);
		}
		RemoveForeignKeyFromIndex(newEntry.EntityKey);
	}

	internal virtual void AddEntryContainingForeignKeyToIndex(EntityReference relatedEnd, EntityKey foreignKey, EntityEntry entry)
	{
		if (!_danglingForeignKeys.TryGetValue(foreignKey, out var value))
		{
			value = new HashSet<Tuple<EntityReference, EntityEntry>>();
			_danglingForeignKeys.Add(foreignKey, value);
		}
		value.Add(Tuple.Create(relatedEnd, entry));
	}

	[Conditional("DEBUG")]
	internal virtual void AssertEntryDoesNotExistInForeignKeyIndex(EntityEntry entry)
	{
		foreach (Tuple<EntityReference, EntityEntry> item in _danglingForeignKeys.SelectMany((KeyValuePair<EntityKey, HashSet<Tuple<EntityReference, EntityEntry>>> kv) => kv.Value))
		{
			if (item.Item2.State != EntityState.Detached)
			{
				_ = entry.State;
				_ = 1;
			}
		}
	}

	[Conditional("DEBUG")]
	internal virtual void AssertAllForeignKeyIndexEntriesAreValid()
	{
		if (GetMaxEntityEntriesForDetectChanges() > 100)
		{
			return;
		}
		new HashSet<ObjectStateEntry>(GetObjectStateEntriesInternal(~EntityState.Detached));
		foreach (Tuple<EntityReference, EntityEntry> item in _danglingForeignKeys.SelectMany((KeyValuePair<EntityKey, HashSet<Tuple<EntityReference, EntityEntry>>> kv) => kv.Value))
		{
			_ = item;
		}
	}

	internal virtual void RemoveEntryFromForeignKeyIndex(EntityReference relatedEnd, EntityKey foreignKey, EntityEntry entry)
	{
		if (_danglingForeignKeys.TryGetValue(foreignKey, out var value))
		{
			value.Remove(Tuple.Create(relatedEnd, entry));
		}
	}

	internal virtual void RemoveForeignKeyFromIndex(EntityKey foreignKey)
	{
		_danglingForeignKeys.Remove(foreignKey);
	}

	internal virtual IEnumerable<EntityEntry> GetNonFixedupEntriesContainingForeignKey(EntityKey foreignKey)
	{
		if (_danglingForeignKeys.TryGetValue(foreignKey, out var value))
		{
			return value.Select((Tuple<EntityReference, EntityEntry> e) => e.Item2).ToList();
		}
		return Enumerable.Empty<EntityEntry>();
	}

	internal virtual void RememberEntryWithConceptualNull(EntityEntry entry)
	{
		if (_entriesWithConceptualNulls == null)
		{
			_entriesWithConceptualNulls = new HashSet<EntityEntry>();
		}
		_entriesWithConceptualNulls.Add(entry);
	}

	internal virtual bool SomeEntryWithConceptualNullExists()
	{
		if (_entriesWithConceptualNulls != null)
		{
			return _entriesWithConceptualNulls.Count != 0;
		}
		return false;
	}

	internal virtual bool EntryHasConceptualNull(EntityEntry entry)
	{
		if (_entriesWithConceptualNulls != null)
		{
			return _entriesWithConceptualNulls.Contains(entry);
		}
		return false;
	}

	internal virtual void ForgetEntryWithConceptualNull(EntityEntry entry, bool resetAllKeys)
	{
		if (entry.IsKeyEntry || _entriesWithConceptualNulls == null || !_entriesWithConceptualNulls.Remove(entry) || !entry.RelationshipManager.HasRelationships)
		{
			return;
		}
		foreach (RelatedEnd relationship in entry.RelationshipManager.Relationships)
		{
			if (relationship is EntityReference entityReference && ForeignKeyFactory.IsConceptualNullKey(entityReference.CachedForeignKey))
			{
				if (!resetAllKeys)
				{
					_entriesWithConceptualNulls.Add(entry);
					break;
				}
				entityReference.SetCachedForeignKey(null, null);
			}
		}
	}

	internal virtual void PromoteKeyEntryInitialization(ObjectContext contextToAttach, EntityEntry keyEntry, IEntityWrapper wrappedEntity, bool replacingEntry)
	{
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = GetOrAddStateManagerTypeMetadata(wrappedEntity.IdentityType, (EntitySet)keyEntry.EntitySet);
		ValidateProxyType(wrappedEntity);
		keyEntry.PromoteKeyEntry(wrappedEntity, orAddStateManagerTypeMetadata);
		AddEntryToKeylessStore(keyEntry);
		if (replacingEntry)
		{
			wrappedEntity.SetChangeTracker(null);
		}
		wrappedEntity.SetChangeTracker(keyEntry);
		if (contextToAttach != null)
		{
			wrappedEntity.AttachContext(contextToAttach, (EntitySet)keyEntry.EntitySet, MergeOption.AppendOnly);
		}
		wrappedEntity.TakeSnapshot(keyEntry);
		OnObjectStateManagerChanged(CollectionChangeAction.Add, keyEntry.Entity);
	}

	internal virtual void PromoteKeyEntry(EntityEntry keyEntry, IEntityWrapper wrappedEntity, bool replacingEntry, bool setIsLoaded, bool keyEntryInitialized)
	{
		if (!keyEntryInitialized)
		{
			PromoteKeyEntryInitialization(null, keyEntry, wrappedEntity, replacingEntry);
		}
		bool flag = true;
		try
		{
			RelationshipEntry[] array = CopyOfRelationshipsByKey(keyEntry.EntityKey);
			foreach (RelationshipEntry relationshipEntry in array)
			{
				if (relationshipEntry.State != EntityState.Deleted)
				{
					AssociationEndMember associationEndMember = keyEntry.GetAssociationEndMember(relationshipEntry);
					AssociationEndMember otherAssociationEnd = MetadataHelper.GetOtherAssociationEnd(associationEndMember);
					EntityEntry otherEndOfRelationship = keyEntry.GetOtherEndOfRelationship(relationshipEntry);
					AddEntityToCollectionOrReference(MergeOption.AppendOnly, wrappedEntity, associationEndMember, otherEndOfRelationship.WrappedEntity, otherAssociationEnd, setIsLoaded, relationshipAlreadyExists: true, inKeyEntryPromotion: true);
				}
			}
			FixupReferencesByForeignKeys(keyEntry);
			flag = false;
		}
		finally
		{
			if (flag)
			{
				keyEntry.DetachObjectStateManagerFromEntity();
				RemoveEntryFromKeylessStore(wrappedEntity);
				keyEntry.DegradeEntry();
			}
		}
		if (TransactionManager.IsAttachTracking)
		{
			TransactionManager.PromotedKeyEntries.Add(wrappedEntity.Entity, keyEntry);
		}
	}

	internal virtual void TrackPromotedRelationship(RelatedEnd relatedEnd, IEntityWrapper wrappedEntity)
	{
		if (!TransactionManager.PromotedRelationships.TryGetValue(relatedEnd, out var value))
		{
			value = new List<IEntityWrapper>();
			TransactionManager.PromotedRelationships.Add(relatedEnd, value);
		}
		value.Add(wrappedEntity);
	}

	internal virtual void DegradePromotedRelationships()
	{
		foreach (KeyValuePair<RelatedEnd, IList<IEntityWrapper>> promotedRelationship in TransactionManager.PromotedRelationships)
		{
			foreach (IEntityWrapper item in promotedRelationship.Value)
			{
				if (promotedRelationship.Key.RemoveFromCache(item, resetIsLoaded: false, preserveForeignKey: false))
				{
					promotedRelationship.Key.OnAssociationChanged(CollectionChangeAction.Remove, item.Entity);
				}
			}
		}
	}

	internal static void AddEntityToCollectionOrReference(MergeOption mergeOption, IEntityWrapper wrappedSource, AssociationEndMember sourceMember, IEntityWrapper wrappedTarget, AssociationEndMember targetMember, bool setIsLoaded, bool relationshipAlreadyExists, bool inKeyEntryPromotion)
	{
		RelatedEnd relatedEndInternal = wrappedSource.RelationshipManager.GetRelatedEndInternal(sourceMember.DeclaringType.FullName, targetMember.Name);
		if (targetMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
		{
			EntityReference entityReference = (EntityReference)relatedEndInternal;
			switch (mergeOption)
			{
			case MergeOption.AppendOnly:
				if (inKeyEntryPromotion && !entityReference.IsEmpty() && entityReference.ReferenceValue.Entity != wrappedTarget.Entity)
				{
					throw new InvalidOperationException(Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
				}
				break;
			case MergeOption.OverwriteChanges:
			case MergeOption.PreserveChanges:
			{
				IEntityWrapper referenceValue = entityReference.ReferenceValue;
				if (referenceValue != null && referenceValue.Entity != null && referenceValue != wrappedTarget)
				{
					RelationshipEntry relationshipEntry = relatedEndInternal.FindRelationshipEntryInObjectStateManager(referenceValue);
					relatedEndInternal.RemoveAll();
					if (relationshipEntry != null && relationshipEntry.State == EntityState.Deleted)
					{
						relationshipEntry.AcceptChanges();
					}
				}
				break;
			}
			}
		}
		RelatedEnd relatedEnd = null;
		if (mergeOption == MergeOption.NoTracking)
		{
			relatedEnd = relatedEndInternal.GetOtherEndOfRelationship(wrappedTarget);
			if (relatedEnd.IsLoaded)
			{
				throw new InvalidOperationException(Strings.Collections_CannotFillTryDifferentMergeOption(relatedEnd.SourceRoleName, relatedEnd.RelationshipName));
			}
		}
		if (relatedEnd == null)
		{
			relatedEnd = relatedEndInternal.GetOtherEndOfRelationship(wrappedTarget);
		}
		relatedEndInternal.Add(wrappedTarget, applyConstraints: true, addRelationshipAsUnchanged: true, relationshipAlreadyExists, allowModifyingOtherEndOfRelationship: true, forceForeignKeyChanges: true);
		UpdateRelatedEnd(relatedEndInternal, wrappedTarget, setIsLoaded, mergeOption);
		UpdateRelatedEnd(relatedEnd, wrappedSource, setIsLoaded, mergeOption);
		if (inKeyEntryPromotion && wrappedSource.Context.ObjectStateManager.TransactionManager.IsAttachTracking)
		{
			wrappedSource.Context.ObjectStateManager.TrackPromotedRelationship(relatedEndInternal, wrappedTarget);
			wrappedSource.Context.ObjectStateManager.TrackPromotedRelationship(relatedEnd, wrappedSource);
		}
	}

	private static void UpdateRelatedEnd(RelatedEnd relatedEnd, IEntityWrapper wrappedRelatedEntity, bool setIsLoaded, MergeOption mergeOption)
	{
		AssociationEndMember associationEndMember = (AssociationEndMember)relatedEnd.ToEndMember;
		if (associationEndMember.RelationshipMultiplicity != RelationshipMultiplicity.One && associationEndMember.RelationshipMultiplicity != 0)
		{
			return;
		}
		if (setIsLoaded)
		{
			relatedEnd.IsLoaded = true;
		}
		if (mergeOption == MergeOption.NoTracking)
		{
			EntityKey entityKey = wrappedRelatedEntity.EntityKey;
			if ((object)entityKey == null)
			{
				throw new InvalidOperationException(Strings.EntityKey_UnexpectedNull);
			}
			((EntityReference)relatedEnd).DetachedEntityKey = entityKey;
		}
	}

	internal virtual int UpdateRelationships(ObjectContext context, MergeOption mergeOption, AssociationSet associationSet, AssociationEndMember sourceMember, IEntityWrapper wrappedSource, AssociationEndMember targetMember, IList targets, bool setIsLoaded)
	{
		int num = 0;
		EntityKey sourceKey = wrappedSource.EntityKey;
		context.ObjectStateManager.TransactionManager.BeginGraphUpdate();
		try
		{
			if (targets != null)
			{
				if (mergeOption == MergeOption.NoTracking)
				{
					RelatedEnd relatedEndInternal = wrappedSource.RelationshipManager.GetRelatedEndInternal(sourceMember.DeclaringType.FullName, targetMember.Name);
					if (!relatedEndInternal.IsEmpty())
					{
						throw new InvalidOperationException(Strings.Collections_CannotFillTryDifferentMergeOption(relatedEndInternal.SourceRoleName, relatedEndInternal.RelationshipName));
					}
				}
				Lazy<ILookup<EntityKey, RelationshipEntry>> lazy = new Lazy<ILookup<EntityKey, RelationshipEntry>>(() => GetRelationshipLookup(context.ObjectStateManager, associationSet, sourceMember, sourceKey));
				foreach (object target in targets)
				{
					IEntityWrapper entityWrapper = target as IEntityWrapper;
					if (entityWrapper == null)
					{
						entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(target, context);
					}
					num++;
					if (mergeOption == MergeOption.NoTracking)
					{
						AddEntityToCollectionOrReference(MergeOption.NoTracking, wrappedSource, sourceMember, entityWrapper, targetMember, setIsLoaded, relationshipAlreadyExists: true, inKeyEntryPromotion: false);
						continue;
					}
					ObjectStateManager objectStateManager = context.ObjectStateManager;
					EntityKey entityKey = entityWrapper.EntityKey;
					if (TryUpdateExistingRelationships(context, mergeOption, associationSet, sourceMember, lazy.Value, wrappedSource, targetMember, entityKey, setIsLoaded, out var newEntryState))
					{
						continue;
					}
					bool flag = true;
					switch (sourceMember.RelationshipMultiplicity)
					{
					case RelationshipMultiplicity.ZeroOrOne:
					case RelationshipMultiplicity.One:
					{
						ILookup<EntityKey, RelationshipEntry> relationshipLookup = GetRelationshipLookup(context.ObjectStateManager, associationSet, targetMember, entityKey);
						flag = !TryUpdateExistingRelationships(context, mergeOption, associationSet, targetMember, relationshipLookup, entityWrapper, sourceMember, sourceKey, setIsLoaded, out newEntryState);
						break;
					}
					}
					if (flag)
					{
						if (newEntryState != EntityState.Deleted)
						{
							AddEntityToCollectionOrReference(mergeOption, wrappedSource, sourceMember, entityWrapper, targetMember, setIsLoaded, relationshipAlreadyExists: false, inKeyEntryPromotion: false);
							continue;
						}
						RelationshipWrapper wrapper = new RelationshipWrapper(associationSet, sourceMember.Name, sourceKey, targetMember.Name, entityKey);
						objectStateManager.AddNewRelation(wrapper, EntityState.Deleted);
					}
				}
			}
			if (num == 0)
			{
				EnsureCollectionNotNull(sourceMember, wrappedSource, targetMember);
			}
		}
		finally
		{
			context.ObjectStateManager.TransactionManager.EndGraphUpdate();
		}
		return num;
	}

	internal static ILookup<EntityKey, RelationshipEntry> GetRelationshipLookup(ObjectStateManager manager, AssociationSet associationSet, AssociationEndMember sourceMember, EntityKey sourceKey)
	{
		List<RelationshipEntry> list = new List<RelationshipEntry>();
		foreach (RelationshipEntry item in manager.FindRelationshipsByKey(sourceKey))
		{
			if (item.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey))
			{
				list.Add(item);
			}
		}
		return list.ToLookup((RelationshipEntry r) => r.RelationshipWrapper.GetOtherEntityKey(sourceKey));
	}

	private static void EnsureCollectionNotNull(AssociationEndMember sourceMember, IEntityWrapper wrappedSource, AssociationEndMember targetMember)
	{
		RelatedEnd relatedEndInternal = wrappedSource.RelationshipManager.GetRelatedEndInternal(sourceMember.DeclaringType.FullName, targetMember.Name);
		AssociationEndMember associationEndMember = (AssociationEndMember)relatedEndInternal.ToEndMember;
		if (associationEndMember != null && associationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many && relatedEndInternal.TargetAccessor.HasProperty)
		{
			wrappedSource.EnsureCollectionNotNull(relatedEndInternal);
		}
	}

	internal virtual void RemoveRelationships(MergeOption mergeOption, AssociationSet associationSet, EntityKey sourceKey, AssociationEndMember sourceMember)
	{
		List<RelationshipEntry> list = new List<RelationshipEntry>(16);
		switch (mergeOption)
		{
		case MergeOption.OverwriteChanges:
			foreach (RelationshipEntry item in FindRelationshipsByKey(sourceKey))
			{
				if (item.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey))
				{
					list.Add(item);
				}
			}
			break;
		case MergeOption.PreserveChanges:
			foreach (RelationshipEntry item2 in FindRelationshipsByKey(sourceKey))
			{
				if (item2.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey) && item2.State != EntityState.Added)
				{
					list.Add(item2);
				}
			}
			break;
		}
		foreach (RelationshipEntry item3 in list)
		{
			RemoveRelatedEndsAndDetachRelationship(item3, setIsLoaded: true);
		}
	}

	internal static bool TryUpdateExistingRelationships(ObjectContext context, MergeOption mergeOption, AssociationSet associationSet, AssociationEndMember sourceMember, ILookup<EntityKey, RelationshipEntry> relationshipLookup, IEntityWrapper wrappedSource, AssociationEndMember targetMember, EntityKey targetKey, bool setIsLoaded, out EntityState newEntryState)
	{
		newEntryState = EntityState.Unchanged;
		if (associationSet.ElementType.IsForeignKey)
		{
			return true;
		}
		bool flag = true;
		ObjectStateManager objectStateManager = context.ObjectStateManager;
		List<RelationshipEntry> list = null;
		List<RelationshipEntry> list2 = null;
		foreach (RelationshipEntry item in relationshipLookup[targetKey])
		{
			if (list2 == null)
			{
				list2 = new List<RelationshipEntry>(16);
			}
			list2.Add(item);
		}
		switch (targetMember.RelationshipMultiplicity)
		{
		case RelationshipMultiplicity.ZeroOrOne:
		case RelationshipMultiplicity.One:
			foreach (RelationshipEntry item2 in relationshipLookup.Where((IGrouping<EntityKey, RelationshipEntry> g) => g.Key != targetKey).SelectMany((IGrouping<EntityKey, RelationshipEntry> re) => re))
			{
				switch (mergeOption)
				{
				case MergeOption.AppendOnly:
					if (item2.State != EntityState.Deleted)
					{
						flag = false;
					}
					break;
				case MergeOption.OverwriteChanges:
					if (list == null)
					{
						list = new List<RelationshipEntry>(16);
					}
					list.Add(item2);
					break;
				case MergeOption.PreserveChanges:
					switch (item2.State)
					{
					case EntityState.Added:
						newEntryState = EntityState.Deleted;
						break;
					case EntityState.Unchanged:
						if (list == null)
						{
							list = new List<RelationshipEntry>(16);
						}
						list.Add(item2);
						break;
					case EntityState.Deleted:
						newEntryState = EntityState.Deleted;
						if (list == null)
						{
							list = new List<RelationshipEntry>(16);
						}
						list.Add(item2);
						break;
					}
					break;
				}
			}
			break;
		}
		if (list != null)
		{
			foreach (RelationshipEntry item3 in list)
			{
				if (item3.State != EntityState.Detached)
				{
					RemoveRelatedEndsAndDetachRelationship(item3, setIsLoaded);
				}
			}
		}
		if (list2 != null)
		{
			foreach (RelationshipEntry item4 in list2)
			{
				flag = false;
				switch (mergeOption)
				{
				case MergeOption.OverwriteChanges:
					if (item4.State == EntityState.Added)
					{
						item4.AcceptChanges();
					}
					else
					{
						if (item4.State != EntityState.Deleted)
						{
							break;
						}
						EntityEntry entityEntry = objectStateManager.GetEntityEntry(targetKey);
						if (entityEntry.State != EntityState.Deleted)
						{
							if (!entityEntry.IsKeyEntry)
							{
								AddEntityToCollectionOrReference(mergeOption, wrappedSource, sourceMember, entityEntry.WrappedEntity, targetMember, setIsLoaded, relationshipAlreadyExists: true, inKeyEntryPromotion: false);
							}
							item4.RevertDelete();
						}
					}
					break;
				case MergeOption.PreserveChanges:
					if (item4.State == EntityState.Added)
					{
						item4.AcceptChanges();
					}
					break;
				}
			}
		}
		return !flag;
	}

	internal static void RemoveRelatedEndsAndDetachRelationship(RelationshipEntry relationshipToRemove, bool setIsLoaded)
	{
		if (setIsLoaded)
		{
			UnloadReferenceRelatedEnds(relationshipToRemove);
		}
		if (relationshipToRemove.State != EntityState.Deleted)
		{
			relationshipToRemove.Delete();
		}
		if (relationshipToRemove.State != EntityState.Detached)
		{
			relationshipToRemove.AcceptChanges();
		}
	}

	private static void UnloadReferenceRelatedEnds(RelationshipEntry relationshipEntry)
	{
		ObjectStateManager objectStateManager = relationshipEntry.ObjectStateManager;
		ReadOnlyMetadataCollection<AssociationEndMember> associationEndMembers = relationshipEntry.RelationshipWrapper.AssociationEndMembers;
		UnloadReferenceRelatedEnds(objectStateManager, relationshipEntry, relationshipEntry.RelationshipWrapper.GetEntityKey(0), associationEndMembers[1].Name);
		UnloadReferenceRelatedEnds(objectStateManager, relationshipEntry, relationshipEntry.RelationshipWrapper.GetEntityKey(1), associationEndMembers[0].Name);
	}

	private static void UnloadReferenceRelatedEnds(ObjectStateManager cache, RelationshipEntry relationshipEntry, EntityKey sourceEntityKey, string targetRoleName)
	{
		EntityEntry entityEntry = cache.GetEntityEntry(sourceEntityKey);
		if (entityEntry.WrappedEntity.Entity != null && entityEntry.WrappedEntity.RelationshipManager.GetRelatedEndInternal(((AssociationSet)relationshipEntry.EntitySet).ElementType.FullName, targetRoleName) is EntityReference entityReference)
		{
			entityReference.IsLoaded = false;
		}
	}

	internal virtual EntityEntry AttachEntry(EntityKey entityKey, IEntityWrapper wrappedObject, EntitySet entitySet)
	{
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = GetOrAddStateManagerTypeMetadata(wrappedObject.IdentityType, entitySet);
		ValidateProxyType(wrappedObject);
		CheckKeyMatchesEntity(wrappedObject, entityKey, entitySet, forAttach: true);
		if (!wrappedObject.OwnsRelationshipManager)
		{
			wrappedObject.RelationshipManager.ClearRelatedEndWrappers();
		}
		EntityEntry entityEntry = new EntityEntry(wrappedObject, entityKey, entitySet, this, orAddStateManagerTypeMetadata, EntityState.Unchanged);
		entityEntry.AttachObjectStateManagerToEntity();
		AddEntityEntryToDictionary(entityEntry, entityEntry.State);
		OnObjectStateManagerChanged(CollectionChangeAction.Add, entityEntry.Entity);
		return entityEntry;
	}

	private void CheckKeyMatchesEntity(IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySetForType, bool forAttach)
	{
		EntitySet entitySet = entityKey.GetEntitySet(MetadataWorkspace);
		if (entitySet == null)
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_InvalidKey);
		}
		entityKey.ValidateEntityKey(_metadataWorkspace, entitySet);
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = GetOrAddStateManagerTypeMetadata(wrappedEntity.IdentityType, entitySetForType);
		for (int i = 0; i < entitySet.ElementType.KeyMembers.Count; i++)
		{
			EdmMember edmMember = entitySet.ElementType.KeyMembers[i];
			int ordinalforCLayerMemberName = orAddStateManagerTypeMetadata.GetOrdinalforCLayerMemberName(edmMember.Name);
			if (ordinalforCLayerMemberName < 0)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_InvalidKey);
			}
			object value = orAddStateManagerTypeMetadata.Member(ordinalforCLayerMemberName).GetValue(wrappedEntity.Entity);
			object y = entityKey.FindValueByName(edmMember.Name);
			if (!ByValueEqualityComparer.Default.Equals(value, y))
			{
				throw new InvalidOperationException(forAttach ? Strings.ObjectStateManager_KeyPropertyDoesntMatchValueInKeyForAttach : Strings.ObjectStateManager_KeyPropertyDoesntMatchValueInKey);
			}
		}
	}

	internal virtual RelationshipEntry AddNewRelation(RelationshipWrapper wrapper, EntityState desiredState)
	{
		RelationshipEntry relationshipEntry = new RelationshipEntry(this, desiredState, wrapper);
		AddRelationshipEntryToDictionary(relationshipEntry, desiredState);
		AddRelationshipToLookup(relationshipEntry);
		return relationshipEntry;
	}

	internal virtual RelationshipEntry AddRelation(RelationshipWrapper wrapper, EntityState desiredState)
	{
		RelationshipEntry relationshipEntry = FindRelationship(wrapper);
		if (relationshipEntry == null)
		{
			relationshipEntry = AddNewRelation(wrapper, desiredState);
		}
		else if (EntityState.Deleted != relationshipEntry.State)
		{
			if (EntityState.Unchanged == desiredState)
			{
				relationshipEntry.AcceptChanges();
			}
			else if (EntityState.Deleted == desiredState)
			{
				relationshipEntry.AcceptChanges();
				relationshipEntry.Delete(doFixup: false);
			}
		}
		else if (EntityState.Deleted != desiredState)
		{
			relationshipEntry.RevertDelete();
		}
		return relationshipEntry;
	}

	private void AddRelationshipToLookup(RelationshipEntry relationship)
	{
		AddRelationshipEndToLookup(relationship.RelationshipWrapper.Key0, relationship);
		if (!relationship.RelationshipWrapper.Key0.Equals(relationship.RelationshipWrapper.Key1))
		{
			AddRelationshipEndToLookup(relationship.RelationshipWrapper.Key1, relationship);
		}
	}

	private void AddRelationshipEndToLookup(EntityKey key, RelationshipEntry relationship)
	{
		GetEntityEntry(key).AddRelationshipEnd(relationship);
	}

	private void DeleteRelationshipFromLookup(RelationshipEntry relationship)
	{
		DeleteRelationshipEndFromLookup(relationship.RelationshipWrapper.Key0, relationship);
		if (!relationship.RelationshipWrapper.Key0.Equals(relationship.RelationshipWrapper.Key1))
		{
			DeleteRelationshipEndFromLookup(relationship.RelationshipWrapper.Key1, relationship);
		}
	}

	private void DeleteRelationshipEndFromLookup(EntityKey key, RelationshipEntry relationship)
	{
		GetEntityEntry(key).RemoveRelationshipEnd(relationship);
	}

	internal virtual RelationshipEntry FindRelationship(RelationshipSet relationshipSet, KeyValuePair<string, EntityKey> roleAndKey1, KeyValuePair<string, EntityKey> roleAndKey2)
	{
		if ((object)roleAndKey1.Value == null || (object)roleAndKey2.Value == null)
		{
			return null;
		}
		return FindRelationship(new RelationshipWrapper((AssociationSet)relationshipSet, roleAndKey1, roleAndKey2));
	}

	internal virtual RelationshipEntry FindRelationship(RelationshipWrapper relationshipWrapper)
	{
		RelationshipEntry value = null;
		if ((_unchangedRelationshipStore != null && _unchangedRelationshipStore.TryGetValue(relationshipWrapper, out value)) || (_deletedRelationshipStore != null && _deletedRelationshipStore.TryGetValue(relationshipWrapper, out value)))
		{
			_ = 1;
		}
		else if (_addedRelationshipStore != null)
		{
			_addedRelationshipStore.TryGetValue(relationshipWrapper, out value);
		}
		else
			_ = 0;
		return value;
	}

	internal virtual RelationshipEntry DeleteRelationship(RelationshipSet relationshipSet, KeyValuePair<string, EntityKey> roleAndKey1, KeyValuePair<string, EntityKey> roleAndKey2)
	{
		RelationshipEntry relationshipEntry = FindRelationship(relationshipSet, roleAndKey1, roleAndKey2);
		relationshipEntry?.Delete(doFixup: false);
		return relationshipEntry;
	}

	internal virtual void DeleteKeyEntry(EntityEntry keyEntry)
	{
		if (keyEntry != null && keyEntry.IsKeyEntry)
		{
			ChangeState(keyEntry, keyEntry.State, EntityState.Detached);
		}
	}

	internal virtual RelationshipEntry[] CopyOfRelationshipsByKey(EntityKey key)
	{
		return FindRelationshipsByKey(key).ToArray();
	}

	internal virtual EntityEntry.RelationshipEndEnumerable FindRelationshipsByKey(EntityKey key)
	{
		return new EntityEntry.RelationshipEndEnumerable(FindEntityEntry(key));
	}

	IEnumerable<IEntityStateEntry> IEntityStateManager.FindRelationshipsByKey(EntityKey key)
	{
		return FindRelationshipsByKey(key);
	}

	[Conditional("DEBUG")]
	private void ValidateKeylessEntityStore()
	{
		Dictionary<EntityKey, EntityEntry>[] array = new Dictionary<EntityKey, EntityEntry>[4] { _unchangedEntityStore, _modifiedEntityStore, _addedEntityStore, _deletedEntityStore };
		if (_keylessEntityStore != null && _keylessEntityStore.Count == array.Sum((Dictionary<EntityKey, EntityEntry> s) => s?.Count ?? 0))
		{
			return;
		}
		if (_keylessEntityStore != null)
		{
			foreach (EntityEntry value3 in _keylessEntityStore.Values)
			{
				bool flag = false;
				EntityEntry value;
				if (_addedEntityStore != null)
				{
					flag = _addedEntityStore.TryGetValue(value3.EntityKey, out value);
				}
				if (_modifiedEntityStore != null)
				{
					flag |= _modifiedEntityStore.TryGetValue(value3.EntityKey, out value);
				}
				if (_deletedEntityStore != null)
				{
					flag |= _deletedEntityStore.TryGetValue(value3.EntityKey, out value);
				}
				if (_unchangedEntityStore != null)
				{
					flag |= _unchangedEntityStore.TryGetValue(value3.EntityKey, out value);
				}
			}
		}
		Dictionary<EntityKey, EntityEntry>[] array2 = array;
		foreach (Dictionary<EntityKey, EntityEntry> dictionary in array2)
		{
			if (dictionary == null)
			{
				continue;
			}
			foreach (EntityEntry value4 in dictionary.Values)
			{
				if (value4.Entity != null && !(value4.Entity is IEntityWithKey))
				{
					_keylessEntityStore.TryGetValue(value4.Entity, out var _);
				}
			}
		}
	}

	private bool TryGetEntryFromKeylessStore(object entity, out EntityEntry entryRef)
	{
		entryRef = null;
		if (entity == null)
		{
			return false;
		}
		if (_keylessEntityStore != null && _keylessEntityStore.TryGetValue(entity, out entryRef))
		{
			return true;
		}
		entryRef = null;
		return false;
	}

	public virtual IEnumerable<ObjectStateEntry> GetObjectStateEntries(EntityState state)
	{
		if ((EntityState.Detached & state) != 0)
		{
			throw new ArgumentException(Strings.ObjectStateManager_DetachedObjectStateEntriesDoesNotExistInObjectStateManager);
		}
		return GetObjectStateEntriesInternal(state);
	}

	IEnumerable<IEntityStateEntry> IEntityStateManager.GetEntityStateEntries(EntityState state)
	{
		foreach (ObjectStateEntry item in GetObjectStateEntriesInternal(state))
		{
			yield return item;
		}
	}

	internal virtual bool HasChanges()
	{
		if ((_addedRelationshipStore == null || _addedRelationshipStore.Count <= 0) && (_addedEntityStore == null || _addedEntityStore.Count <= 0) && (_modifiedEntityStore == null || _modifiedEntityStore.Count <= 0) && (_deletedRelationshipStore == null || _deletedRelationshipStore.Count <= 0))
		{
			if (_deletedEntityStore != null)
			{
				return _deletedEntityStore.Count > 0;
			}
			return false;
		}
		return true;
	}

	internal virtual int GetObjectStateEntriesCount(EntityState state)
	{
		int num = 0;
		if ((EntityState.Added & state) != 0)
		{
			num += ((_addedRelationshipStore != null) ? _addedRelationshipStore.Count : 0);
			num += ((_addedEntityStore != null) ? _addedEntityStore.Count : 0);
		}
		if ((EntityState.Modified & state) != 0)
		{
			num += ((_modifiedEntityStore != null) ? _modifiedEntityStore.Count : 0);
		}
		if ((EntityState.Deleted & state) != 0)
		{
			num += ((_deletedRelationshipStore != null) ? _deletedRelationshipStore.Count : 0);
			num += ((_deletedEntityStore != null) ? _deletedEntityStore.Count : 0);
		}
		if ((EntityState.Unchanged & state) != 0)
		{
			num += ((_unchangedRelationshipStore != null) ? _unchangedRelationshipStore.Count : 0);
			num += ((_unchangedEntityStore != null) ? _unchangedEntityStore.Count : 0);
		}
		return num;
	}

	private int GetMaxEntityEntriesForDetectChanges()
	{
		int num = 0;
		if (_addedEntityStore != null)
		{
			num += _addedEntityStore.Count;
		}
		if (_modifiedEntityStore != null)
		{
			num += _modifiedEntityStore.Count;
		}
		if (_deletedEntityStore != null)
		{
			num += _deletedEntityStore.Count;
		}
		if (_unchangedEntityStore != null)
		{
			num += _unchangedEntityStore.Count;
		}
		return num;
	}

	internal virtual IEnumerable<ObjectStateEntry> GetObjectStateEntriesInternal(EntityState state)
	{
		int objectStateEntriesCount = GetObjectStateEntriesCount(state);
		ObjectStateEntry[] array = new ObjectStateEntry[objectStateEntriesCount];
		objectStateEntriesCount = 0;
		if ((EntityState.Added & state) != 0 && _addedRelationshipStore != null)
		{
			foreach (KeyValuePair<RelationshipWrapper, RelationshipEntry> item in _addedRelationshipStore)
			{
				array[objectStateEntriesCount++] = item.Value;
			}
		}
		if ((EntityState.Deleted & state) != 0 && _deletedRelationshipStore != null)
		{
			foreach (KeyValuePair<RelationshipWrapper, RelationshipEntry> item2 in _deletedRelationshipStore)
			{
				array[objectStateEntriesCount++] = item2.Value;
			}
		}
		if ((EntityState.Unchanged & state) != 0 && _unchangedRelationshipStore != null)
		{
			foreach (KeyValuePair<RelationshipWrapper, RelationshipEntry> item3 in _unchangedRelationshipStore)
			{
				array[objectStateEntriesCount++] = item3.Value;
			}
		}
		if ((EntityState.Added & state) != 0 && _addedEntityStore != null)
		{
			foreach (KeyValuePair<EntityKey, EntityEntry> item4 in _addedEntityStore)
			{
				array[objectStateEntriesCount++] = item4.Value;
			}
		}
		if ((EntityState.Modified & state) != 0 && _modifiedEntityStore != null)
		{
			foreach (KeyValuePair<EntityKey, EntityEntry> item5 in _modifiedEntityStore)
			{
				array[objectStateEntriesCount++] = item5.Value;
			}
		}
		if ((EntityState.Deleted & state) != 0 && _deletedEntityStore != null)
		{
			foreach (KeyValuePair<EntityKey, EntityEntry> item6 in _deletedEntityStore)
			{
				array[objectStateEntriesCount++] = item6.Value;
			}
		}
		if ((EntityState.Unchanged & state) != 0 && _unchangedEntityStore != null)
		{
			foreach (KeyValuePair<EntityKey, EntityEntry> item7 in _unchangedEntityStore)
			{
				array[objectStateEntriesCount++] = item7.Value;
			}
		}
		return array;
	}

	private IList<EntityEntry> GetEntityEntriesForDetectChanges()
	{
		if (!_detectChangesNeeded)
		{
			return null;
		}
		List<EntityEntry> entries = null;
		GetEntityEntriesForDetectChanges(_addedEntityStore, ref entries);
		GetEntityEntriesForDetectChanges(_modifiedEntityStore, ref entries);
		GetEntityEntriesForDetectChanges(_deletedEntityStore, ref entries);
		GetEntityEntriesForDetectChanges(_unchangedEntityStore, ref entries);
		if (entries == null)
		{
			_detectChangesNeeded = false;
		}
		return entries;
	}

	private void GetEntityEntriesForDetectChanges(Dictionary<EntityKey, EntityEntry> entityStore, ref List<EntityEntry> entries)
	{
		if (entityStore == null)
		{
			return;
		}
		foreach (EntityEntry value in entityStore.Values)
		{
			if (value.RequiresAnyChangeTracking)
			{
				if (entries == null)
				{
					entries = new List<EntityEntry>(GetMaxEntityEntriesForDetectChanges());
				}
				entries.Add(value);
			}
		}
	}

	internal virtual void FixupKey(EntityEntry entry)
	{
		EntityKey entityKey = entry.EntityKey;
		EntitySet obj = (EntitySet)entry.EntitySet;
		bool hasForeignKeyRelationships = obj.HasForeignKeyRelationships;
		bool hasIndependentRelationships = obj.HasIndependentRelationships;
		if (hasForeignKeyRelationships)
		{
			entry.FixupForeignKeysByReference();
		}
		EntityKey entityKey2;
		try
		{
			entityKey2 = new EntityKey((EntitySet)entry.EntitySet, entry.CurrentValues);
		}
		catch (ArgumentException innerException)
		{
			throw new ArgumentException(Strings.ObjectStateManager_ChangeStateFromAddedWithNullKeyIsInvalid, innerException);
		}
		EntityEntry entityEntry = FindEntityEntry(entityKey2);
		if (entityEntry != null)
		{
			if (!entityEntry.IsKeyEntry)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_CannotFixUpKeyToExistingValues(entry.WrappedEntity.IdentityType.FullName));
			}
			entityKey2 = entityEntry.EntityKey;
		}
		RelationshipEntry[] array = null;
		if (hasIndependentRelationships)
		{
			array = entry.GetRelationshipEnds().ToArray();
			RelationshipEntry[] array2 = array;
			foreach (RelationshipEntry relationshipEntry in array2)
			{
				RemoveObjectStateEntryFromDictionary(relationshipEntry, relationshipEntry.State);
			}
		}
		RemoveObjectStateEntryFromDictionary(entry, EntityState.Added);
		ResetEntityKey(entry, entityKey2);
		if (hasIndependentRelationships)
		{
			entry.UpdateRelationshipEnds(entityKey, entityEntry);
			RelationshipEntry[] array2 = array;
			foreach (RelationshipEntry relationshipEntry2 in array2)
			{
				AddRelationshipEntryToDictionary(relationshipEntry2, relationshipEntry2.State);
			}
		}
		if (entityEntry != null)
		{
			PromoteKeyEntry(entityEntry, entry.WrappedEntity, replacingEntry: true, setIsLoaded: false, keyEntryInitialized: false);
			entry = entityEntry;
		}
		else
		{
			AddEntityEntryToDictionary(entry, EntityState.Unchanged);
		}
		if (hasForeignKeyRelationships)
		{
			FixupReferencesByForeignKeys(entry);
		}
	}

	internal virtual void ReplaceKeyWithTemporaryKey(EntityEntry entry)
	{
		EntityKey entityKey = entry.EntityKey;
		EntityKey value = new EntityKey(entry.EntitySet);
		RelationshipEntry[] array = entry.GetRelationshipEnds().ToArray();
		RelationshipEntry[] array2 = array;
		foreach (RelationshipEntry relationshipEntry in array2)
		{
			RemoveObjectStateEntryFromDictionary(relationshipEntry, relationshipEntry.State);
		}
		RemoveObjectStateEntryFromDictionary(entry, entry.State);
		ResetEntityKey(entry, value);
		entry.UpdateRelationshipEnds(entityKey, null);
		array2 = array;
		foreach (RelationshipEntry relationshipEntry2 in array2)
		{
			AddRelationshipEntryToDictionary(relationshipEntry2, relationshipEntry2.State);
		}
		AddEntityEntryToDictionary(entry, EntityState.Added);
	}

	private void ResetEntityKey(EntityEntry entry, EntityKey value)
	{
		EntityKey entityKey = entry.WrappedEntity.EntityKey;
		if (entityKey == null || value.Equals(entityKey))
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_AcceptChangesEntityKeyIsNotValid);
		}
		try
		{
			_inRelationshipFixup = true;
			entry.WrappedEntity.EntityKey = value;
			IEntityWrapper wrappedEntity = entry.WrappedEntity;
			if (wrappedEntity.EntityKey != value)
			{
				throw new InvalidOperationException(Strings.EntityKey_DoesntMatchKeyOnEntity(wrappedEntity.Entity.GetType().FullName));
			}
		}
		finally
		{
			_inRelationshipFixup = false;
		}
		entry.EntityKey = value;
	}

	public virtual ObjectStateEntry ChangeObjectState(object entity, EntityState entityState)
	{
		Check.NotNull(entity, "entity");
		EntityUtil.CheckValidStateForChangeEntityState(entityState);
		EntityEntry entityEntry = null;
		TransactionManager.BeginLocalPublicAPI();
		try
		{
			EntityKey entityKey = entity as EntityKey;
			entityEntry = ((entityKey != null) ? FindEntityEntry(entityKey) : FindEntityEntry(entity));
			if (entityEntry == null)
			{
				if (entityState == EntityState.Detached)
				{
					return null;
				}
				throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
			}
			entityEntry.ChangeObjectState(entityState);
			return entityEntry;
		}
		finally
		{
			TransactionManager.EndLocalPublicAPI();
		}
	}

	public virtual ObjectStateEntry ChangeRelationshipState(object sourceEntity, object targetEntity, string navigationProperty, EntityState relationshipState)
	{
		VerifyParametersForChangeRelationshipState(sourceEntity, targetEntity, out var sourceEntry, out var targetEntry);
		Check.NotEmpty(navigationProperty, "navigationProperty");
		RelatedEnd relatedEnd = sourceEntry.WrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty);
		return ChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);
	}

	public virtual ObjectStateEntry ChangeRelationshipState<TEntity>(TEntity sourceEntity, object targetEntity, Expression<Func<TEntity, object>> navigationPropertySelector, EntityState relationshipState) where TEntity : class
	{
		VerifyParametersForChangeRelationshipState(sourceEntity, targetEntity, out var sourceEntry, out var targetEntry);
		bool removedConvert;
		string navigationProperty = ObjectContext.ParsePropertySelectorExpression(navigationPropertySelector, out removedConvert);
		RelatedEnd relatedEnd = sourceEntry.WrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty, removedConvert);
		return ChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);
	}

	public virtual ObjectStateEntry ChangeRelationshipState(object sourceEntity, object targetEntity, string relationshipName, string targetRoleName, EntityState relationshipState)
	{
		VerifyParametersForChangeRelationshipState(sourceEntity, targetEntity, out var sourceEntry, out var targetEntry);
		RelatedEnd relatedEndInternal = sourceEntry.WrappedEntity.RelationshipManager.GetRelatedEndInternal(relationshipName, targetRoleName);
		return ChangeRelationshipState(sourceEntry, targetEntry, relatedEndInternal, relationshipState);
	}

	private ObjectStateEntry ChangeRelationshipState(EntityEntry sourceEntry, EntityEntry targetEntry, RelatedEnd relatedEnd, EntityState relationshipState)
	{
		VerifyInitialStateForChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);
		RelationshipWrapper relationshipWrapper = new RelationshipWrapper((AssociationSet)relatedEnd.RelationshipSet, new KeyValuePair<string, EntityKey>(relatedEnd.SourceRoleName, sourceEntry.EntityKey), new KeyValuePair<string, EntityKey>(relatedEnd.TargetRoleName, targetEntry.EntityKey));
		RelationshipEntry relationshipEntry = FindRelationship(relationshipWrapper);
		if (relationshipEntry == null && relationshipState == EntityState.Detached)
		{
			return null;
		}
		TransactionManager.BeginLocalPublicAPI();
		try
		{
			if (relationshipEntry != null)
			{
				relationshipEntry.ChangeRelationshipState(targetEntry, relatedEnd, relationshipState);
			}
			else
			{
				relationshipEntry = CreateRelationship(targetEntry, relatedEnd, relationshipWrapper, relationshipState);
			}
		}
		finally
		{
			TransactionManager.EndLocalPublicAPI();
		}
		if (relationshipState != EntityState.Detached)
		{
			return relationshipEntry;
		}
		return null;
	}

	private void VerifyParametersForChangeRelationshipState(object sourceEntity, object targetEntity, out EntityEntry sourceEntry, out EntityEntry targetEntry)
	{
		sourceEntry = GetEntityEntryByObjectOrEntityKey(sourceEntity);
		targetEntry = GetEntityEntryByObjectOrEntityKey(targetEntity);
	}

	private static void VerifyInitialStateForChangeRelationshipState(EntityEntry sourceEntry, EntityEntry targetEntry, RelatedEnd relatedEnd, EntityState relationshipState)
	{
		relatedEnd.VerifyType(targetEntry.WrappedEntity);
		if (relatedEnd.IsForeignKey)
		{
			throw new NotSupportedException(Strings.ObjectStateManager_ChangeRelationshipStateNotSupportedForForeignKeyAssociations);
		}
		EntityUtil.CheckValidStateForChangeRelationshipState(relationshipState, "relationshipState");
		if ((sourceEntry.State == EntityState.Deleted || targetEntry.State == EntityState.Deleted) && relationshipState != EntityState.Deleted && relationshipState != EntityState.Detached)
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_CannotChangeRelationshipStateEntityDeleted);
		}
		if ((sourceEntry.State == EntityState.Added || targetEntry.State == EntityState.Added) && relationshipState != EntityState.Added && relationshipState != EntityState.Detached)
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_CannotChangeRelationshipStateEntityAdded);
		}
	}

	private RelationshipEntry CreateRelationship(EntityEntry targetEntry, RelatedEnd relatedEnd, RelationshipWrapper relationshipWrapper, EntityState requestedState)
	{
		RelationshipEntry relationshipEntry = null;
		switch (requestedState)
		{
		case EntityState.Added:
			relatedEnd.Add(targetEntry.WrappedEntity, applyConstraints: true, addRelationshipAsUnchanged: false, relationshipAlreadyExists: false, allowModifyingOtherEndOfRelationship: false, forceForeignKeyChanges: true);
			relationshipEntry = FindRelationship(relationshipWrapper);
			break;
		case EntityState.Unchanged:
			relatedEnd.Add(targetEntry.WrappedEntity, applyConstraints: true, addRelationshipAsUnchanged: false, relationshipAlreadyExists: false, allowModifyingOtherEndOfRelationship: false, forceForeignKeyChanges: true);
			relationshipEntry = FindRelationship(relationshipWrapper);
			relationshipEntry.AcceptChanges();
			break;
		case EntityState.Deleted:
			relationshipEntry = AddNewRelation(relationshipWrapper, EntityState.Deleted);
			break;
		}
		return relationshipEntry;
	}

	private EntityEntry GetEntityEntryByObjectOrEntityKey(object o)
	{
		EntityKey entityKey = o as EntityKey;
		EntityEntry obj = ((entityKey != null) ? FindEntityEntry(entityKey) : FindEntityEntry(o)) ?? throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(o.GetType().FullName));
		if (obj.IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_CannotChangeRelationshipStateKeyEntry);
		}
		return obj;
	}

	IEntityStateEntry IEntityStateManager.GetEntityStateEntry(EntityKey key)
	{
		return GetEntityEntry(key);
	}

	public virtual ObjectStateEntry GetObjectStateEntry(EntityKey key)
	{
		if (!TryGetObjectStateEntry(key, out var entry))
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistForEntityKey);
		}
		return entry;
	}

	internal virtual EntityEntry GetEntityEntry(EntityKey key)
	{
		if (!TryGetEntityEntry(key, out var entry))
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistForEntityKey);
		}
		return entry;
	}

	public virtual ObjectStateEntry GetObjectStateEntry(object entity)
	{
		if (!TryGetObjectStateEntry(entity, out var entry))
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
		}
		return entry;
	}

	internal virtual EntityEntry GetEntityEntry(object entity)
	{
		return FindEntityEntry(entity) ?? throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
	}

	public virtual bool TryGetObjectStateEntry(object entity, out ObjectStateEntry entry)
	{
		Check.NotNull(entity, "entity");
		entry = null;
		EntityKey entityKey = entity as EntityKey;
		if (entityKey != null)
		{
			return TryGetObjectStateEntry(entityKey, out entry);
		}
		entry = FindEntityEntry(entity);
		return entry != null;
	}

	bool IEntityStateManager.TryGetEntityStateEntry(EntityKey key, out IEntityStateEntry entry)
	{
		ObjectStateEntry entry2;
		bool result = TryGetObjectStateEntry(key, out entry2);
		entry = entry2;
		return result;
	}

	bool IEntityStateManager.TryGetReferenceKey(EntityKey dependentKey, AssociationEndMember principalRole, out EntityKey principalKey)
	{
		if (!TryGetEntityEntry(dependentKey, out var entry))
		{
			principalKey = null;
			return false;
		}
		return entry.TryGetReferenceKey(principalRole, out principalKey);
	}

	public virtual bool TryGetObjectStateEntry(EntityKey key, out ObjectStateEntry entry)
	{
		EntityEntry entry2;
		bool result = TryGetEntityEntry(key, out entry2);
		entry = entry2;
		return result;
	}

	internal virtual bool TryGetEntityEntry(EntityKey key, out EntityEntry entry)
	{
		entry = null;
		if (key.IsTemporary)
		{
			return _addedEntityStore != null && _addedEntityStore.TryGetValue(key, out entry);
		}
		return (_unchangedEntityStore != null && _unchangedEntityStore.TryGetValue(key, out entry)) || (_modifiedEntityStore != null && _modifiedEntityStore.TryGetValue(key, out entry)) || (_deletedEntityStore != null && _deletedEntityStore.TryGetValue(key, out entry));
	}

	internal virtual EntityEntry FindEntityEntry(EntityKey key)
	{
		EntityEntry entry = null;
		if ((object)key != null)
		{
			TryGetEntityEntry(key, out entry);
		}
		return entry;
	}

	internal virtual EntityEntry FindEntityEntry(object entity)
	{
		EntityEntry entryRef = null;
		if (entity is IEntityWithKey { EntityKey: var entityKey })
		{
			if ((object)entityKey != null)
			{
				TryGetEntityEntry(entityKey, out entryRef);
			}
		}
		else
		{
			TryGetEntryFromKeylessStore(entity, out entryRef);
		}
		if (entryRef != null && entity != entryRef.Entity)
		{
			entryRef = null;
		}
		return entryRef;
	}

	public virtual RelationshipManager GetRelationshipManager(object entity)
	{
		if (!TryGetRelationshipManager(entity, out var relationshipManager))
		{
			throw new InvalidOperationException(Strings.ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity);
		}
		return relationshipManager;
	}

	public virtual bool TryGetRelationshipManager(object entity, out RelationshipManager relationshipManager)
	{
		Check.NotNull(entity, "entity");
		if (entity is IEntityWithRelationships entityWithRelationships)
		{
			relationshipManager = entityWithRelationships.RelationshipManager;
			if (relationshipManager == null)
			{
				throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
			}
			if (relationshipManager.WrappedOwner.Entity != entity)
			{
				throw new InvalidOperationException(Strings.RelationshipManager_InvalidRelationshipManagerOwner);
			}
		}
		else
		{
			IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingStateManager(entity, this);
			if (entityWrapper.Context == null)
			{
				relationshipManager = null;
				return false;
			}
			relationshipManager = entityWrapper.RelationshipManager;
		}
		return true;
	}

	internal virtual void ChangeState(RelationshipEntry entry, EntityState oldState, EntityState newState)
	{
		if (newState == EntityState.Detached)
		{
			DeleteRelationshipFromLookup(entry);
			RemoveObjectStateEntryFromDictionary(entry, oldState);
			entry.Reset();
		}
		else
		{
			RemoveObjectStateEntryFromDictionary(entry, oldState);
			AddRelationshipEntryToDictionary(entry, newState);
		}
	}

	internal virtual void ChangeState(EntityEntry entry, EntityState oldState, EntityState newState)
	{
		bool flag = !entry.IsKeyEntry;
		if (newState == EntityState.Detached)
		{
			RelationshipEntry[] array = CopyOfRelationshipsByKey(entry.EntityKey);
			foreach (RelationshipEntry relationshipEntry in array)
			{
				ChangeState(relationshipEntry, relationshipEntry.State, EntityState.Detached);
			}
			RemoveObjectStateEntryFromDictionary(entry, oldState);
			IEntityWrapper wrappedEntity = entry.WrappedEntity;
			entry.Reset();
			if (flag && wrappedEntity.Entity != null && !TransactionManager.IsAttachTracking)
			{
				OnEntityDeleted(CollectionChangeAction.Remove, wrappedEntity.Entity);
				OnObjectStateManagerChanged(CollectionChangeAction.Remove, wrappedEntity.Entity);
			}
		}
		else
		{
			RemoveObjectStateEntryFromDictionary(entry, oldState);
			AddEntityEntryToDictionary(entry, newState);
		}
		if (newState == EntityState.Deleted)
		{
			entry.RemoveFromForeignKeyIndex();
			ForgetEntryWithConceptualNull(entry, resetAllKeys: true);
			if (flag)
			{
				OnEntityDeleted(CollectionChangeAction.Remove, entry.Entity);
				OnObjectStateManagerChanged(CollectionChangeAction.Remove, entry.Entity);
			}
		}
	}

	private void AddRelationshipEntryToDictionary(RelationshipEntry entry, EntityState state)
	{
		Dictionary<RelationshipWrapper, RelationshipEntry> dictionary = null;
		switch (state)
		{
		case EntityState.Unchanged:
			if (_unchangedRelationshipStore == null)
			{
				_unchangedRelationshipStore = new Dictionary<RelationshipWrapper, RelationshipEntry>();
			}
			dictionary = _unchangedRelationshipStore;
			break;
		case EntityState.Added:
			if (_addedRelationshipStore == null)
			{
				_addedRelationshipStore = new Dictionary<RelationshipWrapper, RelationshipEntry>();
			}
			dictionary = _addedRelationshipStore;
			break;
		case EntityState.Deleted:
			if (_deletedRelationshipStore == null)
			{
				_deletedRelationshipStore = new Dictionary<RelationshipWrapper, RelationshipEntry>();
			}
			dictionary = _deletedRelationshipStore;
			break;
		}
		dictionary.Add(entry.RelationshipWrapper, entry);
	}

	private void AddEntityEntryToDictionary(EntityEntry entry, EntityState state)
	{
		if (entry.RequiresAnyChangeTracking)
		{
			_detectChangesNeeded = true;
		}
		Dictionary<EntityKey, EntityEntry> dictionary = null;
		switch (state)
		{
		case EntityState.Unchanged:
			if (_unchangedEntityStore == null)
			{
				_unchangedEntityStore = new Dictionary<EntityKey, EntityEntry>();
			}
			dictionary = _unchangedEntityStore;
			break;
		case EntityState.Added:
			if (_addedEntityStore == null)
			{
				_addedEntityStore = new Dictionary<EntityKey, EntityEntry>();
			}
			dictionary = _addedEntityStore;
			break;
		case EntityState.Deleted:
			if (_deletedEntityStore == null)
			{
				_deletedEntityStore = new Dictionary<EntityKey, EntityEntry>();
			}
			dictionary = _deletedEntityStore;
			break;
		case EntityState.Modified:
			if (_modifiedEntityStore == null)
			{
				_modifiedEntityStore = new Dictionary<EntityKey, EntityEntry>();
			}
			dictionary = _modifiedEntityStore;
			break;
		}
		dictionary.Add(entry.EntityKey, entry);
		AddEntryToKeylessStore(entry);
	}

	private void AddEntryToKeylessStore(EntityEntry entry)
	{
		if (entry.Entity != null && !(entry.Entity is IEntityWithKey))
		{
			if (_keylessEntityStore == null)
			{
				_keylessEntityStore = new Dictionary<object, EntityEntry>(ObjectReferenceEqualityComparer.Default);
			}
			if (!_keylessEntityStore.ContainsKey(entry.Entity))
			{
				_keylessEntityStore.Add(entry.Entity, entry);
			}
		}
	}

	private void RemoveObjectStateEntryFromDictionary(RelationshipEntry entry, EntityState state)
	{
		Dictionary<RelationshipWrapper, RelationshipEntry> dictionary = null;
		switch (state)
		{
		case EntityState.Unchanged:
			dictionary = _unchangedRelationshipStore;
			break;
		case EntityState.Added:
			dictionary = _addedRelationshipStore;
			break;
		case EntityState.Deleted:
			dictionary = _deletedRelationshipStore;
			break;
		}
		dictionary.Remove(entry.RelationshipWrapper);
		if (dictionary.Count == 0)
		{
			switch (state)
			{
			case EntityState.Unchanged:
				_unchangedRelationshipStore = null;
				break;
			case EntityState.Added:
				_addedRelationshipStore = null;
				break;
			case EntityState.Deleted:
				_deletedRelationshipStore = null;
				break;
			}
		}
	}

	private void RemoveObjectStateEntryFromDictionary(EntityEntry entry, EntityState state)
	{
		Dictionary<EntityKey, EntityEntry> dictionary = null;
		switch (state)
		{
		case EntityState.Unchanged:
			dictionary = _unchangedEntityStore;
			break;
		case EntityState.Added:
			dictionary = _addedEntityStore;
			break;
		case EntityState.Deleted:
			dictionary = _deletedEntityStore;
			break;
		case EntityState.Modified:
			dictionary = _modifiedEntityStore;
			break;
		}
		dictionary.Remove(entry.EntityKey);
		RemoveEntryFromKeylessStore(entry.WrappedEntity);
		if (dictionary.Count == 0)
		{
			switch (state)
			{
			case EntityState.Unchanged:
				_unchangedEntityStore = null;
				break;
			case EntityState.Added:
				_addedEntityStore = null;
				break;
			case EntityState.Deleted:
				_deletedEntityStore = null;
				break;
			case EntityState.Modified:
				_modifiedEntityStore = null;
				break;
			}
		}
	}

	internal virtual void RemoveEntryFromKeylessStore(IEntityWrapper wrappedEntity)
	{
		if (wrappedEntity != null && wrappedEntity.Entity != null && !(wrappedEntity.Entity is IEntityWithKey))
		{
			_keylessEntityStore.Remove(wrappedEntity.Entity);
		}
	}

	internal virtual StateManagerTypeMetadata GetOrAddStateManagerTypeMetadata(Type entityType, EntitySet entitySet)
	{
		if (!_metadataMapping.TryGetValue(new EntitySetQualifiedType(entityType, entitySet), out var value))
		{
			return AddStateManagerTypeMetadata(entitySet, (ObjectTypeMapping)MetadataWorkspace.GetMap(entityType.FullNameWithNesting(), DataSpace.OSpace, DataSpace.OCSpace));
		}
		return value;
	}

	internal virtual StateManagerTypeMetadata GetOrAddStateManagerTypeMetadata(EdmType edmType)
	{
		if (!_metadataStore.TryGetValue(edmType, out var value))
		{
			return AddStateManagerTypeMetadata(edmType, (ObjectTypeMapping)MetadataWorkspace.GetMap(edmType, DataSpace.OCSpace));
		}
		return value;
	}

	private StateManagerTypeMetadata AddStateManagerTypeMetadata(EntitySet entitySet, ObjectTypeMapping mapping)
	{
		EdmType edmType = mapping.EdmType;
		if (!_metadataStore.TryGetValue(edmType, out var value))
		{
			value = new StateManagerTypeMetadata(edmType, mapping);
			_metadataStore.Add(edmType, value);
		}
		EntitySetQualifiedType key = new EntitySetQualifiedType(mapping.ClrType.ClrType, entitySet);
		if (!_metadataMapping.ContainsKey(key))
		{
			_metadataMapping.Add(key, value);
			return value;
		}
		throw new InvalidOperationException(Strings.Mapping_CannotMapCLRTypeMultipleTimes(value.CdmMetadata.EdmType.FullName));
	}

	private StateManagerTypeMetadata AddStateManagerTypeMetadata(EdmType edmType, ObjectTypeMapping mapping)
	{
		StateManagerTypeMetadata stateManagerTypeMetadata = new StateManagerTypeMetadata(edmType, mapping);
		_metadataStore.Add(edmType, stateManagerTypeMetadata);
		return stateManagerTypeMetadata;
	}

	internal virtual void Dispose()
	{
		_isDisposed = true;
	}

	internal virtual void DetectChanges()
	{
		IList<EntityEntry> entityEntriesForDetectChanges = GetEntityEntriesForDetectChanges();
		if (entityEntriesForDetectChanges == null || !TransactionManager.BeginDetectChanges())
		{
			return;
		}
		try
		{
			DetectChangesInNavigationProperties(entityEntriesForDetectChanges);
			DetectChangesInScalarAndComplexProperties(entityEntriesForDetectChanges);
			DetectChangesInForeignKeys(entityEntriesForDetectChanges);
			DetectConflicts(entityEntriesForDetectChanges);
			TransactionManager.BeginAlignChanges();
			AlignChangesInRelationships(entityEntriesForDetectChanges);
		}
		finally
		{
			TransactionManager.EndAlignChanges();
			TransactionManager.EndDetectChanges();
		}
	}

	private void DetectConflicts(IList<EntityEntry> entries)
	{
		TransactionManager transactionManager = TransactionManager;
		foreach (EntityEntry entry in entries)
		{
			transactionManager.AddedRelationshipsByGraph.TryGetValue(entry.WrappedEntity, out var value);
			transactionManager.AddedRelationshipsByForeignKey.TryGetValue(entry.WrappedEntity, out var value2);
			if (value != null && value.Count > 0 && entry.State == EntityState.Deleted)
			{
				throw new InvalidOperationException(Strings.RelatedEnd_UnableToAddRelationshipWithDeletedEntity);
			}
			if (value2 != null)
			{
				foreach (KeyValuePair<RelatedEnd, HashSet<EntityKey>> item in value2)
				{
					if ((entry.State == EntityState.Unchanged || entry.State == EntityState.Modified) && item.Key.IsDependentEndOfReferentialConstraint(checkIdentifying: true) && item.Value.Count > 0)
					{
						throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
					}
					if (item.Key is EntityReference && item.Value.Count > 1)
					{
						throw new InvalidOperationException(Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(item.Key.RelationshipNavigation.To, item.Key.RelationshipNavigation.RelationshipName));
					}
				}
			}
			if (value == null)
			{
				continue;
			}
			Dictionary<string, KeyValuePair<object, IntBox>> properties = new Dictionary<string, KeyValuePair<object, IntBox>>();
			foreach (KeyValuePair<RelatedEnd, HashSet<IEntityWrapper>> item2 in value)
			{
				if (item2.Key.IsForeignKey && (entry.State == EntityState.Unchanged || entry.State == EntityState.Modified) && item2.Key.IsDependentEndOfReferentialConstraint(checkIdentifying: true) && item2.Value.Count > 0)
				{
					throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
				}
				if (!(item2.Key is EntityReference entityReference))
				{
					continue;
				}
				if (item2.Value.Count > 1)
				{
					throw new InvalidOperationException(Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(item2.Key.RelationshipNavigation.To, item2.Key.RelationshipNavigation.RelationshipName));
				}
				if (item2.Value.Count != 1)
				{
					continue;
				}
				IEntityWrapper entityWrapper = item2.Value.First();
				HashSet<EntityKey> value3 = null;
				Dictionary<RelatedEnd, HashSet<EntityKey>> value4;
				if (value2 != null)
				{
					value2.TryGetValue(item2.Key, out value3);
				}
				else if (transactionManager.AddedRelationshipsByPrincipalKey.TryGetValue(entry.WrappedEntity, out value4))
				{
					value4.TryGetValue(item2.Key, out value3);
				}
				Dictionary<RelatedEnd, HashSet<EntityKey>> value5;
				HashSet<EntityKey> value6;
				if (value3 != null && value3.Count > 0)
				{
					if (GetPermanentKey(entry.WrappedEntity, entityReference, entityWrapper) != value3.First())
					{
						throw new InvalidOperationException(Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(entityReference.RelationshipNavigation.To, entityReference.RelationshipNavigation.RelationshipName));
					}
				}
				else if (transactionManager.DeletedRelationshipsByForeignKey.TryGetValue(entry.WrappedEntity, out value5) && value5.TryGetValue(item2.Key, out value6) && value6.Count > 0)
				{
					throw new InvalidOperationException(Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(entityReference.RelationshipNavigation.To, entityReference.RelationshipNavigation.RelationshipName));
				}
				EntityEntry entityEntry = FindEntityEntry(entityWrapper.Entity);
				if (entityEntry == null || (entityEntry.State != EntityState.Unchanged && entityEntry.State != EntityState.Modified))
				{
					continue;
				}
				Dictionary<string, KeyValuePair<object, IntBox>> dictionary = new Dictionary<string, KeyValuePair<object, IntBox>>();
				entityEntry.GetOtherKeyProperties(dictionary);
				foreach (ReferentialConstraint referentialConstraint in ((AssociationType)entityReference.RelationMetadata).ReferentialConstraints)
				{
					if (referentialConstraint.ToRole == entityReference.FromEndMember)
					{
						for (int i = 0; i < referentialConstraint.FromProperties.Count; i++)
						{
							EntityEntry.AddOrIncreaseCounter(referentialConstraint, properties, referentialConstraint.ToProperties[i].Name, dictionary[referentialConstraint.FromProperties[i].Name].Key);
						}
						break;
					}
				}
			}
		}
	}

	internal virtual EntityKey GetPermanentKey(IEntityWrapper entityFrom, RelatedEnd relatedEndFrom, IEntityWrapper entityTo)
	{
		EntityKey entityKey = null;
		if (entityTo.ObjectStateEntry != null)
		{
			entityKey = entityTo.ObjectStateEntry.EntityKey;
		}
		if (entityKey == null || entityKey.IsTemporary)
		{
			entityKey = CreateEntityKey(GetEntitySetOfOtherEnd(entityFrom, relatedEndFrom), entityTo.Entity);
		}
		return entityKey;
	}

	private static EntitySet GetEntitySetOfOtherEnd(IEntityWrapper entity, RelatedEnd relatedEnd)
	{
		AssociationSet associationSet = (AssociationSet)relatedEnd.RelationshipSet;
		EntitySet entitySet = associationSet.AssociationSetEnds[0].EntitySet;
		if (entitySet.Name != entity.EntityKey.EntitySetName)
		{
			return entitySet;
		}
		return associationSet.AssociationSetEnds[1].EntitySet;
	}

	private static void DetectChangesInForeignKeys(IList<EntityEntry> entries)
	{
		foreach (EntityEntry entry in entries)
		{
			if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
			{
				entry.DetectChangesInForeignKeys();
			}
		}
	}

	private void AlignChangesInRelationships(IList<EntityEntry> entries)
	{
		PerformDelete(entries);
		PerformAdd(entries);
	}

	private void PerformAdd(IList<EntityEntry> entries)
	{
		TransactionManager transactionManager = TransactionManager;
		foreach (EntityEntry entry2 in entries)
		{
			if (entry2.State == EntityState.Detached || entry2.IsKeyEntry)
			{
				continue;
			}
			foreach (RelatedEnd relationship in entry2.WrappedEntity.RelationshipManager.Relationships)
			{
				HashSet<EntityKey> value = null;
				if (relationship is EntityReference && transactionManager.AddedRelationshipsByForeignKey.TryGetValue(entry2.WrappedEntity, out var value2))
				{
					value2.TryGetValue(relationship, out value);
				}
				HashSet<IEntityWrapper> value3 = null;
				if (transactionManager.AddedRelationshipsByGraph.TryGetValue(entry2.WrappedEntity, out var value4))
				{
					value4.TryGetValue(relationship, out value3);
				}
				if (value != null)
				{
					foreach (EntityKey item in value)
					{
						if (TryGetEntityEntry(item, out var entry) && entry.WrappedEntity.Entity != null)
						{
							value3 = ((value3 != null) ? value3 : new HashSet<IEntityWrapper>());
							if (entry.State != EntityState.Deleted)
							{
								value3.Remove(entry.WrappedEntity);
								PerformAdd(entry2.WrappedEntity, relationship, entry.WrappedEntity, isForeignKeyChange: true);
							}
						}
						else
						{
							EntityReference reference = relationship as EntityReference;
							entry2.FixupEntityReferenceByForeignKey(reference);
						}
					}
				}
				if (value3 == null)
				{
					continue;
				}
				foreach (IEntityWrapper item2 in value3)
				{
					PerformAdd(entry2.WrappedEntity, relationship, item2, isForeignKeyChange: false);
				}
			}
		}
	}

	private void PerformAdd(IEntityWrapper wrappedOwner, RelatedEnd relatedEnd, IEntityWrapper entityToAdd, bool isForeignKeyChange)
	{
		relatedEnd.ValidateStateForAdd(relatedEnd.WrappedOwner);
		relatedEnd.ValidateStateForAdd(entityToAdd);
		if (relatedEnd.IsPrincipalEndOfReferentialConstraint())
		{
			if (relatedEnd.GetOtherEndOfRelationship(entityToAdd) is EntityReference entityReference && IsReparentingReference(entityToAdd, entityReference))
			{
				TransactionManager.EntityBeingReparented = entityReference.GetDependentEndOfReferentialConstraint(entityReference.ReferenceValue.Entity);
			}
		}
		else if (relatedEnd.IsDependentEndOfReferentialConstraint(checkIdentifying: false) && relatedEnd is EntityReference entityReference2 && IsReparentingReference(wrappedOwner, entityReference2))
		{
			TransactionManager.EntityBeingReparented = entityReference2.GetDependentEndOfReferentialConstraint(entityReference2.ReferenceValue.Entity);
		}
		try
		{
			relatedEnd.Add(entityToAdd, applyConstraints: false, addRelationshipAsUnchanged: false, relationshipAlreadyExists: false, allowModifyingOtherEndOfRelationship: true, !isForeignKeyChange);
		}
		finally
		{
			TransactionManager.EntityBeingReparented = null;
		}
	}

	private void PerformDelete(IList<EntityEntry> entries)
	{
		TransactionManager transactionManager = TransactionManager;
		foreach (EntityEntry entry2 in entries)
		{
			if (entry2.State == EntityState.Detached || entry2.State == EntityState.Deleted || entry2.IsKeyEntry)
			{
				continue;
			}
			foreach (RelatedEnd relationship in entry2.WrappedEntity.RelationshipManager.Relationships)
			{
				HashSet<EntityKey> value = null;
				EntityReference entityReference = relationship as EntityReference;
				if (entityReference != null && transactionManager.DeletedRelationshipsByForeignKey.TryGetValue(entry2.WrappedEntity, out var value2))
				{
					value2.TryGetValue(entityReference, out value);
				}
				HashSet<IEntityWrapper> value3 = null;
				if (transactionManager.DeletedRelationshipsByGraph.TryGetValue(entry2.WrappedEntity, out var value4))
				{
					value4.TryGetValue(relationship, out value3);
				}
				if (value != null)
				{
					foreach (EntityKey item in value)
					{
						IEntityWrapper entityWrapper = null;
						if (TryGetEntityEntry(item, out var entry) && entry.WrappedEntity.Entity != null)
						{
							entityWrapper = entry.WrappedEntity;
						}
						else if (entityReference != null && entityReference.ReferenceValue != NullEntityWrapper.NullWrapper && entityReference.ReferenceValue.EntityKey.IsTemporary && TryGetEntityEntry(entityReference.ReferenceValue.EntityKey, out entry) && entry.WrappedEntity.Entity != null)
						{
							EntityKey entityKey = new EntityKey((EntitySet)entry.EntitySet, entry.CurrentValues);
							if (item == entityKey)
							{
								entityWrapper = entry.WrappedEntity;
							}
						}
						if (entityWrapper != null)
						{
							value3 = ((value3 != null) ? value3 : new HashSet<IEntityWrapper>());
							bool preserveForeignKey = ShouldPreserveForeignKeyForDependent(entry2.WrappedEntity, relationship, entityWrapper, value3);
							value3.Remove(entityWrapper);
							if (entityReference != null && IsReparentingReference(entry2.WrappedEntity, entityReference))
							{
								TransactionManager.EntityBeingReparented = entityReference.GetDependentEndOfReferentialConstraint(entityReference.ReferenceValue.Entity);
							}
							try
							{
								relationship.Remove(entityWrapper, preserveForeignKey);
							}
							finally
							{
								TransactionManager.EntityBeingReparented = null;
							}
							if (entry2.State == EntityState.Detached || entry2.State == EntityState.Deleted || entry2.IsKeyEntry)
							{
								break;
							}
						}
						if (entityReference != null && entityReference.IsForeignKey && entityReference.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
						{
							entityReference.SetCachedForeignKey(ForeignKeyFactory.CreateKeyFromForeignKeyValues(entry2, entityReference), entry2);
						}
					}
				}
				if (value3 != null)
				{
					foreach (IEntityWrapper item2 in value3)
					{
						bool preserveForeignKey2 = ShouldPreserveForeignKeyForPrincipal(entry2.WrappedEntity, relationship, item2, value3);
						if (entityReference != null && IsReparentingReference(entry2.WrappedEntity, entityReference))
						{
							TransactionManager.EntityBeingReparented = entityReference.GetDependentEndOfReferentialConstraint(entityReference.ReferenceValue.Entity);
						}
						try
						{
							relationship.Remove(item2, preserveForeignKey2);
						}
						finally
						{
							TransactionManager.EntityBeingReparented = null;
						}
						if (entry2.State == EntityState.Detached || entry2.State == EntityState.Deleted || entry2.IsKeyEntry)
						{
							break;
						}
					}
				}
				if (entry2.State == EntityState.Detached || entry2.State == EntityState.Deleted || entry2.IsKeyEntry)
				{
					break;
				}
			}
		}
	}

	private bool ShouldPreserveForeignKeyForPrincipal(IEntityWrapper entity, RelatedEnd relatedEnd, IEntityWrapper relatedEntity, HashSet<IEntityWrapper> entitiesToDelete)
	{
		bool result = false;
		if (relatedEnd.IsForeignKey)
		{
			RelatedEnd otherEndOfRelationship = relatedEnd.GetOtherEndOfRelationship(relatedEntity);
			if (otherEndOfRelationship.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
			{
				HashSet<EntityKey> value = null;
				if (TransactionManager.DeletedRelationshipsByForeignKey.TryGetValue(relatedEntity, out var value2) && value2.TryGetValue(otherEndOfRelationship, out value) && value.Count > 0 && TransactionManager.DeletedRelationshipsByGraph.TryGetValue(relatedEntity, out var value3) && value3.TryGetValue(otherEndOfRelationship, out entitiesToDelete))
				{
					result = ShouldPreserveForeignKeyForDependent(relatedEntity, otherEndOfRelationship, entity, entitiesToDelete);
				}
			}
		}
		return result;
	}

	private bool ShouldPreserveForeignKeyForDependent(IEntityWrapper entity, RelatedEnd relatedEnd, IEntityWrapper relatedEntity, HashSet<IEntityWrapper> entitiesToDelete)
	{
		bool flag = entitiesToDelete.Contains(relatedEntity);
		if (flag)
		{
			if (flag)
			{
				return !HasAddedReference(entity, relatedEnd as EntityReference);
			}
			return false;
		}
		return true;
	}

	private bool HasAddedReference(IEntityWrapper wrappedOwner, EntityReference reference)
	{
		HashSet<IEntityWrapper> value = null;
		if (reference != null && TransactionManager.AddedRelationshipsByGraph.TryGetValue(wrappedOwner, out var value2) && value2.TryGetValue(reference, out value) && value.Count > 0)
		{
			return true;
		}
		return false;
	}

	private bool IsReparentingReference(IEntityWrapper wrappedEntity, EntityReference reference)
	{
		TransactionManager transactionManager = TransactionManager;
		if (reference.IsPrincipalEndOfReferentialConstraint())
		{
			wrappedEntity = reference.ReferenceValue;
			reference = ((wrappedEntity.Entity == null) ? null : (reference.GetOtherEndOfRelationship(wrappedEntity) as EntityReference));
		}
		if (wrappedEntity.Entity != null && reference != null)
		{
			HashSet<EntityKey> value = null;
			if (transactionManager.AddedRelationshipsByForeignKey.TryGetValue(wrappedEntity, out var value2) && value2.TryGetValue(reference, out value) && value.Count > 0)
			{
				return true;
			}
			HashSet<IEntityWrapper> value3 = null;
			if (transactionManager.AddedRelationshipsByGraph.TryGetValue(wrappedEntity, out var value4) && value4.TryGetValue(reference, out value3) && value3.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private static void DetectChangesInNavigationProperties(IList<EntityEntry> entries)
	{
		foreach (EntityEntry entry in entries)
		{
			if (entry.WrappedEntity.RequiresRelationshipChangeTracking)
			{
				entry.DetectChangesInRelationshipsOfSingleEntity();
			}
		}
	}

	private static void DetectChangesInScalarAndComplexProperties(IList<EntityEntry> entries)
	{
		foreach (EntityEntry entry in entries)
		{
			if (entry.State != EntityState.Added && (entry.RequiresScalarChangeTracking || entry.RequiresComplexChangeTracking))
			{
				entry.DetectChangesInProperties(!entry.RequiresScalarChangeTracking);
			}
		}
	}

	internal virtual EntityKey CreateEntityKey(EntitySet entitySet, object entity)
	{
		ReadOnlyMetadataCollection<EdmMember> keyMembers = entitySet.ElementType.KeyMembers;
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = GetOrAddStateManagerTypeMetadata(EntityUtil.GetEntityIdentityType(entity.GetType()), entitySet);
		object[] array = new object[keyMembers.Count];
		for (int i = 0; i < keyMembers.Count; i++)
		{
			string name = keyMembers[i].Name;
			int ordinalforCLayerMemberName = orAddStateManagerTypeMetadata.GetOrdinalforCLayerMemberName(name);
			if (ordinalforCLayerMemberName < 0)
			{
				throw new ArgumentException(Strings.ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType(entity.GetType().FullName, entitySet.Name), "entity");
			}
			array[i] = orAddStateManagerTypeMetadata.Member(ordinalforCLayerMemberName).GetValue(entity);
			if (array[i] == null)
			{
				throw new InvalidOperationException(Strings.EntityKey_NullKeyValue(name, entitySet.ElementType.Name));
			}
		}
		if (array.Length == 1)
		{
			return new EntityKey(entitySet, array[0]);
		}
		return new EntityKey(entitySet, array);
	}
}
