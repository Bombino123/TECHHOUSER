using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class UnionAllOp : SetOp
{
	private readonly Var m_branchDiscriminator;

	internal static readonly UnionAllOp Pattern = new UnionAllOp();

	internal Var BranchDiscriminator => m_branchDiscriminator;

	private UnionAllOp()
		: base(OpType.UnionAll)
	{
	}

	internal UnionAllOp(VarVec outputs, VarMap left, VarMap right, Var branchDiscriminator)
		: base(OpType.UnionAll, outputs, left, right)
	{
		m_branchDiscriminator = branchDiscriminator;
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
