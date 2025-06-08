namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class GroupByClause : Node
{
	private readonly NodeList<AliasedExpr> _groupItems;

	internal NodeList<AliasedExpr> GroupItems => _groupItems;

	internal GroupByClause(NodeList<AliasedExpr> groupItems)
	{
		_groupItems = groupItems;
	}
}
