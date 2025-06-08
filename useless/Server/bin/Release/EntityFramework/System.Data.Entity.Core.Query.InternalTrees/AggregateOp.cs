using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class AggregateOp : ScalarOp
{
	private readonly EdmFunction m_aggFunc;

	private readonly bool m_distinctAgg;

	internal static readonly AggregateOp Pattern = new AggregateOp();

	internal EdmFunction AggFunc => m_aggFunc;

	internal bool IsDistinctAggregate => m_distinctAgg;

	internal override bool IsAggregateOp => true;

	internal AggregateOp(EdmFunction aggFunc, bool distinctAgg)
		: base(OpType.Aggregate, aggFunc.ReturnParameter.TypeUsage)
	{
		m_aggFunc = aggFunc;
		m_distinctAgg = distinctAgg;
	}

	private AggregateOp()
		: base(OpType.Aggregate)
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
