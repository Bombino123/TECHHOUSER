namespace System.Data.Entity.Core.Common.EntitySql;

internal enum ScopeEntryKind
{
	SourceVar,
	GroupKeyDefinition,
	ProjectionItemDefinition,
	FreeVar,
	InvalidGroupInputRef
}
