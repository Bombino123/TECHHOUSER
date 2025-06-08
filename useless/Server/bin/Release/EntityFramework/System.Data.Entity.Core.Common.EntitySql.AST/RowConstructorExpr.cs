namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class RowConstructorExpr : Node
{
	private readonly NodeList<AliasedExpr> _exprList;

	internal NodeList<AliasedExpr> AliasedExprList => _exprList;

	internal RowConstructorExpr(NodeList<AliasedExpr> exprList)
	{
		_exprList = exprList;
	}
}
