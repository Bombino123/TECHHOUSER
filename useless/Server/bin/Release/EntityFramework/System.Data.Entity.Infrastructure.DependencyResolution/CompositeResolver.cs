using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class CompositeResolver<TFirst, TSecond> : IDbDependencyResolver where TFirst : class, IDbDependencyResolver where TSecond : class, IDbDependencyResolver
{
	private readonly TFirst _firstResolver;

	private readonly TSecond _secondResolver;

	public TFirst First => _firstResolver;

	public TSecond Second => _secondResolver;

	public CompositeResolver(TFirst firstResolver, TSecond secondResolver)
	{
		_firstResolver = firstResolver;
		_secondResolver = secondResolver;
	}

	public virtual object GetService(Type type, object key)
	{
		return _firstResolver.GetService(type, key) ?? _secondResolver.GetService(type, key);
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return _firstResolver.GetServices(type, key).Concat(_secondResolver.GetServices(type, key));
	}
}
