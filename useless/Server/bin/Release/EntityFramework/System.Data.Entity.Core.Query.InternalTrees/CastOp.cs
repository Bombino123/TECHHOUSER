using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class CastOp : ScalarOp
{
	internal static readonly CastOp Pattern = new CastOp();

	internal override int Arity => 1;

	internal CastOp(TypeUsage type)
		: base(OpType.Cast, type)
	{
	}

	private CastOp()
		: base(OpType.Cast)
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
