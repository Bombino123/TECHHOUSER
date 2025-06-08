using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ElementOp : ScalarOp
{
	internal static readonly ElementOp Pattern = new ElementOp();

	internal override int Arity => 1;

	internal ElementOp(TypeUsage type)
		: base(OpType.Element, type)
	{
	}

	private ElementOp()
		: base(OpType.Element)
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
