namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal class KeyExpr : Node
{
	private readonly Node _argExpr;

	internal Node ArgExpr => _argExpr;

	internal KeyExpr(Node argExpr)
	{
		_argExpr = argExpr;
	}
}
