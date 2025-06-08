using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.EntitySql.AST;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class FunctionAggregateInfo : GroupAggregateInfo
{
	internal DbAggregate AggregateDefinition;

	internal FunctionAggregateInfo(MethodExpr methodExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
		: base(GroupAggregateKind.Function, methodExpr, errCtx, containingAggregate, definingScopeRegion)
	{
	}

	internal void AttachToAstNode(string aggregateName, DbAggregate aggregateDefinition)
	{
		AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
		AggregateDefinition = aggregateDefinition;
	}
}
