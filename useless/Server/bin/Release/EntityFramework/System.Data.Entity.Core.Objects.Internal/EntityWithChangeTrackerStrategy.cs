using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class EntityWithChangeTrackerStrategy : IChangeTrackingStrategy
{
	private readonly IEntityWithChangeTracker _entity;

	public EntityWithChangeTrackerStrategy(IEntityWithChangeTracker entity)
	{
		_entity = entity;
	}

	public void SetChangeTracker(IEntityChangeTracker changeTracker)
	{
		_entity.SetChangeTracker(changeTracker);
	}

	public void TakeSnapshot(EntityEntry entry)
	{
		if (entry != null && entry.RequiresComplexChangeTracking)
		{
			entry.TakeSnapshot(onlySnapshotComplexProperties: true);
		}
	}

	public void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
	{
		member.SetValue(target, value);
	}

	public void UpdateCurrentValueRecord(object value, EntityEntry entry)
	{
		bool num = entry.WrappedEntity.IdentityType != _entity.GetType();
		entry.UpdateRecordWithoutSetModified(value, entry.CurrentValues);
		if (num)
		{
			entry.DetectChangesInProperties(detectOnlyComplexProperties: true);
		}
	}
}
