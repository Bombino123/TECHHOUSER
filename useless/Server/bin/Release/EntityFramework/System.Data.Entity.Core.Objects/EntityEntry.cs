using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Core.Objects;

internal sealed class EntityEntry : ObjectStateEntry
{
	internal struct RelationshipEndEnumerable : IEnumerable<RelationshipEntry>, IEnumerable, IEnumerable<IEntityStateEntry>
	{
		internal static readonly RelationshipEntry[] EmptyRelationshipEntryArray = new RelationshipEntry[0];

		private readonly EntityEntry _entityEntry;

		internal RelationshipEndEnumerable(EntityEntry entityEntry)
		{
			_entityEntry = entityEntry;
		}

		public RelationshipEndEnumerator GetEnumerator()
		{
			return new RelationshipEndEnumerator(_entityEntry);
		}

		IEnumerator<IEntityStateEntry> IEnumerable<IEntityStateEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<RelationshipEntry> IEnumerable<RelationshipEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal RelationshipEntry[] ToArray()
		{
			RelationshipEntry[] array = null;
			if (_entityEntry != null && 0 < _entityEntry._countRelationshipEnds)
			{
				RelationshipEntry relationshipEntry = _entityEntry._headRelationshipEnds;
				array = new RelationshipEntry[_entityEntry._countRelationshipEnds];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = relationshipEntry;
					relationshipEntry = relationshipEntry.GetNextRelationshipEnd(_entityEntry.EntityKey);
				}
			}
			return array ?? EmptyRelationshipEntryArray;
		}
	}

	internal struct RelationshipEndEnumerator : IEnumerator<RelationshipEntry>, IDisposable, IEnumerator, IEnumerator<IEntityStateEntry>
	{
		private readonly EntityEntry _entityEntry;

		private RelationshipEntry _current;

		public RelationshipEntry Current => _current;

		IEntityStateEntry IEnumerator<IEntityStateEntry>.Current => _current;

		object IEnumerator.Current => _current;

		internal RelationshipEndEnumerator(EntityEntry entityEntry)
		{
			_entityEntry = entityEntry;
			_current = null;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_entityEntry != null)
			{
				if (_current == null)
				{
					_current = _entityEntry._headRelationshipEnds;
				}
				else
				{
					_current = _current.GetNextRelationshipEnd(_entityEntry.EntityKey);
				}
			}
			return _current != null;
		}

		public void Reset()
		{
		}
	}

	private enum UpdateRecordBehavior
	{
		WithoutSetModified,
		WithSetModified
	}

	private StateManagerTypeMetadata _cacheTypeMetadata;

	private EntityKey _entityKey;

	private IEntityWrapper _wrappedEntity;

	private BitArray _modifiedFields;

	private List<StateManagerValue> _originalValues;

	private Dictionary<object, Dictionary<int, object>> _originalComplexObjects;

	private bool _requiresComplexChangeTracking;

	private bool _requiresScalarChangeTracking;

	private bool _requiresAnyChangeTracking;

	private RelationshipEntry _headRelationshipEnds;

	private int _countRelationshipEnds;

	internal const int s_EntityRoot = -1;

	public override bool IsRelationship
	{
		get
		{
			ValidateState();
			return false;
		}
	}

	public override object Entity
	{
		get
		{
			ValidateState();
			return _wrappedEntity.Entity;
		}
	}

	public override EntityKey EntityKey
	{
		get
		{
			ValidateState();
			return _entityKey;
		}
		internal set
		{
			_entityKey = value;
		}
	}

	internal IEnumerable<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyDependents
	{
		get
		{
			foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ((EntitySet)base.EntitySet).ForeignKeyDependents)
			{
				if (MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)foreignKeyDependent.Item2.ToRole).IsAssignableFrom(_cacheTypeMetadata.DataRecordInfo.RecordType.EdmType))
				{
					yield return foreignKeyDependent;
				}
			}
		}
	}

	internal IEnumerable<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyPrincipals
	{
		get
		{
			foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyPrincipal in ((EntitySet)base.EntitySet).ForeignKeyPrincipals)
			{
				if (MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)foreignKeyPrincipal.Item2.FromRole).IsAssignableFrom(_cacheTypeMetadata.DataRecordInfo.RecordType.EdmType))
				{
					yield return foreignKeyPrincipal;
				}
			}
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public override DbDataRecord OriginalValues => InternalGetOriginalValues(readOnly: true);

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public override CurrentValueRecord CurrentValues
	{
		get
		{
			ValidateState();
			if (base.State == EntityState.Deleted)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_CurrentValuesDoesNotExist);
			}
			if (IsKeyEntry)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
			}
			return new ObjectStateEntryDbUpdatableDataRecord(this, _cacheTypeMetadata, _wrappedEntity.Entity);
		}
	}

	public override RelationshipManager RelationshipManager
	{
		get
		{
			ValidateState();
			if (IsKeyEntry)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers);
			}
			if (WrappedEntity.Entity == null)
			{
				throw new InvalidOperationException(Strings.ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity);
			}
			return WrappedEntity.RelationshipManager;
		}
	}

	internal override BitArray ModifiedProperties => _modifiedFields;

	internal override bool IsKeyEntry => _wrappedEntity.Entity == null;

	internal IEntityWrapper WrappedEntity => _wrappedEntity;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal OriginalValueRecord EditableOriginalValues => new ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(this, _cacheTypeMetadata, _wrappedEntity.Entity);

	internal bool RequiresComplexChangeTracking => _requiresComplexChangeTracking;

	internal bool RequiresScalarChangeTracking => _requiresScalarChangeTracking;

	internal bool RequiresAnyChangeTracking => _requiresAnyChangeTracking;

	internal EntityEntry()
		: base(new ObjectStateManager(), null, EntityState.Unchanged)
	{
	}

	internal EntityEntry(ObjectStateManager stateManager)
		: base(stateManager, null, EntityState.Unchanged)
	{
	}

	internal EntityEntry(IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySet, ObjectStateManager cache, StateManagerTypeMetadata typeMetadata, EntityState state)
		: base(cache, entitySet, state)
	{
		_wrappedEntity = wrappedEntity;
		_cacheTypeMetadata = typeMetadata;
		_entityKey = entityKey;
		wrappedEntity.ObjectStateEntry = this;
		SetChangeTrackingFlags();
	}

	private void SetChangeTrackingFlags()
	{
		_requiresScalarChangeTracking = Entity != null && !(Entity is IEntityWithChangeTracker);
		_requiresComplexChangeTracking = Entity != null && (_requiresScalarChangeTracking || (WrappedEntity.IdentityType != Entity.GetType() && _cacheTypeMetadata.Members.Any((StateManagerMemberMetadata m) => m.IsComplex)));
		_requiresAnyChangeTracking = Entity != null && (!(Entity is IEntityWithRelationships) || _requiresComplexChangeTracking || _requiresScalarChangeTracking);
	}

	internal EntityEntry(EntityKey entityKey, EntitySet entitySet, ObjectStateManager cache, StateManagerTypeMetadata typeMetadata)
		: base(cache, entitySet, EntityState.Unchanged)
	{
		_wrappedEntity = NullEntityWrapper.NullWrapper;
		_entityKey = entityKey;
		_cacheTypeMetadata = typeMetadata;
		SetChangeTrackingFlags();
	}

	public override IEnumerable<string> GetModifiedProperties()
	{
		ValidateState();
		if (EntityState.Modified != base.State || _modifiedFields == null)
		{
			yield break;
		}
		for (int i = 0; i < _modifiedFields.Length; i++)
		{
			if (_modifiedFields[i])
			{
				yield return GetCLayerName(i, _cacheTypeMetadata);
			}
		}
	}

	public override void SetModifiedProperty(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		int modifiedPropertyInternal = ValidateAndGetOrdinalForProperty(propertyName, "SetModifiedProperty");
		if (EntityState.Unchanged == base.State)
		{
			base.State = EntityState.Modified;
			_cache.ChangeState(this, EntityState.Unchanged, base.State);
		}
		SetModifiedPropertyInternal(modifiedPropertyInternal);
	}

	internal void SetModifiedPropertyInternal(int ordinal)
	{
		if (_modifiedFields == null)
		{
			_modifiedFields = new BitArray(GetFieldCount(_cacheTypeMetadata));
		}
		_modifiedFields[ordinal] = true;
	}

	private int ValidateAndGetOrdinalForProperty(string propertyName, string methodName)
	{
		ValidateState();
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyEntryState);
		}
		int ordinalforOLayerMemberName = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(propertyName);
		if (ordinalforOLayerMemberName == -1)
		{
			throw new ArgumentException(Strings.ObjectStateEntry_SetModifiedOnInvalidProperty(propertyName));
		}
		if (base.State == EntityState.Added || base.State == EntityState.Deleted)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_SetModifiedStates(methodName));
		}
		return ordinalforOLayerMemberName;
	}

	public override void RejectPropertyChanges(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		int num = ValidateAndGetOrdinalForProperty(propertyName, "RejectPropertyChanges");
		if (base.State == EntityState.Unchanged || _modifiedFields == null || !_modifiedFields[num])
		{
			return;
		}
		DetectChangesInComplexProperties();
		object originalEntityValue = GetOriginalEntityValue(_cacheTypeMetadata, num, _wrappedEntity.Entity, ObjectStateValueRecord.OriginalReadonly);
		SetCurrentEntityValue(_cacheTypeMetadata, num, _wrappedEntity.Entity, originalEntityValue);
		_modifiedFields[num] = false;
		for (int i = 0; i < _modifiedFields.Length; i++)
		{
			if (_modifiedFields[i])
			{
				return;
			}
		}
		ChangeObjectState(EntityState.Unchanged);
	}

	public override OriginalValueRecord GetUpdatableOriginalValues()
	{
		return (OriginalValueRecord)InternalGetOriginalValues(readOnly: false);
	}

	private DbDataRecord InternalGetOriginalValues(bool readOnly)
	{
		ValidateState();
		if (base.State == EntityState.Added)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
		}
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		DetectChangesInComplexProperties();
		if (readOnly)
		{
			return new ObjectStateEntryDbDataRecord(this, _cacheTypeMetadata, _wrappedEntity.Entity);
		}
		return new ObjectStateEntryOriginalDbUpdatableDataRecord_Public(this, _cacheTypeMetadata, _wrappedEntity.Entity, -1);
	}

	private void DetectChangesInComplexProperties()
	{
		if (RequiresScalarChangeTracking)
		{
			base.ObjectStateManager.TransactionManager.BeginOriginalValuesGetter();
			try
			{
				DetectChangesInProperties(detectOnlyComplexProperties: true);
			}
			finally
			{
				base.ObjectStateManager.TransactionManager.EndOriginalValuesGetter();
			}
		}
	}

	public override void Delete()
	{
		Delete(doFixup: true);
	}

	public override void AcceptChanges()
	{
		ValidateState();
		if (base.ObjectStateManager.EntryHasConceptualNull(this))
		{
			throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
		}
		switch (base.State)
		{
		case EntityState.Deleted:
			CascadeAcceptChanges();
			if (_cache != null)
			{
				_cache.ChangeState(this, EntityState.Deleted, EntityState.Detached);
			}
			break;
		case EntityState.Added:
		{
			bool num = RetrieveAndCheckReferentialConstraintValuesInAcceptChanges();
			_cache.FixupKey(this);
			_modifiedFields = null;
			_originalValues = null;
			_originalComplexObjects = null;
			base.State = EntityState.Unchanged;
			if (num)
			{
				RelationshipManager.CheckReferentialConstraintProperties(this);
			}
			_wrappedEntity.TakeSnapshot(this);
			break;
		}
		case EntityState.Modified:
			_cache.ChangeState(this, EntityState.Modified, EntityState.Unchanged);
			_modifiedFields = null;
			_originalValues = null;
			_originalComplexObjects = null;
			base.State = EntityState.Unchanged;
			_cache.FixupReferencesByForeignKeys(this);
			RelationshipManager.CheckReferentialConstraintProperties(this);
			_wrappedEntity.TakeSnapshot(this);
			break;
		}
	}

	public override void SetModified()
	{
		ValidateState();
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyEntryState);
		}
		if (EntityState.Unchanged == base.State)
		{
			base.State = EntityState.Modified;
			_cache.ChangeState(this, EntityState.Unchanged, base.State);
		}
		else if (EntityState.Modified != base.State)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_SetModifiedStates("SetModified"));
		}
	}

	public override void ChangeState(EntityState state)
	{
		EntityUtil.CheckValidStateForChangeEntityState(state);
		if (base.State == EntityState.Detached && state == EntityState.Detached)
		{
			return;
		}
		ValidateState();
		ObjectStateManager objectStateManager = base.ObjectStateManager;
		objectStateManager.TransactionManager.BeginLocalPublicAPI();
		try
		{
			ChangeObjectState(state);
		}
		finally
		{
			objectStateManager.TransactionManager.EndLocalPublicAPI();
		}
	}

	public override void ApplyCurrentValues(object currentEntity)
	{
		Check.NotNull(currentEntity, "currentEntity");
		ValidateState();
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		IEntityWrapper wrappedCurrentEntity = base.ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(currentEntity, base.ObjectStateManager);
		ApplyCurrentValuesInternal(wrappedCurrentEntity);
	}

	public override void ApplyOriginalValues(object originalEntity)
	{
		Check.NotNull(originalEntity, "originalEntity");
		ValidateState();
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		IEntityWrapper wrappedOriginalEntity = base.ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(originalEntity, base.ObjectStateManager);
		ApplyOriginalValuesInternal(wrappedOriginalEntity);
	}

	internal void AddRelationshipEnd(RelationshipEntry item)
	{
		item.SetNextRelationshipEnd(EntityKey, _headRelationshipEnds);
		_headRelationshipEnds = item;
		_countRelationshipEnds++;
	}

	internal bool ContainsRelationshipEnd(RelationshipEntry item)
	{
		for (RelationshipEntry relationshipEntry = _headRelationshipEnds; relationshipEntry != null; relationshipEntry = relationshipEntry.GetNextRelationshipEnd(EntityKey))
		{
			if (relationshipEntry == item)
			{
				return true;
			}
		}
		return false;
	}

	internal void RemoveRelationshipEnd(RelationshipEntry item)
	{
		RelationshipEntry relationshipEntry = _headRelationshipEnds;
		RelationshipEntry relationshipEntry2 = null;
		bool flag = false;
		while (relationshipEntry != null)
		{
			bool flag2 = (object)EntityKey == relationshipEntry.Key0 || ((object)EntityKey != relationshipEntry.Key1 && EntityKey.Equals(relationshipEntry.Key0));
			if (item == relationshipEntry)
			{
				RelationshipEntry relationshipEntry3;
				if (flag2)
				{
					relationshipEntry3 = relationshipEntry.NextKey0;
					relationshipEntry.NextKey0 = null;
				}
				else
				{
					relationshipEntry3 = relationshipEntry.NextKey1;
					relationshipEntry.NextKey1 = null;
				}
				if (relationshipEntry2 == null)
				{
					_headRelationshipEnds = relationshipEntry3;
				}
				else if (flag)
				{
					relationshipEntry2.NextKey0 = relationshipEntry3;
				}
				else
				{
					relationshipEntry2.NextKey1 = relationshipEntry3;
				}
				_countRelationshipEnds--;
				break;
			}
			relationshipEntry2 = relationshipEntry;
			relationshipEntry = (flag2 ? relationshipEntry.NextKey0 : relationshipEntry.NextKey1);
			flag = flag2;
		}
	}

	internal void UpdateRelationshipEnds(EntityKey oldKey, EntityEntry promotedEntry)
	{
		int num = 0;
		RelationshipEntry relationshipEntry = _headRelationshipEnds;
		while (relationshipEntry != null)
		{
			RelationshipEntry relationshipEntry2 = relationshipEntry;
			relationshipEntry = relationshipEntry.GetNextRelationshipEnd(oldKey);
			relationshipEntry2.ChangeRelatedEnd(oldKey, EntityKey);
			if (promotedEntry != null && !promotedEntry.ContainsRelationshipEnd(relationshipEntry2))
			{
				promotedEntry.AddRelationshipEnd(relationshipEntry2);
			}
			num++;
		}
		if (promotedEntry != null)
		{
			_headRelationshipEnds = null;
			_countRelationshipEnds = 0;
		}
	}

	internal RelationshipEndEnumerable GetRelationshipEnds()
	{
		return new RelationshipEndEnumerable(this);
	}

	internal override DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject)
	{
		if (Helper.IsEntityType(metadata.CdmMetadata.EdmType) && (object)_entityKey != null)
		{
			return new EntityRecordInfo(metadata.DataRecordInfo, _entityKey, (EntitySet)base.EntitySet);
		}
		return metadata.DataRecordInfo;
	}

	internal override void Reset()
	{
		RemoveFromForeignKeyIndex();
		_cache.ForgetEntryWithConceptualNull(this, resetAllKeys: true);
		DetachObjectStateManagerFromEntity();
		_wrappedEntity = NullEntityWrapper.NullWrapper;
		_entityKey = null;
		_modifiedFields = null;
		_originalValues = null;
		_originalComplexObjects = null;
		SetChangeTrackingFlags();
		base.Reset();
	}

	internal override Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata)
	{
		return metadata.GetFieldType(ordinal);
	}

	internal override string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata)
	{
		return metadata.CLayerMemberName(ordinal);
	}

	internal override int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata)
	{
		return metadata.GetOrdinalforCLayerMemberName(name);
	}

	internal override void RevertDelete()
	{
		base.State = ((_modifiedFields == null) ? EntityState.Unchanged : EntityState.Modified);
		_cache.ChangeState(this, EntityState.Deleted, base.State);
	}

	internal override int GetFieldCount(StateManagerTypeMetadata metadata)
	{
		return metadata.FieldCount;
	}

	private void CascadeAcceptChanges()
	{
		RelationshipEntry[] array = _cache.CopyOfRelationshipsByKey(EntityKey);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AcceptChanges();
		}
	}

	internal override void SetModifiedAll()
	{
		ValidateState();
		if (_modifiedFields == null)
		{
			_modifiedFields = new BitArray(GetFieldCount(_cacheTypeMetadata));
		}
		_modifiedFields.SetAll(value: true);
	}

	internal override void EntityMemberChanging(string entityMemberName)
	{
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		EntityMemberChanging(entityMemberName, null, null);
	}

	internal override void EntityMemberChanged(string entityMemberName)
	{
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		EntityMemberChanged(entityMemberName, null, null);
	}

	internal override void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		EntityMemberChanging(entityMemberName, complexObject, complexObjectMemberName);
	}

	internal override void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
		}
		EntityMemberChanged(entityMemberName, complexObject, complexObjectMemberName);
	}

	private void EntityMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		try
		{
			StateManagerTypeMetadata typeMetadata;
			string changingMemberName;
			object changingObject;
			int andValidateChangeMemberInfo = GetAndValidateChangeMemberInfo(entityMemberName, complexObject, complexObjectMemberName, out typeMetadata, out changingMemberName, out changingObject);
			if (andValidateChangeMemberInfo == -2)
			{
				return;
			}
			if (changingObject != _cache.ChangingObject || changingMemberName != _cache.ChangingMember || entityMemberName != _cache.ChangingEntityMember)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_EntityMemberChangedWithoutEntityMemberChanging);
			}
			if (base.State != _cache.ChangingState)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_ChangedInDifferentStateFromChanging(_cache.ChangingState, base.State));
			}
			object changingOldValue = _cache.ChangingOldValue;
			object obj = null;
			StateManagerMemberMetadata stateManagerMemberMetadata = null;
			if (_cache.SaveOriginalValues)
			{
				stateManagerMemberMetadata = typeMetadata.Member(andValidateChangeMemberInfo);
				if (stateManagerMemberMetadata.IsComplex && changingOldValue != null)
				{
					obj = stateManagerMemberMetadata.GetValue(changingObject);
					ExpandComplexTypeAndAddValues(stateManagerMemberMetadata, changingOldValue, obj, useOldComplexObject: false);
				}
				else
				{
					AddOriginalValueAt(-1, stateManagerMemberMetadata, changingObject, changingOldValue);
				}
			}
			TransactionManager transactionManager = base.ObjectStateManager.TransactionManager;
			if (complexObject == null && (transactionManager.IsAlignChanges || !transactionManager.IsDetectChanges) && IsPropertyAForeignKey(entityMemberName, out var relationships))
			{
				foreach (Pair<string, string> item in relationships)
				{
					string first = item.First;
					string second = item.Second;
					EntityReference entityReference = WrappedEntity.RelationshipManager.GetRelatedEndInternal(first, second) as EntityReference;
					if (!transactionManager.IsFixupByReference)
					{
						if (stateManagerMemberMetadata == null)
						{
							stateManagerMemberMetadata = typeMetadata.Member(andValidateChangeMemberInfo);
						}
						if (obj == null)
						{
							obj = stateManagerMemberMetadata.GetValue(changingObject);
						}
						bool flag = ForeignKeyFactory.IsConceptualNullKey(entityReference.CachedForeignKey);
						if (!ByValueEqualityComparer.Default.Equals(changingOldValue, obj) || flag)
						{
							FixupEntityReferenceByForeignKey(entityReference);
						}
					}
				}
			}
			if (_cache != null && !_cache.TransactionManager.IsOriginalValuesGetter)
			{
				EntityState state = base.State;
				if (base.State != EntityState.Added)
				{
					base.State = EntityState.Modified;
				}
				if (base.State == EntityState.Modified)
				{
					SetModifiedProperty(entityMemberName);
				}
				if (state != base.State)
				{
					_cache.ChangeState(this, state, base.State);
				}
			}
		}
		finally
		{
			SetCachedChangingValues(null, null, null, EntityState.Detached, null);
		}
	}

	internal void SetCurrentEntityValue(string memberName, object newValue)
	{
		int ordinalforOLayerMemberName = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(memberName);
		SetCurrentEntityValue(_cacheTypeMetadata, ordinalforOLayerMemberName, _wrappedEntity.Entity, newValue);
	}

	internal void SetOriginalEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, object newValue)
	{
		ValidateState();
		if (base.State == EntityState.Added)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
		}
		EntityState state = base.State;
		StateManagerMemberMetadata stateManagerMemberMetadata = metadata.Member(ordinal);
		int num = FindOriginalValueIndex(stateManagerMemberMetadata, userObject);
		if (stateManagerMemberMetadata.IsComplex)
		{
			if (num >= 0)
			{
				_originalValues.RemoveAt(num);
			}
			object value = stateManagerMemberMetadata.GetValue(userObject);
			if (value == null)
			{
				throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(stateManagerMemberMetadata.CLayerName));
			}
			if (newValue is IExtendedDataRecord extendedDataRecord)
			{
				newValue = _cache.ComplexTypeMaterializer.CreateComplex(extendedDataRecord, extendedDataRecord.DataRecordInfo, null);
			}
			ExpandComplexTypeAndAddValues(stateManagerMemberMetadata, value, newValue, useOldComplexObject: true);
		}
		else
		{
			AddOriginalValueAt(num, stateManagerMemberMetadata, userObject, newValue);
		}
		if (state == EntityState.Unchanged)
		{
			base.State = EntityState.Modified;
		}
	}

	private void EntityMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		StateManagerTypeMetadata typeMetadata;
		string changingMemberName;
		object changingObject;
		int andValidateChangeMemberInfo = GetAndValidateChangeMemberInfo(entityMemberName, complexObject, complexObjectMemberName, out typeMetadata, out changingMemberName, out changingObject);
		if (andValidateChangeMemberInfo != -2)
		{
			StateManagerMemberMetadata stateManagerMemberMetadata = typeMetadata.Member(andValidateChangeMemberInfo);
			_cache.SaveOriginalValues = (base.State == EntityState.Unchanged || base.State == EntityState.Modified) && FindOriginalValueIndex(stateManagerMemberMetadata, changingObject) == -1;
			object value = stateManagerMemberMetadata.GetValue(changingObject);
			SetCachedChangingValues(entityMemberName, changingObject, changingMemberName, base.State, value);
		}
	}

	internal object GetOriginalEntityValue(string memberName)
	{
		int ordinalforOLayerMemberName = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(memberName);
		return GetOriginalEntityValue(_cacheTypeMetadata, ordinalforOLayerMemberName, _wrappedEntity.Entity, ObjectStateValueRecord.OriginalReadonly);
	}

	internal object GetOriginalEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord)
	{
		return GetOriginalEntityValue(metadata, ordinal, userObject, updatableRecord, -1);
	}

	internal object GetOriginalEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord, int parentEntityPropertyIndex)
	{
		ValidateState();
		return GetOriginalEntityValue(metadata, metadata.Member(ordinal), ordinal, userObject, updatableRecord, parentEntityPropertyIndex);
	}

	internal object GetOriginalEntityValue(StateManagerTypeMetadata metadata, StateManagerMemberMetadata memberMetadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord, int parentEntityPropertyIndex)
	{
		int num = FindOriginalValueIndex(memberMetadata, userObject);
		if (num >= 0)
		{
			return _originalValues[num].OriginalValue ?? DBNull.Value;
		}
		return GetCurrentEntityValue(metadata, ordinal, userObject, updatableRecord, parentEntityPropertyIndex);
	}

	internal object GetCurrentEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord)
	{
		return GetCurrentEntityValue(metadata, ordinal, userObject, updatableRecord, -1);
	}

	internal object GetCurrentEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord, int parentEntityPropertyIndex)
	{
		ValidateState();
		object obj = null;
		StateManagerMemberMetadata stateManagerMemberMetadata = metadata.Member(ordinal);
		obj = stateManagerMemberMetadata.GetValue(userObject);
		if (stateManagerMemberMetadata.IsComplex && obj != null)
		{
			switch (updatableRecord)
			{
			case ObjectStateValueRecord.OriginalReadonly:
				obj = new ObjectStateEntryDbDataRecord(this, _cache.GetOrAddStateManagerTypeMetadata(stateManagerMemberMetadata.CdmMetadata.TypeUsage.EdmType), obj);
				break;
			case ObjectStateValueRecord.CurrentUpdatable:
				obj = new ObjectStateEntryDbUpdatableDataRecord(this, _cache.GetOrAddStateManagerTypeMetadata(stateManagerMemberMetadata.CdmMetadata.TypeUsage.EdmType), obj);
				break;
			case ObjectStateValueRecord.OriginalUpdatableInternal:
				obj = new ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(this, _cache.GetOrAddStateManagerTypeMetadata(stateManagerMemberMetadata.CdmMetadata.TypeUsage.EdmType), obj);
				break;
			case ObjectStateValueRecord.OriginalUpdatablePublic:
				obj = new ObjectStateEntryOriginalDbUpdatableDataRecord_Public(this, _cache.GetOrAddStateManagerTypeMetadata(stateManagerMemberMetadata.CdmMetadata.TypeUsage.EdmType), obj, parentEntityPropertyIndex);
				break;
			}
		}
		return obj ?? DBNull.Value;
	}

	internal int FindOriginalValueIndex(StateManagerMemberMetadata metadata, object instance)
	{
		if (_originalValues != null)
		{
			for (int i = 0; i < _originalValues.Count; i++)
			{
				if (_originalValues[i].UserObject == instance && _originalValues[i].MemberMetadata == metadata)
				{
					return i;
				}
			}
		}
		return -1;
	}

	internal AssociationEndMember GetAssociationEndMember(RelationshipEntry relationshipEntry)
	{
		ValidateState();
		return relationshipEntry.RelationshipWrapper.GetAssociationEndMember(EntityKey);
	}

	internal EntityEntry GetOtherEndOfRelationship(RelationshipEntry relationshipEntry)
	{
		return _cache.GetEntityEntry(relationshipEntry.RelationshipWrapper.GetOtherEntityKey(EntityKey));
	}

	internal void ExpandComplexTypeAndAddValues(StateManagerMemberMetadata memberMetadata, object oldComplexObject, object newComplexObject, bool useOldComplexObject)
	{
		if (newComplexObject == null)
		{
			throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(memberMetadata.CLayerName));
		}
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = _cache.GetOrAddStateManagerTypeMetadata(memberMetadata.CdmMetadata.TypeUsage.EdmType);
		for (int i = 0; i < orAddStateManagerTypeMetadata.FieldCount; i++)
		{
			StateManagerMemberMetadata stateManagerMemberMetadata = orAddStateManagerTypeMetadata.Member(i);
			if (stateManagerMemberMetadata.IsComplex)
			{
				object obj = null;
				if (oldComplexObject != null)
				{
					obj = stateManagerMemberMetadata.GetValue(oldComplexObject);
					if (obj == null)
					{
						int num = FindOriginalValueIndex(stateManagerMemberMetadata, oldComplexObject);
						if (num >= 0)
						{
							_originalValues.RemoveAt(num);
						}
					}
				}
				ExpandComplexTypeAndAddValues(stateManagerMemberMetadata, obj, stateManagerMemberMetadata.GetValue(newComplexObject), useOldComplexObject);
				continue;
			}
			object userObject = newComplexObject;
			int num2 = -1;
			object value;
			if (useOldComplexObject)
			{
				value = stateManagerMemberMetadata.GetValue(newComplexObject);
				userObject = oldComplexObject;
			}
			else if (oldComplexObject != null)
			{
				value = stateManagerMemberMetadata.GetValue(oldComplexObject);
				num2 = FindOriginalValueIndex(stateManagerMemberMetadata, oldComplexObject);
				if (num2 >= 0)
				{
					value = _originalValues[num2].OriginalValue;
				}
			}
			else
			{
				value = stateManagerMemberMetadata.GetValue(newComplexObject);
			}
			AddOriginalValueAt(num2, stateManagerMemberMetadata, userObject, value);
		}
	}

	internal int GetAndValidateChangeMemberInfo(string entityMemberName, object complexObject, string complexObjectMemberName, out StateManagerTypeMetadata typeMetadata, out string changingMemberName, out object changingObject)
	{
		Check.NotNull(entityMemberName, "entityMemberName");
		typeMetadata = null;
		changingMemberName = null;
		changingObject = null;
		ValidateState();
		int ordinalforOLayerMemberName = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(entityMemberName);
		if (ordinalforOLayerMemberName == -1)
		{
			if (entityMemberName == "-EntityKey-")
			{
				if (!_cache.InRelationshipFixup)
				{
					throw new InvalidOperationException(Strings.ObjectStateEntry_CantSetEntityKey);
				}
				SetCachedChangingValues(null, null, null, base.State, null);
				return -2;
			}
			throw new ArgumentException(Strings.ObjectStateEntry_ChangeOnUnmappedProperty(entityMemberName));
		}
		StateManagerTypeMetadata stateManagerTypeMetadata;
		string text;
		object obj;
		if (complexObject != null)
		{
			if (!_cacheTypeMetadata.Member(ordinalforOLayerMemberName).IsComplex)
			{
				throw new ArgumentException(Strings.ComplexObject_ComplexChangeRequestedOnScalarProperty(entityMemberName));
			}
			stateManagerTypeMetadata = _cache.GetOrAddStateManagerTypeMetadata(complexObject.GetType(), (EntitySet)base.EntitySet);
			ordinalforOLayerMemberName = stateManagerTypeMetadata.GetOrdinalforOLayerMemberName(complexObjectMemberName);
			if (ordinalforOLayerMemberName == -1)
			{
				throw new ArgumentException(Strings.ObjectStateEntry_ChangeOnUnmappedComplexProperty(complexObjectMemberName));
			}
			text = complexObjectMemberName;
			obj = complexObject;
		}
		else
		{
			stateManagerTypeMetadata = _cacheTypeMetadata;
			text = entityMemberName;
			obj = Entity;
			if (WrappedEntity.IdentityType != Entity.GetType() && Entity is IEntityWithChangeTracker && IsPropertyAForeignKey(entityMemberName))
			{
				_cache.EntityInvokingFKSetter = WrappedEntity.Entity;
			}
		}
		VerifyEntityValueIsEditable(stateManagerTypeMetadata, ordinalforOLayerMemberName, text);
		typeMetadata = stateManagerTypeMetadata;
		changingMemberName = text;
		changingObject = obj;
		return ordinalforOLayerMemberName;
	}

	private void SetCachedChangingValues(string entityMemberName, object changingObject, string changingMember, EntityState changingState, object oldValue)
	{
		_cache.ChangingEntityMember = entityMemberName;
		_cache.ChangingObject = changingObject;
		_cache.ChangingMember = changingMember;
		_cache.ChangingState = changingState;
		_cache.ChangingOldValue = oldValue;
		if (changingState == EntityState.Detached)
		{
			_cache.SaveOriginalValues = false;
		}
	}

	internal void DetachObjectStateManagerFromEntity()
	{
		if (!IsKeyEntry)
		{
			_wrappedEntity.SetChangeTracker(null);
			_wrappedEntity.DetachContext();
			if (!_cache.TransactionManager.IsAttachTracking || _cache.TransactionManager.OriginalMergeOption != MergeOption.NoTracking)
			{
				_wrappedEntity.EntityKey = null;
			}
		}
	}

	internal void TakeSnapshot(bool onlySnapshotComplexProperties)
	{
		if (base.State != EntityState.Added)
		{
			StateManagerTypeMetadata cacheTypeMetadata = _cacheTypeMetadata;
			int fieldCount = GetFieldCount(cacheTypeMetadata);
			for (int i = 0; i < fieldCount; i++)
			{
				StateManagerMemberMetadata stateManagerMemberMetadata = cacheTypeMetadata.Member(i);
				if (stateManagerMemberMetadata.IsComplex)
				{
					object value = stateManagerMemberMetadata.GetValue(_wrappedEntity.Entity);
					AddComplexObjectSnapshot(Entity, i, value);
					TakeSnapshotOfComplexType(stateManagerMemberMetadata, value);
				}
				else if (!onlySnapshotComplexProperties)
				{
					object value = stateManagerMemberMetadata.GetValue(_wrappedEntity.Entity);
					AddOriginalValueAt(-1, stateManagerMemberMetadata, _wrappedEntity.Entity, value);
				}
			}
		}
		TakeSnapshotOfForeignKeys();
	}

	internal void TakeSnapshotOfForeignKeys()
	{
		FindRelatedEntityKeysByForeignKeys(out var relatedEntities, useOriginalValues: false);
		if (relatedEntities == null)
		{
			return;
		}
		foreach (KeyValuePair<RelatedEnd, HashSet<EntityKey>> item in relatedEntities)
		{
			EntityReference entityReference = item.Key as EntityReference;
			if (!ForeignKeyFactory.IsConceptualNullKey(entityReference.CachedForeignKey))
			{
				entityReference.SetCachedForeignKey(item.Value.First(), this);
			}
		}
	}

	private void TakeSnapshotOfComplexType(StateManagerMemberMetadata member, object complexValue)
	{
		if (complexValue == null)
		{
			return;
		}
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType);
		for (int i = 0; i < orAddStateManagerTypeMetadata.FieldCount; i++)
		{
			StateManagerMemberMetadata stateManagerMemberMetadata = orAddStateManagerTypeMetadata.Member(i);
			object value = stateManagerMemberMetadata.GetValue(complexValue);
			if (stateManagerMemberMetadata.IsComplex)
			{
				AddComplexObjectSnapshot(complexValue, i, value);
				TakeSnapshotOfComplexType(stateManagerMemberMetadata, value);
			}
			else if (FindOriginalValueIndex(stateManagerMemberMetadata, complexValue) == -1)
			{
				AddOriginalValueAt(-1, stateManagerMemberMetadata, complexValue, value);
			}
		}
	}

	private void AddComplexObjectSnapshot(object userObject, int ordinal, object complexObject)
	{
		if (complexObject != null)
		{
			CheckForDuplicateComplexObjects(complexObject);
			if (_originalComplexObjects == null)
			{
				_originalComplexObjects = new Dictionary<object, Dictionary<int, object>>(ObjectReferenceEqualityComparer.Default);
			}
			if (!_originalComplexObjects.TryGetValue(userObject, out var value))
			{
				value = new Dictionary<int, object>();
				_originalComplexObjects.Add(userObject, value);
			}
			value.Add(ordinal, complexObject);
		}
	}

	private void CheckForDuplicateComplexObjects(object complexObject)
	{
		if (_originalComplexObjects == null || complexObject == null)
		{
			return;
		}
		foreach (Dictionary<int, object> value in _originalComplexObjects.Values)
		{
			foreach (object value2 in value.Values)
			{
				if (complexObject == value2)
				{
					throw new InvalidOperationException(Strings.ObjectStateEntry_ComplexObjectUsedMultipleTimes(Entity.GetType().FullName, complexObject.GetType().FullName));
				}
			}
		}
	}

	public override bool IsPropertyChanged(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DetectChangesInProperty(ValidateAndGetOrdinalForProperty(propertyName, "IsPropertyChanged"), detectOnlyComplexProperties: false, detectOnly: true);
	}

	private bool DetectChangesInProperty(int ordinal, bool detectOnlyComplexProperties, bool detectOnly)
	{
		bool changeDetected = false;
		StateManagerMemberMetadata stateManagerMemberMetadata = _cacheTypeMetadata.Member(ordinal);
		object value = stateManagerMemberMetadata.GetValue(_wrappedEntity.Entity);
		if (stateManagerMemberMetadata.IsComplex)
		{
			if (base.State != EntityState.Deleted)
			{
				object complexObjectSnapshot = GetComplexObjectSnapshot(Entity, ordinal);
				if (DetectChangesInComplexType(stateManagerMemberMetadata, stateManagerMemberMetadata, value, complexObjectSnapshot, ref changeDetected, detectOnly))
				{
					CheckForDuplicateComplexObjects(value);
					if (!detectOnly)
					{
						((IEntityChangeTracker)this).EntityMemberChanging(stateManagerMemberMetadata.CLayerName);
						_cache.ChangingOldValue = complexObjectSnapshot;
						((IEntityChangeTracker)this).EntityMemberChanged(stateManagerMemberMetadata.CLayerName);
					}
					UpdateComplexObjectSnapshot(stateManagerMemberMetadata, Entity, ordinal, value);
					if (!changeDetected)
					{
						DetectChangesInComplexType(stateManagerMemberMetadata, stateManagerMemberMetadata, value, complexObjectSnapshot, ref changeDetected, detectOnly);
					}
				}
			}
		}
		else if (!detectOnlyComplexProperties)
		{
			int num = FindOriginalValueIndex(stateManagerMemberMetadata, _wrappedEntity.Entity);
			if (num < 0)
			{
				return GetModifiedProperties().Contains(stateManagerMemberMetadata.CLayerName);
			}
			object originalValue = _originalValues[num].OriginalValue;
			if (!object.Equals(value, originalValue))
			{
				changeDetected = true;
				if (stateManagerMemberMetadata.IsPartOfKey)
				{
					if (!ByValueEqualityComparer.Default.Equals(value, originalValue))
					{
						throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyProperty(stateManagerMemberMetadata.CLayerName));
					}
				}
				else if (base.State != EntityState.Deleted && !detectOnly)
				{
					((IEntityChangeTracker)this).EntityMemberChanging(stateManagerMemberMetadata.CLayerName);
					((IEntityChangeTracker)this).EntityMemberChanged(stateManagerMemberMetadata.CLayerName);
				}
			}
		}
		return changeDetected;
	}

	internal void DetectChangesInProperties(bool detectOnlyComplexProperties)
	{
		int fieldCount = GetFieldCount(_cacheTypeMetadata);
		for (int i = 0; i < fieldCount; i++)
		{
			DetectChangesInProperty(i, detectOnlyComplexProperties, detectOnly: false);
		}
	}

	private bool DetectChangesInComplexType(StateManagerMemberMetadata topLevelMember, StateManagerMemberMetadata complexMember, object complexValue, object oldComplexValue, ref bool changeDetected, bool detectOnly)
	{
		if (complexValue == null)
		{
			if (oldComplexValue == null)
			{
				return false;
			}
			throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(complexMember.CLayerName));
		}
		if (oldComplexValue != complexValue)
		{
			return true;
		}
		StateManagerTypeMetadata orAddStateManagerTypeMetadata = _cache.GetOrAddStateManagerTypeMetadata(complexMember.CdmMetadata.TypeUsage.EdmType);
		for (int i = 0; i < GetFieldCount(orAddStateManagerTypeMetadata); i++)
		{
			StateManagerMemberMetadata stateManagerMemberMetadata = orAddStateManagerTypeMetadata.Member(i);
			object obj = null;
			obj = stateManagerMemberMetadata.GetValue(complexValue);
			if (stateManagerMemberMetadata.IsComplex)
			{
				if (base.State == EntityState.Deleted)
				{
					continue;
				}
				object complexObjectSnapshot = GetComplexObjectSnapshot(complexValue, i);
				if (DetectChangesInComplexType(topLevelMember, stateManagerMemberMetadata, obj, complexObjectSnapshot, ref changeDetected, detectOnly))
				{
					CheckForDuplicateComplexObjects(obj);
					if (!detectOnly)
					{
						((IEntityChangeTracker)this).EntityComplexMemberChanging(topLevelMember.CLayerName, complexValue, stateManagerMemberMetadata.CLayerName);
						_cache.ChangingOldValue = complexObjectSnapshot;
						((IEntityChangeTracker)this).EntityComplexMemberChanged(topLevelMember.CLayerName, complexValue, stateManagerMemberMetadata.CLayerName);
					}
					UpdateComplexObjectSnapshot(stateManagerMemberMetadata, complexValue, i, obj);
					if (!changeDetected)
					{
						DetectChangesInComplexType(topLevelMember, stateManagerMemberMetadata, obj, complexObjectSnapshot, ref changeDetected, detectOnly);
					}
				}
				continue;
			}
			int num = FindOriginalValueIndex(stateManagerMemberMetadata, complexValue);
			object objB = ((num == -1) ? null : _originalValues[num].OriginalValue);
			if (!object.Equals(obj, objB))
			{
				changeDetected = true;
				if (!detectOnly)
				{
					((IEntityChangeTracker)this).EntityComplexMemberChanging(topLevelMember.CLayerName, complexValue, stateManagerMemberMetadata.CLayerName);
					((IEntityChangeTracker)this).EntityComplexMemberChanged(topLevelMember.CLayerName, complexValue, stateManagerMemberMetadata.CLayerName);
				}
			}
		}
		return false;
	}

	private object GetComplexObjectSnapshot(object parentObject, int parentOrdinal)
	{
		object value = null;
		if (_originalComplexObjects != null && _originalComplexObjects.TryGetValue(parentObject, out var value2))
		{
			value2.TryGetValue(parentOrdinal, out value);
		}
		return value;
	}

	internal void UpdateComplexObjectSnapshot(StateManagerMemberMetadata member, object userObject, int ordinal, object currentValue)
	{
		bool flag = true;
		if (_originalComplexObjects != null && _originalComplexObjects.TryGetValue(userObject, out var value))
		{
			value.TryGetValue(ordinal, out var value2);
			value[ordinal] = currentValue;
			if (value2 != null && _originalComplexObjects.TryGetValue(value2, out value))
			{
				_originalComplexObjects.Remove(value2);
				_originalComplexObjects.Add(currentValue, value);
				StateManagerTypeMetadata orAddStateManagerTypeMetadata = _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType);
				for (int i = 0; i < orAddStateManagerTypeMetadata.FieldCount; i++)
				{
					StateManagerMemberMetadata stateManagerMemberMetadata = orAddStateManagerTypeMetadata.Member(i);
					if (stateManagerMemberMetadata.IsComplex)
					{
						object value3 = stateManagerMemberMetadata.GetValue(currentValue);
						UpdateComplexObjectSnapshot(stateManagerMemberMetadata, currentValue, i, value3);
					}
				}
			}
			flag = false;
		}
		if (flag)
		{
			AddComplexObjectSnapshot(userObject, ordinal, currentValue);
		}
	}

	internal void FixupFKValuesFromNonAddedReferences()
	{
		if (!((EntitySet)base.EntitySet).HasForeignKeyRelationships)
		{
			return;
		}
		Dictionary<int, object> changedFKs = new Dictionary<int, object>();
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			EntityReference entityReference = RelationshipManager.GetRelatedEndInternal(foreignKeyDependent.Item1.ElementType.FullName, foreignKeyDependent.Item2.FromRole.Name) as EntityReference;
			if (entityReference.TargetAccessor.HasProperty)
			{
				object navigationPropertyValue = WrappedEntity.GetNavigationPropertyValue(entityReference);
				if (navigationPropertyValue != null && _cache.TryGetObjectStateEntry(navigationPropertyValue, out var entry) && (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged))
				{
					entityReference.UpdateForeignKeyValues(WrappedEntity, ((EntityEntry)entry).WrappedEntity, changedFKs, forceChange: false);
				}
			}
		}
	}

	internal void TakeSnapshotOfRelationships()
	{
		RelationshipManager relationshipManager = _wrappedEntity.RelationshipManager;
		foreach (NavigationProperty navigationProperty in (_cacheTypeMetadata.CdmMetadata.EdmType as EntityType).NavigationProperties)
		{
			RelatedEnd relatedEndInternal = relationshipManager.GetRelatedEndInternal(navigationProperty.RelationshipType.FullName, navigationProperty.ToEndMember.Name);
			object navigationPropertyValue = WrappedEntity.GetNavigationPropertyValue(relatedEndInternal);
			if (navigationPropertyValue == null)
			{
				continue;
			}
			if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				foreach (object item in (navigationPropertyValue as IEnumerable) ?? throw new EntityException(Strings.ObjectStateEntry_UnableToEnumerateCollection(navigationProperty.Name, Entity.GetType().FullName)))
				{
					if (item != null)
					{
						TakeSnapshotOfSingleRelationship(relatedEndInternal, navigationProperty, item);
					}
				}
			}
			else
			{
				TakeSnapshotOfSingleRelationship(relatedEndInternal, navigationProperty, navigationPropertyValue);
			}
		}
	}

	private void TakeSnapshotOfSingleRelationship(RelatedEnd relatedEnd, NavigationProperty n, object o)
	{
		EntityEntry entityEntry = base.ObjectStateManager.FindEntityEntry(o);
		IEntityWrapper value;
		if (entityEntry != null)
		{
			value = entityEntry._wrappedEntity;
			RelatedEnd relatedEndInternal = value.RelationshipManager.GetRelatedEndInternal(n.RelationshipType.FullName, n.FromEndMember.Name);
			if (!relatedEndInternal.ContainsEntity(_wrappedEntity))
			{
				if (value.ObjectStateEntry.State == EntityState.Deleted)
				{
					throw Error.RelatedEnd_UnableToAddRelationshipWithDeletedEntity();
				}
				if (base.ObjectStateManager.TransactionManager.IsAttachTracking && (base.State & (EntityState.Unchanged | EntityState.Modified)) != 0 && (value.ObjectStateEntry.State & (EntityState.Unchanged | EntityState.Modified)) != 0)
				{
					EntityEntry entityEntry2 = null;
					EntityEntry @object = null;
					if (relatedEnd.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
					{
						entityEntry2 = value.ObjectStateEntry;
						@object = this;
					}
					else if (relatedEndInternal.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
					{
						entityEntry2 = this;
						@object = value.ObjectStateEntry;
					}
					if (entityEntry2 != null)
					{
						ReferentialConstraint referentialConstraint = ((AssociationType)relatedEnd.RelationMetadata).ReferentialConstraints[0];
						if (!RelatedEnd.VerifyRIConstraintsWithRelatedEntry(referentialConstraint, @object.GetCurrentEntityValue, entityEntry2.EntityKey))
						{
							throw new InvalidOperationException(referentialConstraint.BuildConstraintExceptionMessage());
						}
					}
				}
				if (relatedEndInternal is EntityReference entityReference && entityReference.NavigationPropertyIsNullOrMissing())
				{
					base.ObjectStateManager.TransactionManager.AlignedEntityReferences.Add(entityReference);
				}
				relatedEndInternal.AddToLocalCache(_wrappedEntity, applyConstraints: true);
				relatedEndInternal.OnAssociationChanged(CollectionChangeAction.Add, _wrappedEntity.Entity);
			}
		}
		else if (!base.ObjectStateManager.TransactionManager.WrappedEntities.TryGetValue(o, out value))
		{
			value = base.ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(o, base.ObjectStateManager);
		}
		if (!relatedEnd.ContainsEntity(value))
		{
			relatedEnd.AddToLocalCache(value, applyConstraints: true);
			relatedEnd.OnAssociationChanged(CollectionChangeAction.Add, value.Entity);
		}
	}

	internal void DetectChangesInRelationshipsOfSingleEntity()
	{
		foreach (NavigationProperty navigationProperty in (_cacheTypeMetadata.CdmMetadata.EdmType as EntityType).NavigationProperties)
		{
			RelatedEnd relatedEndInternal = WrappedEntity.RelationshipManager.GetRelatedEndInternal(navigationProperty.RelationshipType.FullName, navigationProperty.ToEndMember.Name);
			object navigationPropertyValue = WrappedEntity.GetNavigationPropertyValue(relatedEndInternal);
			HashSet<object> hashSet = new HashSet<object>(ObjectReferenceEqualityComparer.Default);
			if (navigationPropertyValue != null)
			{
				if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
				{
					foreach (object item in (navigationPropertyValue as IEnumerable) ?? throw new EntityException(Strings.ObjectStateEntry_UnableToEnumerateCollection(navigationProperty.Name, Entity.GetType().FullName)))
					{
						if (item != null)
						{
							hashSet.Add(item);
						}
					}
				}
				else
				{
					hashSet.Add(navigationPropertyValue);
				}
			}
			foreach (object item2 in relatedEndInternal.GetInternalEnumerable())
			{
				if (!hashSet.Contains(item2))
				{
					AddRelationshipDetectedByGraph(base.ObjectStateManager.TransactionManager.DeletedRelationshipsByGraph, item2, relatedEndInternal, verifyForAdd: false);
				}
				else
				{
					hashSet.Remove(item2);
				}
			}
			foreach (object item3 in hashSet)
			{
				AddRelationshipDetectedByGraph(base.ObjectStateManager.TransactionManager.AddedRelationshipsByGraph, item3, relatedEndInternal, verifyForAdd: true);
			}
		}
	}

	private void AddRelationshipDetectedByGraph(Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> relationships, object relatedObject, RelatedEnd relatedEndFrom, bool verifyForAdd)
	{
		IEntityWrapper entityWrapper = base.ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(relatedObject, base.ObjectStateManager);
		AddDetectedRelationship(relationships, entityWrapper, relatedEndFrom);
		RelatedEnd otherEndOfRelationship = relatedEndFrom.GetOtherEndOfRelationship(entityWrapper);
		if (verifyForAdd && otherEndOfRelationship is EntityReference && base.ObjectStateManager.FindEntityEntry(relatedObject) == null)
		{
			otherEndOfRelationship.VerifyNavigationPropertyForAdd(_wrappedEntity);
		}
		AddDetectedRelationship(relationships, _wrappedEntity, otherEndOfRelationship);
	}

	private void AddRelationshipDetectedByForeignKey(Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> relationships, Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> principalRelationships, EntityKey relatedKey, EntityEntry relatedEntry, RelatedEnd relatedEndFrom)
	{
		AddDetectedRelationship(relationships, relatedKey, relatedEndFrom);
		if (relatedEntry != null)
		{
			IEntityWrapper wrappedEntity = relatedEntry.WrappedEntity;
			RelatedEnd otherEndOfRelationship = relatedEndFrom.GetOtherEndOfRelationship(wrappedEntity);
			EntityKey permanentKey = base.ObjectStateManager.GetPermanentKey(relatedEntry.WrappedEntity, otherEndOfRelationship, WrappedEntity);
			AddDetectedRelationship(principalRelationships, permanentKey, otherEndOfRelationship);
		}
	}

	private static void AddDetectedRelationship<T>(Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<T>>> relationships, T relatedObject, RelatedEnd relatedEnd)
	{
		if (!relationships.TryGetValue(relatedEnd.WrappedOwner, out var value))
		{
			value = new Dictionary<RelatedEnd, HashSet<T>>();
			relationships.Add(relatedEnd.WrappedOwner, value);
		}
		if (!value.TryGetValue(relatedEnd, out var value2))
		{
			value2 = new HashSet<T>();
			value.Add(relatedEnd, value2);
		}
		else if (relatedEnd is EntityReference && !object.Equals(value2.First(), relatedObject))
		{
			throw new InvalidOperationException(Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(relatedEnd.RelationshipNavigation.To, relatedEnd.RelationshipNavigation.RelationshipName));
		}
		value2.Add(relatedObject);
	}

	internal void Detach()
	{
		ValidateState();
		bool flag = false;
		RelationshipManager relationshipManager = _wrappedEntity.RelationshipManager;
		flag = base.State != EntityState.Added && IsOneEndOfSomeRelationship();
		_cache.TransactionManager.BeginDetaching();
		try
		{
			relationshipManager.DetachEntityFromRelationships(base.State);
		}
		finally
		{
			_cache.TransactionManager.EndDetaching();
		}
		DetachRelationshipsEntries(relationshipManager);
		IEntityWrapper wrappedEntity = _wrappedEntity;
		EntityKey entityKey = _entityKey;
		EntityState state = base.State;
		if (flag)
		{
			DegradeEntry();
		}
		else
		{
			_wrappedEntity.ObjectStateEntry = null;
			_cache.ChangeState(this, base.State, EntityState.Detached);
		}
		if (state != EntityState.Added)
		{
			wrappedEntity.EntityKey = entityKey;
		}
	}

	internal void Delete(bool doFixup)
	{
		ValidateState();
		if (IsKeyEntry)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotDeleteOnKeyEntry);
		}
		if (doFixup && base.State != EntityState.Deleted)
		{
			RelationshipManager.NullAllFKsInDependentsForWhichThisIsThePrincipal();
			NullAllForeignKeys();
			FixupRelationships();
		}
		switch (base.State)
		{
		case EntityState.Added:
			_cache.ChangeState(this, EntityState.Added, EntityState.Detached);
			break;
		case EntityState.Modified:
			if (!doFixup)
			{
				DeleteRelationshipsThatReferenceKeys(null, null);
			}
			_cache.ChangeState(this, EntityState.Modified, EntityState.Deleted);
			base.State = EntityState.Deleted;
			break;
		case EntityState.Unchanged:
			if (!doFixup)
			{
				DeleteRelationshipsThatReferenceKeys(null, null);
			}
			_cache.ChangeState(this, EntityState.Unchanged, EntityState.Deleted);
			base.State = EntityState.Deleted;
			break;
		}
	}

	private void NullAllForeignKeys()
	{
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			(WrappedEntity.RelationshipManager.GetRelatedEndInternal(foreignKeyDependent.Item1.ElementType.FullName, foreignKeyDependent.Item2.FromRole.Name) as EntityReference).NullAllForeignKeys();
		}
	}

	private bool IsOneEndOfSomeRelationship()
	{
		foreach (RelationshipEntry item in _cache.FindRelationshipsByKey(EntityKey))
		{
			RelationshipMultiplicity relationshipMultiplicity = GetAssociationEndMember(item).RelationshipMultiplicity;
			if (relationshipMultiplicity == RelationshipMultiplicity.One || relationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
			{
				EntityKey otherEntityKey = item.RelationshipWrapper.GetOtherEntityKey(EntityKey);
				if (!_cache.GetEntityEntry(otherEntityKey).IsKeyEntry)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void DetachRelationshipsEntries(RelationshipManager relationshipManager)
	{
		RelationshipEntry[] array = _cache.CopyOfRelationshipsByKey(EntityKey);
		foreach (RelationshipEntry relationshipEntry in array)
		{
			EntityKey otherEntityKey = relationshipEntry.RelationshipWrapper.GetOtherEntityKey(EntityKey);
			if (_cache.GetEntityEntry(otherEntityKey).IsKeyEntry)
			{
				if (relationshipEntry.State != EntityState.Deleted)
				{
					AssociationEndMember associationEndMember = relationshipEntry.RelationshipWrapper.GetAssociationEndMember(otherEntityKey);
					((EntityReference)relationshipManager.GetRelatedEndInternal(associationEndMember.DeclaringType.FullName, associationEndMember.Name)).DetachedEntityKey = otherEntityKey;
				}
				relationshipEntry.DeleteUnnecessaryKeyEntries();
				relationshipEntry.DetachRelationshipEntry();
			}
			else if (relationshipEntry.State == EntityState.Deleted && GetAssociationEndMember(relationshipEntry).RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				relationshipEntry.DetachRelationshipEntry();
			}
		}
	}

	private void FixupRelationships()
	{
		_wrappedEntity.RelationshipManager.RemoveEntityFromRelationships();
		DeleteRelationshipsThatReferenceKeys(null, null);
	}

	internal void DeleteRelationshipsThatReferenceKeys(RelationshipSet relationshipSet, RelationshipEndMember endMember)
	{
		if (base.State == EntityState.Detached)
		{
			return;
		}
		RelationshipEntry[] array = _cache.CopyOfRelationshipsByKey(EntityKey);
		foreach (RelationshipEntry relationshipEntry in array)
		{
			if (relationshipEntry.State == EntityState.Deleted || (relationshipSet != null && relationshipSet != relationshipEntry.EntitySet))
			{
				continue;
			}
			EntityEntry otherEndOfRelationship = GetOtherEndOfRelationship(relationshipEntry);
			if (endMember != null && endMember != otherEndOfRelationship.GetAssociationEndMember(relationshipEntry))
			{
				continue;
			}
			for (int j = 0; j < 2; j++)
			{
				if (relationshipEntry.GetCurrentRelationValue(j) is EntityKey key && _cache.GetEntityEntry(key).IsKeyEntry)
				{
					relationshipEntry.Delete(doFixup: false);
					break;
				}
			}
		}
	}

	private bool RetrieveAndCheckReferentialConstraintValuesInAcceptChanges()
	{
		RelationshipManager relationshipManager = _wrappedEntity.RelationshipManager;
		List<string> propertiesToRetrieve;
		bool propertiesToPropagateExist;
		bool result = relationshipManager.FindNamesOfReferentialConstraintProperties(out propertiesToRetrieve, out propertiesToPropagateExist, skipFK: true);
		if (propertiesToRetrieve != null)
		{
			HashSet<object> visited = new HashSet<object>();
			relationshipManager.RetrieveReferentialConstraintProperties(out var properties, visited, includeOwnValues: false);
			foreach (KeyValuePair<string, KeyValuePair<object, IntBox>> item in properties)
			{
				SetCurrentEntityValue(item.Key, item.Value.Key);
			}
		}
		if (propertiesToPropagateExist)
		{
			CheckReferentialConstraintPropertiesInDependents();
		}
		return result;
	}

	internal void RetrieveReferentialConstraintPropertiesFromKeyEntries(Dictionary<string, KeyValuePair<object, IntBox>> properties)
	{
		foreach (RelationshipEntry item in _cache.FindRelationshipsByKey(EntityKey))
		{
			EntityEntry otherEndOfRelationship = GetOtherEndOfRelationship(item);
			if (!otherEndOfRelationship.IsKeyEntry)
			{
				continue;
			}
			foreach (ReferentialConstraint referentialConstraint in ((AssociationSet)item.EntitySet).ElementType.ReferentialConstraints)
			{
				string name = GetAssociationEndMember(item).Name;
				if (!(referentialConstraint.ToRole.Name == name))
				{
					continue;
				}
				foreach (EntityKeyMember item2 in (IEnumerable<EntityKeyMember>)otherEndOfRelationship.EntityKey.EntityKeyValues)
				{
					for (int i = 0; i < referentialConstraint.FromProperties.Count; i++)
					{
						if (referentialConstraint.FromProperties[i].Name == item2.Key)
						{
							AddOrIncreaseCounter(referentialConstraint, properties, referentialConstraint.ToProperties[i].Name, item2.Value);
						}
					}
				}
			}
		}
	}

	internal static void AddOrIncreaseCounter(ReferentialConstraint constraint, Dictionary<string, KeyValuePair<object, IntBox>> properties, string propertyName, object propertyValue)
	{
		if (properties.ContainsKey(propertyName))
		{
			KeyValuePair<object, IntBox> keyValuePair = properties[propertyName];
			if (!ByValueEqualityComparer.Default.Equals(keyValuePair.Key, propertyValue))
			{
				throw new InvalidOperationException(constraint.BuildConstraintExceptionMessage());
			}
			keyValuePair.Value.Value = keyValuePair.Value.Value + 1;
		}
		else
		{
			properties[propertyName] = new KeyValuePair<object, IntBox>(propertyValue, new IntBox(1));
		}
	}

	private void CheckReferentialConstraintPropertiesInDependents()
	{
		foreach (RelationshipEntry item in _cache.FindRelationshipsByKey(EntityKey))
		{
			EntityEntry otherEndOfRelationship = GetOtherEndOfRelationship(item);
			if (otherEndOfRelationship.State != EntityState.Unchanged && otherEndOfRelationship.State != EntityState.Modified)
			{
				continue;
			}
			foreach (ReferentialConstraint referentialConstraint in ((AssociationSet)item.EntitySet).ElementType.ReferentialConstraints)
			{
				string name = GetAssociationEndMember(item).Name;
				if (!(referentialConstraint.FromRole.Name == name))
				{
					continue;
				}
				foreach (EntityKeyMember item2 in (IEnumerable<EntityKeyMember>)otherEndOfRelationship.EntityKey.EntityKeyValues)
				{
					for (int i = 0; i < referentialConstraint.FromProperties.Count; i++)
					{
						if (referentialConstraint.ToProperties[i].Name == item2.Key && !ByValueEqualityComparer.Default.Equals(GetCurrentEntityValue(referentialConstraint.FromProperties[i].Name), item2.Value))
						{
							throw new InvalidOperationException(referentialConstraint.BuildConstraintExceptionMessage());
						}
					}
				}
			}
		}
	}

	internal void PromoteKeyEntry(IEntityWrapper wrappedEntity, StateManagerTypeMetadata typeMetadata)
	{
		_wrappedEntity = wrappedEntity;
		_wrappedEntity.ObjectStateEntry = this;
		_cacheTypeMetadata = typeMetadata;
		SetChangeTrackingFlags();
	}

	internal void DegradeEntry()
	{
		_entityKey = EntityKey;
		RemoveFromForeignKeyIndex();
		_wrappedEntity.SetChangeTracker(null);
		_modifiedFields = null;
		_originalValues = null;
		_originalComplexObjects = null;
		if (base.State == EntityState.Added)
		{
			_wrappedEntity.EntityKey = null;
			_entityKey = null;
		}
		if (base.State != EntityState.Unchanged)
		{
			_cache.ChangeState(this, base.State, EntityState.Unchanged);
			base.State = EntityState.Unchanged;
		}
		_cache.RemoveEntryFromKeylessStore(_wrappedEntity);
		_wrappedEntity.DetachContext();
		_wrappedEntity.ObjectStateEntry = null;
		object entity = _wrappedEntity.Entity;
		_wrappedEntity = NullEntityWrapper.NullWrapper;
		SetChangeTrackingFlags();
		_cache.OnObjectStateManagerChanged(CollectionChangeAction.Remove, entity);
	}

	internal void AttachObjectStateManagerToEntity()
	{
		_wrappedEntity.SetChangeTracker(this);
		_wrappedEntity.TakeSnapshot(this);
	}

	internal void GetOtherKeyProperties(Dictionary<string, KeyValuePair<object, IntBox>> properties)
	{
		foreach (EdmMember keyMember in (_cacheTypeMetadata.DataRecordInfo.RecordType.EdmType as EntityType).KeyMembers)
		{
			if (!properties.ContainsKey(keyMember.Name))
			{
				properties[keyMember.Name] = new KeyValuePair<object, IntBox>(GetCurrentEntityValue(keyMember.Name), new IntBox(1));
			}
		}
	}

	internal void AddOriginalValueAt(int index, StateManagerMemberMetadata memberMetadata, object userObject, object value)
	{
		StateManagerValue stateManagerValue = new StateManagerValue(memberMetadata, userObject, value);
		if (index >= 0)
		{
			_originalValues[index] = stateManagerValue;
			return;
		}
		if (_originalValues == null)
		{
			_originalValues = new List<StateManagerValue>();
		}
		_originalValues.Add(stateManagerValue);
	}

	internal void CompareKeyProperties(object changed)
	{
		StateManagerTypeMetadata cacheTypeMetadata = _cacheTypeMetadata;
		int fieldCount = GetFieldCount(cacheTypeMetadata);
		for (int i = 0; i < fieldCount; i++)
		{
			StateManagerMemberMetadata stateManagerMemberMetadata = cacheTypeMetadata.Member(i);
			if (stateManagerMemberMetadata.IsPartOfKey)
			{
				object value = stateManagerMemberMetadata.GetValue(changed);
				object value2 = stateManagerMemberMetadata.GetValue(_wrappedEntity.Entity);
				if (!ByValueEqualityComparer.Default.Equals(value, value2))
				{
					throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyProperty(stateManagerMemberMetadata.CLayerName));
				}
			}
		}
	}

	internal object GetCurrentEntityValue(string memberName)
	{
		int ordinalforOLayerMemberName = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(memberName);
		return GetCurrentEntityValue(_cacheTypeMetadata, ordinalforOLayerMemberName, _wrappedEntity.Entity, ObjectStateValueRecord.CurrentUpdatable);
	}

	internal void VerifyEntityValueIsEditable(StateManagerTypeMetadata typeMetadata, int ordinal, string memberName)
	{
		if (base.State == EntityState.Deleted)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyDetachedDeletedEntries);
		}
		if (typeMetadata.Member(ordinal).IsPartOfKey && base.State != EntityState.Added)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyProperty(memberName));
		}
	}

	internal void SetCurrentEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, object newValue)
	{
		ValidateState();
		StateManagerMemberMetadata stateManagerMemberMetadata = metadata.Member(ordinal);
		if (stateManagerMemberMetadata.IsComplex)
		{
			if (newValue == null || newValue == DBNull.Value)
			{
				throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(stateManagerMemberMetadata.CLayerName));
			}
			if (!(newValue is IExtendedDataRecord extendedDataRecord))
			{
				throw new ArgumentException(Strings.ObjectStateEntry_InvalidTypeForComplexTypeProperty, "newValue");
			}
			newValue = _cache.ComplexTypeMaterializer.CreateComplex(extendedDataRecord, extendedDataRecord.DataRecordInfo, null);
		}
		_wrappedEntity.SetCurrentValue(this, stateManagerMemberMetadata, ordinal, userObject, newValue);
	}

	private void TransitionRelationshipsForAdd()
	{
		RelationshipEntry[] array = _cache.CopyOfRelationshipsByKey(EntityKey);
		foreach (RelationshipEntry relationshipEntry in array)
		{
			if (relationshipEntry.State == EntityState.Unchanged)
			{
				base.ObjectStateManager.ChangeState(relationshipEntry, EntityState.Unchanged, EntityState.Added);
				relationshipEntry.State = EntityState.Added;
			}
			else if (relationshipEntry.State == EntityState.Deleted)
			{
				relationshipEntry.DeleteUnnecessaryKeyEntries();
				relationshipEntry.DetachRelationshipEntry();
			}
		}
	}

	[Conditional("DEBUG")]
	private void VerifyIsNotRelated()
	{
	}

	internal void ChangeObjectState(EntityState requestedState)
	{
		if (IsKeyEntry)
		{
			if (requestedState != EntityState.Unchanged)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyEntryState);
			}
			return;
		}
		switch (base.State)
		{
		case EntityState.Added:
			switch (requestedState)
			{
			case EntityState.Added:
				TransitionRelationshipsForAdd();
				break;
			case EntityState.Unchanged:
				AcceptChanges();
				break;
			case EntityState.Modified:
				AcceptChanges();
				SetModified();
				SetModifiedAll();
				break;
			case EntityState.Deleted:
				_cache.ForgetEntryWithConceptualNull(this, resetAllKeys: true);
				AcceptChanges();
				Delete(doFixup: true);
				break;
			case EntityState.Detached:
				Detach();
				break;
			default:
				throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
			}
			break;
		case EntityState.Unchanged:
			switch (requestedState)
			{
			case EntityState.Added:
				base.ObjectStateManager.ReplaceKeyWithTemporaryKey(this);
				_modifiedFields = null;
				_originalValues = null;
				_originalComplexObjects = null;
				base.State = EntityState.Added;
				TransitionRelationshipsForAdd();
				break;
			case EntityState.Modified:
				SetModified();
				SetModifiedAll();
				break;
			case EntityState.Deleted:
				Delete(doFixup: true);
				break;
			case EntityState.Detached:
				Detach();
				break;
			default:
				throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
			case EntityState.Unchanged:
				break;
			}
			break;
		case EntityState.Modified:
			switch (requestedState)
			{
			case EntityState.Added:
				base.ObjectStateManager.ReplaceKeyWithTemporaryKey(this);
				_modifiedFields = null;
				_originalValues = null;
				_originalComplexObjects = null;
				base.State = EntityState.Added;
				TransitionRelationshipsForAdd();
				break;
			case EntityState.Unchanged:
				AcceptChanges();
				break;
			case EntityState.Modified:
				SetModified();
				SetModifiedAll();
				break;
			case EntityState.Deleted:
				Delete(doFixup: true);
				break;
			case EntityState.Detached:
				Detach();
				break;
			default:
				throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
			}
			break;
		case EntityState.Deleted:
			switch (requestedState)
			{
			case EntityState.Added:
				TransitionRelationshipsForAdd();
				base.ObjectStateManager.ReplaceKeyWithTemporaryKey(this);
				_modifiedFields = null;
				_originalValues = null;
				_originalComplexObjects = null;
				base.State = EntityState.Added;
				_cache.FixupReferencesByForeignKeys(this);
				_cache.OnObjectStateManagerChanged(CollectionChangeAction.Add, Entity);
				break;
			case EntityState.Unchanged:
				_modifiedFields = null;
				_originalValues = null;
				_originalComplexObjects = null;
				base.ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Unchanged);
				base.State = EntityState.Unchanged;
				_wrappedEntity.TakeSnapshot(this);
				_cache.FixupReferencesByForeignKeys(this);
				_cache.OnObjectStateManagerChanged(CollectionChangeAction.Add, Entity);
				break;
			case EntityState.Modified:
				base.ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Modified);
				base.State = EntityState.Modified;
				SetModifiedAll();
				_cache.FixupReferencesByForeignKeys(this);
				_cache.OnObjectStateManagerChanged(CollectionChangeAction.Add, Entity);
				break;
			case EntityState.Detached:
				Detach();
				break;
			default:
				throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
			case EntityState.Deleted:
				break;
			}
			break;
		}
	}

	internal void UpdateOriginalValues(object entity)
	{
		EntityState state = base.State;
		UpdateRecordWithSetModified(entity, EditableOriginalValues);
		if (state == EntityState.Unchanged && base.State == EntityState.Modified)
		{
			base.ObjectStateManager.ChangeState(this, state, EntityState.Modified);
		}
	}

	internal void UpdateRecordWithoutSetModified(object value, DbUpdatableDataRecord current)
	{
		UpdateRecord(value, current, UpdateRecordBehavior.WithoutSetModified, -1);
	}

	internal void UpdateRecordWithSetModified(object value, DbUpdatableDataRecord current)
	{
		UpdateRecord(value, current, UpdateRecordBehavior.WithSetModified, -1);
	}

	private void UpdateRecord(object value, DbUpdatableDataRecord current, UpdateRecordBehavior behavior, int propertyIndex)
	{
		StateManagerTypeMetadata metadata = current._metadata;
		foreach (FieldMetadata fieldMetadatum in metadata.DataRecordInfo.FieldMetadata)
		{
			int ordinal = fieldMetadatum.Ordinal;
			StateManagerMemberMetadata stateManagerMemberMetadata = metadata.Member(ordinal);
			object obj = stateManagerMemberMetadata.GetValue(value) ?? DBNull.Value;
			if (Helper.IsComplexType(fieldMetadatum.FieldType.TypeUsage.EdmType))
			{
				object value2 = current.GetValue(ordinal);
				if (value2 == DBNull.Value)
				{
					throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(fieldMetadatum.FieldType.Name));
				}
				if (obj != DBNull.Value)
				{
					UpdateRecord(obj, (DbUpdatableDataRecord)value2, behavior, (propertyIndex == -1) ? ordinal : propertyIndex);
				}
			}
			else if (HasRecordValueChanged(current, ordinal, obj) && !stateManagerMemberMetadata.IsPartOfKey)
			{
				current.SetValue(ordinal, obj);
				if (behavior == UpdateRecordBehavior.WithSetModified)
				{
					SetModifiedPropertyInternal((propertyIndex == -1) ? ordinal : propertyIndex);
				}
			}
		}
	}

	internal bool HasRecordValueChanged(DbDataRecord record, int propertyIndex, object newFieldValue)
	{
		object value = record.GetValue(propertyIndex);
		if (value == newFieldValue || (DBNull.Value != newFieldValue && DBNull.Value != value && ByValueEqualityComparer.Default.Equals(value, newFieldValue)))
		{
			if (_cache.EntryHasConceptualNull(this) && _modifiedFields != null)
			{
				return _modifiedFields[propertyIndex];
			}
			return false;
		}
		return true;
	}

	internal void ApplyCurrentValuesInternal(IEntityWrapper wrappedCurrentEntity)
	{
		if (base.State != EntityState.Modified && base.State != EntityState.Unchanged)
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntityMustBeUnchangedOrModified(base.State.ToString()));
		}
		if (WrappedEntity.IdentityType != wrappedCurrentEntity.IdentityType)
		{
			throw new ArgumentException(Strings.ObjectContext_EntitiesHaveDifferentType(Entity.GetType().FullName, wrappedCurrentEntity.Entity.GetType().FullName));
		}
		CompareKeyProperties(wrappedCurrentEntity.Entity);
		UpdateCurrentValueRecord(wrappedCurrentEntity.Entity);
	}

	internal void UpdateCurrentValueRecord(object value)
	{
		_wrappedEntity.UpdateCurrentValueRecord(value, this);
	}

	internal void ApplyOriginalValuesInternal(IEntityWrapper wrappedOriginalEntity)
	{
		if (base.State != EntityState.Modified && base.State != EntityState.Unchanged && base.State != EntityState.Deleted)
		{
			throw new InvalidOperationException(Strings.ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted(base.State.ToString()));
		}
		if (WrappedEntity.IdentityType != wrappedOriginalEntity.IdentityType)
		{
			throw new ArgumentException(Strings.ObjectContext_EntitiesHaveDifferentType(Entity.GetType().FullName, wrappedOriginalEntity.Entity.GetType().FullName));
		}
		CompareKeyProperties(wrappedOriginalEntity.Entity);
		UpdateOriginalValues(wrappedOriginalEntity.Entity);
	}

	internal void RemoveFromForeignKeyIndex()
	{
		if (IsKeyEntry)
		{
			return;
		}
		foreach (EntityReference item in FindFKRelatedEnds())
		{
			foreach (EntityKey allKeyValue in item.GetAllKeyValues())
			{
				_cache.RemoveEntryFromForeignKeyIndex(item, allKeyValue, this);
			}
		}
	}

	internal void FixupReferencesByForeignKeys(bool replaceAddedRefs, EntitySetBase restrictTo = null)
	{
		_cache.TransactionManager.BeginGraphUpdate();
		bool setIsLoaded = !_cache.TransactionManager.IsAttachTracking && !_cache.TransactionManager.IsAddTracking;
		try
		{
			foreach (Tuple<AssociationSet, ReferentialConstraint> item in ForeignKeyDependents.Where((Tuple<AssociationSet, ReferentialConstraint> t) => restrictTo == null || t.Item1.SourceSet.Identity == restrictTo.Identity || t.Item1.TargetSet.Identity == restrictTo.Identity))
			{
				EntityReference entityReference = WrappedEntity.RelationshipManager.GetRelatedEndInternal(item.Item1.ElementType, (AssociationEndMember)item.Item2.FromRole) as EntityReference;
				if (!ForeignKeyFactory.IsConceptualNullKey(entityReference.CachedForeignKey))
				{
					FixupEntityReferenceToPrincipal(entityReference, null, setIsLoaded, replaceAddedRefs);
				}
			}
		}
		finally
		{
			_cache.TransactionManager.EndGraphUpdate();
		}
	}

	internal void FixupEntityReferenceByForeignKey(EntityReference reference)
	{
		reference.IsLoaded = false;
		if (ForeignKeyFactory.IsConceptualNullKey(reference.CachedForeignKey))
		{
			base.ObjectStateManager.ForgetEntryWithConceptualNull(this, resetAllKeys: false);
		}
		IEntityWrapper referenceValue = reference.ReferenceValue;
		EntityKey entityKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, reference);
		bool flag;
		if ((object)entityKey == null || referenceValue.Entity == null)
		{
			flag = true;
		}
		else
		{
			EntityKey entityKey2 = referenceValue.EntityKey;
			EntityEntry objectStateEntry = referenceValue.ObjectStateEntry;
			if ((entityKey2 == null || entityKey2.IsTemporary) && objectStateEntry != null)
			{
				entityKey2 = new EntityKey((EntitySet)objectStateEntry.EntitySet, objectStateEntry.CurrentValues);
			}
			flag = !entityKey.Equals(entityKey2);
		}
		if (_cache.TransactionManager.RelationshipBeingUpdated != reference)
		{
			if (flag)
			{
				_cache.TransactionManager.BeginGraphUpdate();
				if ((object)entityKey != null)
				{
					_cache.TransactionManager.EntityBeingReparented = Entity;
				}
				try
				{
					FixupEntityReferenceToPrincipal(reference, entityKey, setIsLoaded: false, replaceExistingRef: true);
				}
				finally
				{
					_cache.TransactionManager.EntityBeingReparented = null;
					_cache.TransactionManager.EndGraphUpdate();
				}
			}
		}
		else
		{
			FixupEntityReferenceToPrincipal(reference, entityKey, setIsLoaded: false, replaceExistingRef: false);
		}
	}

	internal void FixupEntityReferenceToPrincipal(EntityReference relatedEnd, EntityKey foreignKey, bool setIsLoaded, bool replaceExistingRef)
	{
		if (foreignKey == null)
		{
			foreignKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, relatedEnd);
		}
		bool flag = _cache.TransactionManager.RelationshipBeingUpdated != relatedEnd && (!_cache.TransactionManager.IsForeignKeyUpdate || relatedEnd.ReferenceValue.ObjectStateEntry == null || relatedEnd.ReferenceValue.ObjectStateEntry.State != EntityState.Added);
		relatedEnd.SetCachedForeignKey(foreignKey, this);
		base.ObjectStateManager.ForgetEntryWithConceptualNull(this, resetAllKeys: false);
		if (foreignKey != null)
		{
			if (_cache.TryGetEntityEntry(foreignKey, out var entry) && !entry.IsKeyEntry && entry.State != EntityState.Deleted && (replaceExistingRef || WillNotRefSteal(relatedEnd, entry.WrappedEntity)) && relatedEnd.CanSetEntityType(entry.WrappedEntity))
			{
				if (flag)
				{
					if (_cache.TransactionManager.PopulatedEntityReferences != null)
					{
						_cache.TransactionManager.PopulatedEntityReferences.Add(relatedEnd);
					}
					relatedEnd.SetEntityKey(foreignKey, forceFixup: true);
					if (_cache.TransactionManager.PopulatedEntityReferences != null && relatedEnd.GetOtherEndOfRelationship(entry.WrappedEntity) is EntityReference item)
					{
						_cache.TransactionManager.PopulatedEntityReferences.Add(item);
					}
				}
				if (setIsLoaded && entry.State != EntityState.Added)
				{
					relatedEnd.IsLoaded = true;
				}
			}
			else
			{
				_cache.AddEntryContainingForeignKeyToIndex(relatedEnd, foreignKey, this);
				if (flag && replaceExistingRef && relatedEnd.ReferenceValue.Entity != null)
				{
					relatedEnd.ReferenceValue = NullEntityWrapper.NullWrapper;
				}
			}
		}
		else if (flag)
		{
			if (replaceExistingRef && (relatedEnd.ReferenceValue.Entity != null || relatedEnd.EntityKey != null))
			{
				relatedEnd.ReferenceValue = NullEntityWrapper.NullWrapper;
			}
			if (setIsLoaded)
			{
				relatedEnd.IsLoaded = true;
			}
		}
	}

	private static bool WillNotRefSteal(EntityReference refToPrincipal, IEntityWrapper wrappedPrincipal)
	{
		EntityReference entityReference = refToPrincipal.GetOtherEndOfRelationship(wrappedPrincipal) as EntityReference;
		if (refToPrincipal.ReferenceValue.Entity == null && refToPrincipal.NavigationPropertyIsNullOrMissing() && (entityReference == null || (entityReference.ReferenceValue.Entity == null && entityReference.NavigationPropertyIsNullOrMissing())))
		{
			return true;
		}
		if (entityReference != null && (entityReference.ReferenceValue.Entity == refToPrincipal.WrappedOwner.Entity || entityReference.CheckIfNavigationPropertyContainsEntity(refToPrincipal.WrappedOwner)))
		{
			return true;
		}
		if (entityReference == null || refToPrincipal.ReferenceValue.Entity == wrappedPrincipal.Entity || refToPrincipal.CheckIfNavigationPropertyContainsEntity(wrappedPrincipal))
		{
			return false;
		}
		throw new InvalidOperationException(Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(entityReference.RelationshipNavigation.To, entityReference.RelationshipNavigation.RelationshipName));
	}

	internal bool TryGetReferenceKey(AssociationEndMember principalRole, out EntityKey principalKey)
	{
		EntityReference entityReference = RelationshipManager.GetRelatedEnd(principalRole.DeclaringType.FullName, principalRole.Name) as EntityReference;
		if (entityReference.CachedValue.Entity == null || entityReference.CachedValue.ObjectStateEntry == null)
		{
			principalKey = null;
			return false;
		}
		principalKey = entityReference.EntityKey ?? entityReference.CachedValue.ObjectStateEntry.EntityKey;
		return principalKey != null;
	}

	internal void FixupForeignKeysByReference()
	{
		_cache.TransactionManager.BeginFixupKeysByReference();
		try
		{
			FixupForeignKeysByReference(null);
		}
		finally
		{
			_cache.TransactionManager.EndFixupKeysByReference();
		}
	}

	private void FixupForeignKeysByReference(List<EntityEntry> visited)
	{
		if (!(base.EntitySet as EntitySet).HasForeignKeyRelationships)
		{
			return;
		}
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			EntityReference entityReference = RelationshipManager.GetRelatedEndInternal(foreignKeyDependent.Item1.ElementType.FullName, foreignKeyDependent.Item2.FromRole.Name) as EntityReference;
			IEntityWrapper referenceValue = entityReference.ReferenceValue;
			if (referenceValue.Entity != null)
			{
				EntityEntry objectStateEntry = referenceValue.ObjectStateEntry;
				bool? flag = null;
				if (objectStateEntry != null && objectStateEntry.State == EntityState.Added)
				{
					if (objectStateEntry == this)
					{
						flag = entityReference.GetOtherEndOfRelationship(referenceValue) is EntityReference;
						bool? flag2 = flag;
						if (!flag2.Value)
						{
							goto IL_0119;
						}
					}
					visited = visited ?? new List<EntityEntry>();
					if (visited.Contains(this))
					{
						if (!flag.HasValue)
						{
							flag = entityReference.GetOtherEndOfRelationship(referenceValue) is EntityReference;
						}
						if (flag.Value)
						{
							throw new InvalidOperationException(Strings.RelationshipManager_CircularRelationshipsWithReferentialConstraints);
						}
					}
					else
					{
						visited.Add(this);
						objectStateEntry.FixupForeignKeysByReference(visited);
						visited.Remove(this);
					}
				}
				goto IL_0119;
			}
			EntityKey entityKey = entityReference.EntityKey;
			if (entityKey != null && !entityKey.IsTemporary)
			{
				entityReference.UpdateForeignKeyValues(WrappedEntity, entityKey);
			}
			continue;
			IL_0119:
			entityReference.UpdateForeignKeyValues(WrappedEntity, referenceValue, null, forceChange: false);
		}
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyPrincipal in ForeignKeyPrincipals)
		{
			bool flag3 = false;
			bool flag4 = false;
			RelatedEnd relatedEndInternal = RelationshipManager.GetRelatedEndInternal(foreignKeyPrincipal.Item1.ElementType.FullName, foreignKeyPrincipal.Item2.ToRole.Name);
			foreach (IEntityWrapper wrappedEntity in relatedEndInternal.GetWrappedEntities())
			{
				EntityEntry objectStateEntry2 = wrappedEntity.ObjectStateEntry;
				if (objectStateEntry2.State != EntityState.Added && !flag4)
				{
					flag4 = true;
					foreach (EdmProperty toProperty in foreignKeyPrincipal.Item2.ToProperties)
					{
						int ordinalforOLayerMemberName = objectStateEntry2._cacheTypeMetadata.GetOrdinalforOLayerMemberName(toProperty.Name);
						if (objectStateEntry2._cacheTypeMetadata.Member(ordinalforOLayerMemberName).IsPartOfKey)
						{
							flag3 = true;
							break;
						}
					}
				}
				if (objectStateEntry2.State == EntityState.Added || (objectStateEntry2.State == EntityState.Modified && !flag3))
				{
					(relatedEndInternal.GetOtherEndOfRelationship(wrappedEntity) as EntityReference).UpdateForeignKeyValues(wrappedEntity, WrappedEntity, null, forceChange: false);
				}
			}
		}
	}

	private bool IsPropertyAForeignKey(string propertyName)
	{
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			foreach (EdmProperty toProperty in foreignKeyDependent.Item2.ToProperties)
			{
				if (toProperty.Name == propertyName)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsPropertyAForeignKey(string propertyName, out List<Pair<string, string>> relationships)
	{
		relationships = null;
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			foreach (EdmProperty toProperty in foreignKeyDependent.Item2.ToProperties)
			{
				if (toProperty.Name == propertyName)
				{
					if (relationships == null)
					{
						relationships = new List<Pair<string, string>>();
					}
					relationships.Add(new Pair<string, string>(foreignKeyDependent.Item1.ElementType.FullName, foreignKeyDependent.Item2.FromRole.Name));
					break;
				}
			}
		}
		return relationships != null;
	}

	internal void FindRelatedEntityKeysByForeignKeys(out Dictionary<RelatedEnd, HashSet<EntityKey>> relatedEntities, bool useOriginalValues)
	{
		relatedEntities = null;
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			AssociationSet item = foreignKeyDependent.Item1;
			ReferentialConstraint item2 = foreignKeyDependent.Item2;
			string identity = item2.ToRole.Identity;
			ReadOnlyMetadataCollection<AssociationSetEnd> associationSetEnds = item.AssociationSetEnds;
			AssociationEndMember endMember = ((!(associationSetEnds[0].CorrespondingAssociationEndMember.Identity == identity)) ? associationSetEnds[0].CorrespondingAssociationEndMember : associationSetEnds[1].CorrespondingAssociationEndMember);
			EntitySet entitySetAtEnd = MetadataHelper.GetEntitySetAtEnd(item, endMember);
			EntityKey entityKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, item2, entitySetAtEnd, useOriginalValues);
			if (entityKey != null)
			{
				EntityReference key = RelationshipManager.GetRelatedEndInternal(item.ElementType, (AssociationEndMember)item2.FromRole) as EntityReference;
				relatedEntities = ((relatedEntities != null) ? relatedEntities : new Dictionary<RelatedEnd, HashSet<EntityKey>>());
				if (!relatedEntities.TryGetValue(key, out var value))
				{
					value = new HashSet<EntityKey>();
					relatedEntities.Add(key, value);
				}
				value.Add(entityKey);
			}
		}
	}

	internal IEnumerable<EntityReference> FindFKRelatedEnds()
	{
		HashSet<EntityReference> hashSet = new HashSet<EntityReference>();
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in ForeignKeyDependents)
		{
			EntityReference item = RelationshipManager.GetRelatedEndInternal(foreignKeyDependent.Item1.ElementType.FullName, foreignKeyDependent.Item2.FromRole.Name) as EntityReference;
			hashSet.Add(item);
		}
		return hashSet;
	}

	internal void DetectChangesInForeignKeys()
	{
		TransactionManager transactionManager = base.ObjectStateManager.TransactionManager;
		foreach (EntityReference item in FindFKRelatedEnds())
		{
			EntityKey entityKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, item);
			EntityKey cachedForeignKey = item.CachedForeignKey;
			bool flag = ForeignKeyFactory.IsConceptualNullKey(cachedForeignKey);
			if (!(cachedForeignKey != null) && !(entityKey != null))
			{
				continue;
			}
			if (cachedForeignKey == null)
			{
				base.ObjectStateManager.TryGetEntityEntry(entityKey, out var entry);
				AddRelationshipDetectedByForeignKey(transactionManager.AddedRelationshipsByForeignKey, transactionManager.AddedRelationshipsByPrincipalKey, entityKey, entry, item);
			}
			else if (entityKey == null)
			{
				AddDetectedRelationship(transactionManager.DeletedRelationshipsByForeignKey, cachedForeignKey, item);
			}
			else if (!entityKey.Equals(cachedForeignKey) && (!flag || ForeignKeyFactory.IsConceptualNullKeyChanged(cachedForeignKey, entityKey)))
			{
				base.ObjectStateManager.TryGetEntityEntry(entityKey, out var entry2);
				AddRelationshipDetectedByForeignKey(transactionManager.AddedRelationshipsByForeignKey, transactionManager.AddedRelationshipsByPrincipalKey, entityKey, entry2, item);
				if (!flag)
				{
					AddDetectedRelationship(transactionManager.DeletedRelationshipsByForeignKey, cachedForeignKey, item);
				}
			}
		}
	}
}
