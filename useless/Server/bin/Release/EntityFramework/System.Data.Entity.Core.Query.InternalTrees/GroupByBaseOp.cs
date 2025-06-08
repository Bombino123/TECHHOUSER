using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class GroupByBaseOp : RelOp
{
	private readonly VarVec m_keys;

	private readonly VarVec m_outputs;

	internal VarVec Keys => m_keys;

	internal VarVec Outputs => m_outputs;

	protected GroupByBaseOp(OpType opType)
		: base(opType)
	{
	}

	internal GroupByBaseOp(OpType opType, VarVec keys, VarVec outputs)
		: this(opType)
	{
		m_keys = keys;
		m_outputs = outputs;
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
