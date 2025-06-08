namespace System.Data.SQLite;

[Flags]
internal enum SQLiteTraceFlags
{
	SQLITE_TRACE_NONE = 0,
	SQLITE_TRACE_STMT = 1,
	SQLITE_TRACE_PROFILE = 2,
	SQLITE_TRACE_ROW = 4,
	SQLITE_TRACE_CLOSE = 8
}
