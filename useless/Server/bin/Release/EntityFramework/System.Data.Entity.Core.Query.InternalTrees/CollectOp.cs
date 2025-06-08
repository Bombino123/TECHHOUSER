using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class CollectOp : ScalarOp
{
	internal static readonly CollectOp Pattern = new CollectOp();

	internal override int Arity => 1;

	internal CollectOp(TypeUsage type)
		: base(OpType.Collect, type)
	{
	}

	private CollectOp()
		: base(OpType.Collect)
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
