using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal interface IEntityWrapper
{
	RelationshipManager RelationshipManager { get; }

	bool OwnsRelationshipManager { get; }

	object Entity { get; }

	EntityEntry ObjectStateEntry { get; set; }

	EntityKey EntityKey { get; set; }

	ObjectContext Context { get; set; }

	MergeOption MergeOption { get; }

	Type IdentityType { get; }

	bool InitializingProxyRelatedEnds { get; set; }

	bool RequiresRelationshipChangeTracking { get; }

	bool OverridesEqualsOrGetHashCode { get; }

	void EnsureCollectionNotNull(RelatedEnd relatedEnd);

	EntityKey GetEntityKeyFromEntity();

	void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption);

	void ResetContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption);

	void DetachContext();

	void SetChangeTracker(IEntityChangeTracker changeTracker);

	void TakeSnapshot(EntityEntry entry);

	void TakeSnapshotOfRelationships(EntityEntry entry);

	void CollectionAdd(RelatedEnd relatedEnd, object value);

	bool CollectionRemove(RelatedEnd relatedEnd, object value);

	object GetNavigationPropertyValue(RelatedEnd relatedEnd);

	void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value);

	void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value);

	void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value);

	void UpdateCurrentValueRecord(object value, EntityEntry entry);
}
