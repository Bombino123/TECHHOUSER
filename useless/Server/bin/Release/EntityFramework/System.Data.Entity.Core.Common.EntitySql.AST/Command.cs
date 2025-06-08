namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class Command : Node
{
	private readonly NodeList<NamespaceImport> _namespaceImportList;

	private readonly Statement _statement;

	internal NodeList<NamespaceImport> NamespaceImportList => _namespaceImportList;

	internal Statement Statement => _statement;

	internal Command(NodeList<NamespaceImport> nsImportList, Statement statement)
	{
		_namespaceImportList = nsImportList;
		_statement = statement;
	}
}
