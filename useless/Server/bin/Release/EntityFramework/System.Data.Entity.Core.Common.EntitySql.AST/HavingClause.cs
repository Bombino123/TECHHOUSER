namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class HavingClause : Node
{
	private readonly Node _havingExpr;

	private readonly uint _methodCallCount;

	internal Node HavingPredicate => _havingExpr;

	internal bool HasMethodCall => _methodCallCount != 0;

	internal HavingClause(Node havingExpr, uint methodCallCounter)
	{
		_havingExpr = havingExpr;
		_methodCallCount = methodCallCounter;
	}
}
