namespace System.Data.SQLite;

public interface ISQLiteChangeSetMetadataItem : IDisposable
{
	string TableName { get; }

	int NumberOfColumns { get; }

	SQLiteAuthorizerActionCode OperationCode { get; }

	bool Indirect { get; }

	bool[] PrimaryKeyColumns { get; }

	int NumberOfForeignKeyConflicts { get; }

	SQLiteValue GetOldValue(int columnIndex);

	SQLiteValue GetNewValue(int columnIndex);

	SQLiteValue GetConflictValue(int columnIndex);
}
