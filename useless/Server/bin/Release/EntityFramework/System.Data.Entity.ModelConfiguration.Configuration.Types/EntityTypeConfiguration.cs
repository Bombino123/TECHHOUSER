using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Index;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Edm.Services;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration.Types;

internal class EntityTypeConfiguration : StructuralTypeConfiguration
{
	private readonly List<PropertyInfo> _keyProperties = new List<PropertyInfo>();

	private System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration _keyConfiguration;

	private readonly Dictionary<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration> _indexConfigurations = new Dictionary<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration>();

	private readonly Dictionary<PropertyInfo, NavigationPropertyConfiguration> _navigationPropertyConfigurations = new Dictionary<PropertyInfo, NavigationPropertyConfiguration>(new DynamicEqualityComparer<PropertyInfo>((PropertyInfo p1, PropertyInfo p2) => p1.IsSameAs(p2)));

	private readonly List<EntityMappingConfiguration> _entityMappingConfigurations = new List<EntityMappingConfiguration>();

	private readonly Dictionary<Type, EntityMappingConfiguration> _entitySubTypesMappingConfigurations = new Dictionary<Type, EntityMappingConfiguration>();

	private readonly List<EntityMappingConfiguration> _nonCloneableMappings = new List<EntityMappingConfiguration>();

	private readonly IDictionary<string, object> _annotations = new Dictionary<string, object>();

	private string _entitySetName;

	private ModificationStoredProceduresConfiguration _modificationStoredProceduresConfiguration;

	internal IEnumerable<Type> ConfiguredComplexTypes => from pi in (from c in base.PrimitivePropertyConfigurations
			where c.Key.Count > 1
			select c.Key.Reverse().Skip(1)).SelectMany((IEnumerable<PropertyInfo> p) => p)
		select pi.PropertyType;

	internal bool IsStructuralConfigurationOnly
	{
		get
		{
			if (!_keyProperties.Any() && !_navigationPropertyConfigurations.Any() && !_entityMappingConfigurations.Any() && !_entitySubTypesMappingConfigurations.Any())
			{
				return _entitySetName == null;
			}
			return false;
		}
	}

	internal bool IsKeyConfigured => _keyConfiguration != null;

	internal IEnumerable<PropertyInfo> KeyProperties => _keyProperties;

	internal IEnumerable<PropertyPath> PropertyIndexes => _indexConfigurations.Keys;

	public bool IsTableNameConfigured { get; private set; }

	internal bool IsReplaceable { get; set; }

	internal bool IsExplicitEntity { get; set; }

	internal ModificationStoredProceduresConfiguration ModificationStoredProceduresConfiguration => _modificationStoredProceduresConfiguration;

	public virtual string EntitySetName
	{
		get
		{
			return _entitySetName;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_entitySetName = value;
		}
	}

	internal override IEnumerable<PropertyInfo> ConfiguredProperties => base.ConfiguredProperties.Union(_navigationPropertyConfigurations.Keys);

	public string TableName
	{
		get
		{
			if (!IsTableNameConfigured)
			{
				return null;
			}
			return GetTableName().Name;
		}
	}

	public string SchemaName
	{
		get
		{
			if (!IsTableNameConfigured)
			{
				return null;
			}
			return GetTableName().Schema;
		}
	}

	public IDictionary<string, object> Annotations => _annotations;

	internal Dictionary<Type, EntityMappingConfiguration> SubTypeMappingConfigurations => _entitySubTypesMappingConfigurations;

	internal EntityTypeConfiguration(Type structuralType)
		: base(structuralType)
	{
		IsReplaceable = false;
	}

	private EntityTypeConfiguration(EntityTypeConfiguration source)
		: base(source)
	{
		_keyProperties.AddRange(source._keyProperties);
		_keyConfiguration = source._keyConfiguration;
		source._indexConfigurations.Each(delegate(KeyValuePair<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration> c)
		{
			_indexConfigurations.Add(c.Key, c.Value.Clone());
		});
		source._navigationPropertyConfigurations.Each(delegate(KeyValuePair<PropertyInfo, NavigationPropertyConfiguration> c)
		{
			_navigationPropertyConfigurations.Add(c.Key, c.Value.Clone());
		});
		source._entitySubTypesMappingConfigurations.Each(delegate(KeyValuePair<Type, EntityMappingConfiguration> c)
		{
			_entitySubTypesMappingConfigurations.Add(c.Key, c.Value.Clone());
		});
		_entityMappingConfigurations.AddRange(from e in source._entityMappingConfigurations.Except(source._nonCloneableMappings)
			select e.Clone());
		_entitySetName = source._entitySetName;
		if (source._modificationStoredProceduresConfiguration != null)
		{
			_modificationStoredProceduresConfiguration = source._modificationStoredProceduresConfiguration.Clone();
		}
		IsReplaceable = source.IsReplaceable;
		IsTableNameConfigured = source.IsTableNameConfigured;
		IsExplicitEntity = source.IsExplicitEntity;
		foreach (KeyValuePair<string, object> annotation in source._annotations)
		{
			_annotations.Add(annotation);
		}
	}

	internal virtual EntityTypeConfiguration Clone()
	{
		return new EntityTypeConfiguration(this);
	}

	internal override void RemoveProperty(PropertyPath propertyPath)
	{
		base.RemoveProperty(propertyPath);
		_navigationPropertyConfigurations.Remove(propertyPath.Single());
	}

	internal virtual void Key(IEnumerable<PropertyInfo> keyProperties)
	{
		ClearKey();
		foreach (PropertyInfo keyProperty in keyProperties)
		{
			Key(keyProperty, OverridableConfigurationParts.None);
		}
		if (_keyConfiguration == null)
		{
			_keyConfiguration = new System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration();
		}
	}

	public void Key(PropertyInfo propertyInfo)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		Key(propertyInfo, null);
	}

	internal virtual void Key(PropertyInfo propertyInfo, OverridableConfigurationParts? overridableConfigurationParts)
	{
		if (!propertyInfo.IsValidEdmScalarProperty())
		{
			throw Error.ModelBuilder_KeyPropertiesMustBePrimitive(propertyInfo.Name, base.ClrType);
		}
		if (_keyConfiguration == null && !_keyProperties.ContainsSame(propertyInfo))
		{
			_keyProperties.Add(propertyInfo);
			Property(new PropertyPath(propertyInfo), overridableConfigurationParts);
		}
	}

	internal virtual System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration ConfigureKey()
	{
		if (_keyConfiguration == null)
		{
			_keyConfiguration = new System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration();
		}
		return _keyConfiguration;
	}

	internal virtual System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration Index(PropertyPath indexProperties)
	{
		if (!_indexConfigurations.TryGetValue(indexProperties, out var value))
		{
			_indexConfigurations.Add(indexProperties, value = new System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration());
		}
		return value;
	}

	internal void ClearKey()
	{
		_keyProperties.Clear();
		_keyConfiguration = null;
	}

	internal virtual void MapToStoredProcedures()
	{
		if (_modificationStoredProceduresConfiguration == null)
		{
			_modificationStoredProceduresConfiguration = new ModificationStoredProceduresConfiguration();
		}
	}

	internal virtual void MapToStoredProcedures(ModificationStoredProceduresConfiguration modificationStoredProceduresConfiguration, bool allowOverride)
	{
		if (_modificationStoredProceduresConfiguration == null)
		{
			_modificationStoredProceduresConfiguration = modificationStoredProceduresConfiguration;
		}
		else
		{
			_modificationStoredProceduresConfiguration.Merge(modificationStoredProceduresConfiguration, allowOverride);
		}
	}

	internal void ReplaceFrom(EntityTypeConfiguration existing)
	{
		if (EntitySetName == null)
		{
			EntitySetName = existing.EntitySetName;
		}
	}

	internal DatabaseName GetTableName()
	{
		if (!IsTableNameConfigured)
		{
			return null;
		}
		return _entityMappingConfigurations.First().TableName;
	}

	public void ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		ToTable(tableName, null);
	}

	public void ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		IsTableNameConfigured = true;
		if (!_entityMappingConfigurations.Any())
		{
			_entityMappingConfigurations.Add(new EntityMappingConfiguration());
		}
		_entityMappingConfigurations.First().TableName = (string.IsNullOrWhiteSpace(schemaName) ? new DatabaseName(tableName) : new DatabaseName(tableName, schemaName));
		UpdateTableNameForSubTypes();
	}

	public virtual void SetAnnotation(string name, object value)
	{
		if (!name.IsValidUndottedName())
		{
			throw new ArgumentException(Strings.BadAnnotationName(name));
		}
		_annotations[name] = value;
	}

	private void UpdateTableNameForSubTypes()
	{
		(from stmc in _entitySubTypesMappingConfigurations
			where stmc.Value.TableName == null
			select stmc into tphs
			select tphs.Value).Each((EntityMappingConfiguration tphmc) => tphmc.TableName = GetTableName());
	}

	internal void AddMappingConfiguration(EntityMappingConfiguration mappingConfiguration, bool cloneable = true)
	{
		if (!_entityMappingConfigurations.Contains(mappingConfiguration))
		{
			DatabaseName tableName = mappingConfiguration.TableName;
			if (tableName != null && _entityMappingConfigurations.SingleOrDefault((EntityMappingConfiguration mf) => tableName.Equals(mf.TableName)) != null)
			{
				throw Error.InvalidTableMapping(base.ClrType.Name, tableName);
			}
			_entityMappingConfigurations.Add(mappingConfiguration);
			if (_entityMappingConfigurations.Count > 1 && _entityMappingConfigurations.Any((EntityMappingConfiguration mc) => mc.TableName == null))
			{
				throw Error.InvalidTableMapping_NoTableName(base.ClrType.Name);
			}
			IsTableNameConfigured |= tableName != null;
			if (!cloneable)
			{
				_nonCloneableMappings.Add(mappingConfiguration);
			}
		}
	}

	internal void AddSubTypeMappingConfiguration(Type subType, EntityMappingConfiguration mappingConfiguration)
	{
		if (_entitySubTypesMappingConfigurations.TryGetValue(subType, out var _))
		{
			throw Error.InvalidChainedMappingSyntax(subType.Name);
		}
		_entitySubTypesMappingConfigurations.Add(subType, mappingConfiguration);
	}

	internal NavigationPropertyConfiguration Navigation(PropertyInfo propertyInfo)
	{
		if (!_navigationPropertyConfigurations.TryGetValue(propertyInfo, out var value))
		{
			_navigationPropertyConfigurations.Add(propertyInfo, value = new NavigationPropertyConfiguration(propertyInfo));
		}
		return value;
	}

	internal virtual void Configure(EntityType entityType, EdmModel model)
	{
		ConfigureKey(entityType);
		Configure(entityType.Name, entityType.Properties, entityType.GetMetadataProperties());
		ConfigureAssociations(entityType, model);
		ConfigureEntitySetName(entityType, model);
	}

	private void ConfigureEntitySetName(EntityType entityType, EdmModel model)
	{
		if (EntitySetName != null && entityType.BaseType == null)
		{
			EntitySet entitySet = model.GetEntitySet(entityType);
			entitySet.Name = model.GetEntitySets().Except(new EntitySet[1] { entitySet }).UniquifyName(EntitySetName);
			entitySet.SetConfiguration(this);
		}
	}

	private void ConfigureKey(EntityType entityType)
	{
		if (!_keyProperties.Any())
		{
			return;
		}
		if (entityType.BaseType != null)
		{
			throw Error.KeyRegisteredOnDerivedType(base.ClrType, entityType.GetRootType().GetClrType());
		}
		IEnumerable<PropertyInfo> enumerable = _keyProperties.AsEnumerable();
		if (_keyConfiguration == null)
		{
			var source = _keyProperties.Select((PropertyInfo p) => new
			{
				PropertyInfo = p,
				ColumnOrder = Property(new PropertyPath(p)).ColumnOrder
			});
			if (_keyProperties.Count > 1 && source.Any(p => !p.ColumnOrder.HasValue))
			{
				throw Error.ModelGeneration_UnableToDetermineKeyOrder(base.ClrType);
			}
			enumerable = from p in source
				orderby p.ColumnOrder
				select p.PropertyInfo;
		}
		foreach (PropertyInfo item in enumerable)
		{
			EdmProperty declaredPrimitiveProperty = entityType.GetDeclaredPrimitiveProperty(item);
			if (declaredPrimitiveProperty == null)
			{
				throw Error.KeyPropertyNotFound(item.Name, entityType.Name);
			}
			declaredPrimitiveProperty.Nullable = false;
			entityType.AddKeyMember(declaredPrimitiveProperty);
		}
	}

	private void ConfigureIndexes(DbDatabaseMapping mapping, EntityType entityType)
	{
		IList<EntityTypeMapping> entityTypeMappings = mapping.GetEntityTypeMappings(entityType);
		if (_keyConfiguration != null)
		{
			entityTypeMappings.SelectMany((EntityTypeMapping etm) => etm.Fragments).Each(delegate(MappingFragment f)
			{
				_keyConfiguration.Configure(f.Table);
			});
		}
		foreach (KeyValuePair<PropertyPath, System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration> indexConfiguration in _indexConfigurations)
		{
			foreach (EntityTypeMapping entityTypeMapping in entityTypeMappings)
			{
				Dictionary<PropertyInfo, ColumnMappingBuilder> propertyMappings = indexConfiguration.Key.ToDictionary((PropertyInfo icp) => icp, (PropertyInfo icp) => entityTypeMapping.GetPropertyMapping(entityType.GetDeclaredPrimitiveProperty(icp)));
				if (indexConfiguration.Key.Count > 1 && string.IsNullOrEmpty(indexConfiguration.Value.Name))
				{
					indexConfiguration.Value.Name = IndexOperation.BuildDefaultName(indexConfiguration.Key.Select((PropertyInfo icp) => propertyMappings[icp].ColumnProperty.Name));
				}
				int num = 0;
				foreach (PropertyInfo item in (IEnumerable<PropertyInfo>)indexConfiguration.Key)
				{
					ColumnMappingBuilder columnMappingBuilder = propertyMappings[item];
					indexConfiguration.Value.Configure(columnMappingBuilder.ColumnProperty, (indexConfiguration.Key.Count != 1) ? num : (-1));
					num++;
				}
			}
		}
	}

	private void ConfigureAssociations(EntityType entityType, EdmModel model)
	{
		foreach (KeyValuePair<PropertyInfo, NavigationPropertyConfiguration> navigationPropertyConfiguration in _navigationPropertyConfigurations)
		{
			PropertyInfo propertyInfo = navigationPropertyConfiguration.Key;
			NavigationPropertyConfiguration value = navigationPropertyConfiguration.Value;
			NavigationProperty navigationProperty = entityType.GetNavigationProperty(propertyInfo);
			if (navigationProperty == null)
			{
				EdmProperty edmProperty = entityType.Properties.SingleOrDefault((EdmProperty p) => p.GetClrPropertyInfo() == propertyInfo);
				if (edmProperty != null && edmProperty.ComplexType != null)
				{
					throw new InvalidOperationException(Strings.InvalidNavigationPropertyComplexType(propertyInfo.Name, entityType.Name, edmProperty.ComplexType.Name));
				}
				throw Error.NavigationPropertyNotFound(propertyInfo.Name, entityType.Name);
			}
			if (entityType.DeclaredNavigationProperties.Any((NavigationProperty np) => np.GetClrPropertyInfo().IsSameAs(propertyInfo)))
			{
				value.Configure(navigationProperty, model, this);
			}
		}
	}

	internal void ConfigureTablesAndConditions(EntityTypeMapping entityTypeMapping, DbDatabaseMapping databaseMapping, ICollection<EntitySet> entitySets, DbProviderManifest providerManifest)
	{
		EntityType entityType = ((entityTypeMapping != null) ? entityTypeMapping.EntityType : databaseMapping.Model.GetEntityType(base.ClrType));
		if (_entityMappingConfigurations.Any())
		{
			for (int i = 0; i < _entityMappingConfigurations.Count; i++)
			{
				_entityMappingConfigurations[i].Configure(databaseMapping, entitySets, providerManifest, entityType, ref entityTypeMapping, IsMappingAnyInheritedProperty(entityType), i, _entityMappingConfigurations.Count, _annotations);
			}
		}
		else
		{
			ConfigureUnconfiguredType(databaseMapping, entitySets, providerManifest, entityType, _annotations);
		}
	}

	internal bool IsMappingAnyInheritedProperty(EntityType entityType)
	{
		return _entityMappingConfigurations.Any((EntityMappingConfiguration emc) => emc.MapsAnyInheritedProperties(entityType));
	}

	internal bool IsNavigationPropertyConfigured(PropertyInfo propertyInfo)
	{
		return _navigationPropertyConfigurations.ContainsKey(propertyInfo);
	}

	internal static void ConfigureUnconfiguredType(DbDatabaseMapping databaseMapping, ICollection<EntitySet> entitySets, DbProviderManifest providerManifest, EntityType entityType, IDictionary<string, object> commonAnnotations)
	{
		EntityMappingConfiguration entityMappingConfiguration = new EntityMappingConfiguration();
		EntityTypeMapping entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType.GetClrType());
		entityMappingConfiguration.Configure(databaseMapping, entitySets, providerManifest, entityType, ref entityTypeMapping, isMappingAnyInheritedProperty: false, 0, 1, commonAnnotations);
	}

	internal void Configure(EntityType entityType, DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
	{
		EntityTypeMapping entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType.GetClrType());
		if (entityTypeMapping != null)
		{
			VerifyAllCSpacePropertiesAreMapped(databaseMapping.GetEntityTypeMappings(entityType).ToList(), entityTypeMapping.EntityType.DeclaredProperties, new List<EdmProperty>());
		}
		ConfigurePropertyMappings(databaseMapping, entityType, providerManifest);
		ConfigureIndexes(databaseMapping, entityType);
		ConfigureAssociationMappings(databaseMapping, entityType, providerManifest);
		ConfigureDependentKeys(databaseMapping, providerManifest);
		ConfigureModificationStoredProcedures(databaseMapping, entityType, providerManifest);
	}

	internal void ConfigureFunctionParameters(DbDatabaseMapping databaseMapping, EntityType entityType)
	{
		List<ModificationFunctionParameterBinding> parameterBindings = (from esm in databaseMapping.GetEntitySetMappings()
			from mfm in esm.ModificationFunctionMappings
			where mfm.EntityType == entityType
			from pb in mfm.PrimaryParameterBindings
			select pb).ToList();
		ConfigureFunctionParameters(parameterBindings);
		foreach (EntityType item in databaseMapping.Model.EntityTypes.Where((EntityType et) => et.BaseType == entityType))
		{
			ConfigureFunctionParameters(databaseMapping, item);
		}
	}

	private void ConfigureModificationStoredProcedures(DbDatabaseMapping databaseMapping, EntityType entityType, DbProviderManifest providerManifest)
	{
		if (_modificationStoredProceduresConfiguration != null)
		{
			new ModificationFunctionMappingGenerator(providerManifest).Generate(entityType, databaseMapping);
			EntityTypeModificationFunctionMapping entityTypeModificationFunctionMapping = databaseMapping.GetEntitySetMappings().SelectMany((EntitySetMapping esm) => esm.ModificationFunctionMappings).SingleOrDefault((EntityTypeModificationFunctionMapping mfm) => mfm.EntityType == entityType);
			if (entityTypeModificationFunctionMapping != null)
			{
				_modificationStoredProceduresConfiguration.Configure(entityTypeModificationFunctionMapping, providerManifest);
			}
		}
	}

	private void ConfigurePropertyMappings(DbDatabaseMapping databaseMapping, EntityType entityType, DbProviderManifest providerManifest, bool allowOverride = false)
	{
		IList<EntityTypeMapping> entityTypeMappings = databaseMapping.GetEntityTypeMappings(entityType);
		List<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings = (from etm in entityTypeMappings
			from etmf in etm.MappingFragments
			from pm in etmf.ColumnMappings
			select Tuple.Create(pm, etmf.Table)).ToList();
		ConfigurePropertyMappings(propertyMappings, providerManifest, allowOverride);
		_entityMappingConfigurations.Each(delegate(EntityMappingConfiguration c)
		{
			c.ConfigurePropertyMappings(propertyMappings, providerManifest, allowOverride);
		});
		List<Tuple<ColumnMappingBuilder, EntityType>> inheritedPropertyMappings = (from esm in databaseMapping.GetEntitySetMappings()
			from etm in esm.EntityTypeMappings
			where etm.IsHierarchyMapping && etm.EntityType.IsAncestorOf(entityType)
			from etmf in etm.MappingFragments
			from pm1 in etmf.ColumnMappings
			where !propertyMappings.Any((Tuple<ColumnMappingBuilder, EntityType> pm2) => pm2.Item1.PropertyPath.SequenceEqual(pm1.PropertyPath))
			select Tuple.Create(pm1, etmf.Table)).ToList();
		ConfigurePropertyMappings(inheritedPropertyMappings, providerManifest);
		_entityMappingConfigurations.Each(delegate(EntityMappingConfiguration c)
		{
			c.ConfigurePropertyMappings(inheritedPropertyMappings, providerManifest);
		});
		foreach (EntityType item in databaseMapping.Model.EntityTypes.Where((EntityType et) => et.BaseType == entityType))
		{
			ConfigurePropertyMappings(databaseMapping, item, providerManifest, allowOverride: true);
		}
	}

	private void ConfigureAssociationMappings(DbDatabaseMapping databaseMapping, EntityType entityType, DbProviderManifest providerManifest)
	{
		foreach (KeyValuePair<PropertyInfo, NavigationPropertyConfiguration> navigationPropertyConfiguration in _navigationPropertyConfigurations)
		{
			PropertyInfo key = navigationPropertyConfiguration.Key;
			NavigationPropertyConfiguration value = navigationPropertyConfiguration.Value;
			NavigationProperty navigationProperty = entityType.GetNavigationProperty(key);
			if (navigationProperty == null)
			{
				throw Error.NavigationPropertyNotFound(key.Name, entityType.Name);
			}
			AssociationSetMapping associationSetMapping = databaseMapping.GetAssociationSetMappings().SingleOrDefault((AssociationSetMapping asm) => asm.AssociationSet.ElementType == navigationProperty.Association);
			if (associationSetMapping != null)
			{
				value.Configure(associationSetMapping, databaseMapping, providerManifest);
			}
		}
	}

	private static void ConfigureDependentKeys(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
	{
		IList<EntityType> list = (databaseMapping.Database.EntityTypes as IList<EntityType>) ?? databaseMapping.Database.EntityTypes.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			EntityType entityType = list[i];
			IList<ForeignKeyBuilder> list2 = (entityType.ForeignKeyBuilders as IList<ForeignKeyBuilder>) ?? entityType.ForeignKeyBuilders.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				ForeignKeyBuilder foreignKeyBuilder = list2[j];
				IEnumerable<EdmProperty> dependentColumns = foreignKeyBuilder.DependentColumns;
				IList<EdmProperty> list3 = (dependentColumns as IList<EdmProperty>) ?? dependentColumns.ToList();
				for (int k = 0; k < list3.Count; k++)
				{
					EdmProperty edmProperty = list3[k];
					if (!(edmProperty.GetConfiguration() is System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration { ColumnType: not null }))
					{
						EdmProperty edmProperty2 = foreignKeyBuilder.PrincipalTable.KeyProperties.ElementAt(k);
						edmProperty.PrimitiveType = providerManifest.GetStoreTypeFromName(edmProperty2.TypeName);
						edmProperty.CopyFrom(edmProperty2);
					}
				}
			}
		}
	}

	private static void VerifyAllCSpacePropertiesAreMapped(ICollection<EntityTypeMapping> entityTypeMappings, IEnumerable<EdmProperty> properties, IList<EdmProperty> propertyPath)
	{
		EntityType entityType = entityTypeMappings.First().EntityType;
		foreach (EdmProperty property in properties)
		{
			propertyPath.Add(property);
			if (property.IsComplexType)
			{
				VerifyAllCSpacePropertiesAreMapped(entityTypeMappings, property.ComplexType.Properties, propertyPath);
			}
			else if (!entityTypeMappings.SelectMany((EntityTypeMapping etm) => etm.MappingFragments).SelectMany((MappingFragment mf) => mf.ColumnMappings).Any((ColumnMappingBuilder pm) => pm.PropertyPath.SequenceEqual(propertyPath)) && !entityType.Abstract)
			{
				throw Error.InvalidEntitySplittingProperties(entityType.Name);
			}
			propertyPath.Remove(property);
		}
	}
}
