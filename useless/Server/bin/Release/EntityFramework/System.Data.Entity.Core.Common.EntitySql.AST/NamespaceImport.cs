using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class NamespaceImport : Node
{
	private readonly Identifier _namespaceAlias;

	private readonly Node _namespaceName;

	internal Identifier Alias => _namespaceAlias;

	internal Node NamespaceName => _namespaceName;

	internal NamespaceImport(Identifier identifier)
	{
		_namespaceName = identifier;
	}

	internal NamespaceImport(DotExpr dorExpr)
	{
		_namespaceName = dorExpr;
	}

	internal NamespaceImport(BuiltInExpr bltInExpr)
	{
		_namespaceAlias = null;
		if (!(bltInExpr.Arg1 is Identifier namespaceAlias))
		{
			ErrorContext errCtx = bltInExpr.Arg1.ErrCtx;
			string invalidNamespaceAlias = Strings.InvalidNamespaceAlias;
			throw EntitySqlException.Create(errCtx, invalidNamespaceAlias, null);
		}
		_namespaceAlias = namespaceAlias;
		_namespaceName = bltInExpr.Arg2;
	}
}
