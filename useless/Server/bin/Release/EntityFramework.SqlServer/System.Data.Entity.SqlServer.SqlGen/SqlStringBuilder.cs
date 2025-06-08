using System.Text;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SqlStringBuilder
{
	private readonly StringBuilder _sql;

	public bool UpperCaseKeywords { get; set; }

	internal StringBuilder InnerBuilder => _sql;

	public int Length => _sql.Length;

	public SqlStringBuilder()
	{
		_sql = new StringBuilder();
	}

	public SqlStringBuilder(int capacity)
	{
		_sql = new StringBuilder(capacity);
	}

	public SqlStringBuilder AppendKeyword(string keyword)
	{
		_sql.Append(UpperCaseKeywords ? keyword.ToUpperInvariant() : keyword.ToLowerInvariant());
		return this;
	}

	public SqlStringBuilder AppendLine()
	{
		_sql.AppendLine();
		return this;
	}

	public SqlStringBuilder AppendLine(string s)
	{
		_sql.AppendLine(s);
		return this;
	}

	public SqlStringBuilder Append(string s)
	{
		_sql.Append(s);
		return this;
	}

	public override string ToString()
	{
		return _sql.ToString();
	}
}
