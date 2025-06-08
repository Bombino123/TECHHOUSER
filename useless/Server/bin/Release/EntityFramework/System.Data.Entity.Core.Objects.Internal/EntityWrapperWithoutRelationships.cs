using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class EntityWrapperWithoutRelationships<TEntity> : EntityWrapper<TEntity> where TEntity : class
{
	public override bool OwnsRelationshipManager => false;

	public override bool RequiresRelationshipChangeTracking => true;

	internal EntityWrapperWithoutRelationships(TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
		: base(entity, RelationshipManager.Create(), key, entitySet, context, mergeOption, identityType, propertyStrategy, changeTrackingStrategy, keyStrategy, overridesEquals)
	{
	}

	internal EntityWrapperWithoutRelationships(TEntity entity, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
		: base(entity, RelationshipManager.Create(), propertyStrategy, changeTrackingStrategy, keyStrategy, overridesEquals)
	{
	}

	public override void TakeSnapshotOfRelationships(EntityEntry entry)
	{
		entry.TakeSnapshotOfRelationships();
	}
}
