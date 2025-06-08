using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class LockedAssemblyCache : IDisposable
{
	private object _lockObject;

	private Dictionary<Assembly, ImmutableAssemblyCacheEntry> _globalAssemblyCache;

	internal LockedAssemblyCache(object lockObject, Dictionary<Assembly, ImmutableAssemblyCacheEntry> globalAssemblyCache)
	{
		_lockObject = lockObject;
		_globalAssemblyCache = globalAssemblyCache;
		Monitor.Enter(_lockObject);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Monitor.Exit(_lockObject);
		_lockObject = null;
		_globalAssemblyCache = null;
	}

	[Conditional("DEBUG")]
	private void AssertLockedByThisThread()
	{
		bool lockTaken = false;
		Monitor.TryEnter(_lockObject, ref lockTaken);
		if (lockTaken)
		{
			Monitor.Exit(_lockObject);
		}
	}

	internal bool TryGetValue(Assembly assembly, out ImmutableAssemblyCacheEntry cacheEntry)
	{
		return _globalAssemblyCache.TryGetValue(assembly, out cacheEntry);
	}

	internal void Add(Assembly assembly, ImmutableAssemblyCacheEntry assemblyCacheEntry)
	{
		_globalAssemblyCache.Add(assembly, assemblyCacheEntry);
	}

	internal void Clear()
	{
		_globalAssemblyCache.Clear();
	}
}
