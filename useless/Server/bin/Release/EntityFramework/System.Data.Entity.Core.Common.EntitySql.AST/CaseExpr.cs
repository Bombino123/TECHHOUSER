namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class CaseExpr : Node
{
	private readonly NodeList<WhenThenExpr> _whenThenExpr;

	private readonly Node _elseExpr;

	internal NodeList<WhenThenExpr> WhenThenExprList => _whenThenExpr;

	internal Node ElseExpr => _elseExpr;

	internal CaseExpr(NodeList<WhenThenExpr> whenThenExpr)
		: this(whenThenExpr, null)
	{
	}

	internal CaseExpr(NodeList<WhenThenExpr> whenThenExpr, Node elseExpr)
	{
		_whenThenExpr = whenThenExpr;
		_elseExpr = elseExpr;
	}
}
