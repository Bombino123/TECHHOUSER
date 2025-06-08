using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class TransactionContextInitializerResolver : IDbDependencyResolver
{
	private readonly ConcurrentDictionary<Type, object> _initializers = new ConcurrentDictionary<Type, object>();

	public object GetService(Type type, object key)
	{
		Check.NotNull(type, "type");
		Type type2 = type.TryGetElementType(typeof(IDatabaseInitializer<>));
		if (type2 != null && typeof(TransactionContext).IsAssignableFrom(type2))
		{
			return _initializers.GetOrAdd(type2, CreateInitializerInstance);
		}
		return null;
	}

	private object CreateInitializerInstance(Type type)
	{
		return Activator.CreateInstance(typeof(TransactionContextInitializer<>).MakeGenericType(type));
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
