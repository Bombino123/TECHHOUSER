using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class VarRefOp : ScalarOp
{
	private readonly Var m_var;

	internal static readonly VarRefOp Pattern = new VarRefOp();

	internal override int Arity => 0;

	internal Var Var => m_var;

	internal VarRefOp(Var v)
		: base(OpType.VarRef, v.Type)
	{
		m_var = v;
	}

	private VarRefOp()
		: base(OpType.VarRef)
	{
	}

	internal override bool IsEquivalent(Op other)
	{
		if (other is VarRefOp varRefOp)
		{
			return varRefOp.Var.Equals(Var);
		}
		return false;
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
