using System.Collections.Generic;
using System.Data.Entity.Migrations.Utilities;
using System.Globalization;
using System.IO;

namespace System.Data.Entity.SqlServer.SqlGen;

internal sealed class SqlSelectStatement : ISqlFragment
{
	private List<Symbol> fromExtents;

	private Dictionary<Symbol, bool> outerExtents;

	private readonly SqlSelectClauseBuilder select;

	private readonly SqlBuilder from = new SqlBuilder();

	private SqlBuilder where;

	private SqlBuilder groupBy;

	private SqlBuilder orderBy;

	internal bool OutputColumnsRenamed { get; set; }

	internal Dictionary<string, Symbol> OutputColumns { get; set; }

	internal List<Symbol> AllJoinExtents { get; set; }

	internal List<Symbol> FromExtents
	{
		get
		{
			if (fromExtents == null)
			{
				fromExtents = new List<Symbol>();
			}
			return fromExtents;
		}
	}

	internal Dictionary<Symbol, bool> OuterExtents
	{
		get
		{
			if (outerExtents == null)
			{
				outerExtents = new Dictionary<Symbol, bool>();
			}
			return outerExtents;
		}
	}

	internal SqlSelectClauseBuilder Select => select;

	internal SqlBuilder From => from;

	internal SqlBuilder Where
	{
		get
		{
			if (where == null)
			{
				where = new SqlBuilder();
			}
			return where;
		}
	}

	internal SqlBuilder GroupBy
	{
		get
		{
			if (groupBy == null)
			{
				groupBy = new SqlBuilder();
			}
			return groupBy;
		}
	}

	public SqlBuilder OrderBy
	{
		get
		{
			if (orderBy == null)
			{
				orderBy = new SqlBuilder();
			}
			return orderBy;
		}
	}

	internal bool IsTopMost { get; set; }

	internal SqlSelectStatement()
	{
		select = new SqlSelectClauseBuilder(() => IsTopMost);
	}

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		List<string> list = null;
		if (outerExtents != null && 0 < outerExtents.Count)
		{
			foreach (Symbol key in outerExtents.Keys)
			{
				if (key is JoinSymbol joinSymbol)
				{
					foreach (Symbol flattenedExtent in joinSymbol.FlattenedExtentList)
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(flattenedExtent.NewName);
					}
				}
				else
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(key.NewName);
				}
			}
		}
		List<Symbol> list2 = AllJoinExtents ?? fromExtents;
		if (list2 != null)
		{
			foreach (Symbol item in list2)
			{
				if (list != null && list.Contains(item.Name))
				{
					int num = sqlGenerator.AllExtentNames[item.Name];
					string text;
					do
					{
						num++;
						text = item.Name + num.ToString(CultureInfo.InvariantCulture);
					}
					while (sqlGenerator.AllExtentNames.ContainsKey(text));
					sqlGenerator.AllExtentNames[item.Name] = num;
					item.NewName = text;
					sqlGenerator.AllExtentNames[text] = 0;
				}
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(item.NewName);
			}
		}
		((IndentedTextWriter)writer).Indent = ((IndentedTextWriter)writer).Indent + 1;
		select.WriteSql(writer, sqlGenerator);
		((TextWriter)(object)writer).WriteLine();
		((TextWriter)(object)writer).Write("FROM ");
		From.WriteSql(writer, sqlGenerator);
		if (where != null && !Where.IsEmpty)
		{
			((TextWriter)(object)writer).WriteLine();
			((TextWriter)(object)writer).Write("WHERE ");
			Where.WriteSql(writer, sqlGenerator);
		}
		if (groupBy != null && !GroupBy.IsEmpty)
		{
			((TextWriter)(object)writer).WriteLine();
			((TextWriter)(object)writer).Write("GROUP BY ");
			GroupBy.WriteSql(writer, sqlGenerator);
		}
		if (orderBy != null && !OrderBy.IsEmpty && (IsTopMost || Select.Top != null || Select.Skip != null))
		{
			((TextWriter)(object)writer).WriteLine();
			((TextWriter)(object)writer).Write("ORDER BY ");
			OrderBy.WriteSql(writer, sqlGenerator);
		}
		if (Select.Skip != null)
		{
			((TextWriter)(object)writer).WriteLine();
			WriteOffsetFetch(writer, Select.Top, Select.Skip, sqlGenerator);
		}
		int indent = ((IndentedTextWriter)writer).Indent - 1;
		((IndentedTextWriter)writer).Indent = indent;
	}

	private static void WriteOffsetFetch(SqlWriter writer, TopClause top, SkipClause skip, SqlGenerator sqlGenerator)
	{
		skip.WriteSql(writer, sqlGenerator);
		if (top != null)
		{
			((TextWriter)(object)writer).Write("FETCH NEXT ");
			top.TopCount.WriteSql(writer, sqlGenerator);
			((TextWriter)(object)writer).Write(" ROWS ONLY ");
		}
	}
}
