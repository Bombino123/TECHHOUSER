using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Utilities;

[DebuggerStepThrough]
internal static class IEnumerableExtensions
{
	public static string Uniquify(this IEnumerable<string> inputStrings, string targetString)
	{
		string uniqueString = targetString;
		int num = 0;
		while (inputStrings.Any((string n) => string.Equals(n, uniqueString, StringComparison.Ordinal)))
		{
			int num2 = ++num;
			uniqueString = targetString + num2;
		}
		return uniqueString;
	}

	public static void Each<T>(this IEnumerable<T> ts, Action<T, int> action)
	{
		int num = 0;
		foreach (T t in ts)
		{
			action(t, num++);
		}
	}

	public static void Each<T>(this IEnumerable<T> ts, Action<T> action)
	{
		foreach (T t in ts)
		{
			action(t);
		}
	}

	public static void Each<T, S>(this IEnumerable<T> ts, Func<T, S> action)
	{
		foreach (T t in ts)
		{
			action(t);
		}
	}

	public static string Join<T>(this IEnumerable<T> ts, Func<T, string> selector = null, string separator = ", ")
	{
		selector = selector ?? ((Func<T, string>)((T t) => t.ToString()));
		return string.Join(separator, ts.Where((T t) => t != null).Select(selector));
	}

	public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource value)
	{
		yield return value;
		foreach (TSource item in source)
		{
			yield return item;
		}
	}

	public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource value)
	{
		foreach (TSource item in source)
		{
			yield return item;
		}
		yield return value;
	}
}
