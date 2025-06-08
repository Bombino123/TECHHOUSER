using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class Pair<L, R>
{
	internal L Left;

	internal R Right;

	internal Pair(L left, R right)
	{
		Left = left;
		Right = right;
	}

	internal KeyValuePair<L, R> GetKVP()
	{
		return new KeyValuePair<L, R>(Left, Right);
	}
}
