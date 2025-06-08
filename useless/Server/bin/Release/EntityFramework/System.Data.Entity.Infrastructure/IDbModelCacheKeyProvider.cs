namespace System.Data.Entity.Infrastructure;

public interface IDbModelCacheKeyProvider
{
	string CacheKey { get; }
}
