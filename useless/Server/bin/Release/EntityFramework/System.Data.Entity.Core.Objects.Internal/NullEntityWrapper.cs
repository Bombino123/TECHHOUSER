using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal class NullEntityWrapper : IEntityWrapper
{
	private static readonly IEntityWrapper _nullWrapper = new NullEntityWrapper();

	internal static IEntityWrapper NullWrapper => _nullWrapper;

	public RelationshipManager RelationshipManager => null;

	public bool OwnsRelationshipManager => false;

	public object Entity => null;

	public EntityEntry ObjectStateEntry
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public EntityKey EntityKey
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ObjectContext Context
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public MergeOption MergeOption => MergeOption.NoTracking;

	public Type IdentityType => null;

	public bool InitializingProxyRelatedEnds
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool RequiresRelationshipChangeTracking => false;

	public bool OverridesEqualsOrGetHashCode => false;

	private NullEntityWrapper()
	{
	}

	public void CollectionAdd(RelatedEnd relatedEnd, object value)
	{
	}

	public bool CollectionRemove(RelatedEnd relatedEnd, object value)
	{
		return false;
	}

	public EntityKey GetEntityKeyFromEntity()
	{
		return null;
	}

	public void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
	}

	public void ResetContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
	}

	public void DetachContext()
	{
	}

	public void SetChangeTracker(IEntityChangeTracker changeTracker)
	{
	}

	public void TakeSnapshot(EntityEntry entry)
	{
	}

	public void TakeSnapshotOfRelationships(EntityEntry entry)
	{
	}

	public void EnsureCollectionNotNull(RelatedEnd relatedEnd)
	{
	}

	public object GetNavigationPropertyValue(RelatedEnd relatedEnd)
	{
		return null;
	}

	public void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
	}

	public void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
	}

	public void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
	{
	}

	public void UpdateCurrentValueRecord(object value, EntityEntry entry)
	{
	}
}
