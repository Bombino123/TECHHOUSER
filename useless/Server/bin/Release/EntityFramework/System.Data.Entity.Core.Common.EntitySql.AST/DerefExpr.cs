namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class DerefExpr : Node
{
	private readonly Node _argExpr;

	internal Node ArgExpr => _argExpr;

	internal DerefExpr(Node derefArgExpr)
	{
		_argExpr = derefArgExpr;
	}
}
