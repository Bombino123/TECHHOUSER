namespace System.Data.Entity.Core.Common.QueryCache;

internal class QueryCacheEntry
{
	private readonly QueryCacheKey _queryCacheKey;

	protected readonly object _target;

	internal QueryCacheKey QueryCacheKey => _queryCacheKey;

	internal QueryCacheEntry(QueryCacheKey queryCacheKey, object target)
	{
		_queryCacheKey = queryCacheKey;
		_target = target;
	}

	internal virtual object GetTarget()
	{
		return _target;
	}
}
