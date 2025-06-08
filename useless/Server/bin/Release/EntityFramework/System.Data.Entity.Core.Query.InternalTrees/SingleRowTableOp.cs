using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class SingleRowTableOp : RelOp
{
	internal static readonly SingleRowTableOp Instance = new SingleRowTableOp();

	internal static readonly SingleRowTableOp Pattern = Instance;

	internal override int Arity => 0;

	private SingleRowTableOp()
		: base(OpType.SingleRowTable)
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
