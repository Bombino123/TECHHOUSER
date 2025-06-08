using System.Collections.Generic;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class IDictionaryExtensions
{
	internal static void Add<TKey, TValue>(this IDictionary<TKey, IList<TValue>> map, TKey key, TValue value)
	{
		if (!map.TryGetValue(key, out var value2))
		{
			value2 = (map[key] = new List<TValue>());
		}
		value2.Add(value);
	}
}
