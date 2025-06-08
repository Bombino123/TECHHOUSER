using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class GroupByIntoOp : GroupByBaseOp
{
	private readonly VarVec m_inputs;

	internal static readonly GroupByIntoOp Pattern = new GroupByIntoOp();

	internal VarVec Inputs => m_inputs;

	internal override int Arity => 4;

	private GroupByIntoOp()
		: base(OpType.GroupByInto)
	{
	}

	internal GroupByIntoOp(VarVec keys, VarVec inputs, VarVec outputs)
		: base(OpType.GroupByInto, keys, outputs)
	{
		m_inputs = inputs;
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
