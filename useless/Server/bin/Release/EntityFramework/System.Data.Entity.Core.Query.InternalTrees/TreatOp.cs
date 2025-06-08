using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class TreatOp : ScalarOp
{
	private readonly bool m_isFake;

	internal static readonly TreatOp Pattern = new TreatOp();

	internal override int Arity => 1;

	internal bool IsFakeTreat => m_isFake;

	internal TreatOp(TypeUsage type, bool isFake)
		: base(OpType.Treat, type)
	{
		m_isFake = isFake;
	}

	private TreatOp()
		: base(OpType.Treat)
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
