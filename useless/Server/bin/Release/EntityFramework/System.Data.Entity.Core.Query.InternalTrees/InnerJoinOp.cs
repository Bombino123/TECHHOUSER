using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class InnerJoinOp : JoinBaseOp
{
	internal static readonly InnerJoinOp Instance = new InnerJoinOp();

	internal static readonly InnerJoinOp Pattern = Instance;

	private InnerJoinOp()
		: base(OpType.InnerJoin)
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
