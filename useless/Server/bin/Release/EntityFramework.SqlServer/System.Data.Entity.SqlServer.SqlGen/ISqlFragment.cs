namespace System.Data.Entity.SqlServer.SqlGen;

internal interface ISqlFragment
{
	void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
}
