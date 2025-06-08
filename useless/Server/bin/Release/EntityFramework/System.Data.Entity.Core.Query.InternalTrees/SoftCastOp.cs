using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class SoftCastOp : ScalarOp
{
	internal static readonly SoftCastOp Pattern = new SoftCastOp();

	internal override int Arity => 1;

	internal SoftCastOp(TypeUsage type)
		: base(OpType.SoftCast, type)
	{
	}

	private SoftCastOp()
		: base(OpType.SoftCast)
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
