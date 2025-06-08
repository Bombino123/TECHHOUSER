using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class GetEntityRefOp : ScalarOp
{
	internal static readonly GetEntityRefOp Pattern = new GetEntityRefOp();

	internal override int Arity => 1;

	internal GetEntityRefOp(TypeUsage type)
		: base(OpType.GetEntityRef, type)
	{
	}

	private GetEntityRefOp()
		: base(OpType.GetEntityRef)
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
