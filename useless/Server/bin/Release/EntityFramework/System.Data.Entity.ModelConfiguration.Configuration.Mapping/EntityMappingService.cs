using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal class EntityMappingService
{
	private readonly DbDatabaseMapping _databaseMapping;

	private Dictionary<EntityType, TableMapping> _tableMappings;

	private SortedEntityTypeIndex _entityTypes;

	public EntityMappingService(DbDatabaseMapping databaseMapping)
	{
		_databaseMapping = databaseMapping;
	}

	public void Configure()
	{
		Analyze();
		Transform();
	}

	private void Analyze()
	{
		_tableMappings = new Dictionary<EntityType, TableMapping>();
		_entityTypes = new SortedEntityTypeIndex();
		foreach (EntitySetMapping item in _databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.EntitySetMappings))
		{
			foreach (EntityTypeMapping entityTypeMapping in item.EntityTypeMappings)
			{
				_entityTypes.Add(item.EntitySet, entityTypeMapping.EntityType);
				foreach (MappingFragment mappingFragment in entityTypeMapping.MappingFragments)
				{
					FindOrCreateTableMapping(mappingFragment.Table).AddEntityTypeMappingFragment(item.EntitySet, entityTypeMapping.EntityType, mappingFragment);
				}
			}
		}
	}

	private void Transform()
	{
		foreach (EntitySet entitySet in _entityTypes.GetEntitySets())
		{
			Dictionary<TableMapping, Dictionary<EntityType, EntityTypeMapping>> dictionary = new Dictionary<TableMapping, Dictionary<EntityType, EntityTypeMapping>>();
			foreach (EntityType entityType in _entityTypes.GetEntityTypes(entitySet))
			{
				foreach (TableMapping item in _tableMappings.Values.Where((TableMapping tm) => tm.EntityTypes.Contains(entitySet, entityType)))
				{
					if (!dictionary.TryGetValue(item, out var value))
					{
						value = new Dictionary<EntityType, EntityTypeMapping>();
						dictionary.Add(item, value);
					}
					RemoveRedundantDefaultDiscriminators(item);
					bool flag = DetermineRequiresIsTypeOf(item, entitySet, entityType);
					bool flag2 = false;
					if (!FindPropertyEntityTypeMapping(item, entitySet, entityType, flag, out var propertiesTypeMapping, out var propertiesTypeMappingFragment))
					{
						continue;
					}
					flag2 = DetermineRequiresSplitEntityTypeMapping(item, entityType, flag);
					EntityTypeMapping entityTypeMapping = FindConditionTypeMapping(entityType, flag2, propertiesTypeMapping);
					MappingFragment mappingFragment = FindConditionTypeMappingFragment(_databaseMapping.Database.GetEntitySet(item.Table), propertiesTypeMappingFragment, entityTypeMapping);
					if (flag)
					{
						if (!propertiesTypeMapping.IsHierarchyMapping)
						{
							EntityTypeMapping entityTypeMapping2 = _databaseMapping.GetEntityTypeMappings(entityType).SingleOrDefault((EntityTypeMapping etm) => etm.IsHierarchyMapping);
							if (entityTypeMapping2 == null)
							{
								if (propertiesTypeMapping.MappingFragments.Count > 1)
								{
									EntityTypeMapping entityTypeMapping3 = propertiesTypeMapping.Clone();
									_databaseMapping.GetEntitySetMappings().Single((EntitySetMapping esm) => esm.EntityTypeMappings.Contains(propertiesTypeMapping)).AddTypeMapping(entityTypeMapping3);
									MappingFragment[] array = propertiesTypeMapping.MappingFragments.Where((MappingFragment tmf) => tmf != propertiesTypeMappingFragment).ToArray();
									foreach (MappingFragment fragment in array)
									{
										propertiesTypeMapping.RemoveFragment(fragment);
										entityTypeMapping3.AddFragment(fragment);
									}
								}
								propertiesTypeMapping.AddIsOfType(propertiesTypeMapping.EntityType);
							}
							else
							{
								propertiesTypeMapping.RemoveFragment(propertiesTypeMappingFragment);
								if (propertiesTypeMapping.MappingFragments.Count == 0)
								{
									_databaseMapping.GetEntitySetMapping(entitySet).RemoveTypeMapping(propertiesTypeMapping);
								}
								propertiesTypeMapping = entityTypeMapping2;
								propertiesTypeMapping.AddFragment(propertiesTypeMappingFragment);
							}
						}
						value.Add(entityType, propertiesTypeMapping);
					}
					ConfigureTypeMappings(item, value, entityType, propertiesTypeMappingFragment, mappingFragment);
					if (propertiesTypeMappingFragment.IsUnmappedPropertiesFragment() && propertiesTypeMappingFragment.ColumnMappings.All((ColumnMappingBuilder pm) => entityType.GetKeyProperties().Contains(pm.PropertyPath.First())))
					{
						RemoveFragment(entitySet, propertiesTypeMapping, propertiesTypeMappingFragment);
						if (flag2 && mappingFragment.ColumnMappings.All((ColumnMappingBuilder pm) => entityType.GetKeyProperties().Contains(pm.PropertyPath.First())))
						{
							RemoveFragment(entitySet, entityTypeMapping, mappingFragment);
						}
					}
					EntityMappingConfiguration.CleanupUnmappedArtifacts(_databaseMapping, item.Table);
					foreach (ForeignKeyBuilder foreignKeyBuilder in item.Table.ForeignKeyBuilders)
					{
						AssociationType associationType = foreignKeyBuilder.GetAssociationType();
						if (associationType != null && associationType.IsRequiredToNonRequired())
						{
							foreignKeyBuilder.GetAssociationType().TryGuessPrincipalAndDependentEnds(out var _, out var dependentEnd);
							if (dependentEnd.GetEntityType() == entityType)
							{
								MarkColumnsAsNonNullableIfNoTableSharing(entitySet, item.Table, entityType, foreignKeyBuilder.DependentColumns);
							}
						}
					}
				}
			}
			ConfigureAssociationSetMappingForeignKeys(entitySet);
		}
	}

	private void ConfigureAssociationSetMappingForeignKeys(EntitySet entitySet)
	{
		foreach (AssociationSetMapping item in from asm in _databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.AssociationSetMappings)
			where (asm.AssociationSet.SourceSet == entitySet || asm.AssociationSet.TargetSet == entitySet) && asm.AssociationSet.ElementType.IsRequiredToNonRequired()
			select asm)
		{
			item.AssociationSet.ElementType.TryGuessPrincipalAndDependentEnds(out var _, out var dependentEnd);
			if ((dependentEnd == item.AssociationSet.ElementType.SourceEnd && item.AssociationSet.SourceSet == entitySet) || (dependentEnd == item.AssociationSet.ElementType.TargetEnd && item.AssociationSet.TargetSet == entitySet))
			{
				EndPropertyMapping endPropertyMapping = ((item.SourceEndMapping.AssociationEnd == dependentEnd) ? item.TargetEndMapping : item.SourceEndMapping);
				MarkColumnsAsNonNullableIfNoTableSharing(entitySet, item.Table, dependentEnd.GetEntityType(), endPropertyMapping.PropertyMappings.Select((ScalarPropertyMapping pm) => pm.Column));
			}
		}
	}

	private void MarkColumnsAsNonNullableIfNoTableSharing(EntitySet entitySet, EntityType table, EntityType dependentEndEntityType, IEnumerable<EdmProperty> columns)
	{
		IEnumerable<EntityType> source = from et in _tableMappings[table].EntityTypes.GetEntityTypes(entitySet)
			where et != dependentEndEntityType && (et.IsAncestorOf(dependentEndEntityType) || !dependentEndEntityType.IsAncestorOf(et))
			select et;
		if (source.Count() == 0 || source.All((EntityType et) => et.Abstract))
		{
			columns.Each((EdmProperty c) => c.Nullable = false);
		}
	}

	private static void ConfigureTypeMappings(TableMapping tableMapping, Dictionary<EntityType, EntityTypeMapping> rootMappings, EntityType entityType, MappingFragment propertiesTypeMappingFragment, MappingFragment conditionTypeMappingFragment)
	{
		List<ColumnMappingBuilder> list = new List<ColumnMappingBuilder>(propertiesTypeMappingFragment.ColumnMappings.Where((ColumnMappingBuilder pm) => !pm.ColumnProperty.IsPrimaryKeyColumn));
		List<ConditionPropertyMapping> list2 = new List<ConditionPropertyMapping>(propertiesTypeMappingFragment.ColumnConditions);
		foreach (var columnMapping in from cm in tableMapping.ColumnMappings
			from pm in cm.PropertyMappings
			where pm.EntityType == entityType
			select new
			{
				Column = cm.Column,
				Property = pm
			})
		{
			if (columnMapping.Property.PropertyPath != null && !IsRootTypeMapping(rootMappings, columnMapping.Property.EntityType, columnMapping.Property.PropertyPath))
			{
				ColumnMappingBuilder columnMappingBuilder = propertiesTypeMappingFragment.ColumnMappings.SingleOrDefault((ColumnMappingBuilder x) => x.PropertyPath == columnMapping.Property.PropertyPath);
				if (columnMappingBuilder != null)
				{
					list.Remove(columnMappingBuilder);
				}
				else
				{
					columnMappingBuilder = new ColumnMappingBuilder(columnMapping.Column, columnMapping.Property.PropertyPath);
					propertiesTypeMappingFragment.AddColumnMapping(columnMappingBuilder);
				}
			}
			if (columnMapping.Property.Conditions == null)
			{
				continue;
			}
			foreach (ConditionPropertyMapping condition in columnMapping.Property.Conditions)
			{
				if (conditionTypeMappingFragment.ColumnConditions.Contains(condition))
				{
					list2.Remove(condition);
				}
				else if (!entityType.Abstract)
				{
					conditionTypeMappingFragment.AddConditionProperty(condition);
				}
			}
		}
		foreach (ColumnMappingBuilder item in list)
		{
			propertiesTypeMappingFragment.RemoveColumnMapping(item);
		}
		foreach (ConditionPropertyMapping item2 in list2)
		{
			conditionTypeMappingFragment.RemoveConditionProperty(item2);
		}
		if (entityType.Abstract)
		{
			propertiesTypeMappingFragment.ClearConditions();
		}
	}

	private static MappingFragment FindConditionTypeMappingFragment(EntitySet tableSet, MappingFragment propertiesTypeMappingFragment, EntityTypeMapping conditionTypeMapping)
	{
		EntityType table = tableSet.ElementType;
		MappingFragment mappingFragment = conditionTypeMapping.MappingFragments.SingleOrDefault((MappingFragment x) => x.Table == table);
		if (mappingFragment == null)
		{
			mappingFragment = EntityMappingOperations.CreateTypeMappingFragment(conditionTypeMapping, propertiesTypeMappingFragment, tableSet);
			mappingFragment.SetIsConditionOnlyFragment(isConditionOnlyFragment: true);
			if (propertiesTypeMappingFragment.GetDefaultDiscriminator() != null)
			{
				mappingFragment.SetDefaultDiscriminator(propertiesTypeMappingFragment.GetDefaultDiscriminator());
				propertiesTypeMappingFragment.RemoveDefaultDiscriminatorAnnotation();
			}
		}
		return mappingFragment;
	}

	private EntityTypeMapping FindConditionTypeMapping(EntityType entityType, bool requiresSplit, EntityTypeMapping propertiesTypeMapping)
	{
		EntityTypeMapping entityTypeMapping = propertiesTypeMapping;
		if (requiresSplit)
		{
			if (!entityType.Abstract)
			{
				entityTypeMapping = propertiesTypeMapping.Clone();
				entityTypeMapping.RemoveIsOfType(entityTypeMapping.EntityType);
				_databaseMapping.GetEntitySetMappings().Single((EntitySetMapping esm) => esm.EntityTypeMappings.Contains(propertiesTypeMapping)).AddTypeMapping(entityTypeMapping);
			}
			propertiesTypeMapping.MappingFragments.Each(delegate(MappingFragment tmf)
			{
				tmf.ClearConditions();
			});
		}
		return entityTypeMapping;
	}

	private bool DetermineRequiresIsTypeOf(TableMapping tableMapping, EntitySet entitySet, EntityType entityType)
	{
		if (entityType.IsRootOfSet(tableMapping.EntityTypes.GetEntityTypes(entitySet)))
		{
			if (tableMapping.EntityTypes.GetEntityTypes(entitySet).Count() <= 1 || !tableMapping.EntityTypes.GetEntityTypes(entitySet).Any((EntityType et) => et != entityType && !et.Abstract))
			{
				return _tableMappings.Values.Any((TableMapping tm) => tm != tableMapping && tm.Table.ForeignKeyBuilders.Any((ForeignKeyBuilder fk) => fk.GetIsTypeConstraint() && fk.PrincipalTable == tableMapping.Table));
			}
			return true;
		}
		return false;
	}

	private static bool DetermineRequiresSplitEntityTypeMapping(TableMapping tableMapping, EntityType entityType, bool requiresIsTypeOf)
	{
		if (requiresIsTypeOf)
		{
			return HasConditions(tableMapping, entityType);
		}
		return false;
	}

	private bool FindPropertyEntityTypeMapping(TableMapping tableMapping, EntitySet entitySet, EntityType entityType, bool requiresIsTypeOf, out EntityTypeMapping entityTypeMapping, out MappingFragment fragment)
	{
		entityTypeMapping = null;
		fragment = null;
		var anon = (from etm in _databaseMapping.GetEntityTypeMappings(entityType)
			from tmf in etm.MappingFragments
			where tmf.Table == tableMapping.Table
			select new
			{
				TypeMapping = etm,
				Fragment = tmf
			}).SingleOrDefault();
		if (anon != null)
		{
			entityTypeMapping = anon.TypeMapping;
			fragment = anon.Fragment;
			if (!requiresIsTypeOf && entityType.Abstract)
			{
				RemoveFragment(entitySet, anon.TypeMapping, anon.Fragment);
				return false;
			}
			return true;
		}
		return false;
	}

	private void RemoveFragment(EntitySet entitySet, EntityTypeMapping entityTypeMapping, MappingFragment fragment)
	{
		EdmProperty defaultDiscriminator = fragment.GetDefaultDiscriminator();
		if (defaultDiscriminator != null && entityTypeMapping.EntityType.BaseType != null && !entityTypeMapping.EntityType.Abstract)
		{
			ColumnMapping columnMapping = _tableMappings[fragment.Table].ColumnMappings.SingleOrDefault((ColumnMapping cm) => cm.Column == defaultDiscriminator);
			if (columnMapping != null)
			{
				PropertyMappingSpecification propertyMappingSpecification = columnMapping.PropertyMappings.SingleOrDefault((PropertyMappingSpecification pm) => pm.EntityType == entityTypeMapping.EntityType);
				if (propertyMappingSpecification != null)
				{
					columnMapping.PropertyMappings.Remove(propertyMappingSpecification);
				}
			}
			defaultDiscriminator.Nullable = true;
		}
		if (entityTypeMapping.EntityType.Abstract)
		{
			foreach (ColumnMapping item in _tableMappings[fragment.Table].ColumnMappings.Where((ColumnMapping cm) => cm.PropertyMappings.All((PropertyMappingSpecification pm) => pm.EntityType == entityTypeMapping.EntityType)))
			{
				fragment.Table.RemoveMember(item.Column);
			}
		}
		entityTypeMapping.RemoveFragment(fragment);
		if (!entityTypeMapping.MappingFragments.Any())
		{
			_databaseMapping.GetEntitySetMapping(entitySet).RemoveTypeMapping(entityTypeMapping);
		}
	}

	private static void RemoveRedundantDefaultDiscriminators(TableMapping tableMapping)
	{
		foreach (EntitySet entitySet in tableMapping.EntityTypes.GetEntitySets())
		{
			(from cm in tableMapping.ColumnMappings
				from pm in cm.PropertyMappings
				where cm.PropertyMappings.Where((PropertyMappingSpecification pm1) => tableMapping.EntityTypes.GetEntityTypes(entitySet).Contains(pm1.EntityType)).Count((PropertyMappingSpecification pms) => pms.IsDefaultDiscriminatorCondition) == 1
				select new
				{
					ColumnMapping = cm,
					PropertyMapping = pm
				}).ToArray().Each(x =>
			{
				x.PropertyMapping.Conditions.Clear();
				if (x.PropertyMapping.PropertyPath == null)
				{
					x.ColumnMapping.PropertyMappings.Remove(x.PropertyMapping);
				}
			});
		}
	}

	private static bool HasConditions(TableMapping tableMapping, EntityType entityType)
	{
		return tableMapping.ColumnMappings.SelectMany((ColumnMapping cm) => cm.PropertyMappings).Any((PropertyMappingSpecification pm) => pm.EntityType == entityType && pm.Conditions.Count > 0);
	}

	private static bool IsRootTypeMapping(Dictionary<EntityType, EntityTypeMapping> rootMappings, EntityType entityType, IList<EdmProperty> propertyPath)
	{
		for (EntityType entityType2 = (EntityType)entityType.BaseType; entityType2 != null; entityType2 = (EntityType)entityType2.BaseType)
		{
			if (rootMappings.TryGetValue(entityType2, out var value))
			{
				return value.MappingFragments.SelectMany((MappingFragment etmf) => etmf.ColumnMappings).Any((ColumnMappingBuilder pm) => pm.PropertyPath.SequenceEqual(propertyPath));
			}
		}
		return false;
	}

	private TableMapping FindOrCreateTableMapping(EntityType table)
	{
		if (!_tableMappings.TryGetValue(table, out var value))
		{
			value = new TableMapping(table);
			_tableMappings.Add(table, value);
		}
		return value;
	}
}
