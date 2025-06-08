using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

internal abstract class ScopeEntry
{
	private readonly ScopeEntryKind _scopeEntryKind;

	internal ScopeEntryKind EntryKind => _scopeEntryKind;

	internal ScopeEntry(ScopeEntryKind scopeEntryKind)
	{
		_scopeEntryKind = scopeEntryKind;
	}

	internal abstract DbExpression GetExpression(string refName, ErrorContext errCtx);
}
