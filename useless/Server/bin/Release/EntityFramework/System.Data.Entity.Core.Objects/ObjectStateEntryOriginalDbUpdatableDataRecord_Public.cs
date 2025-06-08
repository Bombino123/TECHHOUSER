using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects;

internal sealed class ObjectStateEntryOriginalDbUpdatableDataRecord_Public : ObjectStateEntryOriginalDbUpdatableDataRecord_Internal
{
	private readonly int _parentEntityPropertyIndex;

	internal ObjectStateEntryOriginalDbUpdatableDataRecord_Public(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject, int parentEntityPropertyIndex)
		: base(cacheEntry, metadata, userObject)
	{
		_parentEntityPropertyIndex = parentEntityPropertyIndex;
	}

	protected override object GetRecordValue(int ordinal)
	{
		return (_cacheEntry as EntityEntry).GetOriginalEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalUpdatablePublic, GetPropertyIndex(ordinal));
	}

	protected override void SetRecordValue(int ordinal, object value)
	{
		StateManagerMemberMetadata stateManagerMemberMetadata = _metadata.Member(ordinal);
		if (stateManagerMemberMetadata.IsComplex)
		{
			throw new InvalidOperationException(Strings.ObjectStateEntry_SetOriginalComplexProperties(stateManagerMemberMetadata.CLayerName));
		}
		object obj = value ?? DBNull.Value;
		EntityEntry entityEntry = _cacheEntry as EntityEntry;
		EntityState state = entityEntry.State;
		if (entityEntry.HasRecordValueChanged(this, ordinal, obj))
		{
			if (stateManagerMemberMetadata.IsPartOfKey)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_SetOriginalPrimaryKey(stateManagerMemberMetadata.CLayerName));
			}
			Type clrType = stateManagerMemberMetadata.ClrType;
			if (DBNull.Value == obj && clrType.IsValueType() && !stateManagerMemberMetadata.CdmMetadata.Nullable)
			{
				throw new InvalidOperationException(Strings.ObjectStateEntry_NullOriginalValueForNonNullableProperty(stateManagerMemberMetadata.CLayerName, stateManagerMemberMetadata.ClrMetadata.Name, stateManagerMemberMetadata.ClrMetadata.DeclaringType.FullName));
			}
			base.SetRecordValue(ordinal, value);
			if (state == EntityState.Unchanged && entityEntry.State == EntityState.Modified)
			{
				entityEntry.ObjectStateManager.ChangeState(entityEntry, state, EntityState.Modified);
			}
			entityEntry.SetModifiedPropertyInternal(GetPropertyIndex(ordinal));
		}
	}

	private int GetPropertyIndex(int ordinal)
	{
		if (_parentEntityPropertyIndex != -1)
		{
			return _parentEntityPropertyIndex;
		}
		return ordinal;
	}
}
