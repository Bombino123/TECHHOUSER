using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;

namespace System.Data.Entity.Core.Objects;

internal sealed class RelationshipEntry : ObjectStateEntry
{
	internal RelationshipWrapper _relationshipWrapper;

	internal EntityKey Key0 => RelationshipWrapper.Key0;

	internal EntityKey Key1 => RelationshipWrapper.Key1;

	internal override BitArray ModifiedProperties => null;

	public override bool IsRelationship
	{
		get
		{
			ValidateState();
			return true;
		}
	}

	public override object Entity
	{
		get
		{
			ValidateState();
			return null;
		}
	}

	public override EntityKey EntityKey
	{
		get
		{
			ValidateState();
			return null;
		}
		internal set
		{
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public override DbDataRecord OriginalValues
	{
		get
		{
			ValidateState();
			if (base.State == EntityState.Added)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
			}
			return new ObjectStateEntryDbDataRecord(this);
		}
	}

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
			return new ObjectStateEntryDbUpdatableDataRecord(this);
		}
	}

	public override RelationshipManager RelationshipManager
	{
		get
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers);
		}
	}

	internal override bool IsKeyEntry => false;

	internal RelationshipWrapper RelationshipWrapper
	{
		get
		{
			return _relationshipWrapper;
		}
		set
		{
			_relationshipWrapper = value;
		}
	}

	internal RelationshipEntry NextKey0 { get; set; }

	internal RelationshipEntry NextKey1 { get; set; }

	internal RelationshipEntry(ObjectStateManager cache, EntityState state, RelationshipWrapper relationshipWrapper)
		: base(cache, null, state)
	{
		_entitySet = relationshipWrapper.AssociationSet;
		_relationshipWrapper = relationshipWrapper;
	}

	public override void AcceptChanges()
	{
		ValidateState();
		EntityState state = base.State;
		if (state <= EntityState.Added)
		{
			if (state != EntityState.Unchanged && state == EntityState.Added)
			{
				_cache.ChangeState(this, EntityState.Added, EntityState.Unchanged);
				base.State = EntityState.Unchanged;
			}
		}
		else if (state != EntityState.Deleted)
		{
			_ = 16;
		}
		else
		{
			DeleteUnnecessaryKeyEntries();
			if (_cache != null)
			{
				_cache.ChangeState(this, EntityState.Deleted, EntityState.Detached);
			}
		}
	}

	public override void Delete()
	{
		Delete(doFixup: true);
	}

	public override IEnumerable<string> GetModifiedProperties()
	{
		ValidateState();
		yield break;
	}

	public override void SetModified()
	{
		ValidateState();
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
	}

	public override void SetModifiedProperty(string propertyName)
	{
		ValidateState();
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
	}

	public override void RejectPropertyChanges(string propertyName)
	{
		ValidateState();
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
	}

	public override bool IsPropertyChanged(string propertyName)
	{
		ValidateState();
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
	}

	public override OriginalValueRecord GetUpdatableOriginalValues()
	{
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	public override void ChangeState(EntityState state)
	{
		EntityUtil.CheckValidStateForChangeRelationshipState(state, "state");
		if (base.State != EntityState.Detached || state != EntityState.Detached)
		{
			ValidateState();
			if (RelationshipWrapper.Key0 == Key0)
			{
				base.ObjectStateManager.ChangeRelationshipState(Key0, Key1, RelationshipWrapper.AssociationSet.ElementType.FullName, RelationshipWrapper.AssociationEndMembers[1].Name, state);
			}
			else
			{
				base.ObjectStateManager.ChangeRelationshipState(Key0, Key1, RelationshipWrapper.AssociationSet.ElementType.FullName, RelationshipWrapper.AssociationEndMembers[0].Name, state);
			}
		}
	}

	public override void ApplyCurrentValues(object currentEntity)
	{
		Check.NotNull(currentEntity, "currentEntity");
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	public override void ApplyOriginalValues(object originalEntity)
	{
		Check.NotNull(originalEntity, "originalEntity");
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	internal override int GetFieldCount(StateManagerTypeMetadata metadata)
	{
		return _relationshipWrapper.AssociationEndMembers.Count;
	}

	internal override DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject)
	{
		return new DataRecordInfo(TypeUsage.Create(((RelationshipSet)base.EntitySet).ElementType));
	}

	internal override void SetModifiedAll()
	{
		ValidateState();
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
	}

	internal override Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata)
	{
		return typeof(EntityKey);
	}

	internal override string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata)
	{
		ValidateRelationshipRange(ordinal);
		return _relationshipWrapper.AssociationEndMembers[ordinal].Name;
	}

	internal override int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata)
	{
		ReadOnlyMetadataCollection<AssociationEndMember> associationEndMembers = _relationshipWrapper.AssociationEndMembers;
		if (associationEndMembers.TryGetValue(name, ignoreCase: false, out var item))
		{
			return associationEndMembers.IndexOf(item);
		}
		return -1;
	}

	internal override void RevertDelete()
	{
		base.State = EntityState.Unchanged;
		_cache.ChangeState(this, EntityState.Deleted, base.State);
	}

	internal override void EntityMemberChanging(string entityMemberName)
	{
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	internal override void EntityMemberChanged(string entityMemberName)
	{
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	internal override void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	internal override void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
	}

	internal bool IsSameAssociationSetAndRole(AssociationSet associationSet, AssociationEndMember associationMember, EntityKey entityKey)
	{
		if (_entitySet != associationSet)
		{
			return false;
		}
		if (_relationshipWrapper.AssociationSet.ElementType.AssociationEndMembers[0].Name == associationMember.Name)
		{
			return entityKey == Key0;
		}
		return entityKey == Key1;
	}

	private object GetCurrentRelationValue(int ordinal, bool throwException)
	{
		ValidateRelationshipRange(ordinal);
		ValidateState();
		if (base.State == EntityState.Deleted && throwException)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CurrentValuesDoesNotExist);
		}
		return _relationshipWrapper.GetEntityKey(ordinal);
	}

	private static void ValidateRelationshipRange(int ordinal)
	{
		if (1u < (uint)ordinal)
		{
			throw new ArgumentOutOfRangeException("ordinal");
		}
	}

	internal object GetCurrentRelationValue(int ordinal)
	{
		return GetCurrentRelationValue(ordinal, throwException: true);
	}

	internal override void Reset()
	{
		_relationshipWrapper = null;
		base.Reset();
	}

	internal void ChangeRelatedEnd(EntityKey oldKey, EntityKey newKey)
	{
		if (oldKey.Equals(Key0))
		{
			if (oldKey.Equals(Key1))
			{
				RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper.AssociationSet, newKey);
			}
			else
			{
				RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper, 0, newKey);
			}
		}
		else
		{
			RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper, 1, newKey);
		}
	}

	internal void DeleteUnnecessaryKeyEntries()
	{
		for (int i = 0; i < 2; i++)
		{
			EntityKey key = GetCurrentRelationValue(i, throwException: false) as EntityKey;
			EntityEntry entityEntry = _cache.GetEntityEntry(key);
			if (!entityEntry.IsKeyEntry)
			{
				continue;
			}
			bool flag = false;
			foreach (RelationshipEntry item in _cache.FindRelationshipsByKey(key))
			{
				if (item != this)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_cache.DeleteKeyEntry(entityEntry);
				break;
			}
		}
	}

	internal void Delete(bool doFixup)
	{
		ValidateState();
		if (doFixup)
		{
			if (base.State == EntityState.Deleted)
			{
				return;
			}
			EntityEntry entityEntry = _cache.GetEntityEntry((EntityKey)GetCurrentRelationValue(0));
			IEntityWrapper wrappedEntity = entityEntry.WrappedEntity;
			EntityEntry entityEntry2 = _cache.GetEntityEntry((EntityKey)GetCurrentRelationValue(1));
			IEntityWrapper wrappedEntity2 = entityEntry2.WrappedEntity;
			if (wrappedEntity.Entity != null && wrappedEntity2.Entity != null)
			{
				string name = _relationshipWrapper.AssociationEndMembers[1].Name;
				string fullName = ((AssociationSet)_entitySet).ElementType.FullName;
				wrappedEntity.RelationshipManager.RemoveEntity(name, fullName, wrappedEntity2);
				return;
			}
			EntityKey entityKey = null;
			RelationshipManager relationshipManager = null;
			if (wrappedEntity.Entity == null)
			{
				entityKey = entityEntry.EntityKey;
				relationshipManager = wrappedEntity2.RelationshipManager;
			}
			else
			{
				entityKey = entityEntry2.EntityKey;
				relationshipManager = wrappedEntity.RelationshipManager;
			}
			AssociationEndMember associationEndMember = RelationshipWrapper.GetAssociationEndMember(entityKey);
			((EntityReference)relationshipManager.GetRelatedEndInternal(associationEndMember.DeclaringType.FullName, associationEndMember.Name)).DetachedEntityKey = null;
			if (base.State == EntityState.Added)
			{
				DeleteUnnecessaryKeyEntries();
				DetachRelationshipEntry();
			}
			else
			{
				_cache.ChangeState(this, base.State, EntityState.Deleted);
				base.State = EntityState.Deleted;
			}
		}
		else
		{
			switch (base.State)
			{
			default:
				_ = 16;
				break;
			case EntityState.Added:
				DeleteUnnecessaryKeyEntries();
				DetachRelationshipEntry();
				break;
			case EntityState.Unchanged:
				_cache.ChangeState(this, EntityState.Unchanged, EntityState.Deleted);
				base.State = EntityState.Deleted;
				break;
			}
		}
	}

	internal object GetOriginalRelationValue(int ordinal)
	{
		return GetCurrentRelationValue(ordinal, throwException: false);
	}

	internal void DetachRelationshipEntry()
	{
		if (_cache != null)
		{
			_cache.ChangeState(this, base.State, EntityState.Detached);
		}
	}

	internal void ChangeRelationshipState(EntityEntry targetEntry, RelatedEnd relatedEnd, EntityState requestedState)
	{
		switch (base.State)
		{
		case EntityState.Added:
			switch (requestedState)
			{
			case EntityState.Unchanged:
				AcceptChanges();
				break;
			case EntityState.Deleted:
				AcceptChanges();
				Delete();
				break;
			case EntityState.Detached:
				Delete();
				break;
			}
			break;
		case EntityState.Unchanged:
			switch (requestedState)
			{
			case EntityState.Added:
				base.ObjectStateManager.ChangeState(this, EntityState.Unchanged, EntityState.Added);
				base.State = EntityState.Added;
				break;
			case EntityState.Deleted:
				Delete();
				break;
			case EntityState.Detached:
				Delete();
				AcceptChanges();
				break;
			}
			break;
		case EntityState.Deleted:
			switch (requestedState)
			{
			default:
				_ = 8;
				break;
			case EntityState.Added:
				relatedEnd.Add(targetEntry.WrappedEntity, applyConstraints: true, addRelationshipAsUnchanged: false, relationshipAlreadyExists: true, allowModifyingOtherEndOfRelationship: false, forceForeignKeyChanges: true);
				base.ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Added);
				base.State = EntityState.Added;
				break;
			case EntityState.Unchanged:
				relatedEnd.Add(targetEntry.WrappedEntity, applyConstraints: true, addRelationshipAsUnchanged: false, relationshipAlreadyExists: true, allowModifyingOtherEndOfRelationship: false, forceForeignKeyChanges: true);
				base.ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Unchanged);
				base.State = EntityState.Unchanged;
				break;
			case EntityState.Detached:
				AcceptChanges();
				break;
			case EntityState.Detached | EntityState.Unchanged:
				break;
			}
			break;
		}
	}

	internal RelationshipEntry GetNextRelationshipEnd(EntityKey entityKey)
	{
		if (!entityKey.Equals(Key0))
		{
			return NextKey1;
		}
		return NextKey0;
	}

	internal void SetNextRelationshipEnd(EntityKey entityKey, RelationshipEntry nextEnd)
	{
		if (entityKey.Equals(Key0))
		{
			NextKey0 = nextEnd;
		}
		else
		{
			NextKey1 = nextEnd;
		}
	}
}
