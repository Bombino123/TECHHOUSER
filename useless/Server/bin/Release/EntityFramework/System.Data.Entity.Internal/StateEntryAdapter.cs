using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal class StateEntryAdapter : IEntityStateEntry
{
	private readonly ObjectStateEntry _stateEntry;

	public object Entity => _stateEntry.Entity;

	public EntityState State => _stateEntry.State;

	public DbUpdatableDataRecord CurrentValues => _stateEntry.CurrentValues;

	public EntitySetBase EntitySet => _stateEntry.EntitySet;

	public EntityKey EntityKey => _stateEntry.EntityKey;

	public StateEntryAdapter(ObjectStateEntry stateEntry)
	{
		_stateEntry = stateEntry;
	}

	public void ChangeState(EntityState state)
	{
		_stateEntry.ChangeState(state);
	}

	public DbUpdatableDataRecord GetUpdatableOriginalValues()
	{
		return _stateEntry.GetUpdatableOriginalValues();
	}

	public IEnumerable<string> GetModifiedProperties()
	{
		return _stateEntry.GetModifiedProperties();
	}

	public void SetModifiedProperty(string propertyName)
	{
		_stateEntry.SetModifiedProperty(propertyName);
	}

	public void RejectPropertyChanges(string propertyName)
	{
		_stateEntry.RejectPropertyChanges(propertyName);
	}

	public bool IsPropertyChanged(string propertyName)
	{
		return _stateEntry.IsPropertyChanged(propertyName);
	}
}
