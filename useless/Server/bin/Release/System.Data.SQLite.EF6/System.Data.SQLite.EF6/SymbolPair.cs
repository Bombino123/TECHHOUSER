namespace System.Data.SQLite.EF6;

internal class SymbolPair : ISqlFragment
{
	public Symbol Source;

	public Symbol Column;

	public SymbolPair(Symbol source, Symbol column)
	{
		Source = source;
		Column = column;
	}

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
	}
}
