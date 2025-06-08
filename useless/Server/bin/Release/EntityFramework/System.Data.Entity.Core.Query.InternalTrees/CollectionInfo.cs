using System.Collections.Generic;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class CollectionInfo
{
	private readonly Var m_collectionVar;

	private readonly ColumnMap m_columnMap;

	private readonly VarList m_flattenedElementVars;

	private readonly VarVec m_keys;

	private readonly List<SortKey> m_sortKeys;

	private readonly object m_discriminatorValue;

	internal Var CollectionVar => m_collectionVar;

	internal ColumnMap ColumnMap => m_columnMap;

	internal VarList FlattenedElementVars => m_flattenedElementVars;

	internal VarVec Keys => m_keys;

	internal List<SortKey> SortKeys => m_sortKeys;

	internal object DiscriminatorValue => m_discriminatorValue;

	internal CollectionInfo(Var collectionVar, ColumnMap columnMap, VarList flattenedElementVars, VarVec keys, List<SortKey> sortKeys, object discriminatorValue)
	{
		m_collectionVar = collectionVar;
		m_columnMap = columnMap;
		m_flattenedElementVars = flattenedElementVars;
		m_keys = keys;
		m_sortKeys = sortKeys;
		m_discriminatorValue = discriminatorValue;
	}
}
