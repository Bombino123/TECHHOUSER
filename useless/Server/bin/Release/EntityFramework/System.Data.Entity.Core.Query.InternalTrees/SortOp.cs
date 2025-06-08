using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class SortOp : SortBaseOp
{
	internal static readonly SortOp Pattern = new SortOp();

	internal override int Arity => 1;

	private SortOp()
		: base(OpType.Sort)
	{
	}

	internal SortOp(List<SortKey> sortKeys)
		: base(OpType.Sort, sortKeys)
	{
	}

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
}
