using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

[DebuggerDisplay("{{{ToString()}}}")]
internal class VarList : List<Var>
{
	internal VarList()
	{
	}

	internal VarList(IEnumerable<Var> vars)
		: base(vars)
	{
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Var current = enumerator.Current;
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", new object[2] { text, current.Id });
				text = ",";
			}
		}
		return stringBuilder.ToString();
	}
}
