using System.Globalization;
using System.IO;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SkipClause : ISqlFragment
{
	private readonly ISqlFragment skipCount;

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
		((TextWriter)(object)writer).Write("OFFSET ");
		SkipCount.WriteSql(writer, sqlGenerator);
		((TextWriter)(object)writer).Write(" ROWS ");
	}
}
