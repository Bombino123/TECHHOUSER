using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class EntityWithKeyStrategy : IEntityKeyStrategy
{
	private readonly IEntityWithKey _entity;

	public EntityWithKeyStrategy(IEntityWithKey entity)
	{
		_entity = entity;
	}

	public EntityKey GetEntityKey()
	{
		return _entity.EntityKey;
	}

	public void SetEntityKey(EntityKey key)
	{
		_entity.EntityKey = key;
	}

	public EntityKey GetEntityKeyFromEntity()
	{
		return _entity.EntityKey;
	}
}
