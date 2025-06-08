using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class FullOuterJoinOp : JoinBaseOp
{
	internal static readonly FullOuterJoinOp Instance = new FullOuterJoinOp();

	internal static readonly FullOuterJoinOp Pattern = Instance;

	private FullOuterJoinOp()
		: base(OpType.FullOuterJoin)
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
