namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class CreateRefExpr : Node
{
	private readonly Node _entitySet;

	private readonly Node _keys;

	private readonly Node _typeIdentifier;

	internal Node EntitySet => _entitySet;

	internal Node Keys => _keys;

	internal Node TypeIdentifier => _typeIdentifier;

	internal CreateRefExpr(Node entitySet, Node keys)
		: this(entitySet, keys, null)
	{
	}

	internal CreateRefExpr(Node entitySet, Node keys, Node typeIdentifier)
	{
		_entitySet = entitySet;
		_keys = keys;
		_typeIdentifier = typeIdentifier;
	}
}
