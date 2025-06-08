using System.Collections.Generic;
using System.IO;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SqlBuilder : ISqlFragment
{
	private List<object> _sqlFragments;

	private List<object> sqlFragments
	{
		get
		{
			if (_sqlFragments == null)
			{
				_sqlFragments = new List<object>();
			}
			return _sqlFragments;
		}
	}

	public virtual bool IsEmpty
	{
		get
		{
			if (_sqlFragments != null)
			{
				return _sqlFragments.Count == 0;
			}
			return true;
		}
	}

	public void Append(object s)
	{
		sqlFragments.Add(s);
	}

	public void AppendLine()
	{
		sqlFragments.Add("\r\n");
	}

	public virtual void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		if (_sqlFragments == null)
		{
			return;
		}
		foreach (object sqlFragment2 in _sqlFragments)
		{
			if (sqlFragment2 is string value)
			{
				((TextWriter)(object)writer).Write(value);
				continue;
			}
			if (sqlFragment2 is ISqlFragment sqlFragment)
			{
				sqlFragment.WriteSql(writer, sqlGenerator);
				continue;
			}
			throw new InvalidOperationException();
		}
	}
}
