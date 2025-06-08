using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class CrossApplyOp : ApplyBaseOp
{
	internal static readonly CrossApplyOp Instance = new CrossApplyOp();

	internal static readonly CrossApplyOp Pattern = Instance;

	private CrossApplyOp()
		: base(OpType.CrossApply)
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
