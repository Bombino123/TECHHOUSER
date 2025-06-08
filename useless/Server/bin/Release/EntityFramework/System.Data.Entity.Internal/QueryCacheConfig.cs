using System.Data.Entity.Internal.ConfigFile;

namespace System.Data.Entity.Internal;

internal class QueryCacheConfig
{
	private const int DefaultSize = 1000;

	private const int DefaultCleaningIntervalInSeconds = 60;

	private readonly EntityFrameworkSection _entityFrameworkSection;

	public QueryCacheConfig(EntityFrameworkSection entityFrameworkSection)
	{
		_entityFrameworkSection = entityFrameworkSection;
	}

	public int GetQueryCacheSize()
	{
		int size = _entityFrameworkSection.QueryCache.Size;
		if (size == 0)
		{
			return 1000;
		}
		return size;
	}

	public int GetCleaningIntervalInSeconds()
	{
		int cleaningIntervalInSeconds = _entityFrameworkSection.QueryCache.CleaningIntervalInSeconds;
		if (cleaningIntervalInSeconds == 0)
		{
			return 60;
		}
		return cleaningIntervalInSeconds;
	}
}
