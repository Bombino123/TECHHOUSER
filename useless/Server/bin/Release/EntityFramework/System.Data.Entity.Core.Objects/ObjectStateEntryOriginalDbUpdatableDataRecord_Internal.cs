namespace System.Data.Entity.Core.Objects;

internal class ObjectStateEntryOriginalDbUpdatableDataRecord_Internal : OriginalValueRecord
{
	internal ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
		: base(cacheEntry, metadata, userObject)
	{
		EntityState state = cacheEntry.State;
		if (state != EntityState.Unchanged && state != EntityState.Deleted)
		{
			_ = 16;
		}
	}

	protected override object GetRecordValue(int ordinal)
	{
		return (_cacheEntry as EntityEntry).GetOriginalEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalUpdatableInternal);
	}

	protected override void SetRecordValue(int ordinal, object value)
	{
		(_cacheEntry as EntityEntry).SetOriginalEntityValue(_metadata, ordinal, _userObject, value);
	}
}
