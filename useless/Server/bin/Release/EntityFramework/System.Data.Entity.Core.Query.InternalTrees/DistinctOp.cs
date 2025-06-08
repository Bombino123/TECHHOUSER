using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class DistinctOp : RelOp
{
	private readonly VarVec m_keys;

	internal static readonly DistinctOp Pattern = new DistinctOp();

	internal override int Arity => 1;

	internal VarVec Keys => m_keys;

	private DistinctOp()
		: base(OpType.Distinct)
	{
	}

	internal DistinctOp(VarVec keyVars)
		: this()
	{
		m_keys = keyVars;
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
