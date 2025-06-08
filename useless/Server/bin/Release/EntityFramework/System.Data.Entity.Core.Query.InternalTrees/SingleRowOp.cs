using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class SingleRowOp : RelOp
{
	internal static readonly SingleRowOp Instance = new SingleRowOp();

	internal static readonly SingleRowOp Pattern = Instance;

	internal override int Arity => 1;

	private SingleRowOp()
		: base(OpType.SingleRow)
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
