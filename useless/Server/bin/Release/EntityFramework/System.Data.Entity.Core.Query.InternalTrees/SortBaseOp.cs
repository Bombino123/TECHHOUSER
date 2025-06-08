using System.Collections.Generic;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class SortBaseOp : RelOp
{
	private readonly List<SortKey> m_keys;

	internal List<SortKey> Keys => m_keys;

	internal SortBaseOp(OpType opType)
		: base(opType)
	{
	}

	internal SortBaseOp(OpType opType, List<SortKey> sortKeys)
		: this(opType)
	{
		m_keys = sortKeys;
	}
}
