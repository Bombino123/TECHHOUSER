using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class CachingDependencyResolver : IDbDependencyResolver
{
	private readonly IDbDependencyResolver _underlyingResolver;

	private readonly ConcurrentDictionary<Tuple<Type, object>, object> _resolvedDependencies = new ConcurrentDictionary<Tuple<Type, object>, object>();

	private readonly ConcurrentDictionary<Tuple<Type, object>, IEnumerable<object>> _resolvedAllDependencies = new ConcurrentDictionary<Tuple<Type, object>, IEnumerable<object>>();

	public CachingDependencyResolver(IDbDependencyResolver underlyingResolver)
	{
		_underlyingResolver = underlyingResolver;
	}

	public virtual object GetService(Type type, object key)
	{
		return _resolvedDependencies.GetOrAdd(Tuple.Create(type, key), (Tuple<Type, object> k) => _underlyingResolver.GetService(type, key));
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return _resolvedAllDependencies.GetOrAdd(Tuple.Create(type, key), (Tuple<Type, object> k) => _underlyingResolver.GetServices(type, key));
	}
}
