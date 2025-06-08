using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ProjectOp : RelOp
{
	private readonly VarVec m_vars;

	internal static readonly ProjectOp Pattern = new ProjectOp();

	internal override int Arity => 2;

	internal VarVec Outputs => m_vars;

	private ProjectOp()
		: base(OpType.Project)
	{
	}

	internal ProjectOp(VarVec vars)
		: this()
	{
		m_vars = vars;
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
