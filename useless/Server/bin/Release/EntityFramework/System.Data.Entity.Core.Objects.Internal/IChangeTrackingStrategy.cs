using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal interface IChangeTrackingStrategy
{
	void SetChangeTracker(IEntityChangeTracker changeTracker);

	void TakeSnapshot(EntityEntry entry);

	void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value);

	void UpdateCurrentValueRecord(object value, EntityEntry entry);
}
