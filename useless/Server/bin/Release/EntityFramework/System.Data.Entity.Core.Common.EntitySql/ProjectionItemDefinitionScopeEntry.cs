using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class ProjectionItemDefinitionScopeEntry : ScopeEntry
{
	private readonly DbExpression _expression;

	internal ProjectionItemDefinitionScopeEntry(DbExpression expression)
		: base(ScopeEntryKind.ProjectionItemDefinition)
	{
		_expression = expression;
	}

	internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
	{
		return _expression;
	}
}
