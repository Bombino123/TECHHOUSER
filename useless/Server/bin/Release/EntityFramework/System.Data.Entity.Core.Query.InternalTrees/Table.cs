using System.Collections.Generic;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class Table
{
	private readonly TableMD m_tableMetadata;

	private readonly VarList m_columns;

	private readonly VarVec m_referencedColumns;

	private readonly VarVec m_keys;

	private readonly VarVec m_nonnullableColumns;

	private readonly int m_tableId;

	internal TableMD TableMetadata => m_tableMetadata;

	internal VarList Columns => m_columns;

	internal VarVec ReferencedColumns => m_referencedColumns;

	internal VarVec NonNullableColumns => m_nonnullableColumns;

	internal VarVec Keys => m_keys;

	internal int TableId => m_tableId;

	internal Table(Command command, TableMD tableMetadata, int tableId)
	{
		m_tableMetadata = tableMetadata;
		m_columns = Command.CreateVarList();
		m_keys = command.CreateVarVec();
		m_nonnullableColumns = command.CreateVarVec();
		m_tableId = tableId;
		Dictionary<string, ColumnVar> dictionary = new Dictionary<string, ColumnVar>();
		foreach (ColumnMD column in tableMetadata.Columns)
		{
			ColumnVar columnVar = command.CreateColumnVar(this, column);
			dictionary[column.Name] = columnVar;
			if (!column.IsNullable)
			{
				m_nonnullableColumns.Set(columnVar);
			}
		}
		foreach (ColumnMD key in tableMetadata.Keys)
		{
			ColumnVar v = dictionary[key.Name];
			m_keys.Set(v);
		}
		m_referencedColumns = command.CreateVarVec(m_columns);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}::{1}", new object[2] { m_tableMetadata, TableId });
	}
}
