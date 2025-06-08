using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects;

internal sealed class ObjectStateEntryDbUpdatableDataRecord : CurrentValueRecord
{
	internal ObjectStateEntryDbUpdatableDataRecord(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
		: base(cacheEntry, metadata, userObject)
	{
		EntityState state = cacheEntry.State;
		if (state != EntityState.Unchanged && state != EntityState.Added)
		{
			_ = 16;
		}
	}

	internal ObjectStateEntryDbUpdatableDataRecord(RelationshipEntry cacheEntry)
		: base(cacheEntry)
	{
		EntityState state = cacheEntry.State;
		if (state != EntityState.Unchanged && state != EntityState.Added)
		{
			_ = 16;
		}
	}

	protected override object GetRecordValue(int ordinal)
	{
		if (_cacheEntry.IsRelationship)
		{
			return (_cacheEntry as RelationshipEntry).GetCurrentRelationValue(ordinal);
		}
		return (_cacheEntry as EntityEntry).GetCurrentEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.CurrentUpdatable);
	}

	protected override void SetRecordValue(int ordinal, object value)
	{
		if (_cacheEntry.IsRelationship)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
		}
		(_cacheEntry as EntityEntry).SetCurrentEntityValue(_metadata, ordinal, _userObject, value);
	}
}
