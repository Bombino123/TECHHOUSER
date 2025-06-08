using System.Collections.Generic;

namespace System.Data.SQLite.Linq;

internal sealed class SymbolTable
{
	private List<Dictionary<string, Symbol>> symbols = new List<Dictionary<string, Symbol>>();

	internal void EnterScope()
	{
		symbols.Add(new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase));
	}

	internal void ExitScope()
	{
		symbols.RemoveAt(symbols.Count - 1);
	}

	internal void Add(string name, Symbol value)
	{
		symbols[symbols.Count - 1][name] = value;
	}

	internal Symbol Lookup(string name)
	{
		for (int num = symbols.Count - 1; num >= 0; num--)
		{
			if (symbols[num].ContainsKey(name))
			{
				return symbols[num][name];
			}
		}
		return null;
	}
}
