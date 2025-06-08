using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class CrossJoinOp : JoinBaseOp
{
	internal static readonly CrossJoinOp Instance = new CrossJoinOp();

	internal static readonly CrossJoinOp Pattern = Instance;

	internal override int Arity => -1;

	private CrossJoinOp()
		: base(OpType.CrossJoin)
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
