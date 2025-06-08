namespace System.Data.Entity.Infrastructure.MappingViews;

internal class DefaultDbMappingViewCacheFactory : DbMappingViewCacheFactory
{
	private readonly Type _cacheType;

	public DefaultDbMappingViewCacheFactory(Type cacheType)
	{
		_cacheType = cacheType;
	}

	public override DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName)
	{
		return (DbMappingViewCache)Activator.CreateInstance(_cacheType);
	}

	public override int GetHashCode()
	{
		return (_cacheType.GetHashCode() * 397) ^ typeof(DefaultDbMappingViewCacheFactory).GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is DefaultDbMappingViewCacheFactory defaultDbMappingViewCacheFactory)
		{
			return (object)defaultDbMappingViewCacheFactory._cacheType == _cacheType;
		}
		return false;
	}
}
