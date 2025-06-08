using System.Collections.Generic;
using System.Threading;

namespace dnlib.DotNet.Pdb.Portable;

internal static class ListCache<T>
{
	private static volatile List<T> cachedList;

	public static List<T> AllocList()
	{
		return Interlocked.Exchange(ref cachedList, null) ?? new List<T>();
	}

	public static void Free(ref List<T> list)
	{
		list.Clear();
		cachedList = list;
	}

	public static T[] FreeAndToArray(ref List<T> list)
	{
		T[] result = list.ToArray();
		Free(ref list);
		return result;
	}
}
