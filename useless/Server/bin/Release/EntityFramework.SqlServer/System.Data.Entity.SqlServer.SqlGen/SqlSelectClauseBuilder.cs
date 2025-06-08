using System.Collections.Generic;
using System.IO;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SqlSelectClauseBuilder : SqlBuilder
{
	private List<OptionalColumn> m_optionalColumns;

	private TopClause m_top;

	private SkipClause m_skip;

	private readonly Func<bool> m_isPartOfTopMostStatement;

	internal TopClause Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	internal SkipClause Skip
	{
		get
		{
			return m_skip;
		}
		set
		{
			m_skip = value;
		}
	}

	internal bool IsDistinct { get; set; }

	public override bool IsEmpty
	{
		get
		{
			if (base.IsEmpty)
			{
				if (m_optionalColumns != null)
				{
					return m_optionalColumns.Count == 0;
				}
				return true;
			}
			return false;
		}
	}

	internal void AddOptionalColumn(OptionalColumn column)
	{
		if (m_optionalColumns == null)
		{
			m_optionalColumns = new List<OptionalColumn>();
		}
		m_optionalColumns.Add(column);
	}

	internal SqlSelectClauseBuilder(Func<bool> isPartOfTopMostStatement)
	{
		m_isPartOfTopMostStatement = isPartOfTopMostStatement;
	}

	public override void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		((TextWriter)(object)writer).Write("SELECT ");
		if (IsDistinct)
		{
			((TextWriter)(object)writer).Write("DISTINCT ");
		}
		if (Top != null && Skip == null)
		{
			Top.WriteSql(writer, sqlGenerator);
		}
		if (IsEmpty)
		{
			((TextWriter)(object)writer).Write("*");
			return;
		}
		bool flag = WriteOptionalColumns(writer, sqlGenerator);
		if (!base.IsEmpty)
		{
			if (flag)
			{
				((TextWriter)(object)writer).Write(", ");
			}
			base.WriteSql(writer, sqlGenerator);
		}
		else if (!flag)
		{
			m_optionalColumns[0].MarkAsUsed();
			m_optionalColumns[0].WriteSqlIfUsed(writer, sqlGenerator, "");
		}
	}

	private bool WriteOptionalColumns(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		if (m_optionalColumns == null)
		{
			return false;
		}
		if (m_isPartOfTopMostStatement() || IsDistinct)
		{
			foreach (OptionalColumn optionalColumn in m_optionalColumns)
			{
				optionalColumn.MarkAsUsed();
			}
		}
		string separator = "";
		bool result = false;
		foreach (OptionalColumn optionalColumn2 in m_optionalColumns)
		{
			if (optionalColumn2.WriteSqlIfUsed(writer, sqlGenerator, separator))
			{
				result = true;
				separator = ", ";
			}
		}
		return result;
	}
}
