using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Edm.Services;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal class EntityMappingConfiguration
{
	private DatabaseName _tableName;

	private List<PropertyPath> _properties;

	private readonly List<ValueConditionConfiguration> _valueConditions = new List<ValueConditionConfiguration>();

	private readonly List<NotNullConditionConfiguration> _notNullConditions = new List<NotNullConditionConfiguration>();

	private readonly Dictionary<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> _primitivePropertyConfigurations = new Dictionary<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>();

	private readonly IDictionary<string, object> _annotations = new Dictionary<string, object>();

	public bool MapInheritedProperties { get; set; }

	public DatabaseName TableName
	{
		get
		{
			return _tableName;
		}
		set
		{
			_tableName = value;
		}
	}

	public IDictionary<string, object> Annotations => _annotations;

	internal List<PropertyPath> Properties
	{
		get
		{
			return _properties;
		}
		set
		{
			if (_properties == null)
			{
				_properties = new List<PropertyPath>();
			}
			value.Each(Property);
		}
	}

	internal IDictionary<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> PrimitivePropertyConfigurations => _primitivePropertyConfigurations;

	public List<ValueConditionConfiguration> ValueConditions => _valueConditions;

	public List<NotNullConditionConfiguration> NullabilityConditions
	{
		get
		{
			return _notNullConditions;
		}
		set
		{
			value.Each(AddNullabilityCondition);
		}
	}

	internal EntityMappingConfiguration()
	{
	}

	private EntityMappingConfiguration(EntityMappingConfiguration source)
	{
		_tableName = source._tableName;
		MapInheritedProperties = source.MapInheritedProperties;
		if (source._properties != null)
		{
			_properties = new List<PropertyPath>(source._properties);
		}
		_valueConditions.AddRange(source._valueConditions.Select((ValueConditionConfiguration c) => c.Clone(this)));
		_notNullConditions.AddRange(source._notNullConditions.Select((NotNullConditionConfiguration c) => c.Clone(this)));
		source._primitivePropertyConfigurations.Each(delegate(KeyValuePair<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> c)
		{
			_primitivePropertyConfigurations.Add(c.Key, c.Value.Clone());
		});
		foreach (KeyValuePair<string, object> annotation in source._annotations)
		{
			_annotations.Add(annotation);
		}
	}

	internal virtual EntityMappingConfiguration Clone()
	{
		return new EntityMappingConfiguration(this);
	}

	public virtual void SetAnnotation(string name, object value)
	{
		if (!name.IsValidUndottedName())
		{
			throw new ArgumentException(Strings.BadAnnotationName(name));
		}
		_annotations[name] = value;
	}

	internal TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(PropertyPath propertyPath, Func<TPrimitivePropertyConfiguration> primitivePropertyConfigurationCreator) where TPrimitivePropertyConfiguration : System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration
	{
		if (_properties == null)
		{
			_properties = new List<PropertyPath>();
		}
		Property(propertyPath);
		if (!_primitivePropertyConfigurations.TryGetValue(propertyPath, out var value))
		{
			_primitivePropertyConfigurations.Add(propertyPath, value = primitivePropertyConfigurationCreator());
		}
		return (TPrimitivePropertyConfiguration)value;
	}

	private void Property(PropertyPath property)
	{
		if (!_properties.Where((PropertyPath pp) => pp.SequenceEqual(property)).Any())
		{
			_properties.Add(property);
		}
	}

	public void AddValueCondition(ValueConditionConfiguration valueCondition)
	{
		ValueConditionConfiguration valueConditionConfiguration = ValueConditions.SingleOrDefault((ValueConditionConfiguration vc) => vc.Discriminator.Equals(valueCondition.Discriminator, StringComparison.Ordinal));
		if (valueConditionConfiguration == null)
		{
			ValueConditions.Add(valueCondition);
		}
		else
		{
			valueConditionConfiguration.Value = valueCondition.Value;
		}
	}

	public void AddNullabilityCondition(NotNullConditionConfiguration notNullConditionConfiguration)
	{
		if (!NullabilityConditions.Contains(notNullConditionConfiguration))
		{
			NullabilityConditions.Add(notNullConditionConfiguration);
		}
	}

	public bool MapsAnyInheritedProperties(EntityType entityType)
	{
		HashSet<EdmPropertyPath> properties = new HashSet<EdmPropertyPath>();
		if (Properties != null)
		{
			Properties.Each(delegate(PropertyPath p)
			{
				properties.AddRange(PropertyPathToEdmPropertyPath(p, entityType));
			});
		}
		if (!MapInheritedProperties)
		{
			return properties.Any((EdmPropertyPath x) => !entityType.KeyProperties().Contains(x.First()) && !entityType.DeclaredProperties.Contains(x.First()));
		}
		return true;
	}

	public void Configure(DbDatabaseMapping databaseMapping, ICollection<EntitySet> entitySets, DbProviderManifest providerManifest, EntityType entityType, ref EntityTypeMapping entityTypeMapping, bool isMappingAnyInheritedProperty, int configurationIndex, int configurationCount, IDictionary<string, object> commonAnnotations)
	{
		EntityType baseType = (EntityType)entityType.BaseType;
		bool flag = baseType == null && configurationIndex == 0;
		MappingFragment mappingFragment = FindOrCreateTypeMappingFragment(databaseMapping, ref entityTypeMapping, configurationIndex, entityType, providerManifest);
		EntityType table = mappingFragment.Table;
		bool isTableSharing;
		EntityType entityType2 = FindOrCreateTargetTable(databaseMapping, mappingFragment, entityType, table, out isTableSharing);
		bool isSharingTableWithBase = DiscoverIsSharingWithBase(databaseMapping, entityType, entityType2);
		HashSet<EdmPropertyPath> hashSet = DiscoverAllMappingsToContain(databaseMapping, entityType, entityType2, isSharingTableWithBase);
		List<ColumnMappingBuilder> list = mappingFragment.ColumnMappings.ToList();
		foreach (EdmPropertyPath propertyPath in hashSet)
		{
			ColumnMappingBuilder columnMappingBuilder = mappingFragment.ColumnMappings.SingleOrDefault((ColumnMappingBuilder pm) => pm.PropertyPath.SequenceEqual(propertyPath));
			if (columnMappingBuilder == null)
			{
				throw Error.EntityMappingConfiguration_DuplicateMappedProperty(entityType.Name, propertyPath.ToString());
			}
			list.Remove(columnMappingBuilder);
		}
		if (!flag)
		{
			bool isSplitting;
			EntityType entityType3 = FindParentTable(databaseMapping, table, entityTypeMapping, entityType2, isMappingAnyInheritedProperty, configurationIndex, configurationCount, out isSplitting);
			if (entityType3 != null)
			{
				DatabaseOperations.AddTypeConstraint(databaseMapping.Database, entityType, entityType3, entityType2, isSplitting);
			}
		}
		if (table != entityType2)
		{
			if (Properties == null)
			{
				AssociationMappingOperations.MoveAllDeclaredAssociationSetMappings(databaseMapping, entityType, table, entityType2, !isTableSharing);
				ForeignKeyPrimitiveOperations.MoveAllDeclaredForeignKeyConstraintsForPrimaryKeyColumns(entityType, table, entityType2);
			}
			if (isMappingAnyInheritedProperty)
			{
				IEnumerable<EntityType> baseTables = from mf in databaseMapping.GetEntityTypeMappings(baseType).SelectMany((EntityTypeMapping etm) => etm.MappingFragments)
					select mf.Table;
				AssociationSetMapping associationSetMapping = databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping asm) => asm.AssociationSetMappings).FirstOrDefault((AssociationSetMapping a) => baseTables.Contains(a.Table) && (baseType == a.AssociationSet.ElementType.SourceEnd.GetEntityType() || baseType == a.AssociationSet.ElementType.TargetEnd.GetEntityType()));
				if (associationSetMapping != null)
				{
					AssociationType elementType = associationSetMapping.AssociationSet.ElementType;
					throw Error.EntityMappingConfiguration_TPCWithIAsOnNonLeafType(elementType.Name, elementType.SourceEnd.GetEntityType().Name, elementType.TargetEnd.GetEntityType().Name);
				}
				ForeignKeyPrimitiveOperations.CopyAllForeignKeyConstraintsForPrimaryKeyColumns(databaseMapping.Database, table, entityType2);
			}
		}
		if (list.Any())
		{
			EntityType extraTable = null;
			if (configurationIndex < configurationCount - 1)
			{
				ColumnMappingBuilder pm2 = list.First();
				extraTable = FindTableForTemporaryExtraPropertyMapping(databaseMapping, entityType, table, entityType2, pm2);
				MappingFragment toFragment = EntityMappingOperations.CreateTypeMappingFragment(entityTypeMapping, mappingFragment, databaseMapping.Database.GetEntitySet(extraTable));
				bool requiresUpdate = extraTable != table;
				foreach (ColumnMappingBuilder item in list)
				{
					EntityMappingOperations.MovePropertyMapping(databaseMapping, entitySets, mappingFragment, toFragment, item, requiresUpdate, useExisting: true);
				}
			}
			else
			{
				EntityType unmappedTable = null;
				foreach (ColumnMappingBuilder item2 in list)
				{
					extraTable = FindTableForExtraPropertyMapping(databaseMapping, entityType, table, entityType2, ref unmappedTable, item2);
					MappingFragment mappingFragment2 = entityTypeMapping.MappingFragments.SingleOrDefault((MappingFragment tmf) => tmf.Table == extraTable);
					if (mappingFragment2 == null)
					{
						mappingFragment2 = EntityMappingOperations.CreateTypeMappingFragment(entityTypeMapping, mappingFragment, databaseMapping.Database.GetEntitySet(extraTable));
						mappingFragment2.SetIsUnmappedPropertiesFragment(isUnmappedPropertiesFragment: true);
					}
					if (extraTable == table)
					{
						CopyDefaultDiscriminator(mappingFragment, mappingFragment2);
					}
					bool requiresUpdate2 = extraTable != table;
					EntityMappingOperations.MovePropertyMapping(databaseMapping, entitySets, mappingFragment, mappingFragment2, item2, requiresUpdate2, useExisting: true);
				}
			}
		}
		EntityMappingOperations.UpdatePropertyMappings(databaseMapping, entitySets, table, mappingFragment, !isTableSharing);
		ConfigureDefaultDiscriminator(entityType, mappingFragment);
		ConfigureConditions(databaseMapping, entityType, mappingFragment, providerManifest);
		EntityMappingOperations.UpdateConditions(databaseMapping.Database, table, mappingFragment);
		ForeignKeyPrimitiveOperations.UpdatePrincipalTables(databaseMapping, entityType, table, entityType2, isMappingAnyInheritedProperty);
		CleanupUnmappedArtifacts(databaseMapping, table);
		CleanupUnmappedArtifacts(databaseMapping, entityType2);
		ConfigureAnnotations(entityType2, commonAnnotations);
		ConfigureAnnotations(entityType2, _annotations);
		entityType2.SetConfiguration(this);
	}

	private static void ConfigureAnnotations(EdmType toTable, IDictionary<string, object> annotations)
	{
		foreach (KeyValuePair<string, object> annotation in annotations)
		{
			string name = "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:" + annotation.Key;
			MetadataProperty metadataProperty = toTable.Annotations.FirstOrDefault((MetadataProperty a) => a.Name == name && !object.Equals(a.Value, annotation.Value));
			if (metadataProperty != null)
			{
				throw new InvalidOperationException(Strings.ConflictingTypeAnnotation(annotation.Key, annotation.Value, metadataProperty.Value, toTable.Name));
			}
			toTable.AddAnnotation(name, annotation.Value);
		}
	}

	internal void ConfigurePropertyMappings(IList<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings, DbProviderManifest providerManifest, bool allowOverride = false)
	{
		foreach (KeyValuePair<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> primitivePropertyConfiguration in _primitivePropertyConfigurations)
		{
			PropertyPath propertyPath = primitivePropertyConfiguration.Key;
			primitivePropertyConfiguration.Value.Configure(propertyMappings.Where((Tuple<ColumnMappingBuilder, EntityType> pm) => propertyPath.Equals(new PropertyPath(from p in pm.Item1.PropertyPath.Skip(pm.Item1.PropertyPath.Count - propertyPath.Count)
				select p.GetClrPropertyInfo())) && object.Equals(TableName, pm.Item2.GetTableName())), providerManifest, allowOverride, fillFromExistingConfiguration: true);
		}
	}

	private void ConfigureDefaultDiscriminator(EntityType entityType, MappingFragment fragment)
	{
		if (ValueConditions.Any() || NullabilityConditions.Any())
		{
			EdmProperty edmProperty = fragment.RemoveDefaultDiscriminatorCondition();
			if (edmProperty != null && entityType.BaseType != null)
			{
				edmProperty.Nullable = true;
			}
		}
	}

	private static void CopyDefaultDiscriminator(MappingFragment fromFragment, MappingFragment toFragment)
	{
		EdmProperty discriminatorColumn = fromFragment.GetDefaultDiscriminator();
		if (discriminatorColumn != null)
		{
			ConditionPropertyMapping conditionPropertyMapping = fromFragment.ColumnConditions.SingleOrDefault((ConditionPropertyMapping cc) => cc.Column == discriminatorColumn);
			if (conditionPropertyMapping != null)
			{
				toFragment.AddDiscriminatorCondition(conditionPropertyMapping.Column, conditionPropertyMapping.Value);
				toFragment.SetDefaultDiscriminator(conditionPropertyMapping.Column);
			}
		}
	}

	private static EntityType FindTableForTemporaryExtraPropertyMapping(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType fromTable, EntityType toTable, ColumnMappingBuilder pm)
	{
		EntityType entityType2 = fromTable;
		if (fromTable == toTable)
		{
			entityType2 = databaseMapping.Database.AddTable(entityType.Name, fromTable);
		}
		else if (entityType.BaseType == null)
		{
			entityType2 = fromTable;
		}
		else
		{
			entityType2 = FindBaseTableForExtraPropertyMapping(databaseMapping, entityType, pm);
			if (entityType2 == null)
			{
				entityType2 = fromTable;
			}
		}
		return entityType2;
	}

	private static EntityType FindTableForExtraPropertyMapping(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType fromTable, EntityType toTable, ref EntityType unmappedTable, ColumnMappingBuilder pm)
	{
		EntityType entityType2 = FindBaseTableForExtraPropertyMapping(databaseMapping, entityType, pm);
		if (entityType2 == null)
		{
			if (fromTable != toTable && entityType.BaseType == null)
			{
				return fromTable;
			}
			if (unmappedTable == null)
			{
				unmappedTable = databaseMapping.Database.AddTable(fromTable.Name, fromTable);
			}
			entityType2 = unmappedTable;
		}
		return entityType2;
	}

	private static EntityType FindBaseTableForExtraPropertyMapping(DbDatabaseMapping databaseMapping, EntityType entityType, ColumnMappingBuilder pm)
	{
		EntityType entityType2 = (EntityType)entityType.BaseType;
		MappingFragment mappingFragment = null;
		while (entityType2 != null && mappingFragment == null)
		{
			EntityTypeMapping entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType2);
			if (entityTypeMapping != null)
			{
				mappingFragment = entityTypeMapping.MappingFragments.SingleOrDefault((MappingFragment f) => f.ColumnMappings.Any((ColumnMappingBuilder bpm) => bpm.PropertyPath.SequenceEqual(pm.PropertyPath)));
				if (mappingFragment != null)
				{
					return mappingFragment.Table;
				}
			}
			entityType2 = (EntityType)entityType2.BaseType;
		}
		return null;
	}

	private bool DiscoverIsSharingWithBase(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable)
	{
		bool flag = false;
		if (entityType.BaseType != null)
		{
			EdmType baseType = entityType.BaseType;
			bool flag2 = false;
			while (baseType != null && !flag)
			{
				IList<EntityTypeMapping> entityTypeMappings = databaseMapping.GetEntityTypeMappings((EntityType)baseType);
				if (entityTypeMappings.Any())
				{
					flag = entityTypeMappings.SelectMany((EntityTypeMapping m) => m.MappingFragments).Any((MappingFragment tmf) => tmf.Table == toTable);
					flag2 = true;
				}
				baseType = baseType.BaseType;
			}
			if (!flag2)
			{
				flag = TableName == null || string.IsNullOrWhiteSpace(TableName.Name);
			}
		}
		return flag;
	}

	private static EntityType FindParentTable(DbDatabaseMapping databaseMapping, EntityType fromTable, EntityTypeMapping entityTypeMapping, EntityType toTable, bool isMappingInheritedProperties, int configurationIndex, int configurationCount, out bool isSplitting)
	{
		EntityType entityType = null;
		isSplitting = false;
		if ((entityTypeMapping.UsesOtherTables(toTable) || configurationCount > 1) && configurationIndex != 0)
		{
			entityType = entityTypeMapping.GetPrimaryTable();
			isSplitting = true;
		}
		if (entityType == null && fromTable != toTable && !isMappingInheritedProperties)
		{
			EdmType baseType = entityTypeMapping.EntityType.BaseType;
			while (baseType != null && entityType == null)
			{
				EntityTypeMapping entityTypeMapping2 = databaseMapping.GetEntityTypeMappings((EntityType)baseType).FirstOrDefault();
				if (entityTypeMapping2 != null)
				{
					entityType = entityTypeMapping2.GetPrimaryTable();
				}
				baseType = baseType.BaseType;
			}
		}
		return entityType;
	}

	private MappingFragment FindOrCreateTypeMappingFragment(DbDatabaseMapping databaseMapping, ref EntityTypeMapping entityTypeMapping, int configurationIndex, EntityType entityType, DbProviderManifest providerManifest)
	{
		MappingFragment mappingFragment = null;
		if (entityTypeMapping == null)
		{
			new TableMappingGenerator(providerManifest).Generate(entityType, databaseMapping);
			entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);
			configurationIndex = 0;
		}
		if (configurationIndex < entityTypeMapping.MappingFragments.Count)
		{
			return entityTypeMapping.MappingFragments[configurationIndex];
		}
		if (MapInheritedProperties)
		{
			throw Error.EntityMappingConfiguration_DuplicateMapInheritedProperties(entityType.Name);
		}
		if (Properties == null)
		{
			throw Error.EntityMappingConfiguration_DuplicateMappedProperties(entityType.Name);
		}
		Properties.Each(delegate(PropertyPath p)
		{
			if (PropertyPathToEdmPropertyPath(p, entityType).Any((EdmPropertyPath pp) => !entityType.KeyProperties().Contains(pp.First())))
			{
				throw Error.EntityMappingConfiguration_DuplicateMappedProperty(entityType.Name, p.ToString());
			}
		});
		EntityType table = entityTypeMapping.MappingFragments[0].Table;
		EntityType entityType2 = databaseMapping.Database.AddTable(table.Name, table);
		return EntityMappingOperations.CreateTypeMappingFragment(entityTypeMapping, entityTypeMapping.MappingFragments[0], databaseMapping.Database.GetEntitySet(entityType2));
	}

	private EntityType FindOrCreateTargetTable(DbDatabaseMapping databaseMapping, MappingFragment fragment, EntityType entityType, EntityType fromTable, out bool isTableSharing)
	{
		isTableSharing = false;
		EntityType entityType2;
		if (TableName == null)
		{
			entityType2 = fragment.Table;
		}
		else
		{
			entityType2 = databaseMapping.Database.FindTableByName(TableName);
			if (entityType2 == null)
			{
				entityType2 = ((entityType.BaseType != null) ? databaseMapping.Database.AddTable(TableName.Name, fromTable) : fragment.Table);
			}
			isTableSharing = UpdateColumnNamesForTableSharing(databaseMapping, entityType, entityType2, fragment);
			fragment.TableSet = databaseMapping.Database.GetEntitySet(entityType2);
			foreach (ColumnMappingBuilder columnMapping in fragment.ColumnMappings.Where((ColumnMappingBuilder cm) => cm.ColumnProperty.IsPrimaryKeyColumn))
			{
				EdmProperty edmProperty = entityType2.Properties.SingleOrDefault((EdmProperty c) => string.Equals(c.Name, columnMapping.ColumnProperty.Name, StringComparison.Ordinal));
				columnMapping.ColumnProperty = edmProperty ?? columnMapping.ColumnProperty;
			}
			entityType2.SetTableName(TableName);
		}
		return entityType2;
	}

	private HashSet<EdmPropertyPath> DiscoverAllMappingsToContain(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable, bool isSharingTableWithBase)
	{
		HashSet<EdmPropertyPath> mappingsToContain = new HashSet<EdmPropertyPath>();
		entityType.KeyProperties().Each(delegate(EdmProperty p)
		{
			mappingsToContain.AddRange(p.ToPropertyPathList());
		});
		if (MapInheritedProperties)
		{
			entityType.Properties.Except(entityType.DeclaredProperties).Each(delegate(EdmProperty p)
			{
				mappingsToContain.AddRange(p.ToPropertyPathList());
			});
		}
		if (isSharingTableWithBase)
		{
			HashSet<EdmPropertyPath> baseMappingsToContain = new HashSet<EdmPropertyPath>();
			EntityType entityType2 = (EntityType)entityType.BaseType;
			EntityTypeMapping entityTypeMapping = null;
			MappingFragment mappingFragment = null;
			while (entityType2 != null && entityTypeMapping == null)
			{
				entityTypeMapping = databaseMapping.GetEntityTypeMapping((EntityType)entityType.BaseType);
				if (entityTypeMapping != null)
				{
					mappingFragment = entityTypeMapping.MappingFragments.SingleOrDefault((MappingFragment tmf) => tmf.Table == toTable);
				}
				if (mappingFragment == null)
				{
					entityType2.DeclaredProperties.Each(delegate(EdmProperty p)
					{
						baseMappingsToContain.AddRange(p.ToPropertyPathList());
					});
				}
				entityType2 = (EntityType)entityType2.BaseType;
			}
			if (mappingFragment != null)
			{
				foreach (ColumnMappingBuilder columnMapping in mappingFragment.ColumnMappings)
				{
					mappingsToContain.Add(new EdmPropertyPath(columnMapping.PropertyPath));
				}
			}
			mappingsToContain.AddRange(baseMappingsToContain);
		}
		if (Properties == null)
		{
			entityType.DeclaredProperties.Each(delegate(EdmProperty p)
			{
				mappingsToContain.AddRange(p.ToPropertyPathList());
			});
		}
		else
		{
			Properties.Each(delegate(PropertyPath p)
			{
				mappingsToContain.AddRange(PropertyPathToEdmPropertyPath(p, entityType));
			});
		}
		return mappingsToContain;
	}

	private void ConfigureConditions(DbDatabaseMapping databaseMapping, EntityType entityType, MappingFragment fragment, DbProviderManifest providerManifest)
	{
		if (!ValueConditions.Any() && !NullabilityConditions.Any())
		{
			return;
		}
		fragment.ClearConditions();
		foreach (ValueConditionConfiguration valueCondition in ValueConditions)
		{
			valueCondition.Configure(databaseMapping, fragment, entityType, providerManifest);
		}
		foreach (NotNullConditionConfiguration nullabilityCondition in NullabilityConditions)
		{
			nullabilityCondition.Configure(databaseMapping, fragment, entityType);
		}
	}

	internal static void CleanupUnmappedArtifacts(DbDatabaseMapping databaseMapping, EntityType table)
	{
		AssociationSetMapping[] source = (from asm in databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.AssociationSetMappings)
			where asm.Table == table
			select asm).ToArray();
		MappingFragment[] source2 = (from f in databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.EntitySetMappings).SelectMany((EntitySetMapping esm) => esm.EntityTypeMappings).SelectMany((EntityTypeMapping etm) => etm.MappingFragments)
			where f.Table == table
			select f).ToArray();
		if (!source.Any() && !source2.Any())
		{
			databaseMapping.Database.RemoveEntityType(table);
			databaseMapping.Database.AssociationTypes.Where((AssociationType t) => t.SourceEnd.GetEntityType() == table || t.TargetEnd.GetEntityType() == table).ToArray().Each(delegate(AssociationType t)
			{
				databaseMapping.Database.RemoveAssociationType(t);
			});
			return;
		}
		EdmProperty[] array = table.Properties.ToArray();
		foreach (EdmProperty column in array)
		{
			if (source2.SelectMany((MappingFragment f) => f.ColumnMappings).All((ColumnMappingBuilder pm) => pm.ColumnProperty != column) && source2.SelectMany((MappingFragment f) => f.ColumnConditions).All((ConditionPropertyMapping cc) => cc.Column != column) && source.SelectMany((AssociationSetMapping am) => am.SourceEndMapping.PropertyMappings).All((ScalarPropertyMapping pm) => pm.Column != column) && source.SelectMany((AssociationSetMapping am) => am.SourceEndMapping.PropertyMappings).All((ScalarPropertyMapping pm) => pm.Column != column))
			{
				ForeignKeyPrimitiveOperations.RemoveAllForeignKeyConstraintsForColumn(table, column, databaseMapping);
				TablePrimitiveOperations.RemoveColumn(table, column);
			}
		}
		table.ForeignKeyBuilders.Where((ForeignKeyBuilder fk) => fk.PrincipalTable == table && fk.DependentColumns.SequenceEqual(table.KeyProperties)).ToArray().Each(table.RemoveForeignKey);
	}

	internal static IEnumerable<EdmPropertyPath> PropertyPathToEdmPropertyPath(PropertyPath path, EntityType entityType)
	{
		List<EdmProperty> list = new List<EdmProperty>();
		StructuralType structuralType = entityType;
		int i;
		for (i = 0; i < path.Count; i++)
		{
			EdmProperty edmProperty = structuralType.Members.OfType<EdmProperty>().SingleOrDefault((EdmProperty p) => p.GetClrPropertyInfo().IsSameAs(path[i]));
			if (edmProperty == null)
			{
				throw Error.EntityMappingConfiguration_CannotMapIgnoredProperty(entityType.Name, path.ToString());
			}
			list.Add(edmProperty);
			if (edmProperty.IsComplexType)
			{
				structuralType = edmProperty.ComplexType;
			}
		}
		EdmProperty edmProperty2 = list.Last();
		if (edmProperty2.IsUnderlyingPrimitiveType)
		{
			return new EdmPropertyPath[1]
			{
				new EdmPropertyPath(list)
			};
		}
		if (edmProperty2.IsComplexType)
		{
			list.Remove(edmProperty2);
			return edmProperty2.ToPropertyPathList(list);
		}
		return new EdmPropertyPath[1] { EdmPropertyPath.Empty };
	}

	private static List<EntityTypeMapping> FindAllTypeMappingsUsingTable(DbDatabaseMapping databaseMapping, EntityType toTable)
	{
		List<EntityTypeMapping> list = new List<EntityTypeMapping>();
		IList<EntityContainerMapping> entityContainerMappings = databaseMapping.EntityContainerMappings;
		for (int i = 0; i < entityContainerMappings.Count; i++)
		{
			List<EntitySetMapping> list2 = entityContainerMappings[i].EntitySetMappings.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				ReadOnlyCollection<EntityTypeMapping> entityTypeMappings = list2[j].EntityTypeMappings;
				for (int k = 0; k < entityTypeMappings.Count; k++)
				{
					EntityTypeMapping entityTypeMapping = entityTypeMappings[k];
					EntityTypeConfiguration entityTypeConfiguration = entityTypeMapping.EntityType.GetConfiguration() as EntityTypeConfiguration;
					for (int l = 0; l < entityTypeMapping.MappingFragments.Count; l++)
					{
						bool flag = entityTypeConfiguration?.IsTableNameConfigured ?? false;
						if ((!flag && entityTypeMapping.MappingFragments[l].Table == toTable) || (flag && IsTableNameEqual(toTable, entityTypeConfiguration.GetTableName())))
						{
							list.Add(entityTypeMapping);
							break;
						}
					}
				}
			}
		}
		return list;
	}

	private static bool IsTableNameEqual(EntityType table, DatabaseName otherTableName)
	{
		DatabaseName tableName = table.GetTableName();
		if (tableName != null)
		{
			return otherTableName.Equals(tableName);
		}
		if (otherTableName.Name.Equals(table.Name, StringComparison.Ordinal))
		{
			return otherTableName.Schema == null;
		}
		return false;
	}

	private static IEnumerable<AssociationType> FindAllOneToOneFKAssociationTypes(EdmModel model, EntityType entityType, EntityType candidateType)
	{
		List<AssociationType> list = new List<AssociationType>();
		foreach (EntityContainer container in model.Containers)
		{
			ReadOnlyMetadataCollection<AssociationSet> associationSets = container.AssociationSets;
			for (int i = 0; i < associationSets.Count; i++)
			{
				AssociationSet associationSet = associationSets[i];
				AssociationEndMember sourceEnd = associationSet.ElementType.SourceEnd;
				AssociationEndMember targetEnd = associationSet.ElementType.TargetEnd;
				EntityType entityType2 = sourceEnd.GetEntityType();
				EntityType entityType3 = targetEnd.GetEntityType();
				if (associationSet.ElementType.Constraint != null && sourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.One && targetEnd.RelationshipMultiplicity == RelationshipMultiplicity.One && ((entityType2 == entityType && entityType3 == candidateType) || (entityType3 == entityType && entityType2 == candidateType)))
				{
					list.Add(associationSet.ElementType);
				}
			}
		}
		return list;
	}

	private static bool UpdateColumnNamesForTableSharing(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable, MappingFragment fragment)
	{
		List<EntityTypeMapping> list = FindAllTypeMappingsUsingTable(databaseMapping, toTable);
		Dictionary<EntityType, List<AssociationType>> dictionary = new Dictionary<EntityType, List<AssociationType>>();
		foreach (EntityTypeMapping item in list)
		{
			EntityType entityType2 = item.EntityType;
			if (entityType != entityType2)
			{
				IEnumerable<AssociationType> enumerable = FindAllOneToOneFKAssociationTypes(databaseMapping.Model, entityType, entityType2);
				EntityType rootType = entityType2.GetRootType();
				if (!dictionary.ContainsKey(rootType))
				{
					dictionary.Add(rootType, enumerable.ToList());
				}
				else
				{
					dictionary[rootType].AddRange(enumerable);
				}
			}
		}
		List<EntityType> list2 = new List<EntityType>();
		foreach (KeyValuePair<EntityType, List<AssociationType>> item2 in dictionary)
		{
			if (item2.Key != entityType.GetRootType() && item2.Value.Count == 0)
			{
				list2.Add(item2.Key);
			}
		}
		if (list2.Count > 0 && list2.Count == dictionary.Count)
		{
			DatabaseName tableName = toTable.GetTableName();
			throw Error.EntityMappingConfiguration_InvalidTableSharing(entityType.Name, list2.First().Name, (tableName != null) ? tableName.Name : databaseMapping.Database.GetEntitySet(toTable).Table);
		}
		IEnumerable<AssociationType> source = dictionary.Values.SelectMany((List<AssociationType> l) => l);
		if (source.Any())
		{
			AssociationType associationType = source.First();
			EntityType entityType3 = associationType.Constraint.FromRole.GetEntityType();
			EntityType dependentEntityType = ((entityType == entityType3) ? associationType.Constraint.ToRole.GetEntityType() : entityType);
			MappingFragment mappingFragment = ((entityType == entityType3) ? list.Single((EntityTypeMapping etm) => etm.EntityType == dependentEntityType).Fragments.SingleOrDefault((MappingFragment mf) => mf.Table == toTable) : fragment);
			if (mappingFragment != null)
			{
				List<EdmProperty> list3 = entityType3.KeyProperties().ToList();
				List<EdmProperty> list4 = dependentEntityType.KeyProperties().ToList();
				for (int i = 0; i < list3.Count; i++)
				{
					EdmProperty dependentKey = list4[i];
					dependentKey.SetStoreGeneratedPattern(StoreGeneratedPattern.None);
					mappingFragment.ColumnMappings.Single((ColumnMappingBuilder pm) => pm.PropertyPath.First() == dependentKey).ColumnProperty.Name = list3[i].Name;
				}
			}
			return true;
		}
		return false;
	}
}
