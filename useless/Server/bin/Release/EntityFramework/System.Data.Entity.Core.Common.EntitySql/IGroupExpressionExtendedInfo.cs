using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

internal interface IGroupExpressionExtendedInfo
{
	DbExpression GroupVarBasedExpression { get; }

	DbExpression GroupAggBasedExpression { get; }
}
