using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class GetRefKeyOp : ScalarOp
{
	internal static readonly GetRefKeyOp Pattern = new GetRefKeyOp();

	internal override int Arity => 1;

	internal GetRefKeyOp(TypeUsage type)
		: base(OpType.GetRefKey, type)
	{
	}

	private GetRefKeyOp()
		: base(OpType.GetRefKey)
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
