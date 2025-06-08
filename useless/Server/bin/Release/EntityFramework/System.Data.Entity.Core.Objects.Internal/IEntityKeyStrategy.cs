namespace System.Data.Entity.Core.Objects.Internal;

internal interface IEntityKeyStrategy
{
	EntityKey GetEntityKey();

	void SetEntityKey(EntityKey key);

	EntityKey GetEntityKeyFromEntity();
}
