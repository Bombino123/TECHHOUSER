namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class ParenExpr : Node
{
	private readonly Node _expr;

	internal Node Expr => _expr;

	internal ParenExpr(Node expr)
	{
		_expr = expr;
	}
}
