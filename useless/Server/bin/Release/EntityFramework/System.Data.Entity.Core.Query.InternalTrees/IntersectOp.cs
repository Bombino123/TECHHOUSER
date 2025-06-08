using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class IntersectOp : SetOp
{
	internal static readonly IntersectOp Pattern = new IntersectOp();

	private IntersectOp()
		: base(OpType.Intersect)
	{
	}

	internal IntersectOp(VarVec outputs, VarMap left, VarMap right)
		: base(OpType.Intersect, outputs, left, right)
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
