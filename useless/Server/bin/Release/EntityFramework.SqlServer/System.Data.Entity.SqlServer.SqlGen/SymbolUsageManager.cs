using System.Collections.Generic;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SymbolUsageManager
{
	private readonly Dictionary<Symbol, BoolWrapper> optionalColumnUsage = new Dictionary<Symbol, BoolWrapper>();

	internal bool ContainsKey(Symbol key)
	{
		return optionalColumnUsage.ContainsKey(key);
	}

	internal bool TryGetValue(Symbol key, out bool value)
	{
		if (optionalColumnUsage.TryGetValue(key, out var value2))
		{
			value = value2.Value;
			return true;
		}
		value = false;
		return false;
	}

	internal void Add(Symbol sourceSymbol, Symbol symbolToAdd)
	{
		if (sourceSymbol == null || !optionalColumnUsage.TryGetValue(sourceSymbol, out var value))
		{
			value = new BoolWrapper();
		}
		optionalColumnUsage.Add(symbolToAdd, value);
	}

	internal void MarkAsUsed(Symbol key)
	{
		if (optionalColumnUsage.ContainsKey(key))
		{
			optionalColumnUsage[key].Value = true;
		}
	}

	internal bool IsUsed(Symbol key)
	{
		return optionalColumnUsage[key].Value;
	}
}
