namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class RowTypeDefinition : Node
{
	private readonly NodeList<PropDefinition> _propDefList;

	internal NodeList<PropDefinition> Properties => _propDefList;

	internal RowTypeDefinition(NodeList<PropDefinition> propDefList)
	{
		_propDefList = propDefList;
	}
}
