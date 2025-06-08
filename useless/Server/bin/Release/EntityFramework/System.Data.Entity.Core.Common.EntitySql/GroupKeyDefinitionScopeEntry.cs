using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class GroupKeyDefinitionScopeEntry : ScopeEntry, IGroupExpressionExtendedInfo, IGetAlternativeName
{
	private readonly DbExpression _varBasedExpression;

	private readonly DbExpression _groupVarBasedExpression;

	private readonly DbExpression _groupAggBasedExpression;

	private readonly string[] _alternativeName;

	DbExpression IGroupExpressionExtendedInfo.GroupVarBasedExpression => _groupVarBasedExpression;

	DbExpression IGroupExpressionExtendedInfo.GroupAggBasedExpression => _groupAggBasedExpression;

	string[] IGetAlternativeName.AlternativeName => _alternativeName;

	internal GroupKeyDefinitionScopeEntry(DbExpression varBasedExpression, DbExpression groupVarBasedExpression, DbExpression groupAggBasedExpression, string[] alternativeName)
		: base(ScopeEntryKind.GroupKeyDefinition)
	{
		_varBasedExpression = varBasedExpression;
		_groupVarBasedExpression = groupVarBasedExpression;
		_groupAggBasedExpression = groupAggBasedExpression;
		_alternativeName = alternativeName;
	}

	internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
	{
		return _varBasedExpression;
	}
}
