using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class LightweightEntityWrapper<TEntity> : BaseEntityWrapper<TEntity> where TEntity : class, IEntityWithRelationships, IEntityWithKey, IEntityWithChangeTracker
{
	private readonly TEntity _entity;

	public override EntityKey EntityKey
	{
		get
		{
			return _entity.EntityKey;
		}
		set
		{
			_entity.EntityKey = value;
		}
	}

	public override bool OwnsRelationshipManager => true;

	public override object Entity => _entity;

	public override TEntity TypedEntity => _entity;

	public override bool RequiresRelationshipChangeTracking => false;

	internal LightweightEntityWrapper(TEntity entity, bool overridesEquals)
		: base(entity, entity.RelationshipManager, overridesEquals)
	{
		_entity = entity;
	}

	internal LightweightEntityWrapper(TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType, bool overridesEquals)
		: base(entity, entity.RelationshipManager, entitySet, context, mergeOption, identityType, overridesEquals)
	{
		_entity = entity;
		_entity.EntityKey = key;
	}

	public override void SetChangeTracker(IEntityChangeTracker changeTracker)
	{
		_entity.SetChangeTracker(changeTracker);
	}

	public override void TakeSnapshot(EntityEntry entry)
	{
	}

	public override void TakeSnapshotOfRelationships(EntityEntry entry)
	{
	}

	public override EntityKey GetEntityKeyFromEntity()
	{
		return _entity.EntityKey;
	}

	public override void CollectionAdd(RelatedEnd relatedEnd, object value)
	{
	}

	public override bool CollectionRemove(RelatedEnd relatedEnd, object value)
	{
		return false;
	}

	public override void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
	}

	public override void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
	}

	public override void EnsureCollectionNotNull(RelatedEnd relatedEnd)
	{
	}

	public override object GetNavigationPropertyValue(RelatedEnd relatedEnd)
	{
		return null;
	}

	public override void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
	{
		member.SetValue(target, value);
	}

	public override void UpdateCurrentValueRecord(object value, EntityEntry entry)
	{
		entry.UpdateRecordWithoutSetModified(value, entry.CurrentValues);
	}
}
