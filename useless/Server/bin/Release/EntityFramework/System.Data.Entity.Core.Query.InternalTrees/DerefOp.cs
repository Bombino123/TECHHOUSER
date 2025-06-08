using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class DerefOp : ScalarOp
{
	internal static readonly DerefOp Pattern = new DerefOp();

	internal override int Arity => 1;

	internal DerefOp(TypeUsage type)
		: base(OpType.Deref, type)
	{
	}

	private DerefOp()
		: base(OpType.Deref)
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
