namespace System.Data.Entity.Core.Common.QueryCache;

internal sealed class CompiledQueryCacheKey : QueryCacheKey
{
	private readonly Guid _cacheIdentity;

	internal CompiledQueryCacheKey(Guid cacheIdentity)
	{
		_cacheIdentity = cacheIdentity;
	}

	public override bool Equals(object compareTo)
	{
		if (typeof(CompiledQueryCacheKey) != compareTo.GetType())
		{
			return false;
		}
		Guid cacheIdentity = ((CompiledQueryCacheKey)compareTo)._cacheIdentity;
		return cacheIdentity.Equals(_cacheIdentity);
	}

	public override int GetHashCode()
	{
		return _cacheIdentity.GetHashCode();
	}

	public override string ToString()
	{
		return _cacheIdentity.ToString();
	}
}
