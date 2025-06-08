namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal class WhenThenExpr : Node
{
	private readonly Node _whenExpr;

	private readonly Node _thenExpr;

	internal Node WhenExpr => _whenExpr;

	internal Node ThenExpr => _thenExpr;

	internal WhenThenExpr(Node whenExpr, Node thenExpr)
	{
		_whenExpr = whenExpr;
		_thenExpr = thenExpr;
	}
}
