using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DatabaseInitializerResolver : IDbDependencyResolver
{
	private readonly ConcurrentDictionary<Type, object> _initializers = new ConcurrentDictionary<Type, object>();

	public virtual object GetService(Type type, object key)
	{
		Type type2 = type.TryGetElementType(typeof(IDatabaseInitializer<>));
		if (type2 != null && _initializers.TryGetValue(type2, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual void SetInitializer(Type contextType, object initializer)
	{
		_initializers.AddOrUpdate(contextType, initializer, (Type c, object i) => initializer);
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
