using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class LikeOp : ScalarOp
{
	internal static readonly LikeOp Pattern = new LikeOp();

	internal override int Arity => 3;

	internal LikeOp(TypeUsage boolType)
		: base(OpType.Like, boolType)
	{
	}

	private LikeOp()
		: base(OpType.Like)
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
