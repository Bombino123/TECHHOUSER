namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class GroupPartitionExpr : GroupAggregateExpr
{
	private readonly Node _argExpr;

	internal Node ArgExpr => _argExpr;

	internal GroupPartitionExpr(DistinctKind distinctKind, Node refArgExpr)
		: base(distinctKind)
	{
		_argExpr = refArgExpr;
	}
}
