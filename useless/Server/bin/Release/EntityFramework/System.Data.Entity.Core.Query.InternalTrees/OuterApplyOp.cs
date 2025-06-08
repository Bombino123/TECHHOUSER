using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class OuterApplyOp : ApplyBaseOp
{
	internal static readonly OuterApplyOp Instance = new OuterApplyOp();

	internal static readonly OuterApplyOp Pattern = Instance;

	private OuterApplyOp()
		: base(OpType.OuterApply)
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
