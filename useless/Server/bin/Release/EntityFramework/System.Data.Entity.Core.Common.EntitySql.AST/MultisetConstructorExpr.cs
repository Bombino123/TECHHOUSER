namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class MultisetConstructorExpr : Node
{
	private readonly NodeList<Node> _exprList;

	internal NodeList<Node> ExprList => _exprList;

	internal MultisetConstructorExpr(NodeList<Node> exprList)
	{
		_exprList = exprList;
	}
}
