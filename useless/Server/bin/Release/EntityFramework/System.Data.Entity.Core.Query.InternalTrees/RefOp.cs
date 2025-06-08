using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class RefOp : ScalarOp
{
	private readonly EntitySet m_entitySet;

	internal static readonly RefOp Pattern = new RefOp();

	internal override int Arity => 1;

	internal EntitySet EntitySet => m_entitySet;

	internal RefOp(EntitySet entitySet, TypeUsage type)
		: base(OpType.Ref, type)
	{
		m_entitySet = entitySet;
	}

	private RefOp()
		: base(OpType.Ref)
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
