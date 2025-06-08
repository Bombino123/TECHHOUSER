namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class CollectionTypeDefinition : Node
{
	private readonly Node _elementTypeDef;

	internal Node ElementTypeDef => _elementTypeDef;

	internal CollectionTypeDefinition(Node elementTypeDef)
	{
		_elementTypeDef = elementTypeDef;
	}
}
