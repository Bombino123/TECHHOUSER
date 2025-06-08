using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class OcAssemblyCache
{
	private readonly Dictionary<Assembly, ImmutableAssemblyCacheEntry> _conventionalOcCache;

	internal OcAssemblyCache()
	{
		_conventionalOcCache = new Dictionary<Assembly, ImmutableAssemblyCacheEntry>();
	}

	internal bool TryGetConventionalOcCacheFromAssemblyCache(Assembly assemblyToLookup, out ImmutableAssemblyCacheEntry cacheEntry)
	{
		cacheEntry = null;
		return _conventionalOcCache.TryGetValue(assemblyToLookup, out cacheEntry);
	}

	internal void AddAssemblyToOcCacheFromAssemblyCache(Assembly assembly, ImmutableAssemblyCacheEntry cacheEntry)
	{
		if (!_conventionalOcCache.ContainsKey(assembly))
		{
			_conventionalOcCache.Add(assembly, cacheEntry);
		}
	}
}
