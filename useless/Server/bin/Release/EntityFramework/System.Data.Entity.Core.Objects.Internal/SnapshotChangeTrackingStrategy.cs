using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class SnapshotChangeTrackingStrategy : IChangeTrackingStrategy
{
	private static readonly SnapshotChangeTrackingStrategy _instance = new SnapshotChangeTrackingStrategy();

	public static SnapshotChangeTrackingStrategy Instance => _instance;

	private SnapshotChangeTrackingStrategy()
	{
	}

	public void SetChangeTracker(IEntityChangeTracker changeTracker)
	{
	}

	public void TakeSnapshot(EntityEntry entry)
	{
		entry?.TakeSnapshot(onlySnapshotComplexProperties: false);
	}

	public void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
	{
		if (target == entry.Entity)
		{
			((IEntityChangeTracker)entry).EntityMemberChanging(member.CLayerName);
			member.SetValue(target, value);
			((IEntityChangeTracker)entry).EntityMemberChanged(member.CLayerName);
			if (member.IsComplex)
			{
				entry.UpdateComplexObjectSnapshot(member, target, ordinal, value);
			}
		}
		else
		{
			member.SetValue(target, value);
			if (entry.State != EntityState.Added)
			{
				entry.DetectChangesInProperties(detectOnlyComplexProperties: true);
			}
		}
	}

	public void UpdateCurrentValueRecord(object value, EntityEntry entry)
	{
		entry.UpdateRecordWithoutSetModified(value, entry.CurrentValues);
		entry.DetectChangesInProperties(detectOnlyComplexProperties: false);
	}
}
