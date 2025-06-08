namespace System.Data.SQLite.EF6;

internal interface ISqlFragment
{
	void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
}
