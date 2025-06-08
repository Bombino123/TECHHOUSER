namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class ApplyClauseItem : Node
{
	private readonly FromClauseItem _applyLeft;

	private readonly FromClauseItem _applyRight;

	private readonly ApplyKind _applyKind;

	internal FromClauseItem LeftExpr => _applyLeft;

	internal FromClauseItem RightExpr => _applyRight;

	internal ApplyKind ApplyKind => _applyKind;

	internal ApplyClauseItem(FromClauseItem applyLeft, FromClauseItem applyRight, ApplyKind applyKind)
	{
		_applyLeft = applyLeft;
		_applyRight = applyRight;
		_applyKind = applyKind;
	}
}
