using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class FreeVariableScopeEntry : ScopeEntry
{
	private readonly DbVariableReferenceExpression _varRef;

	internal FreeVariableScopeEntry(DbVariableReferenceExpression varRef)
		: base(ScopeEntryKind.FreeVar)
	{
		_varRef = varRef;
	}

	internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
	{
		return _varRef;
	}
}
