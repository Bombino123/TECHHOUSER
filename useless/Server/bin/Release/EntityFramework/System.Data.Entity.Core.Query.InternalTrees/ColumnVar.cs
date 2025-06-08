namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ColumnVar : Var
{
	private readonly ColumnMD m_columnMetadata;

	private readonly Table m_table;

	internal Table Table => m_table;

	internal ColumnMD ColumnMetadata => m_columnMetadata;

	internal ColumnVar(int id, Table table, ColumnMD columnMetadata)
		: base(id, VarType.Column, columnMetadata.Type)
	{
		m_table = table;
		m_columnMetadata = columnMetadata;
	}

	internal override bool TryGetName(out string name)
	{
		name = m_columnMetadata.Name;
		return true;
	}
}
