using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ScanTableOp : ScanTableBaseOp
{
	internal static readonly ScanTableOp Pattern = new ScanTableOp();

	internal override int Arity => 0;

	internal ScanTableOp(Table table)
		: base(OpType.ScanTable, table)
	{
	}

	private ScanTableOp()
		: base(OpType.ScanTable)
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
