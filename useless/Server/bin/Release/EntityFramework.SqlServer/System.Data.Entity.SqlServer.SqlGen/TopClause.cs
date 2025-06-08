using System.Globalization;
using System.IO;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class TopClause : ISqlFragment
{
	private readonly ISqlFragment topCount;

	private readonly bool withTies;

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
		((TextWriter)(object)writer).Write("TOP ");
		if (sqlGenerator.SqlVersion != SqlVersion.Sql8)
		{
			((TextWriter)(object)writer).Write("(");
		}
		TopCount.WriteSql(writer, sqlGenerator);
		if (sqlGenerator.SqlVersion != SqlVersion.Sql8)
		{
			((TextWriter)(object)writer).Write(")");
		}
		((TextWriter)(object)writer).Write(" ");
		if (WithTies)
		{
			((TextWriter)(object)writer).Write("WITH TIES ");
		}
	}
}
