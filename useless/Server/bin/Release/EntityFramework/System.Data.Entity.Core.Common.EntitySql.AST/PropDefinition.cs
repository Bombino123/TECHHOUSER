namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class PropDefinition : Node
{
	private readonly Identifier _name;

	private readonly Node _typeDefExpr;

	internal Identifier Name => _name;

	internal Node Type => _typeDefExpr;

	internal PropDefinition(Identifier name, Node typeDefExpr)
	{
		_name = name;
		_typeDefExpr = typeDefExpr;
	}
}
