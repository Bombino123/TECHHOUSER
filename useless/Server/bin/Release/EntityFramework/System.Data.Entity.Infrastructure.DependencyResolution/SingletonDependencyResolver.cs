using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

public class SingletonDependencyResolver<T> : IDbDependencyResolver where T : class
{
	private readonly T _singletonInstance;

	private readonly Func<object, bool> _keyPredicate;

	public SingletonDependencyResolver(T singletonInstance)
		: this(singletonInstance, (object)null)
	{
	}

	public SingletonDependencyResolver(T singletonInstance, object key)
	{
		Check.NotNull(singletonInstance, "singletonInstance");
		_singletonInstance = singletonInstance;
		_keyPredicate = (object k) => key == null || object.Equals(key, k);
	}

	public SingletonDependencyResolver(T singletonInstance, Func<object, bool> keyPredicate)
	{
		Check.NotNull(singletonInstance, "singletonInstance");
		Check.NotNull(keyPredicate, "keyPredicate");
		_singletonInstance = singletonInstance;
		_keyPredicate = keyPredicate;
	}

	public object GetService(Type type, object key)
	{
		return (type == typeof(T) && _keyPredicate(key)) ? _singletonInstance : null;
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
