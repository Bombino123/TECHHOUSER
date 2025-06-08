using System.Globalization;

namespace System.Data.SQLite.Linq;

internal class SkipClause : ISqlFragment
{
	private ISqlFragment skipCount;

	internal ISqlFragment SkipCount => skipCount;

	internal SkipClause(ISqlFragment skipCount)
	{
		this.skipCount = skipCount;
	}

	internal SkipClause(int skipCount)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(skipCount.ToString(CultureInfo.InvariantCulture));
		this.skipCount = sqlBuilder;
	}

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		writer.Write(" OFFSET ");
		SkipCount.WriteSql(writer, sqlGenerator);
	}
}
