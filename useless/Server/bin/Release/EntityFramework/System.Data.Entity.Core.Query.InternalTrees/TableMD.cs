using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.PlanCompiler;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class TableMD
{
	private readonly List<ColumnMD> m_columns;

	private readonly List<ColumnMD> m_keys;

	private readonly EntitySetBase m_extent;

	private readonly bool m_flattened;

	internal EntitySetBase Extent => m_extent;

	internal List<ColumnMD> Columns => m_columns;

	internal List<ColumnMD> Keys => m_keys;

	internal bool Flattened => m_flattened;

	private TableMD(EntitySetBase extent)
	{
		m_columns = new List<ColumnMD>();
		m_keys = new List<ColumnMD>();
		m_extent = extent;
	}

	internal TableMD(TypeUsage type, EntitySetBase extent)
		: this(extent)
	{
		m_columns.Add(new ColumnMD("element", type));
		m_flattened = !TypeUtils.IsStructuredType(type);
	}

	internal TableMD(IEnumerable<EdmProperty> properties, IEnumerable<EdmMember> keyProperties, EntitySetBase extent)
		: this(extent)
	{
		Dictionary<string, ColumnMD> dictionary = new Dictionary<string, ColumnMD>();
		m_flattened = true;
		foreach (EdmProperty property in properties)
		{
			ColumnMD columnMD = new ColumnMD(property);
			m_columns.Add(columnMD);
			dictionary[property.Name] = columnMD;
		}
		foreach (EdmMember keyProperty in keyProperties)
		{
			if (dictionary.TryGetValue(keyProperty.Name, out var value))
			{
				m_keys.Add(value);
			}
		}
	}

	public override string ToString()
	{
		if (m_extent == null)
		{
			return "Transient";
		}
		return m_extent.Name;
	}
}
