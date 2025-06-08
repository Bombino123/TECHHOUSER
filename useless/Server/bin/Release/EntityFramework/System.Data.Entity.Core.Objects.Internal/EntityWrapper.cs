using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects.Internal;

internal abstract class EntityWrapper<TEntity> : BaseEntityWrapper<TEntity> where TEntity : class
{
	private readonly TEntity _entity;

	private readonly IPropertyAccessorStrategy _propertyStrategy;

	private readonly IChangeTrackingStrategy _changeTrackingStrategy;

	private readonly IEntityKeyStrategy _keyStrategy;

	public override EntityKey EntityKey
	{
		get
		{
			return _keyStrategy.GetEntityKey();
		}
		set
		{
			_keyStrategy.SetEntityKey(value);
		}
	}

	public override object Entity => _entity;

	public override TEntity TypedEntity => _entity;

	protected EntityWrapper(TEntity entity, RelationshipManager relationshipManager, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
		: base(entity, relationshipManager, overridesEquals)
	{
		if (relationshipManager == null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
		}
		_entity = entity;
		_propertyStrategy = propertyStrategy(entity);
		_changeTrackingStrategy = changeTrackingStrategy(entity);
		_keyStrategy = keyStrategy(entity);
	}

	protected EntityWrapper(TEntity entity, RelationshipManager relationshipManager, EntityKey key, EntitySet set, ObjectContext context, MergeOption mergeOption, Type identityType, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
		: base(entity, relationshipManager, set, context, mergeOption, identityType, overridesEquals)
	{
		if (relationshipManager == null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
		}
		_entity = entity;
		_propertyStrategy = propertyStrategy(entity);
		_changeTrackingStrategy = changeTrackingStrategy(entity);
		_keyStrategy = keyStrategy(entity);
		_keyStrategy.SetEntityKey(key);
	}

	public override void SetChangeTracker(IEntityChangeTracker changeTracker)
	{
		_changeTrackingStrategy.SetChangeTracker(changeTracker);
	}

	public override void TakeSnapshot(EntityEntry entry)
	{
		_changeTrackingStrategy.TakeSnapshot(entry);
	}

	public override EntityKey GetEntityKeyFromEntity()
	{
		return _keyStrategy.GetEntityKeyFromEntity();
	}

	public override void CollectionAdd(RelatedEnd relatedEnd, object value)
	{
		if (_propertyStrategy != null)
		{
			_propertyStrategy.CollectionAdd(relatedEnd, value);
		}
	}

	public override bool CollectionRemove(RelatedEnd relatedEnd, object value)
	{
		if (_propertyStrategy == null)
		{
			return false;
		}
		return _propertyStrategy.CollectionRemove(relatedEnd, value);
	}

	public override void EnsureCollectionNotNull(RelatedEnd relatedEnd)
	{
		if (_propertyStrategy != null)
		{
			object navigationPropertyValue = _propertyStrategy.GetNavigationPropertyValue(relatedEnd);
			if (navigationPropertyValue == null)
			{
				navigationPropertyValue = _propertyStrategy.CollectionCreate(relatedEnd);
				_propertyStrategy.SetNavigationPropertyValue(relatedEnd, navigationPropertyValue);
			}
		}
	}

	public override object GetNavigationPropertyValue(RelatedEnd relatedEnd)
	{
		if (_propertyStrategy == null)
		{
			return null;
		}
		return _propertyStrategy.GetNavigationPropertyValue(relatedEnd);
	}

	public override void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
		if (_propertyStrategy != null)
		{
			_propertyStrategy.SetNavigationPropertyValue(relatedEnd, value);
		}
	}

	public override void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
		if (_propertyStrategy != null && _propertyStrategy.GetNavigationPropertyValue(relatedEnd) == value)
		{
			_propertyStrategy.SetNavigationPropertyValue(relatedEnd, null);
		}
	}

	public override void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
	{
		_changeTrackingStrategy.SetCurrentValue(entry, member, ordinal, target, value);
	}

	public override void UpdateCurrentValueRecord(object value, EntityEntry entry)
	{
		_changeTrackingStrategy.UpdateCurrentValueRecord(value, entry);
	}
}
