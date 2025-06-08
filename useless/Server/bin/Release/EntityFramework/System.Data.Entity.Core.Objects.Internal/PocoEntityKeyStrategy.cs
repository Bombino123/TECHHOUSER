namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class PocoEntityKeyStrategy : IEntityKeyStrategy
{
	private EntityKey _key;

	public EntityKey GetEntityKey()
	{
		return _key;
	}

	public void SetEntityKey(EntityKey key)
	{
		_key = key;
	}

	public EntityKey GetEntityKeyFromEntity()
	{
		return null;
	}
}
