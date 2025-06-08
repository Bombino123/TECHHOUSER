using System.Globalization;

namespace System.Data.SQLite.EF6;

internal class TopClause : ISqlFragment
{
	private ISqlFragment topCount;

	private bool withTies;

	internal bool WithTies => withTies;

	internal ISqlFragment TopCount => topCount;

	internal TopClause(ISqlFragment topCount, bool withTies)
	{
		this.topCount = topCount;
		this.withTies = withTies;
	}

	internal TopClause(int topCount, bool withTies)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(topCount.ToString(CultureInfo.InvariantCulture));
		this.topCount = sqlBuilder;
		this.withTies = withTies;
	}

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		writer.Write(" LIMIT ");
		TopCount.WriteSql(writer, sqlGenerator);
		if (WithTies)
		{
			throw new NotSupportedException("WITH TIES");
		}
	}
}
