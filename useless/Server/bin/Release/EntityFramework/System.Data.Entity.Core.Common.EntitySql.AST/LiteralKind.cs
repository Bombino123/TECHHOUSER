namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal enum LiteralKind
{
	Number,
	String,
	UnicodeString,
	Boolean,
	Binary,
	DateTime,
	Time,
	DateTimeOffset,
	Guid,
	Null
}
