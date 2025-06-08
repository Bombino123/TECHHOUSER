using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace System.Data.Entity.Core.Common.Utils;

internal sealed class AliasGenerator
{
	private const int MaxPrefixCount = 500;

	private const int CacheSize = 250;

	private static readonly string[] _counterNames = new string[250];

	private static Dictionary<string, string[]> _prefixCounter;

	private int _counter;

	private readonly string _prefix;

	private readonly string[] _cache;

	internal AliasGenerator(string prefix)
		: this(prefix, 250)
	{
	}

	internal AliasGenerator(string prefix, int cacheSize)
	{
		_prefix = prefix ?? string.Empty;
		if (0 >= cacheSize)
		{
			return;
		}
		string[] array = null;
		Dictionary<string, string[]> prefixCounter;
		while ((prefixCounter = _prefixCounter) == null || !prefixCounter.TryGetValue(prefix, out _cache))
		{
			if (array == null)
			{
				array = new string[cacheSize];
			}
			int num = 1 + (prefixCounter?.Count ?? 0);
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(num, StringComparer.InvariantCultureIgnoreCase);
			if (prefixCounter != null && num < 500)
			{
				foreach (KeyValuePair<string, string[]> item in prefixCounter)
				{
					dictionary.Add(item.Key, item.Value);
				}
			}
			dictionary.Add(prefix, array);
			Interlocked.CompareExchange(ref _prefixCounter, dictionary, prefixCounter);
		}
	}

	internal string Next()
	{
		_counter = Math.Max(1 + _counter, 0);
		return GetName(_counter);
	}

	internal string GetName(int index)
	{
		string result;
		if (_cache == null || (uint)_cache.Length <= (uint)index)
		{
			result = _prefix + index.ToString(CultureInfo.InvariantCulture);
		}
		else if ((result = _cache[index]) == null)
		{
			if ((uint)_counterNames.Length <= (uint)index)
			{
				result = index.ToString(CultureInfo.InvariantCulture);
			}
			else if ((result = _counterNames[index]) == null)
			{
				result = (_counterNames[index] = index.ToString(CultureInfo.InvariantCulture));
			}
			result = (_cache[index] = _prefix + result);
		}
		return result;
	}
}
