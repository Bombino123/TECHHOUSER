using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
[DataContract]
public abstract class EntityReference : RelatedEnd
{
	private EntityKey _detachedEntityKey;

	[NonSerialized]
	private EntityKey _cachedForeignKey;

	[DataMember]
	public EntityKey EntityKey
	{
		get
		{
			if (ObjectContext != null && !base.UsingNoTracking)
			{
				EntityKey entityKey = null;
				if (CachedValue.Entity != null)
				{
					entityKey = CachedValue.EntityKey;
					if (entityKey != null && !RelatedEnd.IsValidEntityKeyType(entityKey))
					{
						entityKey = null;
					}
				}
				else if (base.IsForeignKey)
				{
					if (IsDependentEndOfReferentialConstraint(checkIdentifying: false) && _cachedForeignKey != null)
					{
						if (!ForeignKeyFactory.IsConceptualNullKey(_cachedForeignKey))
						{
							entityKey = _cachedForeignKey;
						}
					}
					else
					{
						entityKey = DetachedEntityKey;
					}
				}
				else
				{
					EntityKey entityKey2 = WrappedOwner.EntityKey;
					foreach (RelationshipEntry item in ObjectContext.ObjectStateManager.FindRelationshipsByKey(entityKey2))
					{
						if (item.State != EntityState.Deleted && item.IsSameAssociationSetAndRole((AssociationSet)RelationshipSet, (AssociationEndMember)FromEndMember, entityKey2))
						{
							entityKey = item.RelationshipWrapper.GetOtherEntityKey(entityKey2);
						}
					}
				}
				return entityKey;
			}
			return DetachedEntityKey;
		}
		set
		{
			SetEntityKey(value, forceFixup: false);
		}
	}

	internal EntityKey AttachedEntityKey => EntityKey;

	internal EntityKey DetachedEntityKey
	{
		get
		{
			return _detachedEntityKey;
		}
		set
		{
			_detachedEntityKey = value;
		}
	}

	internal EntityKey CachedForeignKey => EntityKey ?? _cachedForeignKey;

	internal abstract IEntityWrapper CachedValue { get; }

	internal abstract IEntityWrapper ReferenceValue { get; set; }

	internal override bool CanDeferredLoad => IsEmpty();

	internal EntityReference()
	{
	}

	internal EntityReference(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
		: base(wrappedOwner, navigation, relationshipFixer)
	{
	}

	internal void SetEntityKey(EntityKey value, bool forceFixup)
	{
		if (value != null && value == EntityKey && (ReferenceValue.Entity != null || (ReferenceValue.Entity == null && !forceFixup)))
		{
			return;
		}
		if (ObjectContext != null && !base.UsingNoTracking)
		{
			if (value != null && !RelatedEnd.IsValidEntityKeyType(value))
			{
				throw new ArgumentException(Strings.EntityReference_CannotSetSpecialKeys, "value");
			}
			if (value == null)
			{
				if (AttemptToNullFKsOnRefOrKeySetToNull())
				{
					DetachedEntityKey = null;
				}
				else
				{
					ReferenceValue = NullEntityWrapper.NullWrapper;
				}
				return;
			}
			EntitySet entitySet = value.GetEntitySet(ObjectContext.MetadataWorkspace);
			CheckRelationEntitySet(entitySet);
			value.ValidateEntityKey(ObjectContext.MetadataWorkspace, entitySet, isArgumentException: true, "value");
			ObjectStateManager objectStateManager = ObjectContext.ObjectStateManager;
			bool flag = false;
			bool flag2 = false;
			EntityEntry entityEntry = objectStateManager.FindEntityEntry(value);
			if (entityEntry != null)
			{
				if (!entityEntry.IsKeyEntry)
				{
					ReferenceValue = entityEntry.WrappedEntity;
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag2 = !base.IsForeignKey;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			EntityKey entityKey = ValidateOwnerWithRIConstraints(entityEntry?.WrappedEntity, value, checkBothEnds: true);
			ValidateStateForAdd(WrappedOwner);
			if (flag2)
			{
				objectStateManager.AddKeyEntry(value, entitySet);
			}
			objectStateManager.TransactionManager.EntityBeingReparented = WrappedOwner.Entity;
			try
			{
				ClearCollectionOrRef(null, null, doCascadeDelete: false);
			}
			finally
			{
				objectStateManager.TransactionManager.EntityBeingReparented = null;
			}
			if (base.IsForeignKey)
			{
				DetachedEntityKey = value;
				if (IsDependentEndOfReferentialConstraint(checkIdentifying: false))
				{
					UpdateForeignKeyValues(WrappedOwner, value);
				}
				return;
			}
			RelationshipWrapper wrapper = new RelationshipWrapper((AssociationSet)RelationshipSet, base.RelationshipNavigation.From, entityKey, base.RelationshipNavigation.To, value);
			EntityState desiredState = EntityState.Added;
			if (!entityKey.IsTemporary && IsDependentEndOfReferentialConstraint(checkIdentifying: false))
			{
				desiredState = EntityState.Unchanged;
			}
			objectStateManager.AddNewRelation(wrapper, desiredState);
		}
		else
		{
			DetachedEntityKey = value;
		}
	}

	internal bool AttemptToNullFKsOnRefOrKeySetToNull()
	{
		if (ReferenceValue.Entity == null && WrappedOwner.Entity != null && WrappedOwner.Context != null && !base.UsingNoTracking && base.IsForeignKey)
		{
			if (WrappedOwner.ObjectStateEntry.State != EntityState.Added && IsDependentEndOfReferentialConstraint(checkIdentifying: true))
			{
				throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
			}
			RemoveFromLocalCache(NullEntityWrapper.NullWrapper, resetIsLoaded: true, preserveForeignKey: false);
			return true;
		}
		return false;
	}

	internal void SetCachedForeignKey(EntityKey newForeignKey, EntityEntry source)
	{
		if (ObjectContext != null && ObjectContext.ObjectStateManager != null && source != null && _cachedForeignKey != null && !ForeignKeyFactory.IsConceptualNullKey(_cachedForeignKey) && _cachedForeignKey != newForeignKey)
		{
			ObjectContext.ObjectStateManager.RemoveEntryFromForeignKeyIndex(this, _cachedForeignKey, source);
		}
		_cachedForeignKey = newForeignKey;
	}

	internal IEnumerable<EntityKey> GetAllKeyValues()
	{
		if (EntityKey != null)
		{
			yield return EntityKey;
		}
		if (_cachedForeignKey != null)
		{
			yield return _cachedForeignKey;
		}
		if (_detachedEntityKey != null)
		{
			yield return _detachedEntityKey;
		}
	}

	internal EntityKey ValidateOwnerWithRIConstraints(IEntityWrapper targetEntity, EntityKey targetEntityKey, bool checkBothEnds)
	{
		EntityKey entityKey = WrappedOwner.EntityKey;
		if ((object)entityKey != null && !entityKey.IsTemporary && IsDependentEndOfReferentialConstraint(checkIdentifying: true))
		{
			ValidateSettingRIConstraints(targetEntity, targetEntityKey == null, CachedForeignKey != null && CachedForeignKey != targetEntityKey);
		}
		else if (checkBothEnds && targetEntity != null && targetEntity.Entity != null && GetOtherEndOfRelationship(targetEntity) is EntityReference entityReference)
		{
			entityReference.ValidateOwnerWithRIConstraints(WrappedOwner, entityKey, checkBothEnds: false);
		}
		return entityKey;
	}

	internal void ValidateSettingRIConstraints(IEntityWrapper targetEntity, bool settingToNull, bool changingForeignKeyValue)
	{
		bool flag = targetEntity != null && targetEntity.MergeOption == MergeOption.NoTracking;
		if (settingToNull || changingForeignKeyValue || (targetEntity != null && !flag && (targetEntity.ObjectStateEntry == null || (EntityKey == null && targetEntity.ObjectStateEntry.State == EntityState.Deleted) || (CachedForeignKey == null && targetEntity.ObjectStateEntry.State == EntityState.Added))))
		{
			throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
		}
	}

	internal void UpdateForeignKeyValues(IEntityWrapper dependentEntity, IEntityWrapper principalEntity, Dictionary<int, object> changedFKs, bool forceChange)
	{
		ReferentialConstraint referentialConstraint = ((AssociationType)RelationMetadata).ReferentialConstraints[0];
		bool flag = (object)WrappedOwner.EntityKey != null && !WrappedOwner.EntityKey.IsTemporary && IsDependentEndOfReferentialConstraint(checkIdentifying: true);
		ObjectStateManager objectStateManager = ObjectContext.ObjectStateManager;
		objectStateManager.TransactionManager.BeginForeignKeyUpdate(this);
		try
		{
			EntitySet entitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[ToEndMember.Name].EntitySet;
			StateManagerTypeMetadata orAddStateManagerTypeMetadata = objectStateManager.GetOrAddStateManagerTypeMetadata(principalEntity.IdentityType, entitySet);
			EntitySet entitySet2 = ((AssociationSet)RelationshipSet).AssociationSetEnds[FromEndMember.Name].EntitySet;
			StateManagerTypeMetadata orAddStateManagerTypeMetadata2 = objectStateManager.GetOrAddStateManagerTypeMetadata(dependentEntity.IdentityType, entitySet2);
			ReadOnlyMetadataCollection<EdmProperty> fromProperties = referentialConstraint.FromProperties;
			int count = fromProperties.Count;
			string[] array = null;
			object[] array2 = null;
			if (count > 1)
			{
				array = entitySet.ElementType.KeyMemberNames;
				array2 = new object[count];
			}
			for (int i = 0; i < count; i++)
			{
				int ordinalforOLayerMemberName = orAddStateManagerTypeMetadata.GetOrdinalforOLayerMemberName(fromProperties[i].Name);
				object value = orAddStateManagerTypeMetadata.Member(ordinalforOLayerMemberName).GetValue(principalEntity.Entity);
				int ordinalforOLayerMemberName2 = orAddStateManagerTypeMetadata2.GetOrdinalforOLayerMemberName(referentialConstraint.ToProperties[i].Name);
				bool flag2 = !ByValueEqualityComparer.Default.Equals(orAddStateManagerTypeMetadata2.Member(ordinalforOLayerMemberName2).GetValue(dependentEntity.Entity), value);
				if (forceChange || flag2)
				{
					if (flag)
					{
						ValidateSettingRIConstraints(principalEntity, value == null, flag2);
					}
					if (changedFKs != null)
					{
						if (changedFKs.TryGetValue(ordinalforOLayerMemberName2, out var value2))
						{
							if (!ByValueEqualityComparer.Default.Equals(value2, value))
							{
								throw new InvalidOperationException(Strings.Update_ReferentialConstraintIntegrityViolation);
							}
						}
						else
						{
							changedFKs[ordinalforOLayerMemberName2] = value;
						}
					}
					if (flag2)
					{
						dependentEntity.SetCurrentValue(dependentEntity.ObjectStateEntry, orAddStateManagerTypeMetadata2.Member(ordinalforOLayerMemberName2), -1, dependentEntity.Entity, value);
					}
				}
				if (count > 1)
				{
					int num = Array.IndexOf(array, fromProperties[i].Name);
					array2[num] = value;
				}
				else
				{
					SetCachedForeignKey((value == null) ? null : new EntityKey(entitySet, value), dependentEntity.ObjectStateEntry);
				}
			}
			if (count > 1)
			{
				SetCachedForeignKey(array2.Any((object v) => v == null) ? null : new EntityKey(entitySet, array2), dependentEntity.ObjectStateEntry);
			}
			if (WrappedOwner.ObjectStateEntry != null)
			{
				objectStateManager.ForgetEntryWithConceptualNull(WrappedOwner.ObjectStateEntry, resetAllKeys: false);
			}
		}
		finally
		{
			objectStateManager.TransactionManager.EndForeignKeyUpdate();
		}
	}

	internal void UpdateForeignKeyValues(IEntityWrapper dependentEntity, EntityKey principalKey)
	{
		ReferentialConstraint referentialConstraint = ((AssociationType)RelationMetadata).ReferentialConstraints[0];
		ObjectStateManager objectStateManager = ObjectContext.ObjectStateManager;
		objectStateManager.TransactionManager.BeginForeignKeyUpdate(this);
		try
		{
			EntitySet entitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[FromEndMember.Name].EntitySet;
			StateManagerTypeMetadata orAddStateManagerTypeMetadata = objectStateManager.GetOrAddStateManagerTypeMetadata(dependentEntity.IdentityType, entitySet);
			for (int i = 0; i < referentialConstraint.FromProperties.Count; i++)
			{
				object obj = principalKey.FindValueByName(referentialConstraint.FromProperties[i].Name);
				int ordinalforOLayerMemberName = orAddStateManagerTypeMetadata.GetOrdinalforOLayerMemberName(referentialConstraint.ToProperties[i].Name);
				object value = orAddStateManagerTypeMetadata.Member(ordinalforOLayerMemberName).GetValue(dependentEntity.Entity);
				if (!ByValueEqualityComparer.Default.Equals(value, obj))
				{
					dependentEntity.SetCurrentValue(dependentEntity.ObjectStateEntry, orAddStateManagerTypeMetadata.Member(ordinalforOLayerMemberName), -1, dependentEntity.Entity, obj);
				}
			}
			SetCachedForeignKey(principalKey, dependentEntity.ObjectStateEntry);
			if (WrappedOwner.ObjectStateEntry != null)
			{
				objectStateManager.ForgetEntryWithConceptualNull(WrappedOwner.ObjectStateEntry, resetAllKeys: false);
			}
		}
		finally
		{
			objectStateManager.TransactionManager.EndForeignKeyUpdate();
		}
	}

	internal object GetDependentEndOfReferentialConstraint(object relatedValue)
	{
		if (!IsDependentEndOfReferentialConstraint(checkIdentifying: false))
		{
			return relatedValue;
		}
		return WrappedOwner.Entity;
	}

	internal bool NavigationPropertyIsNullOrMissing()
	{
		if (base.TargetAccessor.HasProperty)
		{
			return WrappedOwner.GetNavigationPropertyValue(this) == null;
		}
		return true;
	}

	internal override void AddEntityToObjectStateManager(IEntityWrapper wrappedEntity, bool doAttach)
	{
		base.AddEntityToObjectStateManager(wrappedEntity, doAttach);
		if (DetachedEntityKey != null)
		{
			EntityKey entityKey = wrappedEntity.EntityKey;
			if (DetachedEntityKey != entityKey)
			{
				throw new InvalidOperationException(Strings.EntityReference_EntityKeyValueMismatch);
			}
		}
	}

	internal override void AddToNavigationPropertyIfCompatible(RelatedEnd otherRelatedEnd)
	{
		if (NavigationPropertyIsNullOrMissing())
		{
			AddToNavigationProperty(otherRelatedEnd.WrappedOwner);
			if (otherRelatedEnd.ObjectContext.ObjectStateManager.FindEntityEntry(otherRelatedEnd.WrappedOwner.Entity) != null && otherRelatedEnd.ObjectContext.ObjectStateManager.TransactionManager.IsAddTracking && otherRelatedEnd.IsForeignKey && IsDependentEndOfReferentialConstraint(checkIdentifying: false))
			{
				MarkForeignKeyPropertiesModified();
			}
		}
		else if (!CheckIfNavigationPropertyContainsEntity(otherRelatedEnd.WrappedOwner))
		{
			throw Error.ObjectStateManager_ConflictingChangesOfRelationshipDetected(base.RelationshipNavigation.To, base.RelationshipNavigation.RelationshipName);
		}
	}

	internal override bool CachedForeignKeyIsConceptualNull()
	{
		return ForeignKeyFactory.IsConceptualNullKey(CachedForeignKey);
	}

	internal override bool UpdateDependentEndForeignKey(RelatedEnd targetRelatedEnd, bool forceForeignKeyChanges)
	{
		if (IsDependentEndOfReferentialConstraint(checkIdentifying: false))
		{
			UpdateForeignKeyValues(WrappedOwner, targetRelatedEnd.WrappedOwner, null, forceForeignKeyChanges);
			return true;
		}
		return false;
	}

	internal override void ValidateDetachedEntityKey()
	{
		if (IsEmpty() && DetachedEntityKey != null)
		{
			EntityKey detachedEntityKey = DetachedEntityKey;
			if (!RelatedEnd.IsValidEntityKeyType(detachedEntityKey))
			{
				throw Error.EntityReference_CannotSetSpecialKeys();
			}
			EntitySet entitySet = detachedEntityKey.GetEntitySet(ObjectContext.MetadataWorkspace);
			CheckRelationEntitySet(entitySet);
			detachedEntityKey.ValidateEntityKey(ObjectContext.MetadataWorkspace, entitySet);
		}
	}

	internal override void VerifyDetachedKeyMatches(EntityKey entityKey)
	{
		if (!(DetachedEntityKey != null))
		{
			return;
		}
		if (DetachedEntityKey != entityKey)
		{
			if (entityKey.IsTemporary)
			{
				throw Error.RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities(base.RelationshipNavigation.To);
			}
			throw new InvalidOperationException(Strings.EntityReference_EntityKeyValueMismatch);
		}
	}

	internal override void DetachAll(EntityState ownerEntityState)
	{
		DetachedEntityKey = AttachedEntityKey;
		base.DetachAll(ownerEntityState);
		if (base.IsForeignKey)
		{
			DetachedEntityKey = null;
		}
	}

	internal override bool CheckReferentialConstraintPrincipalProperty(EntityEntry ownerEntry, ReferentialConstraint constraint)
	{
		EntityKey principalKey;
		if (!IsEmpty())
		{
			IEntityWrapper referenceValue = ReferenceValue;
			if (referenceValue.ObjectStateEntry != null && referenceValue.ObjectStateEntry.State == EntityState.Added)
			{
				return true;
			}
			principalKey = ExtractPrincipalKey(referenceValue);
		}
		else
		{
			if ((ToEndMember.RelationshipMultiplicity != 0 && ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.One) || !(DetachedEntityKey != null))
			{
				return true;
			}
			principalKey = ((!base.IsForeignKey || ObjectContext.ObjectStateManager.TransactionManager.IsAddTracking || ObjectContext.ObjectStateManager.TransactionManager.IsAttachTracking) ? DetachedEntityKey : EntityKey);
		}
		return RelatedEnd.VerifyRIConstraintsWithRelatedEntry(constraint, ownerEntry.GetCurrentEntityValue, principalKey);
	}

	internal override bool CheckReferentialConstraintDependentProperty(EntityEntry ownerEntry, ReferentialConstraint constraint)
	{
		if (!IsEmpty())
		{
			return base.CheckReferentialConstraintDependentProperty(ownerEntry, constraint);
		}
		if ((ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne || ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One) && DetachedEntityKey != null)
		{
			EntityKey detachedEntityKey = DetachedEntityKey;
			if (!RelatedEnd.VerifyRIConstraintsWithRelatedEntry(constraint, detachedEntityKey.FindValueByName, ownerEntry.EntityKey))
			{
				return false;
			}
		}
		return true;
	}

	private EntityKey ExtractPrincipalKey(IEntityWrapper wrappedRelatedEntity)
	{
		EntitySet targetEntitySetFromRelationshipSet = GetTargetEntitySetFromRelationshipSet();
		EntityKey entityKey = wrappedRelatedEntity.EntityKey;
		if ((object)entityKey != null && !entityKey.IsTemporary)
		{
			EntityUtil.ValidateEntitySetInKey(entityKey, targetEntitySetFromRelationshipSet);
			entityKey.ValidateEntityKey(ObjectContext.MetadataWorkspace, targetEntitySetFromRelationshipSet);
		}
		else
		{
			entityKey = ObjectContext.ObjectStateManager.CreateEntityKey(targetEntitySetFromRelationshipSet, wrappedRelatedEntity.Entity);
		}
		return entityKey;
	}

	internal void NullAllForeignKeys()
	{
		ObjectStateManager objectStateManager = ObjectContext.ObjectStateManager;
		EntityEntry objectStateEntry = WrappedOwner.ObjectStateEntry;
		TransactionManager transactionManager = objectStateManager.TransactionManager;
		if (transactionManager.IsGraphUpdate || transactionManager.IsAttachTracking || transactionManager.IsRelatedEndAdd)
		{
			return;
		}
		ReferentialConstraint referentialConstraint = ((AssociationType)RelationMetadata).ReferentialConstraints.Single();
		if (!(TargetRoleName == referentialConstraint.FromRole.Name))
		{
			return;
		}
		if (transactionManager.IsDetaching)
		{
			EntityKey entityKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(objectStateEntry, this);
			if (entityKey != null)
			{
				objectStateManager.AddEntryContainingForeignKeyToIndex(this, entityKey, objectStateEntry);
			}
		}
		else
		{
			if (objectStateManager.EntityInvokingFKSetter == WrappedOwner.Entity || transactionManager.IsForeignKeyUpdate)
			{
				return;
			}
			transactionManager.BeginForeignKeyUpdate(this);
			try
			{
				bool flag = true;
				bool flag2 = objectStateEntry != null && (objectStateEntry.State == EntityState.Modified || objectStateEntry.State == EntityState.Unchanged);
				EntitySet entitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[FromEndMember.Name].EntitySet;
				StateManagerTypeMetadata orAddStateManagerTypeMetadata = objectStateManager.GetOrAddStateManagerTypeMetadata(WrappedOwner.IdentityType, entitySet);
				for (int i = 0; i < referentialConstraint.FromProperties.Count; i++)
				{
					string name = referentialConstraint.ToProperties[i].Name;
					int ordinalforOLayerMemberName = orAddStateManagerTypeMetadata.GetOrdinalforOLayerMemberName(name);
					StateManagerMemberMetadata stateManagerMemberMetadata = orAddStateManagerTypeMetadata.Member(ordinalforOLayerMemberName);
					if (stateManagerMemberMetadata.ClrMetadata.Nullable)
					{
						if (stateManagerMemberMetadata.GetValue(WrappedOwner.Entity) != null)
						{
							WrappedOwner.SetCurrentValue(WrappedOwner.ObjectStateEntry, orAddStateManagerTypeMetadata.Member(ordinalforOLayerMemberName), -1, WrappedOwner.Entity, null);
						}
						else if (flag2 && WrappedOwner.ObjectStateEntry.OriginalValues.GetValue(ordinalforOLayerMemberName) != null)
						{
							objectStateEntry.SetModifiedProperty(name);
						}
						flag = false;
					}
					else if (flag2)
					{
						objectStateEntry.SetModifiedProperty(name);
					}
				}
				if (flag)
				{
					if (objectStateEntry != null)
					{
						EntityKey entityKey2 = CachedForeignKey;
						if (entityKey2 == null)
						{
							entityKey2 = ForeignKeyFactory.CreateKeyFromForeignKeyValues(objectStateEntry, this);
						}
						if (entityKey2 != null)
						{
							SetCachedForeignKey(ForeignKeyFactory.CreateConceptualNullKey(entityKey2), objectStateEntry);
							objectStateManager.RememberEntryWithConceptualNull(objectStateEntry);
						}
					}
				}
				else
				{
					SetCachedForeignKey(null, objectStateEntry);
				}
			}
			finally
			{
				transactionManager.EndForeignKeyUpdate();
			}
		}
	}
}
[Serializable]
[DataContract]
public class EntityReference<TEntity> : EntityReference where TEntity : class
{
	private TEntity _cachedValue;

	[NonSerialized]
	private IEntityWrapper _wrappedCachedValue;

	[SoapIgnore]
	[XmlIgnore]
	public TEntity Value
	{
		get
		{
			DeferredLoad();
			return (TEntity)ReferenceValue.Entity;
		}
		set
		{
			ReferenceValue = EntityWrapperFactory.WrapEntityUsingContext(value, ObjectContext);
		}
	}

	internal override IEntityWrapper CachedValue => _wrappedCachedValue;

	internal override IEntityWrapper ReferenceValue
	{
		get
		{
			CheckOwnerNull();
			return _wrappedCachedValue;
		}
		set
		{
			CheckOwnerNull();
			if (value.Entity != null && value.Entity == _wrappedCachedValue.Entity)
			{
				return;
			}
			if (value.Entity != null)
			{
				ValidateOwnerWithRIConstraints(value, (value == NullEntityWrapper.NullWrapper) ? null : value.EntityKey, checkBothEnds: true);
				ObjectContext objectContext = ObjectContext ?? value.Context;
				if (objectContext != null)
				{
					objectContext.ObjectStateManager.TransactionManager.EntityBeingReparented = GetDependentEndOfReferentialConstraint(value.Entity);
				}
				try
				{
					Add(value, applyConstraints: false);
					return;
				}
				finally
				{
					if (objectContext != null)
					{
						objectContext.ObjectStateManager.TransactionManager.EntityBeingReparented = null;
					}
				}
			}
			if (base.UsingNoTracking)
			{
				if (_wrappedCachedValue.Entity != null)
				{
					GetOtherEndOfRelationship(_wrappedCachedValue).OnRelatedEndClear();
				}
				_isLoaded = false;
			}
			else if (ObjectContext != null && ObjectContext.ContextOptions.UseConsistentNullReferenceBehavior)
			{
				AttemptToNullFKsOnRefOrKeySetToNull();
			}
			ClearCollectionOrRef(null, null, doCascadeDelete: false);
		}
	}

	public EntityReference()
	{
		_wrappedCachedValue = NullEntityWrapper.NullWrapper;
	}

	internal EntityReference(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
		: base(wrappedOwner, navigation, relationshipFixer)
	{
		_wrappedCachedValue = NullEntityWrapper.NullWrapper;
	}

	public override void Load(MergeOption mergeOption)
	{
		CheckOwnerNull();
		bool hasResults;
		ObjectQuery<TEntity> objectQuery = ValidateLoad<TEntity>(mergeOption, "EntityReference", out hasResults);
		_suppressEvents = true;
		try
		{
			IList<TEntity> refreshedValue = null;
			if (hasResults)
			{
				refreshedValue = objectQuery.Execute(objectQuery.MergeOption).ToList();
			}
			HandleRefreshedValue(mergeOption, refreshedValue);
		}
		finally
		{
			_suppressEvents = false;
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	public override async Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken)
	{
		CheckOwnerNull();
		cancellationToken.ThrowIfCancellationRequested();
		bool hasResults;
		ObjectQuery<TEntity> objectQuery = ValidateLoad<TEntity>(mergeOption, "EntityReference", out hasResults);
		_suppressEvents = true;
		try
		{
			IList<TEntity> refreshedValue = null;
			if (hasResults)
			{
				refreshedValue = await (await objectQuery.ExecuteAsync(objectQuery.MergeOption, cancellationToken).WithCurrentCulture()).ToListAsync(cancellationToken).WithCurrentCulture();
			}
			HandleRefreshedValue(mergeOption, refreshedValue);
		}
		finally
		{
			_suppressEvents = false;
		}
		OnAssociationChanged(CollectionChangeAction.Refresh, null);
	}

	private void HandleRefreshedValue(MergeOption mergeOption, IList<TEntity> refreshedValue)
	{
		if (refreshedValue == null || !refreshedValue.Any())
		{
			if (!((AssociationType)RelationMetadata).IsForeignKey && ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
			{
				throw Error.EntityReference_LessThanExpectedRelatedEntitiesFound();
			}
			if (mergeOption == MergeOption.OverwriteChanges || mergeOption == MergeOption.PreserveChanges)
			{
				EntityKey entityKey = WrappedOwner.EntityKey;
				if ((object)entityKey == null)
				{
					throw Error.EntityKey_UnexpectedNull();
				}
				ObjectContext.ObjectStateManager.RemoveRelationships(mergeOption, (AssociationSet)RelationshipSet, entityKey, (AssociationEndMember)FromEndMember);
			}
			_isLoaded = true;
		}
		else
		{
			if (refreshedValue.Count() != 1)
			{
				throw Error.EntityReference_MoreThanExpectedRelatedEntitiesFound();
			}
			Merge(refreshedValue, mergeOption, setIsLoaded: true);
		}
	}

	internal override IEnumerable GetInternalEnumerable()
	{
		CheckOwnerNull();
		if (ReferenceValue.Entity != null)
		{
			return new object[1] { ReferenceValue.Entity };
		}
		return Enumerable.Empty<object>();
	}

	internal override IEnumerable<IEntityWrapper> GetWrappedEntities()
	{
		if (_wrappedCachedValue.Entity != null)
		{
			return new IEntityWrapper[1] { _wrappedCachedValue };
		}
		return new IEntityWrapper[0];
	}

	public void Attach(TEntity entity)
	{
		Check.NotNull(entity, "entity");
		CheckOwnerNull();
		Attach(new IEntityWrapper[1] { EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext) }, allowCollection: false);
	}

	internal override void Include(bool addRelationshipAsUnchanged, bool doAttach)
	{
		if (_wrappedCachedValue.Entity != null)
		{
			IEntityWrapper entityWrapper = EntityWrapperFactory.WrapEntityUsingContext(_wrappedCachedValue.Entity, WrappedOwner.Context);
			if (entityWrapper != _wrappedCachedValue)
			{
				_wrappedCachedValue = entityWrapper;
			}
			IncludeEntity(_wrappedCachedValue, addRelationshipAsUnchanged, doAttach);
		}
		else if (base.DetachedEntityKey != null)
		{
			IncludeEntityKey(doAttach);
		}
	}

	private void IncludeEntityKey(bool doAttach)
	{
		ObjectStateManager objectStateManager = ObjectContext.ObjectStateManager;
		bool flag = false;
		bool flag2 = false;
		EntityEntry entityEntry = objectStateManager.FindEntityEntry(base.DetachedEntityKey);
		if (entityEntry == null)
		{
			flag2 = true;
			flag = true;
		}
		else if (entityEntry.IsKeyEntry)
		{
			if (FromEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
			{
				foreach (RelationshipEntry item in ObjectContext.ObjectStateManager.FindRelationshipsByKey(base.DetachedEntityKey))
				{
					if (item.IsSameAssociationSetAndRole((AssociationSet)RelationshipSet, (AssociationEndMember)ToEndMember, base.DetachedEntityKey) && item.State != EntityState.Deleted)
					{
						throw new InvalidOperationException(Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
					}
				}
			}
			flag = true;
		}
		else
		{
			IEntityWrapper wrappedEntity = entityEntry.WrappedEntity;
			if (entityEntry.State == EntityState.Deleted)
			{
				throw new InvalidOperationException(Strings.RelatedEnd_UnableToAddRelationshipWithDeletedEntity);
			}
			RelatedEnd relatedEndInternal = wrappedEntity.RelationshipManager.GetRelatedEndInternal(base.RelationshipName, base.RelationshipNavigation.From);
			if (FromEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many && !relatedEndInternal.IsEmpty())
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
			}
			Add(wrappedEntity, applyConstraints: true, doAttach, relationshipAlreadyExists: false, allowModifyingOtherEndOfRelationship: true, forceForeignKeyChanges: true);
			objectStateManager.TransactionManager.PopulatedEntityReferences.Add(this);
		}
		if (flag && !base.IsForeignKey)
		{
			if (flag2)
			{
				EntitySet entitySet = base.DetachedEntityKey.GetEntitySet(ObjectContext.MetadataWorkspace);
				objectStateManager.AddKeyEntry(base.DetachedEntityKey, entitySet);
			}
			EntityKey entityKey = WrappedOwner.EntityKey;
			if ((object)entityKey == null)
			{
				throw Error.EntityKey_UnexpectedNull();
			}
			RelationshipWrapper wrapper = new RelationshipWrapper((AssociationSet)RelationshipSet, base.RelationshipNavigation.From, entityKey, base.RelationshipNavigation.To, base.DetachedEntityKey);
			objectStateManager.AddNewRelation(wrapper, doAttach ? EntityState.Unchanged : EntityState.Added);
		}
	}

	internal override void Exclude()
	{
		if (_wrappedCachedValue.Entity != null)
		{
			TransactionManager transactionManager = ObjectContext.ObjectStateManager.TransactionManager;
			bool flag = transactionManager.PopulatedEntityReferences.Contains(this);
			bool flag2 = transactionManager.AlignedEntityReferences.Contains(this);
			if ((transactionManager.ProcessedEntities == null || !transactionManager.ProcessedEntities.Contains(_wrappedCachedValue)) && (flag || flag2))
			{
				RelationshipEntry relationshipEntry = (base.IsForeignKey ? null : FindRelationshipEntryInObjectStateManager(_wrappedCachedValue));
				Remove(_wrappedCachedValue, flag, deleteEntity: false, deleteOwner: false, applyReferentialConstraints: false, preserveForeignKey: true);
				if (relationshipEntry != null && relationshipEntry.State != EntityState.Detached)
				{
					relationshipEntry.AcceptChanges();
				}
				if (flag)
				{
					transactionManager.PopulatedEntityReferences.Remove(this);
				}
				else
				{
					transactionManager.AlignedEntityReferences.Remove(this);
				}
			}
			else
			{
				ExcludeEntity(_wrappedCachedValue);
			}
		}
		else if (base.DetachedEntityKey != null)
		{
			ExcludeEntityKey();
		}
	}

	private void ExcludeEntityKey()
	{
		EntityKey entityKey = WrappedOwner.EntityKey;
		RelationshipEntry relationshipEntry = ObjectContext.ObjectStateManager.FindRelationship(RelationshipSet, new KeyValuePair<string, EntityKey>(base.RelationshipNavigation.From, entityKey), new KeyValuePair<string, EntityKey>(base.RelationshipNavigation.To, base.DetachedEntityKey));
		if (relationshipEntry != null)
		{
			relationshipEntry.Delete(doFixup: false);
			if (relationshipEntry.State != EntityState.Detached)
			{
				relationshipEntry.AcceptChanges();
			}
		}
	}

	internal override void ClearCollectionOrRef(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete)
	{
		if (wrappedEntity == null)
		{
			wrappedEntity = NullEntityWrapper.NullWrapper;
		}
		if (_wrappedCachedValue.Entity != null)
		{
			if (wrappedEntity.Entity == _wrappedCachedValue.Entity && navigation.Equals(base.RelationshipNavigation))
			{
				Remove(_wrappedCachedValue, doFixup: false, deleteEntity: false, deleteOwner: false, applyReferentialConstraints: false, preserveForeignKey: false);
			}
			else
			{
				Remove(_wrappedCachedValue, doFixup: true, doCascadeDelete, deleteOwner: false, applyReferentialConstraints: true, preserveForeignKey: false);
			}
		}
		else if (WrappedOwner.Entity != null && WrappedOwner.Context != null && !base.UsingNoTracking)
		{
			WrappedOwner.Context.ObjectStateManager.GetEntityEntry(WrappedOwner.Entity).DeleteRelationshipsThatReferenceKeys(RelationshipSet, ToEndMember);
		}
		if (WrappedOwner.Entity != null)
		{
			base.DetachedEntityKey = null;
		}
	}

	internal override void ClearWrappedValues()
	{
		_cachedValue = null;
		_wrappedCachedValue = NullEntityWrapper.NullWrapper;
	}

	internal override bool CanSetEntityType(IEntityWrapper wrappedEntity)
	{
		return wrappedEntity.Entity is TEntity;
	}

	internal override void VerifyType(IEntityWrapper wrappedEntity)
	{
		if (!CanSetEntityType(wrappedEntity))
		{
			throw new InvalidOperationException(Strings.RelatedEnd_InvalidContainedType_Reference(wrappedEntity.Entity.GetType().FullName, typeof(TEntity).FullName));
		}
	}

	internal override void DisconnectedAdd(IEntityWrapper wrappedEntity)
	{
		CheckOwnerNull();
	}

	internal override bool DisconnectedRemove(IEntityWrapper wrappedEntity)
	{
		CheckOwnerNull();
		return false;
	}

	internal override bool RemoveFromLocalCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey)
	{
		_wrappedCachedValue = NullEntityWrapper.NullWrapper;
		_cachedValue = null;
		if (resetIsLoaded)
		{
			_isLoaded = false;
		}
		if (ObjectContext != null && base.IsForeignKey && !preserveForeignKey)
		{
			NullAllForeignKeys();
		}
		return true;
	}

	internal override bool RemoveFromObjectCache(IEntityWrapper wrappedEntity)
	{
		if (base.TargetAccessor.HasProperty)
		{
			WrappedOwner.RemoveNavigationPropertyValue(this, wrappedEntity.Entity);
		}
		return true;
	}

	internal override void RetrieveReferentialConstraintProperties(Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited)
	{
		if (_wrappedCachedValue.Entity == null)
		{
			return;
		}
		foreach (ReferentialConstraint referentialConstraint in ((AssociationType)RelationMetadata).ReferentialConstraints)
		{
			if (referentialConstraint.ToRole == FromEndMember)
			{
				if (visited.Contains(_wrappedCachedValue))
				{
					throw new InvalidOperationException(Strings.RelationshipManager_CircularRelationshipsWithReferentialConstraints);
				}
				visited.Add(_wrappedCachedValue);
				_wrappedCachedValue.RelationshipManager.RetrieveReferentialConstraintProperties(out var properties2, visited, includeOwnValues: true);
				for (int i = 0; i < referentialConstraint.FromProperties.Count; i++)
				{
					EntityEntry.AddOrIncreaseCounter(referentialConstraint, properties, referentialConstraint.ToProperties[i].Name, properties2[referentialConstraint.FromProperties[i].Name].Key);
				}
			}
		}
	}

	internal override bool IsEmpty()
	{
		return _wrappedCachedValue.Entity == null;
	}

	internal override void VerifyMultiplicityConstraintsForAdd(bool applyConstraints)
	{
		if (applyConstraints && !IsEmpty())
		{
			throw new InvalidOperationException(Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(base.RelationshipNavigation.To, base.RelationshipNavigation.RelationshipName));
		}
	}

	internal override void OnRelatedEndClear()
	{
		_isLoaded = false;
	}

	internal override bool ContainsEntity(IEntityWrapper wrappedEntity)
	{
		if (_wrappedCachedValue.Entity != null)
		{
			return _wrappedCachedValue.Entity == wrappedEntity.Entity;
		}
		return false;
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

	internal void InitializeWithValue(RelatedEnd relatedEnd)
	{
		if (relatedEnd is EntityReference<TEntity> entityReference && entityReference._wrappedCachedValue.Entity != null)
		{
			_wrappedCachedValue = entityReference._wrappedCachedValue;
			_cachedValue = (TEntity)_wrappedCachedValue.Entity;
		}
	}

	internal override bool CheckIfNavigationPropertyContainsEntity(IEntityWrapper wrapper)
	{
		if (!base.TargetAccessor.HasProperty)
		{
			return false;
		}
		return WrappedOwner.GetNavigationPropertyValue(this) == wrapper.Entity;
	}

	internal override void VerifyNavigationPropertyForAdd(IEntityWrapper wrapper)
	{
		if (base.TargetAccessor.HasProperty)
		{
			object navigationPropertyValue = WrappedOwner.GetNavigationPropertyValue(this);
			if (navigationPropertyValue != null && navigationPropertyValue != wrapper.Entity)
			{
				throw new InvalidOperationException(Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(base.RelationshipNavigation.To, base.RelationshipNavigation.RelationshipName));
			}
		}
	}

	[OnDeserialized]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void OnRefDeserialized(StreamingContext context)
	{
		_wrappedCachedValue = EntityWrapperFactory.WrapEntityUsingContext(_cachedValue, ObjectContext);
	}

	[OnSerializing]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void OnSerializing(StreamingContext context)
	{
		if (!(WrappedOwner.Entity is IEntityWithRelationships))
		{
			throw new InvalidOperationException(Strings.RelatedEnd_CannotSerialize("EntityReference"));
		}
	}

	internal override void AddToLocalCache(IEntityWrapper wrappedEntity, bool applyConstraints)
	{
		if (wrappedEntity == _wrappedCachedValue)
		{
			return;
		}
		TransactionManager transactionManager = ((ObjectContext != null) ? ObjectContext.ObjectStateManager.TransactionManager : null);
		if (applyConstraints && _wrappedCachedValue.Entity != null && (transactionManager == null || transactionManager.ProcessedEntities == null || transactionManager.ProcessedEntities.Contains(_wrappedCachedValue)))
		{
			throw new InvalidOperationException(Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(base.RelationshipNavigation.To, base.RelationshipNavigation.RelationshipName));
		}
		if (transactionManager != null && wrappedEntity.Entity != null)
		{
			transactionManager.BeginRelatedEndAdd();
		}
		try
		{
			ClearCollectionOrRef(null, null, doCascadeDelete: false);
			_wrappedCachedValue = wrappedEntity;
			_cachedValue = (TEntity)wrappedEntity.Entity;
		}
		finally
		{
			if (transactionManager != null && transactionManager.IsRelatedEndAdd)
			{
				transactionManager.EndRelatedEndAdd();
			}
		}
	}

	internal override void AddToObjectCache(IEntityWrapper wrappedEntity)
	{
		if (base.TargetAccessor.HasProperty)
		{
			WrappedOwner.SetNavigationPropertyValue(this, wrappedEntity.Entity);
		}
	}
}
