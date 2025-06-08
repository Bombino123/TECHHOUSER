namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class FromClauseItem : Node
{
	private readonly Node _fromClauseItemExpr;

	private readonly FromClauseItemKind _fromClauseItemKind;

	internal Node FromExpr => _fromClauseItemExpr;

	internal FromClauseItemKind FromClauseItemKind => _fromClauseItemKind;

	internal FromClauseItem(AliasedExpr aliasExpr)
	{
		_fromClauseItemExpr = aliasExpr;
		_fromClauseItemKind = FromClauseItemKind.AliasedFromClause;
	}

	internal FromClauseItem(JoinClauseItem joinClauseItem)
	{
		_fromClauseItemExpr = joinClauseItem;
		_fromClauseItemKind = FromClauseItemKind.JoinFromClause;
	}

	internal FromClauseItem(ApplyClauseItem applyClauseItem)
	{
		_fromClauseItemExpr = applyClauseItem;
		_fromClauseItemKind = FromClauseItemKind.ApplyFromClause;
	}
}
