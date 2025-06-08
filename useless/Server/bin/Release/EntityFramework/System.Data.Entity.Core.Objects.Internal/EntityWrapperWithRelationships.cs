using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class EntityWrapperWithRelationships<TEntity> : EntityWrapper<TEntity> where TEntity : class, IEntityWithRelationships
{
	public override bool OwnsRelationshipManager => true;

	public override bool RequiresRelationshipChangeTracking => false;

	internal EntityWrapperWithRelationships(TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
		: base(entity, entity.RelationshipManager, key, entitySet, context, mergeOption, identityType, propertyStrategy, changeTrackingStrategy, keyStrategy, overridesEquals)
	{
	}

	internal EntityWrapperWithRelationships(TEntity entity, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
		: base(entity, entity.RelationshipManager, propertyStrategy, changeTrackingStrategy, keyStrategy, overridesEquals)
	{
	}

	public override void TakeSnapshotOfRelationships(EntityEntry entry)
	{
	}
}
