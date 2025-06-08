namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class FromClause : Node
{
	private readonly NodeList<FromClauseItem> _fromClauseItems;

	internal NodeList<FromClauseItem> FromClauseItems => _fromClauseItems;

	internal FromClause(NodeList<FromClauseItem> fromClauseItems)
	{
		_fromClauseItems = fromClauseItems;
	}
}
