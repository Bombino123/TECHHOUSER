using System.Collections.Generic;

namespace dnlib.DotNet;

internal static class MemberMDInitializer
{
	public static void Initialize<T>(IEnumerable<T> coll)
	{
		if (coll == null)
		{
			return;
		}
		foreach (T item in coll)
		{
			_ = item;
		}
	}

	public static void Initialize(object o)
	{
	}
}
