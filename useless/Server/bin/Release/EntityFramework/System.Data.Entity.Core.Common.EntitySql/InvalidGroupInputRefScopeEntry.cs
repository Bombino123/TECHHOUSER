using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class InvalidGroupInputRefScopeEntry : ScopeEntry
{
	internal InvalidGroupInputRefScopeEntry()
		: base(ScopeEntryKind.InvalidGroupInputRef)
	{
	}

	internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
	{
		string errorMessage = Strings.InvalidGroupIdentifierReference(refName);
		throw EntitySqlException.Create(errCtx, errorMessage, null);
	}
}
