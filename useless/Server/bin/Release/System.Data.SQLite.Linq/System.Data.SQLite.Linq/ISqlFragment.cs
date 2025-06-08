namespace System.Data.SQLite.Linq;

internal interface ISqlFragment
{
	void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
}
