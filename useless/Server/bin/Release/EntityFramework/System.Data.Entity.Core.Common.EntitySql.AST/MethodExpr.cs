namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class MethodExpr : GroupAggregateExpr
{
	private readonly Node _expr;

	private readonly NodeList<Node> _args;

	private readonly NodeList<RelshipNavigationExpr> _relationships;

	internal Node Expr => _expr;

	internal NodeList<Node> Args => _args;

	internal bool HasRelationships
	{
		get
		{
			if (_relationships != null)
			{
				return _relationships.Count > 0;
			}
			return false;
		}
	}

	internal NodeList<RelshipNavigationExpr> Relationships => _relationships;

	internal MethodExpr(Node expr, DistinctKind distinctKind, NodeList<Node> args)
		: this(expr, distinctKind, args, null)
	{
	}

	internal MethodExpr(Node expr, DistinctKind distinctKind, NodeList<Node> args, NodeList<RelshipNavigationExpr> relationships)
		: base(distinctKind)
	{
		_expr = expr;
		_args = args;
		_relationships = relationships;
	}
}
