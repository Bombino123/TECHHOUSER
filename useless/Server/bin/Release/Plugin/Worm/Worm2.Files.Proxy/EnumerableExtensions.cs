using System.Collections.Generic;

namespace Worm2.Files.Proxy;

public static class EnumerableExtensions
{
	public static T Random<T>(this IEnumerable<T> input)
	{
		return EnumerableHelper.Random(input);
	}
}
