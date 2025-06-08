using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ModelConfiguration : ConfigurationBase
{
	private readonly Dictionary<Type, EntityTypeConfiguration> _entityConfigurations = new Dictionary<Type, EntityTypeConfiguration>();

	private readonly Dictionary<Type, ComplexTypeConfiguration> _complexTypeConfigurations = new Dictionary<Type, ComplexTypeConfiguration>();

	private readonly HashSet<Type> _ignoredTypes = new HashSet<Type>();

	public virtual IEnumerable<Type> ConfiguredTypes => _entityConfigurations.Keys.Union(_complexTypeConfigurations.Keys).Union(_ignoredTypes);

	internal virtual IEnumerable<Type> Entities => _entityConfigurations.Keys.Except(_ignoredTypes).ToList();

	internal virtual IEnumerable<Type> ComplexTypes => _complexTypeConfigurations.Keys.Except(_ignoredTypes).ToList();

	internal virtual IEnumerable<Type> StructuralTypes => _entityConfigurations.Keys.Union(_complexTypeConfigurations.Keys).Except(_ignoredTypes).ToList();

	public string DefaultSchema { get; set; }

	public string ModelNamespace { get; set; }

	private IEnumerable<EntityTypeConfiguration> ActiveEntityConfigurations => _entityConfigurations.Where(delegate(KeyValuePair<Type, EntityTypeConfiguration> keyValuePair)
	{
		HashSet<Type> ignoredTypes = _ignoredTypes;
		KeyValuePair<Type, EntityTypeConfiguration> keyValuePair3 = keyValuePair;
		return !ignoredTypes.Contains(keyValuePair3.Key);
	}).Select(delegate(KeyValuePair<Type, EntityTypeConfiguration> keyValuePair)
	{
		KeyValuePair<Type, EntityTypeConfiguration> keyValuePair2 = keyValuePair;
		return keyValuePair2.Value;
	}).ToList();

	private IEnumerable<ComplexTypeConfiguration> ActiveComplexTypeConfigurations => _complexTypeConfigurations.Where(delegate(KeyValuePair<Type, ComplexTypeConfiguration> keyValuePair)
	{
		HashSet<Type> ignoredTypes = _ignoredTypes;
		KeyValuePair<Type, ComplexTypeConfiguration> keyValuePair3 = keyValuePair;
		return !ignoredTypes.Contains(keyValuePair3.Key);
	}).Select(delegate(KeyValuePair<Type, ComplexTypeConfiguration> keyValuePair)
	{
		KeyValuePair<Type, ComplexTypeConfiguration> keyValuePair2 = keyValuePair;
		return keyValuePair2.Value;
	}).ToList();

	internal ModelConfiguration()
	{
	}

	private ModelConfiguration(ModelConfiguration source)
	{
		source._entityConfigurations.Each(delegate(KeyValuePair<Type, EntityTypeConfiguration> c)
		{
			_entityConfigurations.Add(c.Key, c.Value.Clone());
		});
		source._complexTypeConfigurations.Each(delegate(KeyValuePair<Type, ComplexTypeConfiguration> c)
		{
			_complexTypeConfigurations.Add(c.Key, c.Value.Clone());
		});
		_ignoredTypes.AddRange(source._ignoredTypes);
		DefaultSchema = source.DefaultSchema;
		ModelNamespace = source.ModelNamespace;
	}

	internal virtual ModelConfiguration Clone()
	{
		return new ModelConfiguration(this);
	}

	internal virtual void Add(EntityTypeConfiguration entityTypeConfiguration)
	{
		if ((_entityConfigurations.TryGetValue(entityTypeConfiguration.ClrType, out var value) && !value.IsReplaceable) || _complexTypeConfigurations.ContainsKey(entityTypeConfiguration.ClrType))
		{
			throw Error.DuplicateStructuralTypeConfiguration(entityTypeConfiguration.ClrType);
		}
		if (value != null && value.IsReplaceable)
		{
			_entityConfigurations.Remove(value.ClrType);
			entityTypeConfiguration.ReplaceFrom(value);
		}
		else
		{
			entityTypeConfiguration.IsReplaceable = false;
		}
		_entityConfigurations.Add(entityTypeConfiguration.ClrType, entityTypeConfiguration);
	}

	internal virtual void Add(ComplexTypeConfiguration complexTypeConfiguration)
	{
		if (_entityConfigurations.ContainsKey(complexTypeConfiguration.ClrType) || _complexTypeConfigurations.ContainsKey(complexTypeConfiguration.ClrType))
		{
			throw Error.DuplicateStructuralTypeConfiguration(complexTypeConfiguration.ClrType);
		}
		_complexTypeConfigurations.Add(complexTypeConfiguration.ClrType, complexTypeConfiguration);
	}

	public virtual EntityTypeConfiguration Entity(Type entityType)
	{
		Check.NotNull(entityType, "entityType");
		return Entity(entityType, explicitEntity: false);
	}

	internal virtual EntityTypeConfiguration Entity(Type entityType, bool explicitEntity)
	{
		if (_complexTypeConfigurations.ContainsKey(entityType))
		{
			throw Error.EntityTypeConfigurationMismatch(entityType.Name);
		}
		if (!_entityConfigurations.TryGetValue(entityType, out var value))
		{
			Dictionary<Type, EntityTypeConfiguration> entityConfigurations = _entityConfigurations;
			EntityTypeConfiguration obj = new EntityTypeConfiguration(entityType)
			{
				IsExplicitEntity = explicitEntity
			};
			value = obj;
			entityConfigurations.Add(entityType, obj);
		}
		return value;
	}

	public virtual ComplexTypeConfiguration ComplexType(Type complexType)
	{
		Check.NotNull(complexType, "complexType");
		if (_entityConfigurations.ContainsKey(complexType))
		{
			throw Error.ComplexTypeConfigurationMismatch(complexType.Name);
		}
		if (!_complexTypeConfigurations.TryGetValue(complexType, out var value))
		{
			_complexTypeConfigurations.Add(complexType, value = new ComplexTypeConfiguration(complexType));
		}
		return value;
	}

	public virtual void Ignore(Type type)
	{
		Check.NotNull(type, "type");
		_ignoredTypes.Add(type);
	}

	internal virtual StructuralTypeConfiguration GetStructuralTypeConfiguration(Type type)
	{
		if (_entityConfigurations.TryGetValue(type, out var value))
		{
			return value;
		}
		if (_complexTypeConfigurations.TryGetValue(type, out var value2))
		{
			return value2;
		}
		return null;
	}

	public virtual bool IsComplexType(Type type)
	{
		Check.NotNull(type, "type");
		return _complexTypeConfigurations.ContainsKey(type);
	}

	public virtual bool IsIgnoredType(Type type)
	{
		Check.NotNull(type, "type");
		return _ignoredTypes.Contains(type);
	}

	public virtual IEnumerable<PropertyInfo> GetConfiguredProperties(Type type)
	{
		Check.NotNull(type, "type");
		StructuralTypeConfiguration structuralTypeConfiguration = GetStructuralTypeConfiguration(type);
		if (structuralTypeConfiguration == null)
		{
			return Enumerable.Empty<PropertyInfo>();
		}
		return structuralTypeConfiguration.ConfiguredProperties;
	}

	public virtual bool IsIgnoredProperty(Type type, PropertyInfo propertyInfo)
	{
		Check.NotNull(type, "type");
		Check.NotNull(propertyInfo, "propertyInfo");
		while (type != null)
		{
			StructuralTypeConfiguration structuralTypeConfiguration = GetStructuralTypeConfiguration(type);
			if (structuralTypeConfiguration != null && structuralTypeConfiguration.IgnoredProperties.Any((PropertyInfo p) => p.IsSameAs(propertyInfo)))
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}

	internal void Configure(EdmModel model)
	{
		ConfigureEntities(model);
		ConfigureComplexTypes(model);
	}

	private void ConfigureEntities(EdmModel model)
	{
		foreach (EntityTypeConfiguration activeEntityConfiguration in ActiveEntityConfigurations)
		{
			ConfigureFunctionMappings(model, activeEntityConfiguration, model.GetEntityType(activeEntityConfiguration.ClrType));
		}
		foreach (EntityTypeConfiguration activeEntityConfiguration2 in ActiveEntityConfigurations)
		{
			activeEntityConfiguration2.Configure(model.GetEntityType(activeEntityConfiguration2.ClrType), model);
		}
	}

	private void ConfigureFunctionMappings(EdmModel model, EntityTypeConfiguration entityTypeConfiguration, EntityType entityType)
	{
		if (entityTypeConfiguration.ModificationStoredProceduresConfiguration == null)
		{
			return;
		}
		while (entityType.BaseType != null)
		{
			Type clrType = ((EntityType)entityType.BaseType).GetClrType();
			if (!entityType.BaseType.Abstract && (!_entityConfigurations.TryGetValue(clrType, out var value) || value.ModificationStoredProceduresConfiguration == null))
			{
				throw Error.BaseTypeNotMappedToFunctions(clrType.Name, entityTypeConfiguration.ClrType.Name);
			}
			entityType = (EntityType)entityType.BaseType;
		}
		model.GetSelfAndAllDerivedTypes(entityType).Each(delegate(EntityType e)
		{
			EntityTypeConfiguration entityTypeConfiguration2 = Entity(e.GetClrType());
			if (entityTypeConfiguration2.ModificationStoredProceduresConfiguration == null)
			{
				entityTypeConfiguration2.MapToStoredProcedures();
			}
		});
	}

	private void ConfigureComplexTypes(EdmModel model)
	{
		foreach (ComplexTypeConfiguration activeComplexTypeConfiguration in ActiveComplexTypeConfigurations)
		{
			ComplexType complexType = model.GetComplexType(activeComplexTypeConfiguration.ClrType);
			activeComplexTypeConfiguration.Configure(complexType);
		}
	}

	internal void Configure(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
	{
		foreach (StructuralTypeConfiguration item in from StructuralTypeConfiguration c in databaseMapping.Model.ComplexTypes.Select((ComplexType ct) => ct.GetConfiguration())
			where c != null
			select c)
		{
			item.ConfigurePropertyMappings(databaseMapping.GetComplexPropertyMappings(item.ClrType).ToList(), providerManifest);
		}
		ConfigureEntityTypes(databaseMapping, databaseMapping.Model.Container.EntitySets, providerManifest);
		RemoveRedundantColumnConditions(databaseMapping);
		RemoveRedundantTables(databaseMapping);
		ConfigureTables(databaseMapping.Database);
		ConfigureDefaultSchema(databaseMapping);
		UniquifyFunctionNames(databaseMapping);
		ConfigureFunctionParameters(databaseMapping);
		RemoveDuplicateTphColumns(databaseMapping);
	}

	private static void ConfigureFunctionParameters(DbDatabaseMapping databaseMapping)
	{
		foreach (StructuralTypeConfiguration item in from StructuralTypeConfiguration c in databaseMapping.Model.ComplexTypes.Select((ComplexType ct) => ct.GetConfiguration())
			where c != null
			select c)
		{
			item.ConfigureFunctionParameters(databaseMapping.GetComplexParameterBindings(item.ClrType).ToList());
		}
		foreach (EntityType item2 in databaseMapping.Model.EntityTypes.Where((EntityType e) => e.GetConfiguration() != null))
		{
			((EntityTypeConfiguration)item2.GetConfiguration()).ConfigureFunctionParameters(databaseMapping, item2);
		}
	}

	private static void UniquifyFunctionNames(DbDatabaseMapping databaseMapping)
	{
		foreach (EntityTypeModificationFunctionMapping item in databaseMapping.GetEntitySetMappings().SelectMany((EntitySetMapping esm) => esm.ModificationFunctionMappings))
		{
			EntityTypeConfiguration entityTypeConfiguration = (EntityTypeConfiguration)item.EntityType.GetConfiguration();
			if (entityTypeConfiguration.ModificationStoredProceduresConfiguration != null)
			{
				ModificationStoredProceduresConfiguration modificationStoredProceduresConfiguration = entityTypeConfiguration.ModificationStoredProceduresConfiguration;
				UniquifyFunctionName(databaseMapping, modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration, item.InsertFunctionMapping);
				UniquifyFunctionName(databaseMapping, modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration, item.UpdateFunctionMapping);
				UniquifyFunctionName(databaseMapping, modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration, item.DeleteFunctionMapping);
			}
		}
		foreach (AssociationSetModificationFunctionMapping item2 in from asm in databaseMapping.GetAssociationSetMappings()
			select asm.ModificationFunctionMapping into asm
			where asm != null
			select asm)
		{
			NavigationPropertyConfiguration navigationPropertyConfiguration = (NavigationPropertyConfiguration)item2.AssociationSet.ElementType.GetConfiguration();
			if (navigationPropertyConfiguration.ModificationStoredProceduresConfiguration != null)
			{
				UniquifyFunctionName(databaseMapping, navigationPropertyConfiguration.ModificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration, item2.InsertFunctionMapping);
				UniquifyFunctionName(databaseMapping, navigationPropertyConfiguration.ModificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration, item2.DeleteFunctionMapping);
			}
		}
	}

	private static void UniquifyFunctionName(DbDatabaseMapping databaseMapping, ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration, ModificationFunctionMapping functionMapping)
	{
		if (modificationStoredProcedureConfiguration == null || string.IsNullOrWhiteSpace(modificationStoredProcedureConfiguration.Name))
		{
			functionMapping.Function.StoreFunctionNameAttribute = (from f in databaseMapping.Database.Functions.Except(new EdmFunction[1] { functionMapping.Function })
				select f.FunctionName).Uniquify(functionMapping.Function.FunctionName);
		}
	}

	private void ConfigureDefaultSchema(DbDatabaseMapping databaseMapping)
	{
		(from es in databaseMapping.Database.GetEntitySets()
			where string.IsNullOrWhiteSpace(es.Schema)
			select es).Each(delegate(EntitySet es)
		{
			string obj2 = DefaultSchema ?? "dbo";
			string result2 = obj2;
			es.Schema = obj2;
			return result2;
		});
		databaseMapping.Database.Functions.Where((EdmFunction f) => string.IsNullOrWhiteSpace(f.Schema)).Each(delegate(EdmFunction f)
		{
			string obj = DefaultSchema ?? "dbo";
			string result = obj;
			f.Schema = obj;
			return result;
		});
	}

	private void ConfigureEntityTypes(DbDatabaseMapping databaseMapping, ICollection<EntitySet> entitySets, DbProviderManifest providerManifest)
	{
		IList<EntityTypeConfiguration> list = SortEntityConfigurationsByInheritance(databaseMapping);
		foreach (EntityTypeConfiguration item in list)
		{
			EntityTypeMapping entityTypeMapping = databaseMapping.GetEntityTypeMapping(item.ClrType);
			item.ConfigureTablesAndConditions(entityTypeMapping, databaseMapping, entitySets, providerManifest);
			ConfigureUnconfiguredDerivedTypes(databaseMapping, entitySets, providerManifest, databaseMapping.Model.GetEntityType(item.ClrType), list);
		}
		new EntityMappingService(databaseMapping).Configure();
		foreach (EntityType item2 in databaseMapping.Model.EntityTypes.Where((EntityType e) => e.GetConfiguration() != null))
		{
			((EntityTypeConfiguration)item2.GetConfiguration()).Configure(item2, databaseMapping, providerManifest);
		}
	}

	private static void ConfigureUnconfiguredDerivedTypes(DbDatabaseMapping databaseMapping, ICollection<EntitySet> entitySets, DbProviderManifest providerManifest, EntityType entityType, IList<EntityTypeConfiguration> sortedEntityConfigurations)
	{
		List<EntityType> list = databaseMapping.Model.GetDerivedTypes(entityType).ToList();
		while (list.Count > 0)
		{
			EntityType currentType = list[0];
			list.RemoveAt(0);
			if (!currentType.Abstract && sortedEntityConfigurations.All((EntityTypeConfiguration etc) => etc.ClrType != currentType.GetClrType()))
			{
				EntityTypeConfiguration.ConfigureUnconfiguredType(databaseMapping, entitySets, providerManifest, currentType, new Dictionary<string, object>());
				list.AddRange(databaseMapping.Model.GetDerivedTypes(currentType));
			}
		}
	}

	private static void ConfigureTables(EdmModel database)
	{
		foreach (EntityType item in database.EntityTypes.ToList())
		{
			ConfigureTable(database, item);
		}
	}

	private static void ConfigureTable(EdmModel database, EntityType table)
	{
		DatabaseName tableName = table.GetTableName();
		if (tableName != null)
		{
			EntitySet entitySet = database.GetEntitySet(table);
			if (!string.IsNullOrWhiteSpace(tableName.Schema))
			{
				entitySet.Schema = tableName.Schema;
			}
			entitySet.Table = tableName.Name;
		}
	}

	private IList<EntityTypeConfiguration> SortEntityConfigurationsByInheritance(DbDatabaseMapping databaseMapping)
	{
		List<EntityTypeConfiguration> list = new List<EntityTypeConfiguration>();
		foreach (EntityTypeConfiguration activeEntityConfiguration in ActiveEntityConfigurations)
		{
			EntityType entityType = databaseMapping.Model.GetEntityType(activeEntityConfiguration.ClrType);
			if (entityType == null)
			{
				continue;
			}
			if (entityType.BaseType == null)
			{
				if (!list.Contains(activeEntityConfiguration))
				{
					list.Add(activeEntityConfiguration);
				}
				continue;
			}
			Stack<EntityType> stack = new Stack<EntityType>();
			while (entityType != null)
			{
				stack.Push(entityType);
				entityType = (EntityType)entityType.BaseType;
			}
			while (stack.Count > 0)
			{
				entityType = stack.Pop();
				EntityTypeConfiguration entityTypeConfiguration = ActiveEntityConfigurations.SingleOrDefault((EntityTypeConfiguration ec) => ec.ClrType == entityType.GetClrType());
				if (entityTypeConfiguration != null && !list.Contains(entityTypeConfiguration))
				{
					list.Add(entityTypeConfiguration);
				}
			}
		}
		return list;
	}

	internal void NormalizeConfigurations()
	{
		DiscoverIndirectlyConfiguredComplexTypes();
		ReassignSubtypeMappings();
	}

	private void DiscoverIndirectlyConfiguredComplexTypes()
	{
		ActiveEntityConfigurations.SelectMany((EntityTypeConfiguration ec) => ec.ConfiguredComplexTypes).Each((Type t) => ComplexType(t));
	}

	private void ReassignSubtypeMappings()
	{
		foreach (EntityTypeConfiguration activeEntityConfiguration in ActiveEntityConfigurations)
		{
			foreach (KeyValuePair<Type, EntityMappingConfiguration> subTypeMappingConfiguration in activeEntityConfiguration.SubTypeMappingConfigurations)
			{
				Type subTypeClrType = subTypeMappingConfiguration.Key;
				EntityTypeConfiguration entityTypeConfiguration = ActiveEntityConfigurations.SingleOrDefault((EntityTypeConfiguration ec) => ec.ClrType == subTypeClrType);
				if (entityTypeConfiguration == null)
				{
					entityTypeConfiguration = new EntityTypeConfiguration(subTypeClrType);
					_entityConfigurations.Add(subTypeClrType, entityTypeConfiguration);
				}
				entityTypeConfiguration.AddMappingConfiguration(subTypeMappingConfiguration.Value, cloneable: false);
			}
		}
	}

	private static void RemoveDuplicateTphColumns(DbDatabaseMapping databaseMapping)
	{
		foreach (EntityType entityType in databaseMapping.Database.EntityTypes)
		{
			EntityType currentTable = entityType;
			new TphColumnFixer((from f in databaseMapping.GetEntitySetMappings().SelectMany((EntitySetMapping e) => e.EntityTypeMappings).SelectMany((EntityTypeMapping e) => e.MappingFragments)
				where f.Table == currentTable
				select f).SelectMany((MappingFragment f) => f.ColumnMappings), currentTable, databaseMapping.Database).RemoveDuplicateTphColumns();
		}
	}

	private static void RemoveRedundantColumnConditions(DbDatabaseMapping databaseMapping)
	{
		(from esm in databaseMapping.GetEntitySetMappings()
			select new
			{
				Set = esm,
				Fragments = from etm in esm.EntityTypeMappings
					from etmf in etm.MappingFragments
					group etmf by etmf.Table into g
					where g.Count((MappingFragment x) => x.GetDefaultDiscriminator() != null) == 1
					select g.Single((MappingFragment x) => x.GetDefaultDiscriminator() != null)
			}).Each(x =>
		{
			x.Fragments.Each(delegate(MappingFragment f)
			{
				f.RemoveDefaultDiscriminator(x.Set);
			});
		});
	}

	private static void RemoveRedundantTables(DbDatabaseMapping databaseMapping)
	{
		databaseMapping.Database.EntityTypes.Where((EntityType t) => databaseMapping.GetEntitySetMappings().SelectMany((EntitySetMapping esm) => esm.EntityTypeMappings).SelectMany((EntityTypeMapping etm) => etm.MappingFragments)
			.All((MappingFragment etmf) => etmf.Table != t) && databaseMapping.GetAssociationSetMappings().All((AssociationSetMapping asm) => asm.Table != t)).ToList().Each(delegate(EntityType t)
		{
			DatabaseName tableName = t.GetTableName();
			if (tableName != null)
			{
				throw Error.OrphanedConfiguredTableDetected(tableName);
			}
			databaseMapping.Database.RemoveEntityType(t);
			databaseMapping.Database.AssociationTypes.Where((AssociationType at) => at.SourceEnd.GetEntityType() == t || at.TargetEnd.GetEntityType() == t).ToList().Each(delegate(AssociationType at)
			{
				databaseMapping.Database.RemoveAssociationType(at);
			});
		});
	}
}
