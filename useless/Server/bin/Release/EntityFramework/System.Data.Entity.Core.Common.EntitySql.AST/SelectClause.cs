namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class SelectClause : Node
{
	private readonly NodeList<AliasedExpr> _selectClauseItems;

	private readonly SelectKind _selectKind;

	private readonly DistinctKind _distinctKind;

	private readonly Node _topExpr;

	private readonly uint _methodCallCount;

	internal NodeList<AliasedExpr> Items => _selectClauseItems;

	internal SelectKind SelectKind => _selectKind;

	internal DistinctKind DistinctKind => _distinctKind;

	internal Node TopExpr => _topExpr;

	internal bool HasMethodCall => _methodCallCount != 0;

	internal SelectClause(NodeList<AliasedExpr> items, SelectKind selectKind, DistinctKind distinctKind, Node topExpr, uint methodCallCount)
	{
		_selectKind = selectKind;
		_selectClauseItems = items;
		_distinctKind = distinctKind;
		_topExpr = topExpr;
		_methodCallCount = methodCallCount;
	}
}
