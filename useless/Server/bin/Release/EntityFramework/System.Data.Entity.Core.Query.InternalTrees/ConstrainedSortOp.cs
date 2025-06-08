using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ConstrainedSortOp : SortBaseOp
{
	internal static readonly ConstrainedSortOp Pattern = new ConstrainedSortOp();

	internal bool WithTies { get; set; }

	internal override int Arity => 3;

	private ConstrainedSortOp()
		: base(OpType.ConstrainedSort)
	{
	}

	internal ConstrainedSortOp(List<SortKey> sortKeys, bool withTies)
		: base(OpType.ConstrainedSort, sortKeys)
	{
		WithTies = withTies;
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
