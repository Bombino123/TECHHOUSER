using System.Collections.Generic;
using System.Globalization;

namespace System.Data.SQLite.EF6;

internal sealed class SqlSelectStatement : ISqlFragment
{
	private bool isDistinct;

	private List<Symbol> allJoinExtents;

	private List<Symbol> fromExtents;

	private Dictionary<Symbol, bool> outerExtents;

	private TopClause top;

	private SkipClause skip;

	private SqlBuilder select = new SqlBuilder();

	private SqlBuilder from = new SqlBuilder();

	private SqlBuilder where;

	private SqlBuilder groupBy;

	private SqlBuilder orderBy;

	private bool isTopMost;

	internal bool IsDistinct
	{
		get
		{
			return isDistinct;
		}
		set
		{
			isDistinct = value;
		}
	}

	internal List<Symbol> AllJoinExtents
	{
		get
		{
			return allJoinExtents;
		}
		set
		{
			allJoinExtents = value;
		}
	}

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

	internal TopClause Top
	{
		get
		{
			return top;
		}
		set
		{
			top = value;
		}
	}

	internal SkipClause Skip
	{
		get
		{
			return skip;
		}
		set
		{
			skip = value;
		}
	}

	internal SqlBuilder Select => select;

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

	internal bool IsTopMost
	{
		get
		{
			return isTopMost;
		}
		set
		{
			isTopMost = value;
		}
	}

	public bool HaveOrderByLimitOrOffset()
	{
		if (orderBy != null && !orderBy.IsEmpty)
		{
			return true;
		}
		if (top != null)
		{
			return true;
		}
		if (skip != null)
		{
			return true;
		}
		return false;
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
		writer.Indent++;
		writer.Write("SELECT ");
		if (IsDistinct)
		{
			writer.Write("DISTINCT ");
		}
		if (select == null || Select.IsEmpty)
		{
			writer.Write("*");
		}
		else
		{
			Select.WriteSql(writer, sqlGenerator);
		}
		writer.WriteLine();
		writer.Write("FROM ");
		From.WriteSql(writer, sqlGenerator);
		if (where != null && !Where.IsEmpty)
		{
			writer.WriteLine();
			writer.Write("WHERE ");
			Where.WriteSql(writer, sqlGenerator);
		}
		if (groupBy != null && !GroupBy.IsEmpty)
		{
			writer.WriteLine();
			writer.Write("GROUP BY ");
			GroupBy.WriteSql(writer, sqlGenerator);
		}
		if (orderBy != null && !OrderBy.IsEmpty && (IsTopMost || Top != null))
		{
			writer.WriteLine();
			writer.Write("ORDER BY ");
			OrderBy.WriteSql(writer, sqlGenerator);
		}
		if (Top != null)
		{
			Top.WriteSql(writer, sqlGenerator);
		}
		if (skip != null)
		{
			Skip.WriteSql(writer, sqlGenerator);
		}
		int indent = writer.Indent - 1;
		writer.Indent = indent;
	}
}
