using System.Collections.Concurrent;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal static class ObjectContextTypeCache
{
	private static readonly ConcurrentDictionary<Type, Type> _typeCache = new ConcurrentDictionary<Type, Type>();

	public static Type GetObjectType(Type type)
	{
		return _typeCache.GetOrAdd(type, ObjectContext.GetObjectType);
	}
}
