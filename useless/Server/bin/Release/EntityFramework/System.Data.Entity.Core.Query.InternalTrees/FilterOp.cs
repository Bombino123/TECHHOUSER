using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class FilterOp : RelOp
{
	internal static readonly FilterOp Instance = new FilterOp();

	internal static readonly FilterOp Pattern = Instance;

	internal override int Arity => 2;

	private FilterOp()
		: base(OpType.Filter)
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
