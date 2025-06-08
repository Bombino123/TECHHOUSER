using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class ResolverChain : IDbDependencyResolver
{
	private readonly ConcurrentStack<IDbDependencyResolver> _resolvers = new ConcurrentStack<IDbDependencyResolver>();

	private volatile IDbDependencyResolver[] _resolversSnapshot = new IDbDependencyResolver[0];

	public virtual IEnumerable<IDbDependencyResolver> Resolvers => _resolversSnapshot.Reverse();

	public virtual void Add(IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		_resolvers.Push(resolver);
		_resolversSnapshot = _resolvers.ToArray();
	}

	public virtual object GetService(Type type, object key)
	{
		return _resolversSnapshot.Select((IDbDependencyResolver r) => r.GetService(type, key)).FirstOrDefault((object s) => s != null);
	}

	public virtual IEnumerable<object> GetServices(Type type, object key)
	{
		return _resolversSnapshot.SelectMany((IDbDependencyResolver r) => r.GetServices(type, key));
	}
}
