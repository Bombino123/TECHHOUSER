using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class VarDefOp : AncillaryOp
{
	private readonly Var m_var;

	internal static readonly VarDefOp Pattern = new VarDefOp();

	internal override int Arity => 1;

	internal Var Var => m_var;

	internal VarDefOp(Var v)
		: this()
	{
		m_var = v;
	}

	private VarDefOp()
		: base(OpType.VarDef)
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
