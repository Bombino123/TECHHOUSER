using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal static class EntityMappingOperations
{
	public static MappingFragment CreateTypeMappingFragment(EntityTypeMapping entityTypeMapping, MappingFragment templateFragment, EntitySet tableSet)
	{
		MappingFragment mappingFragment = new MappingFragment(tableSet, entityTypeMapping, makeColumnsDistinct: false);
		entityTypeMapping.AddFragment(mappingFragment);
		foreach (ColumnMappingBuilder item in templateFragment.ColumnMappings.Where((ColumnMappingBuilder pm) => pm.ColumnProperty.IsPrimaryKeyColumn))
		{
			CopyPropertyMappingToFragment(item, mappingFragment, TablePrimitiveOperations.GetNameMatcher(item.ColumnProperty.Name), useExisting: true);
		}
		return mappingFragment;
	}

	private static void UpdatePropertyMapping(DbDatabaseMapping databaseMapping, IEnumerable<EntitySet> entitySets, Dictionary<EdmProperty, IList<ColumnMappingBuilder>> columnMappingIndex, ColumnMappingBuilder propertyMappingBuilder, EntityType fromTable, EntityType toTable, bool useExisting)
	{
		propertyMappingBuilder.ColumnProperty = TableOperations.CopyColumnAndAnyConstraints(databaseMapping.Database, fromTable, toTable, propertyMappingBuilder.ColumnProperty, GetPropertyPathMatcher(columnMappingIndex, propertyMappingBuilder), useExisting);
		propertyMappingBuilder.SyncNullabilityCSSpace(databaseMapping, entitySets, toTable);
	}

	private static Func<EdmProperty, bool> GetPropertyPathMatcher(Dictionary<EdmProperty, IList<ColumnMappingBuilder>> columnMappingIndex, ColumnMappingBuilder propertyMappingBuilder)
	{
		return delegate(EdmProperty c)
		{
			if (!columnMappingIndex.ContainsKey(c))
			{
				return false;
			}
			IList<ColumnMappingBuilder> list = columnMappingIndex[c];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].PropertyPath.PathEqual(propertyMappingBuilder.PropertyPath))
				{
					return true;
				}
			}
			return false;
		};
	}

	private static bool PathEqual(this IList<EdmProperty> listA, IList<EdmProperty> listB)
	{
		if (listA == null || listB == null)
		{
			return false;
		}
		if (listA.Count != listB.Count)
		{
			return false;
		}
		for (int i = 0; i < listA.Count; i++)
		{
			if (listA[i] != listB[i])
			{
				return false;
			}
		}
		return true;
	}

	private static Dictionary<EdmProperty, IList<ColumnMappingBuilder>> GetColumnMappingIndex(DbDatabaseMapping databaseMapping)
	{
		Dictionary<EdmProperty, IList<ColumnMappingBuilder>> dictionary = new Dictionary<EdmProperty, IList<ColumnMappingBuilder>>();
		IEnumerable<EntitySetMapping> entitySetMappings = databaseMapping.EntityContainerMappings.Single().EntitySetMappings;
		if (entitySetMappings == null)
		{
			return dictionary;
		}
		List<EntitySetMapping> list = entitySetMappings.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			IList<EntityTypeMapping> entityTypeMappings = list[i].EntityTypeMappings;
			if (entityTypeMappings == null)
			{
				continue;
			}
			for (int j = 0; j < entityTypeMappings.Count; j++)
			{
				IList<MappingFragment> mappingFragments = entityTypeMappings[j].MappingFragments;
				if (mappingFragments == null)
				{
					continue;
				}
				for (int k = 0; k < mappingFragments.Count; k++)
				{
					if (!(mappingFragments[k].ColumnMappings is IList<ColumnMappingBuilder> list2))
					{
						continue;
					}
					for (int l = 0; l < list2.Count; l++)
					{
						ColumnMappingBuilder columnMappingBuilder = list2[l];
						IList<ColumnMappingBuilder> list3 = null;
						if (dictionary.ContainsKey(columnMappingBuilder.ColumnProperty))
						{
							list3 = dictionary[columnMappingBuilder.ColumnProperty];
						}
						else
						{
							dictionary.Add(columnMappingBuilder.ColumnProperty, list3 = new List<ColumnMappingBuilder>());
						}
						list3.Add(columnMappingBuilder);
					}
				}
			}
		}
		return dictionary;
	}

	public static void UpdatePropertyMappings(DbDatabaseMapping databaseMapping, IEnumerable<EntitySet> entitySets, EntityType fromTable, MappingFragment fragment, bool useExisting)
	{
		if (fromTable != fragment.Table)
		{
			Dictionary<EdmProperty, IList<ColumnMappingBuilder>> columnMappingIndex = GetColumnMappingIndex(databaseMapping);
			List<ColumnMappingBuilder> list = fragment.ColumnMappings.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				UpdatePropertyMapping(databaseMapping, entitySets, columnMappingIndex, list[i], fromTable, fragment.Table, useExisting);
			}
		}
	}

	public static void MovePropertyMapping(DbDatabaseMapping databaseMapping, IEnumerable<EntitySet> entitySets, MappingFragment fromFragment, MappingFragment toFragment, ColumnMappingBuilder propertyMappingBuilder, bool requiresUpdate, bool useExisting)
	{
		if (requiresUpdate && fromFragment.Table != toFragment.Table)
		{
			UpdatePropertyMapping(databaseMapping, entitySets, GetColumnMappingIndex(databaseMapping), propertyMappingBuilder, fromFragment.Table, toFragment.Table, useExisting);
		}
		fromFragment.RemoveColumnMapping(propertyMappingBuilder);
		toFragment.AddColumnMapping(propertyMappingBuilder);
	}

	public static void CopyPropertyMappingToFragment(ColumnMappingBuilder propertyMappingBuilder, MappingFragment fragment, Func<EdmProperty, bool> isCompatible, bool useExisting)
	{
		EdmProperty columnProperty = TablePrimitiveOperations.IncludeColumn(fragment.Table, propertyMappingBuilder.ColumnProperty, isCompatible, useExisting);
		fragment.AddColumnMapping(new ColumnMappingBuilder(columnProperty, propertyMappingBuilder.PropertyPath));
	}

	public static void UpdateConditions(EdmModel database, EntityType fromTable, MappingFragment fragment)
	{
		if (fromTable != fragment.Table)
		{
			fragment.ColumnConditions.Each(delegate(ConditionPropertyMapping cc)
			{
				cc.Column = TableOperations.CopyColumnAndAnyConstraints(database, fromTable, fragment.Table, cc.Column, TablePrimitiveOperations.GetNameMatcher(cc.Column.Name), useExisting: true);
			});
		}
	}
}
