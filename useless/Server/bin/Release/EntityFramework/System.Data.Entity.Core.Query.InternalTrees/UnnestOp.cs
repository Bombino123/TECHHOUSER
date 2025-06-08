using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class UnnestOp : RelOp
{
	private readonly Table m_table;

	private readonly Var m_var;

	internal static readonly UnnestOp Pattern = new UnnestOp();

	internal Var Var => m_var;

	internal Table Table => m_table;

	internal override int Arity => 1;

	internal UnnestOp(Var v, Table t)
		: this()
	{
		m_var = v;
		m_table = t;
	}

	private UnnestOp()
		: base(OpType.Unnest)
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
