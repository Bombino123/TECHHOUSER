namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal abstract class GroupAggregateExpr : Node
{
	internal readonly DistinctKind DistinctKind;

	internal GroupAggregateInfo AggregateInfo;

	internal GroupAggregateExpr(DistinctKind distinctKind)
	{
		DistinctKind = distinctKind;
	}
}
