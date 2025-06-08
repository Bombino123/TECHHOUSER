using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class LeftOuterJoinOp : JoinBaseOp
{
	internal static readonly LeftOuterJoinOp Instance = new LeftOuterJoinOp();

	internal static readonly LeftOuterJoinOp Pattern = Instance;

	private LeftOuterJoinOp()
		: base(OpType.LeftOuterJoin)
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
