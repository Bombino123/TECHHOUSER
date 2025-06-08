using System.Collections.Generic;

namespace System.Data.Entity.Utilities;

internal static class HashSetExtensions
{
	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			set.Add(item);
		}
	}
}
