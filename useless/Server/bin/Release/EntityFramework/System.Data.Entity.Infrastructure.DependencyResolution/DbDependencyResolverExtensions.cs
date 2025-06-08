using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

public static class DbDependencyResolverExtensions
{
	public static T GetService<T>(this IDbDependencyResolver resolver, object key)
	{
		Check.NotNull(resolver, "resolver");
		return (T)resolver.GetService(typeof(T), key);
	}

	public static T GetService<T>(this IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		return (T)resolver.GetService(typeof(T), null);
	}

	public static object GetService(this IDbDependencyResolver resolver, Type type)
	{
		Check.NotNull(resolver, "resolver");
		Check.NotNull(type, "type");
		return resolver.GetService(type, null);
	}

	public static IEnumerable<T> GetServices<T>(this IDbDependencyResolver resolver, object key)
	{
		Check.NotNull(resolver, "resolver");
		return resolver.GetServices(typeof(T), key).OfType<T>();
	}

	public static IEnumerable<T> GetServices<T>(this IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		return resolver.GetServices(typeof(T), null).OfType<T>();
	}

	public static IEnumerable<object> GetServices(this IDbDependencyResolver resolver, Type type)
	{
		Check.NotNull(resolver, "resolver");
		Check.NotNull(type, "type");
		return resolver.GetServices(type, null);
	}

	internal static IEnumerable<object> GetServiceAsServices(this IDbDependencyResolver resolver, Type type, object key)
	{
		object service = resolver.GetService(type, key);
		if (service != null)
		{
			return new object[1] { service };
		}
		return Enumerable.Empty<object>();
	}
}
