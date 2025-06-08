using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Diagnostics;

namespace System.Data.Entity.Core.Objects;

public abstract class ObjectStateEntry : IEntityStateEntry, IEntityChangeTracker
{
	internal ObjectStateManager _cache;

	internal EntitySetBase _entitySet;

	internal EntityState _state;

	public ObjectStateManager ObjectStateManager
	{
		get
		{
			ValidateState();
			return _cache;
		}
	}

	public EntitySetBase EntitySet
	{
		get
		{
			ValidateState();
			return _entitySet;
		}
	}

	public EntityState State
	{
		get
		{
			return _state;
		}
		internal set
		{
			_state = value;
		}
	}

	public abstract object Entity { get; }

	public abstract EntityKey EntityKey { get; internal set; }

	public abstract bool IsRelationship { get; }

	internal abstract BitArray ModifiedProperties { get; }

	BitArray IEntityStateEntry.ModifiedProperties => ModifiedProperties;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public abstract DbDataRecord OriginalValues { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public abstract CurrentValueRecord CurrentValues { get; }

	public abstract RelationshipManager RelationshipManager { get; }

	IEntityStateManager IEntityStateEntry.StateManager => ObjectStateManager;

	bool IEntityStateEntry.IsKeyEntry => IsKeyEntry;

	EntityState IEntityChangeTracker.EntityState => State;

	internal abstract bool IsKeyEntry { get; }

	internal ObjectStateEntry()
	{
	}

	internal ObjectStateEntry(ObjectStateManager cache, EntitySet entitySet, EntityState state)
	{
		_cache = cache;
		_entitySet = entitySet;
		_state = state;
	}

	public abstract OriginalValueRecord GetUpdatableOriginalValues();

	public abstract void AcceptChanges();

	public abstract void Delete();

	public abstract IEnumerable<string> GetModifiedProperties();

	public abstract void SetModified();

	public abstract void SetModifiedProperty(string propertyName);

	public abstract void RejectPropertyChanges(string propertyName);

	public abstract bool IsPropertyChanged(string propertyName);

	public abstract void ChangeState(EntityState state);

	public abstract void ApplyCurrentValues(object currentEntity);

	public abstract void ApplyOriginalValues(object originalEntity);

	void IEntityChangeTracker.EntityMemberChanging(string entityMemberName)
	{
		EntityMemberChanging(entityMemberName);
	}

	void IEntityChangeTracker.EntityMemberChanged(string entityMemberName)
	{
		EntityMemberChanged(entityMemberName);
	}

	void IEntityChangeTracker.EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		EntityComplexMemberChanging(entityMemberName, complexObject, complexObjectMemberName);
	}

	void IEntityChangeTracker.EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
	{
		EntityComplexMemberChanged(entityMemberName, complexObject, complexObjectMemberName);
	}

	internal abstract int GetFieldCount(StateManagerTypeMetadata metadata);

	internal abstract Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata);

	internal abstract string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata);

	internal abstract int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata);

	internal abstract void RevertDelete();

	internal abstract void SetModifiedAll();

	internal abstract void EntityMemberChanging(string entityMemberName);

	internal abstract void EntityMemberChanged(string entityMemberName);

	internal abstract void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName);

	internal abstract void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName);

	internal abstract DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject);

	internal virtual void Reset()
	{
		_cache = null;
		_entitySet = null;
		_state = EntityState.Detached;
	}

	internal void ValidateState()
	{
		if (_state == EntityState.Detached)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_InvalidState);
		}
	}
}
