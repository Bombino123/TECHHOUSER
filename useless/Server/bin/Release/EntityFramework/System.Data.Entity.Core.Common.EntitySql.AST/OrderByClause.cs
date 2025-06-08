namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class OrderByClause : Node
{
	private readonly NodeList<OrderByClauseItem> _orderByClauseItem;

	private readonly Node _skipExpr;

	private readonly Node _limitExpr;

	private readonly uint _methodCallCount;

	internal NodeList<OrderByClauseItem> OrderByClauseItem => _orderByClauseItem;

	internal Node SkipSubClause => _skipExpr;

	internal Node LimitSubClause => _limitExpr;

	internal bool HasMethodCall => _methodCallCount != 0;

	internal OrderByClause(NodeList<OrderByClauseItem> orderByClauseItem, Node skipExpr, Node limitExpr, uint methodCallCount)
	{
		_orderByClauseItem = orderByClauseItem;
		_skipExpr = skipExpr;
		_limitExpr = limitExpr;
		_methodCallCount = methodCallCount;
	}
}
