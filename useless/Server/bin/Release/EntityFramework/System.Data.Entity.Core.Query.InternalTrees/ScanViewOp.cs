using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ScanViewOp : ScanTableBaseOp
{
	internal static readonly ScanViewOp Pattern = new ScanViewOp();

	internal override int Arity => 1;

	internal ScanViewOp(Table table)
		: base(OpType.ScanView, table)
	{
	}

	private ScanViewOp()
		: base(OpType.ScanView)
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
