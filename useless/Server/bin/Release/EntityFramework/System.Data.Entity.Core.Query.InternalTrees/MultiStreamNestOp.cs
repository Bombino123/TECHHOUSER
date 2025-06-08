using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class MultiStreamNestOp : NestBaseOp
{
	[DebuggerNonUserCode]
	internal override void Accept(BasicOpVisitor v, Node n)
	{
		v.Visit(this, n);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
	{
		return v.Visit(this, n);
	}

	internal MultiStreamNestOp(List<SortKey> prefixSortKeys, VarVec outputVars, List<CollectionInfo> collectionInfoList)
		: base(OpType.MultiStreamNest, prefixSortKeys, outputVars, collectionInfoList)
	{
	}
}
