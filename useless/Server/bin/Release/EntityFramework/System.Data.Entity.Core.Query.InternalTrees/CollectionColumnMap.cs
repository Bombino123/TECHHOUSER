using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class CollectionColumnMap : ColumnMap
{
	private readonly ColumnMap m_element;

	private readonly SimpleColumnMap[] m_foreignKeys;

	private readonly SimpleColumnMap[] m_keys;

	internal SimpleColumnMap[] ForeignKeys => m_foreignKeys;

	internal SimpleColumnMap[] Keys => m_keys;

	internal ColumnMap Element => m_element;

	internal CollectionColumnMap(TypeUsage type, string name, ColumnMap elementMap, SimpleColumnMap[] keys, SimpleColumnMap[] foreignKeys)
		: base(type, name)
	{
		m_element = elementMap;
		m_keys = keys ?? new SimpleColumnMap[0];
		m_foreignKeys = foreignKeys ?? new SimpleColumnMap[0];
	}
}
