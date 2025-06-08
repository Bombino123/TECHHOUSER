using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class AliasedExpr : Node
{
	private readonly Node _expr;

	private readonly Identifier _alias;

	internal Node Expr => _expr;

	internal Identifier Alias => _alias;

	internal AliasedExpr(Node expr, Identifier alias)
	{
		if (string.IsNullOrEmpty(alias.Name))
		{
			ErrorContext errCtx = alias.ErrCtx;
			string invalidEmptyIdentifier = Strings.InvalidEmptyIdentifier;
			throw EntitySqlException.Create(errCtx, invalidEmptyIdentifier, null);
		}
		_expr = expr;
		_alias = alias;
	}

	internal AliasedExpr(Node expr)
	{
		_expr = expr;
	}
}
