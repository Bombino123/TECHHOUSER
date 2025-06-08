using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal readonly struct AllTypesHelper
{
	public static IEnumerable<TypeDef> Types(IEnumerable<TypeDef> types)
	{
		Dictionary<TypeDef, bool> visited = new Dictionary<TypeDef, bool>();
		Stack<IEnumerator<TypeDef>> stack = new Stack<IEnumerator<TypeDef>>();
		if (types != null)
		{
			stack.Push(types.GetEnumerator());
		}
		while (stack.Count > 0)
		{
			IEnumerator<TypeDef> enumerator = stack.Pop();
			while (enumerator.MoveNext())
			{
				TypeDef type = enumerator.Current;
				if (!visited.ContainsKey(type))
				{
					visited[type] = true;
					yield return type;
					if (type.NestedTypes.Count > 0)
					{
						stack.Push(enumerator);
						enumerator = type.NestedTypes.GetEnumerator();
					}
				}
			}
		}
	}
}
