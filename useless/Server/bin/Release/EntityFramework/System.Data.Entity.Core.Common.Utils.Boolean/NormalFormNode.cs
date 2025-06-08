namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class NormalFormNode<T_Identifier>
{
	private readonly BoolExpr<T_Identifier> _expr;

	internal BoolExpr<T_Identifier> Expr => _expr;

	protected NormalFormNode(BoolExpr<T_Identifier> expr)
	{
		_expr = expr.Simplify();
	}

	protected static BoolExpr<T_Identifier> ExprSelector<T_NormalFormNode>(T_NormalFormNode node) where T_NormalFormNode : NormalFormNode<T_Identifier>
	{
		return node._expr;
	}
}
