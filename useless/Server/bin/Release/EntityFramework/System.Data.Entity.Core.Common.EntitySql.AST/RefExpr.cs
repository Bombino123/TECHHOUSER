namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class RefExpr : Node
{
	private readonly Node _argExpr;

	internal Node ArgExpr => _argExpr;

	internal RefExpr(Node refArgExpr)
	{
		_argExpr = refArgExpr;
	}
}
