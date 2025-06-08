using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Mapping.Update.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal class ModificationFunctionMappingGenerator : StructuralTypeMappingGenerator
{
	public ModificationFunctionMappingGenerator(DbProviderManifest providerManifest)
		: base(providerManifest)
	{
	}

	public void Generate(EntityType entityType, DbDatabaseMapping databaseMapping)
	{
		if (!entityType.Abstract)
		{
			EntitySet entitySet = databaseMapping.Model.GetEntitySet(entityType);
			EntitySetMapping entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet);
			List<ColumnMappingBuilder> columnMappings = GetColumnMappings(entityType, entitySetMapping).ToList();
			List<Tuple<ModificationFunctionMemberPath, EdmProperty>> iaFkProperties = GetIndependentFkColumns(entityType, databaseMapping).ToList();
			ModificationFunctionMapping insertFunctionMapping = GenerateFunctionMapping(ModificationOperator.Insert, entitySetMapping.EntitySet, entityType, databaseMapping, entityType.Properties, iaFkProperties, columnMappings, entityType.Properties.Where((EdmProperty p) => p.HasStoreGeneratedPattern()));
			ModificationFunctionMapping updateFunctionMapping = GenerateFunctionMapping(ModificationOperator.Update, entitySetMapping.EntitySet, entityType, databaseMapping, entityType.Properties, iaFkProperties, columnMappings, entityType.Properties.Where((EdmProperty p) => p.GetStoreGeneratedPattern() == StoreGeneratedPattern.Computed));
			ModificationFunctionMapping deleteFunctionMapping = GenerateFunctionMapping(ModificationOperator.Delete, entitySetMapping.EntitySet, entityType, databaseMapping, entityType.Properties, iaFkProperties, columnMappings);
			EntityTypeModificationFunctionMapping modificationFunctionMapping = new EntityTypeModificationFunctionMapping(entityType, deleteFunctionMapping, insertFunctionMapping, updateFunctionMapping);
			entitySetMapping.AddModificationFunctionMapping(modificationFunctionMapping);
		}
	}

	private static IEnumerable<ColumnMappingBuilder> GetColumnMappings(EntityType entityType, EntitySetMapping entitySetMapping)
	{
		return new EntityType[1] { entityType }.Concat(GetParents(entityType)).SelectMany((EntityType et) => entitySetMapping.TypeMappings.Where((TypeMapping stm) => stm.Types.Contains(et)).SelectMany((TypeMapping stm) => stm.MappingFragments).SelectMany((MappingFragment mf) => mf.ColumnMappings));
	}

	public void Generate(AssociationSetMapping associationSetMapping, DbDatabaseMapping databaseMapping)
	{
		List<Tuple<ModificationFunctionMemberPath, EdmProperty>> iaFkProperties = GetIndependentFkColumns(associationSetMapping).ToList();
		EntityType entityType = associationSetMapping.AssociationSet.ElementType.SourceEnd.GetEntityType();
		string functionNamePrefix = string.Concat(str1: associationSetMapping.AssociationSet.ElementType.TargetEnd.GetEntityType().Name, str0: entityType.Name);
		ModificationFunctionMapping insertFunctionMapping = GenerateFunctionMapping(ModificationOperator.Insert, associationSetMapping.AssociationSet, associationSetMapping.AssociationSet.ElementType, databaseMapping, Enumerable.Empty<EdmProperty>(), iaFkProperties, new ColumnMappingBuilder[0], null, functionNamePrefix);
		ModificationFunctionMapping deleteFunctionMapping = GenerateFunctionMapping(ModificationOperator.Delete, associationSetMapping.AssociationSet, associationSetMapping.AssociationSet.ElementType, databaseMapping, Enumerable.Empty<EdmProperty>(), iaFkProperties, new ColumnMappingBuilder[0], null, functionNamePrefix);
		associationSetMapping.ModificationFunctionMapping = new AssociationSetModificationFunctionMapping(associationSetMapping.AssociationSet, deleteFunctionMapping, insertFunctionMapping);
	}

	private static IEnumerable<Tuple<ModificationFunctionMemberPath, EdmProperty>> GetIndependentFkColumns(AssociationSetMapping associationSetMapping)
	{
		foreach (ScalarPropertyMapping propertyMapping in associationSetMapping.SourceEndMapping.PropertyMappings)
		{
			yield return Tuple.Create(new ModificationFunctionMemberPath(new EdmMember[2]
			{
				propertyMapping.Property,
				associationSetMapping.SourceEndMapping.AssociationEnd
			}, associationSetMapping.AssociationSet), propertyMapping.Column);
		}
		foreach (ScalarPropertyMapping propertyMapping2 in associationSetMapping.TargetEndMapping.PropertyMappings)
		{
			yield return Tuple.Create(new ModificationFunctionMemberPath(new EdmMember[2]
			{
				propertyMapping2.Property,
				associationSetMapping.TargetEndMapping.AssociationEnd
			}, associationSetMapping.AssociationSet), propertyMapping2.Column);
		}
	}

	private static IEnumerable<Tuple<ModificationFunctionMemberPath, EdmProperty>> GetIndependentFkColumns(EntityType entityType, DbDatabaseMapping databaseMapping)
	{
		foreach (AssociationSetMapping associationSetMapping in databaseMapping.GetAssociationSetMappings())
		{
			AssociationType elementType = associationSetMapping.AssociationSet.ElementType;
			if (elementType.IsManyToMany())
			{
				continue;
			}
			if (!elementType.TryGuessPrincipalAndDependentEnds(out var _, out var dependentEnd))
			{
				dependentEnd = elementType.TargetEnd;
			}
			EntityType entityType2 = dependentEnd.GetEntityType();
			if (entityType2 == entityType || GetParents(entityType).Contains(entityType2))
			{
				EndPropertyMapping endPropertyMapping = ((associationSetMapping.TargetEndMapping.AssociationEnd != dependentEnd) ? associationSetMapping.TargetEndMapping : associationSetMapping.SourceEndMapping);
				foreach (ScalarPropertyMapping propertyMapping in endPropertyMapping.PropertyMappings)
				{
					yield return Tuple.Create(new ModificationFunctionMemberPath(new EdmMember[2] { propertyMapping.Property, dependentEnd }, associationSetMapping.AssociationSet), propertyMapping.Column);
				}
			}
			dependentEnd = null;
		}
	}

	private static IEnumerable<EntityType> GetParents(EntityType entityType)
	{
		while (entityType.BaseType != null)
		{
			yield return (EntityType)entityType.BaseType;
			entityType = (EntityType)entityType.BaseType;
		}
	}

	private ModificationFunctionMapping GenerateFunctionMapping(ModificationOperator modificationOperator, EntitySetBase entitySetBase, EntityTypeBase entityTypeBase, DbDatabaseMapping databaseMapping, IEnumerable<EdmProperty> parameterProperties, IEnumerable<Tuple<ModificationFunctionMemberPath, EdmProperty>> iaFkProperties, IList<ColumnMappingBuilder> columnMappings, IEnumerable<EdmProperty> resultProperties = null, string functionNamePrefix = null)
	{
		bool useOriginalValues = modificationOperator == ModificationOperator.Delete;
		FunctionParameterMappingGenerator functionParameterMappingGenerator = new FunctionParameterMappingGenerator(_providerManifest);
		List<ModificationFunctionParameterBinding> list = functionParameterMappingGenerator.Generate((modificationOperator != ModificationOperator.Insert || !IsTableSplitDependent(entityTypeBase, databaseMapping)) ? modificationOperator : ModificationOperator.Update, parameterProperties, columnMappings, new List<EdmProperty>(), useOriginalValues).Concat(functionParameterMappingGenerator.Generate(iaFkProperties, useOriginalValues)).ToList();
		List<FunctionParameter> list2 = list.Select((ModificationFunctionParameterBinding b) => b.Parameter).ToList();
		UniquifyParameterNames(list2);
		EdmFunctionPayload functionPayload = new EdmFunctionPayload
		{
			ReturnParameters = new FunctionParameter[0],
			Parameters = list2.ToArray(),
			IsComposable = false
		};
		EdmFunction function = databaseMapping.Database.AddFunction((functionNamePrefix ?? entityTypeBase.Name) + "_" + modificationOperator, functionPayload);
		return new ModificationFunctionMapping(entitySetBase, entityTypeBase, function, list, null, resultProperties?.Select((EdmProperty p) => new ModificationFunctionResultBinding(columnMappings.First((ColumnMappingBuilder cm) => cm.PropertyPath.SequenceEqual(new EdmProperty[1] { p })).ColumnProperty.Name, p)));
	}

	private static bool IsTableSplitDependent(EntityTypeBase entityTypeBase, DbDatabaseMapping databaseMapping)
	{
		AssociationType associationType = databaseMapping.Model.AssociationTypes.SingleOrDefault((AssociationType at) => at.IsForeignKey && at.IsRequiredToRequired() && !at.IsSelfReferencing() && (at.SourceEnd.GetEntityType().IsAssignableFrom(entityTypeBase) || at.TargetEnd.GetEntityType().IsAssignableFrom(entityTypeBase)) && databaseMapping.Database.AssociationTypes.All((AssociationType fk) => fk.Name != at.Name));
		if (associationType != null)
		{
			return associationType.TargetEnd.GetEntityType() == entityTypeBase;
		}
		return false;
	}

	private static void UniquifyParameterNames(IList<FunctionParameter> parameters)
	{
		foreach (FunctionParameter parameter in parameters)
		{
			parameter.Name = parameters.Except(new FunctionParameter[1] { parameter }).UniquifyName(parameter.Name);
		}
	}
}
