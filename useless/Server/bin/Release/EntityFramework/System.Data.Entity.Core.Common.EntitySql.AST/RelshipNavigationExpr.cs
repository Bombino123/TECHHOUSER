namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class RelshipNavigationExpr : Node
{
	private readonly Node _refExpr;

	private readonly Node _relshipTypeName;

	private readonly Identifier _toEndIdentifier;

	private readonly Identifier _fromEndIdentifier;

	internal Node RefExpr => _refExpr;

	internal Node TypeName => _relshipTypeName;

	internal Identifier ToEndIdentifier => _toEndIdentifier;

	internal Identifier FromEndIdentifier => _fromEndIdentifier;

	internal RelshipNavigationExpr(Node refExpr, Node relshipTypeName, Identifier toEndIdentifier, Identifier fromEndIdentifier)
	{
		_refExpr = refExpr;
		_relshipTypeName = relshipTypeName;
		_toEndIdentifier = toEndIdentifier;
		_fromEndIdentifier = fromEndIdentifier;
	}
}
