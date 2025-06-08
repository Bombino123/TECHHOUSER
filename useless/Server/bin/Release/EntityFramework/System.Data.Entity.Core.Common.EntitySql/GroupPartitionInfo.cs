using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.EntitySql.AST;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class GroupPartitionInfo : GroupAggregateInfo
{
	internal DbExpression AggregateDefinition;

	internal GroupPartitionInfo(GroupPartitionExpr groupPartitionExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
		: base(GroupAggregateKind.Partition, groupPartitionExpr, errCtx, containingAggregate, definingScopeRegion)
	{
	}

	internal void AttachToAstNode(string aggregateName, DbExpression aggregateDefinition)
	{
		AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
		AggregateDefinition = aggregateDefinition;
	}
}
