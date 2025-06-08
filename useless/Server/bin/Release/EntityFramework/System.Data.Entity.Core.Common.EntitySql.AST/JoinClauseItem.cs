namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class JoinClauseItem : Node
{
	private readonly FromClauseItem _joinLeft;

	private readonly FromClauseItem _joinRight;

	private readonly Node _onExpr;

	internal FromClauseItem LeftExpr => _joinLeft;

	internal FromClauseItem RightExpr => _joinRight;

	internal JoinKind JoinKind { get; set; }

	internal Node OnExpr => _onExpr;

	internal JoinClauseItem(FromClauseItem joinLeft, FromClauseItem joinRight, JoinKind joinKind)
		: this(joinLeft, joinRight, joinKind, null)
	{
	}

	internal JoinClauseItem(FromClauseItem joinLeft, FromClauseItem joinRight, JoinKind joinKind, Node onExpr)
	{
		_joinLeft = joinLeft;
		_joinRight = joinRight;
		JoinKind = joinKind;
		_onExpr = onExpr;
	}
}
