namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal enum JoinKind
{
	Cross,
	Inner,
	LeftOuter,
	FullOuter,
	RightOuter
}
