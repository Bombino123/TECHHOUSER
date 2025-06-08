namespace System.Data.Entity.Infrastructure;

public interface IDbModelCacheKey
{
	new bool Equals(object other);

	new int GetHashCode();
}
