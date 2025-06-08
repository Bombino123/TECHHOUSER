using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class GroupByOp : GroupByBaseOp
{
	internal static readonly GroupByOp Pattern = new GroupByOp();

	internal override int Arity => 3;

	private GroupByOp()
		: base(OpType.GroupBy)
	{
	}

	internal GroupByOp(VarVec keys, VarVec outputs)
		: base(OpType.GroupBy, keys, outputs)
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
