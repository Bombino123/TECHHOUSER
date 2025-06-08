namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class QueryStatement : Statement
{
	private readonly NodeList<FunctionDefinition> _functionDefList;

	private readonly Node _expr;

	internal NodeList<FunctionDefinition> FunctionDefList => _functionDefList;

	internal Node Expr => _expr;

	internal QueryStatement(NodeList<FunctionDefinition> functionDefList, Node expr)
	{
		_functionDefList = functionDefList;
		_expr = expr;
	}
}
