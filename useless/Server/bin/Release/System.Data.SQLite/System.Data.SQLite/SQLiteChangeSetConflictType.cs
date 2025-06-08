namespace System.Data.SQLite;

public enum SQLiteChangeSetConflictType
{
	Data = 1,
	NotFound,
	Conflict,
	Constraint,
	ForeignKey
}
