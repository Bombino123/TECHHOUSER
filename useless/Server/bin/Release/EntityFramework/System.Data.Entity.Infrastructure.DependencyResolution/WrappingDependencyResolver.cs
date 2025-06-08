using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class WrappingDependencyResolver<TService> : IDbDependencyResolver
{
	private readonly IDbDependencyResolver _snapshot;

	private readonly Func<TService, object, TService> _serviceWrapper;

	public WrappingDependencyResolver(IDbDependencyResolver snapshot, Func<TService, object, TService> serviceWrapper)
	{
		_snapshot = snapshot;
		_serviceWrapper = serviceWrapper;
	}

	public object GetService(Type type, object key)
	{
		if (!(type == typeof(TService)))
		{
			return null;
		}
		return _serviceWrapper(_snapshot.GetService<TService>(key), key);
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		if (!(type == typeof(TService)))
		{
			return Enumerable.Empty<object>();
		}
		return (IEnumerable<object>)(from s in _snapshot.GetServices<TService>(key)
			select _serviceWrapper(s, key));
	}
}
