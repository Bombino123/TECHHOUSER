namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class RefTypeDefinition : Node
{
	private readonly Node _refTypeIdentifier;

	internal Node RefTypeIdentifier => _refTypeIdentifier;

	internal RefTypeDefinition(Node refTypeIdentifier)
	{
		_refTypeIdentifier = refTypeIdentifier;
	}
}
