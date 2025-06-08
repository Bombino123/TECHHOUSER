using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class PhysicalProjectOp : PhysicalOp
{
	internal static readonly PhysicalProjectOp Pattern = new PhysicalProjectOp();

	private readonly SimpleCollectionColumnMap m_columnMap;

	private readonly VarList m_outputVars;

	internal SimpleCollectionColumnMap ColumnMap => m_columnMap;

	internal VarList Outputs => m_outputVars;

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

	internal PhysicalProjectOp(VarList outputVars, SimpleCollectionColumnMap columnMap)
		: this()
	{
		m_outputVars = outputVars;
		m_columnMap = columnMap;
	}

	private PhysicalProjectOp()
		: base(OpType.PhysicalProject)
	{
	}
}
