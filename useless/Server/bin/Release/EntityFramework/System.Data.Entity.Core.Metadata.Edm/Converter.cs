using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class Converter
{
	internal class ConversionCache
	{
		internal readonly ItemCollection ItemCollection;

		private readonly Dictionary<EdmType, TypeUsage> _nullFacetsTypeUsage;

		private readonly Dictionary<EdmType, TypeUsage> _nullFacetsCollectionTypeUsage;

		internal ConversionCache(ItemCollection itemCollection)
		{
			ItemCollection = itemCollection;
			_nullFacetsTypeUsage = new Dictionary<EdmType, TypeUsage>();
			_nullFacetsCollectionTypeUsage = new Dictionary<EdmType, TypeUsage>();
		}

		internal TypeUsage GetTypeUsageWithNullFacets(EdmType edmType)
		{
			if (_nullFacetsTypeUsage.TryGetValue(edmType, out var value))
			{
				return value;
			}
			value = TypeUsage.Create(edmType, FacetValues.NullFacetValues);
			_nullFacetsTypeUsage.Add(edmType, value);
			return value;
		}

		internal TypeUsage GetCollectionTypeUsageWithNullFacets(EdmType edmType)
		{
			if (_nullFacetsCollectionTypeUsage.TryGetValue(edmType, out var value))
			{
				return value;
			}
			value = TypeUsage.Create(new CollectionType(GetTypeUsageWithNullFacets(edmType)), FacetValues.NullFacetValues);
			_nullFacetsCollectionTypeUsage.Add(edmType, value);
			return value;
		}
	}

	internal static readonly FacetDescription ConcurrencyModeFacet;

	internal static readonly FacetDescription StoreGeneratedPatternFacet;

	internal static readonly FacetDescription CollationFacet;

	static Converter()
	{
		EnumType enumType = new EnumType("ConcurrencyMode", "Edm", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), isFlags: false, DataSpace.CSpace);
		string[] names = Enum.GetNames(typeof(ConcurrencyMode));
		foreach (string text in names)
		{
			enumType.AddMember(new EnumMember(text, (int)Enum.Parse(typeof(ConcurrencyMode), text, ignoreCase: false)));
		}
		EnumType enumType2 = new EnumType("StoreGeneratedPattern", "Edm", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), isFlags: false, DataSpace.CSpace);
		names = Enum.GetNames(typeof(StoreGeneratedPattern));
		foreach (string text2 in names)
		{
			enumType2.AddMember(new EnumMember(text2, (int)Enum.Parse(typeof(StoreGeneratedPattern), text2, ignoreCase: false)));
		}
		ConcurrencyModeFacet = new FacetDescription("ConcurrencyMode", enumType, null, null, ConcurrencyMode.None);
		StoreGeneratedPatternFacet = new FacetDescription("StoreGeneratedPattern", enumType2, null, null, StoreGeneratedPattern.None);
		CollationFacet = new FacetDescription("Collation", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.String), null, null, string.Empty);
	}

	internal static IEnumerable<GlobalItem> ConvertSchema(Schema somSchema, DbProviderManifest providerManifest, ItemCollection itemCollection)
	{
		Dictionary<SchemaElement, GlobalItem> dictionary = new Dictionary<SchemaElement, GlobalItem>();
		ConvertSchema(somSchema, providerManifest, new ConversionCache(itemCollection), dictionary);
		return dictionary.Values;
	}

	internal static IEnumerable<GlobalItem> ConvertSchema(IList<Schema> somSchemas, DbProviderManifest providerManifest, ItemCollection itemCollection)
	{
		Dictionary<SchemaElement, GlobalItem> dictionary = new Dictionary<SchemaElement, GlobalItem>();
		ConversionCache convertedItemCache = new ConversionCache(itemCollection);
		foreach (Schema somSchema in somSchemas)
		{
			ConvertSchema(somSchema, providerManifest, convertedItemCache, dictionary);
		}
		return dictionary.Values;
	}

	private static void ConvertSchema(Schema somSchema, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		List<Function> list = new List<Function>();
		foreach (System.Data.Entity.Core.SchemaObjectModel.SchemaType schemaType in somSchema.SchemaTypes)
		{
			if (LoadSchemaElement(schemaType, providerManifest, convertedItemCache, newGlobalItems) == null && schemaType is Function item)
			{
				list.Add(item);
			}
		}
		foreach (SchemaEntityType item2 in somSchema.SchemaTypes.OfType<SchemaEntityType>())
		{
			LoadEntityTypePhase2(item2, providerManifest, convertedItemCache, newGlobalItems);
		}
		foreach (Function item3 in list)
		{
			LoadSchemaElement(item3, providerManifest, convertedItemCache, newGlobalItems);
		}
		if (convertedItemCache.ItemCollection.DataSpace == DataSpace.CSpace)
		{
			((EdmItemCollection)convertedItemCache.ItemCollection).EdmVersion = somSchema.SchemaVersion;
		}
		else if (convertedItemCache.ItemCollection is StoreItemCollection storeItemCollection)
		{
			storeItemCollection.StoreSchemaVersion = somSchema.SchemaVersion;
		}
	}

	internal static MetadataItem LoadSchemaElement(System.Data.Entity.Core.SchemaObjectModel.SchemaType element, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		if (newGlobalItems.TryGetValue(element, out var value))
		{
			return value;
		}
		if (element is System.Data.Entity.Core.SchemaObjectModel.EntityContainer element2)
		{
			return ConvertToEntityContainer(element2, providerManifest, convertedItemCache, newGlobalItems);
		}
		if (element is SchemaEntityType)
		{
			return ConvertToEntityType((SchemaEntityType)element, providerManifest, convertedItemCache, newGlobalItems);
		}
		if (element is Relationship)
		{
			return ConvertToAssociationType((Relationship)element, providerManifest, convertedItemCache, newGlobalItems);
		}
		if (element is SchemaComplexType)
		{
			return ConvertToComplexType((SchemaComplexType)element, providerManifest, convertedItemCache, newGlobalItems);
		}
		if (element is Function)
		{
			return ConvertToFunction((Function)element, providerManifest, convertedItemCache, null, newGlobalItems);
		}
		if (element is SchemaEnumType)
		{
			return ConvertToEnumType((SchemaEnumType)element, newGlobalItems);
		}
		return null;
	}

	private static EntityContainer ConvertToEntityContainer(System.Data.Entity.Core.SchemaObjectModel.EntityContainer element, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		EntityContainer entityContainer = new EntityContainer(element.Name, GetDataSpace(providerManifest));
		newGlobalItems.Add(element, entityContainer);
		foreach (EntityContainerEntitySet entitySet in element.EntitySets)
		{
			entityContainer.AddEntitySetBase(ConvertToEntitySet(entitySet, providerManifest, convertedItemCache, newGlobalItems));
		}
		foreach (EntityContainerRelationshipSet relationshipSet in element.RelationshipSets)
		{
			entityContainer.AddEntitySetBase(ConvertToAssociationSet(relationshipSet, providerManifest, convertedItemCache, entityContainer, newGlobalItems));
		}
		foreach (Function functionImport in element.FunctionImports)
		{
			entityContainer.AddFunctionImport(ConvertToFunction(functionImport, providerManifest, convertedItemCache, entityContainer, newGlobalItems));
		}
		if (element.Documentation != null)
		{
			entityContainer.Documentation = ConvertToDocumentation(element.Documentation);
		}
		AddOtherContent(element, entityContainer);
		return entityContainer;
	}

	private static EntityType ConvertToEntityType(SchemaEntityType element, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		string[] array = null;
		if (element.DeclaredKeyProperties.Count != 0)
		{
			array = new string[element.DeclaredKeyProperties.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = element.DeclaredKeyProperties[i].Property.Name;
			}
		}
		EdmProperty[] array2 = new EdmProperty[element.Properties.Count];
		int num = 0;
		foreach (StructuredProperty property in element.Properties)
		{
			array2[num++] = ConvertToProperty(property, providerManifest, convertedItemCache, newGlobalItems);
		}
		EntityType entityType = new EntityType(element.Name, element.Namespace, GetDataSpace(providerManifest), array, array2);
		if (element.BaseType != null)
		{
			entityType.BaseType = (EdmType)LoadSchemaElement(element.BaseType, providerManifest, convertedItemCache, newGlobalItems);
		}
		entityType.Abstract = element.IsAbstract;
		if (element.Documentation != null)
		{
			entityType.Documentation = ConvertToDocumentation(element.Documentation);
		}
		AddOtherContent(element, entityType);
		newGlobalItems.Add(element, entityType);
		return entityType;
	}

	private static void LoadEntityTypePhase2(SchemaEntityType element, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		EntityType entityType = (EntityType)newGlobalItems[element];
		foreach (System.Data.Entity.Core.SchemaObjectModel.NavigationProperty navigationProperty in element.NavigationProperties)
		{
			entityType.AddMember(ConvertToNavigationProperty(entityType, navigationProperty, providerManifest, convertedItemCache, newGlobalItems));
		}
	}

	private static ComplexType ConvertToComplexType(SchemaComplexType element, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		ComplexType complexType = new ComplexType(element.Name, element.Namespace, GetDataSpace(providerManifest));
		newGlobalItems.Add(element, complexType);
		foreach (StructuredProperty property in element.Properties)
		{
			complexType.AddMember(ConvertToProperty(property, providerManifest, convertedItemCache, newGlobalItems));
		}
		complexType.Abstract = element.IsAbstract;
		if (element.BaseType != null)
		{
			complexType.BaseType = (EdmType)LoadSchemaElement(element.BaseType, providerManifest, convertedItemCache, newGlobalItems);
		}
		if (element.Documentation != null)
		{
			complexType.Documentation = ConvertToDocumentation(element.Documentation);
		}
		AddOtherContent(element, complexType);
		return complexType;
	}

	private static AssociationType ConvertToAssociationType(Relationship element, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		AssociationType associationType = new AssociationType(element.Name, element.Namespace, element.IsForeignKey, GetDataSpace(providerManifest));
		newGlobalItems.Add(element, associationType);
		foreach (RelationshipEnd end in element.Ends)
		{
			EntityType endMemberType = (EntityType)LoadSchemaElement(end.Type, providerManifest, convertedItemCache, newGlobalItems);
			AssociationEndMember associationEndMember = InitializeAssociationEndMember(associationType, end, endMemberType);
			AddOtherContent(end, associationEndMember);
			foreach (OnOperation operation in end.Operations)
			{
				if (operation.Operation == Operation.Delete)
				{
					OperationAction deleteBehavior = OperationAction.None;
					switch (operation.Action)
					{
					case System.Data.Entity.Core.SchemaObjectModel.Action.Cascade:
						deleteBehavior = OperationAction.Cascade;
						break;
					case System.Data.Entity.Core.SchemaObjectModel.Action.None:
						deleteBehavior = OperationAction.None;
						break;
					}
					associationEndMember.DeleteBehavior = deleteBehavior;
				}
			}
			if (end.Documentation != null)
			{
				associationEndMember.Documentation = ConvertToDocumentation(end.Documentation);
			}
		}
		for (int i = 0; i < element.Constraints.Count; i++)
		{
			System.Data.Entity.Core.SchemaObjectModel.ReferentialConstraint referentialConstraint = element.Constraints[i];
			AssociationEndMember obj = (AssociationEndMember)associationType.Members[referentialConstraint.PrincipalRole.Name];
			AssociationEndMember associationEndMember2 = (AssociationEndMember)associationType.Members[referentialConstraint.DependentRole.Name];
			EntityTypeBase elementType = ((RefType)obj.TypeUsage.EdmType).ElementType;
			ReferentialConstraint referentialConstraint2 = new ReferentialConstraint(toProperties: GetProperties(((RefType)associationEndMember2.TypeUsage.EdmType).ElementType, referentialConstraint.DependentRole.RoleProperties), fromRole: obj, toRole: associationEndMember2, fromProperties: GetProperties(elementType, referentialConstraint.PrincipalRole.RoleProperties));
			if (referentialConstraint.Documentation != null)
			{
				referentialConstraint2.Documentation = ConvertToDocumentation(referentialConstraint.Documentation);
			}
			if (referentialConstraint.PrincipalRole.Documentation != null)
			{
				referentialConstraint2.FromRole.Documentation = ConvertToDocumentation(referentialConstraint.PrincipalRole.Documentation);
			}
			if (referentialConstraint.DependentRole.Documentation != null)
			{
				referentialConstraint2.ToRole.Documentation = ConvertToDocumentation(referentialConstraint.DependentRole.Documentation);
			}
			associationType.AddReferentialConstraint(referentialConstraint2);
			AddOtherContent(element.Constraints[i], referentialConstraint2);
		}
		if (element.Documentation != null)
		{
			associationType.Documentation = ConvertToDocumentation(element.Documentation);
		}
		AddOtherContent(element, associationType);
		return associationType;
	}

	private static AssociationEndMember InitializeAssociationEndMember(AssociationType associationType, IRelationshipEnd end, EntityType endMemberType)
	{
		AssociationEndMember associationEndMember;
		if (!associationType.Members.TryGetValue(end.Name, ignoreCase: false, out var item))
		{
			associationEndMember = new AssociationEndMember(end.Name, endMemberType.GetReferenceType(), end.Multiplicity.Value);
			associationType.AddKeyMember(associationEndMember);
		}
		else
		{
			associationEndMember = (AssociationEndMember)item;
		}
		if (end is RelationshipEnd { Documentation: not null } relationshipEnd)
		{
			associationEndMember.Documentation = ConvertToDocumentation(relationshipEnd.Documentation);
		}
		return associationEndMember;
	}

	private static EdmProperty[] GetProperties(EntityTypeBase entityType, IList<PropertyRefElement> properties)
	{
		EdmProperty[] array = new EdmProperty[properties.Count];
		for (int i = 0; i < properties.Count; i++)
		{
			array[i] = (EdmProperty)entityType.Members[properties[i].Name];
		}
		return array;
	}

	private static void AddOtherContent(SchemaElement element, MetadataItem item)
	{
		if (element.OtherContent.Count > 0)
		{
			item.AddMetadataProperties(element.OtherContent);
		}
	}

	private static EntitySet ConvertToEntitySet(EntityContainerEntitySet set, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		EntitySet entitySet = new EntitySet(set.Name, set.DbSchema, set.Table, set.DefiningQuery, (EntityType)LoadSchemaElement(set.EntityType, providerManifest, convertedItemCache, newGlobalItems));
		if (set.Documentation != null)
		{
			entitySet.Documentation = ConvertToDocumentation(set.Documentation);
		}
		AddOtherContent(set, entitySet);
		return entitySet;
	}

	private static EntitySet GetEntitySet(EntityContainerEntitySet set, EntityContainer container)
	{
		return container.GetEntitySetByName(set.Name, ignoreCase: false);
	}

	private static AssociationSet ConvertToAssociationSet(EntityContainerRelationshipSet relationshipSet, DbProviderManifest providerManifest, ConversionCache convertedItemCache, EntityContainer container, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		AssociationType associationType = (AssociationType)LoadSchemaElement((System.Data.Entity.Core.SchemaObjectModel.SchemaType)relationshipSet.Relationship, providerManifest, convertedItemCache, newGlobalItems);
		AssociationSet associationSet = new AssociationSet(relationshipSet.Name, associationType);
		foreach (EntityContainerRelationshipSetEnd end in relationshipSet.Ends)
		{
			AssociationEndMember endMember = (AssociationEndMember)associationType.Members[end.Name];
			AssociationSetEnd associationSetEnd = new AssociationSetEnd(GetEntitySet(end.EntitySet, container), associationSet, endMember);
			AddOtherContent(end, associationSetEnd);
			associationSet.AddAssociationSetEnd(associationSetEnd);
			if (end.Documentation != null)
			{
				associationSetEnd.Documentation = ConvertToDocumentation(end.Documentation);
			}
		}
		if (relationshipSet.Documentation != null)
		{
			associationSet.Documentation = ConvertToDocumentation(relationshipSet.Documentation);
		}
		AddOtherContent(relationshipSet, associationSet);
		return associationSet;
	}

	private static EdmProperty ConvertToProperty(StructuredProperty somProperty, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		TypeUsage typeUsage = null;
		ScalarType scalarType = somProperty.Type as ScalarType;
		if (scalarType != null && somProperty.Schema.DataModel != 0)
		{
			typeUsage = somProperty.TypeUsage;
			UpdateSentinelValuesInFacets(ref typeUsage);
		}
		else
		{
			EdmType edmType = ((scalarType == null) ? ((EdmType)LoadSchemaElement(somProperty.Type, providerManifest, convertedItemCache, newGlobalItems)) : convertedItemCache.ItemCollection.GetItem<PrimitiveType>(somProperty.TypeUsage.EdmType.FullName));
			if (somProperty.CollectionKind != 0)
			{
				typeUsage = TypeUsage.Create(new CollectionType(edmType));
			}
			else
			{
				SchemaEnumType obj = ((scalarType == null) ? (somProperty.Type as SchemaEnumType) : null);
				typeUsage = TypeUsage.Create(edmType);
				if (obj != null)
				{
					somProperty.EnsureEnumTypeFacets(convertedItemCache, newGlobalItems);
				}
				if (somProperty.TypeUsage != null)
				{
					ApplyTypePropertyFacets(somProperty.TypeUsage, ref typeUsage);
				}
			}
		}
		PopulateGeneralFacets(somProperty, ref typeUsage);
		EdmProperty edmProperty = new EdmProperty(somProperty.Name, typeUsage);
		if (somProperty.Documentation != null)
		{
			edmProperty.Documentation = ConvertToDocumentation(somProperty.Documentation);
		}
		AddOtherContent(somProperty, edmProperty);
		return edmProperty;
	}

	private static NavigationProperty ConvertToNavigationProperty(EntityType declaringEntityType, System.Data.Entity.Core.SchemaObjectModel.NavigationProperty somNavigationProperty, DbProviderManifest providerManifest, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		EntityType entityType = (EntityType)LoadSchemaElement(somNavigationProperty.Type, providerManifest, convertedItemCache, newGlobalItems);
		EdmType edmType = entityType;
		AssociationType associationType = (AssociationType)LoadSchemaElement((Relationship)somNavigationProperty.Relationship, providerManifest, convertedItemCache, newGlobalItems);
		IRelationshipEnd end = null;
		somNavigationProperty.Relationship.TryGetEnd(somNavigationProperty.ToEnd.Name, out end);
		edmType = ((end.Multiplicity != RelationshipMultiplicity.Many) ? ((EdmType)entityType) : ((EdmType)entityType.GetCollectionType()));
		TypeUsage typeUsage = ((end.Multiplicity != RelationshipMultiplicity.One) ? TypeUsage.Create(edmType) : TypeUsage.Create(edmType, new FacetValues
		{
			Nullable = false
		}));
		InitializeAssociationEndMember(associationType, somNavigationProperty.ToEnd, entityType);
		InitializeAssociationEndMember(associationType, somNavigationProperty.FromEnd, declaringEntityType);
		NavigationProperty navigationProperty = new NavigationProperty(somNavigationProperty.Name, typeUsage);
		navigationProperty.RelationshipType = associationType;
		navigationProperty.ToEndMember = (RelationshipEndMember)associationType.Members[somNavigationProperty.ToEnd.Name];
		navigationProperty.FromEndMember = (RelationshipEndMember)associationType.Members[somNavigationProperty.FromEnd.Name];
		if (somNavigationProperty.Documentation != null)
		{
			navigationProperty.Documentation = ConvertToDocumentation(somNavigationProperty.Documentation);
		}
		AddOtherContent(somNavigationProperty, navigationProperty);
		return navigationProperty;
	}

	private static EdmFunction ConvertToFunction(Function somFunction, DbProviderManifest providerManifest, ConversionCache convertedItemCache, EntityContainer functionImportEntityContainer, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		GlobalItem value = null;
		if (!somFunction.IsFunctionImport && newGlobalItems.TryGetValue(somFunction, out value))
		{
			return (EdmFunction)value;
		}
		bool flag = somFunction.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel;
		List<FunctionParameter> list = new List<FunctionParameter>();
		if (somFunction.ReturnTypeList != null)
		{
			int num = 0;
			foreach (ReturnType returnType in somFunction.ReturnTypeList)
			{
				TypeUsage functionTypeUsage = GetFunctionTypeUsage(somFunction is ModelFunction, somFunction, returnType, providerManifest, flag, returnType.Type, returnType.CollectionKind, returnType.IsRefType, convertedItemCache, newGlobalItems);
				if (functionTypeUsage != null)
				{
					string text = ((num == 0) ? string.Empty : num.ToString(CultureInfo.InvariantCulture));
					num++;
					FunctionParameter item = new FunctionParameter("ReturnType" + text, functionTypeUsage, ParameterMode.ReturnValue);
					AddOtherContent(returnType, item);
					list.Add(item);
					continue;
				}
				return null;
			}
		}
		else if (somFunction.Type != null)
		{
			TypeUsage functionTypeUsage2 = GetFunctionTypeUsage(somFunction is ModelFunction, somFunction, null, providerManifest, flag, somFunction.Type, somFunction.CollectionKind, somFunction.IsReturnAttributeReftype, convertedItemCache, newGlobalItems);
			if (functionTypeUsage2 == null)
			{
				return null;
			}
			list.Add(new FunctionParameter("ReturnType", functionTypeUsage2, ParameterMode.ReturnValue));
		}
		EntitySet[] entitySets = null;
		string namespaceName;
		if (somFunction.IsFunctionImport)
		{
			FunctionImportElement functionImportElement = (FunctionImportElement)somFunction;
			namespaceName = functionImportElement.Container.Name;
			if (functionImportElement.EntitySet != null)
			{
				EntityContainer container = functionImportEntityContainer;
				entitySets = new EntitySet[1] { GetEntitySet(functionImportElement.EntitySet, container) };
			}
			else if (functionImportElement.ReturnTypeList != null)
			{
				entitySets = functionImportElement.ReturnTypeList.Select((ReturnType returnType) => (returnType.EntitySet == null) ? null : GetEntitySet(returnType.EntitySet, functionImportEntityContainer)).ToArray();
			}
		}
		else
		{
			namespaceName = somFunction.Namespace;
		}
		List<FunctionParameter> list2 = new List<FunctionParameter>();
		foreach (Parameter parameter in somFunction.Parameters)
		{
			TypeUsage functionTypeUsage3 = GetFunctionTypeUsage(somFunction is ModelFunction, somFunction, parameter, providerManifest, flag, parameter.Type, parameter.CollectionKind, parameter.IsRefType, convertedItemCache, newGlobalItems);
			if (functionTypeUsage3 == null)
			{
				return null;
			}
			FunctionParameter functionParameter = new FunctionParameter(parameter.Name, functionTypeUsage3, GetParameterMode(parameter.ParameterDirection));
			AddOtherContent(parameter, functionParameter);
			if (parameter.Documentation != null)
			{
				functionParameter.Documentation = ConvertToDocumentation(parameter.Documentation);
			}
			list2.Add(functionParameter);
		}
		EdmFunction edmFunction = new EdmFunction(somFunction.Name, namespaceName, GetDataSpace(providerManifest), new EdmFunctionPayload
		{
			Schema = somFunction.DbSchema,
			StoreFunctionName = somFunction.StoreFunctionName,
			CommandText = somFunction.CommandText,
			EntitySets = entitySets,
			IsAggregate = somFunction.IsAggregate,
			IsBuiltIn = somFunction.IsBuiltIn,
			IsNiladic = somFunction.IsNiladicFunction,
			IsComposable = somFunction.IsComposable,
			IsFromProviderManifest = flag,
			IsFunctionImport = somFunction.IsFunctionImport,
			ReturnParameters = list.ToArray(),
			Parameters = list2.ToArray(),
			ParameterTypeSemantics = somFunction.ParameterTypeSemantics
		});
		if (!somFunction.IsFunctionImport)
		{
			newGlobalItems.Add(somFunction, edmFunction);
		}
		if (somFunction.Documentation != null)
		{
			edmFunction.Documentation = ConvertToDocumentation(somFunction.Documentation);
		}
		AddOtherContent(somFunction, edmFunction);
		return edmFunction;
	}

	private static EnumType ConvertToEnumType(SchemaEnumType somEnumType, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		ScalarType scalarType = (ScalarType)somEnumType.UnderlyingType;
		EnumType enumType = new EnumType(somEnumType.Name, somEnumType.Namespace, scalarType.Type, somEnumType.IsFlags, DataSpace.CSpace);
		Type clrEquivalentType = scalarType.Type.ClrEquivalentType;
		foreach (SchemaEnumMember enumMember2 in somEnumType.EnumMembers)
		{
			EnumMember enumMember = new EnumMember(enumMember2.Name, Convert.ChangeType(enumMember2.Value, clrEquivalentType, CultureInfo.InvariantCulture));
			if (enumMember2.Documentation != null)
			{
				enumMember.Documentation = ConvertToDocumentation(enumMember2.Documentation);
			}
			AddOtherContent(enumMember2, enumMember);
			enumType.AddMember(enumMember);
		}
		if (somEnumType.Documentation != null)
		{
			enumType.Documentation = ConvertToDocumentation(somEnumType.Documentation);
		}
		AddOtherContent(somEnumType, enumType);
		newGlobalItems.Add(somEnumType, enumType);
		return enumType;
	}

	private static Documentation ConvertToDocumentation(DocumentationElement element)
	{
		return element.MetadataDocumentation;
	}

	private static TypeUsage GetFunctionTypeUsage(bool isModelFunction, Function somFunction, FacetEnabledSchemaElement somParameter, DbProviderManifest providerManifest, bool areConvertingForProviderManifest, System.Data.Entity.Core.SchemaObjectModel.SchemaType type, CollectionKind collectionKind, bool isRefType, ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		if (somParameter != null && areConvertingForProviderManifest && somParameter.HasUserDefinedFacets)
		{
			return somParameter.TypeUsage;
		}
		if (type == null)
		{
			if (isModelFunction && somParameter != null && somParameter is Parameter)
			{
				((Parameter)somParameter).ResolveNestedTypeNames(convertedItemCache, newGlobalItems);
				return somParameter.TypeUsage;
			}
			if (somParameter != null && somParameter is ReturnType)
			{
				((ReturnType)somParameter).ResolveNestedTypeNames(convertedItemCache, newGlobalItems);
				return somParameter.TypeUsage;
			}
			return null;
		}
		EdmType edmType;
		if (areConvertingForProviderManifest)
		{
			edmType = ((!(type is TypeElement)) ? (type as ScalarType).Type : (type as TypeElement).PrimitiveType);
		}
		else if (type is ScalarType scalarType)
		{
			if (isModelFunction && somParameter != null)
			{
				if (somParameter.TypeUsage == null)
				{
					somParameter.ValidateAndSetTypeUsage(scalarType);
				}
				return somParameter.TypeUsage;
			}
			if (isModelFunction)
			{
				ModelFunction modelFunction = somFunction as ModelFunction;
				if (modelFunction.TypeUsage == null)
				{
					modelFunction.ValidateAndSetTypeUsage(scalarType);
				}
				return modelFunction.TypeUsage;
			}
			if (somParameter != null && somParameter.HasUserDefinedFacets && somFunction.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
			{
				somParameter.ValidateAndSetTypeUsage(scalarType);
				return somParameter.TypeUsage;
			}
			edmType = GetPrimitiveType(scalarType, providerManifest);
		}
		else
		{
			edmType = (EdmType)LoadSchemaElement(type, providerManifest, convertedItemCache, newGlobalItems);
			if (isModelFunction && type is SchemaEnumType)
			{
				if (somParameter != null)
				{
					somParameter.ValidateAndSetTypeUsage(edmType);
					return somParameter.TypeUsage;
				}
				if (somFunction != null)
				{
					ModelFunction obj = (ModelFunction)somFunction;
					obj.ValidateAndSetTypeUsage(edmType);
					return obj.TypeUsage;
				}
			}
		}
		if (collectionKind != 0)
		{
			return convertedItemCache.GetCollectionTypeUsageWithNullFacets(edmType);
		}
		EntityType entityType = edmType as EntityType;
		if (entityType != null && isRefType)
		{
			return TypeUsage.Create(new RefType(entityType));
		}
		return convertedItemCache.GetTypeUsageWithNullFacets(edmType);
	}

	private static ParameterMode GetParameterMode(ParameterDirection parameterDirection)
	{
		return parameterDirection switch
		{
			ParameterDirection.Input => ParameterMode.In, 
			ParameterDirection.Output => ParameterMode.Out, 
			_ => ParameterMode.InOut, 
		};
	}

	private static void ApplyTypePropertyFacets(TypeUsage sourceType, ref TypeUsage targetType)
	{
		Dictionary<string, Facet> dictionary = targetType.Facets.ToDictionary((Facet f) => f.Name);
		bool flag = false;
		foreach (Facet facet in sourceType.Facets)
		{
			if (dictionary.TryGetValue(facet.Name, out var value))
			{
				if (!value.Description.IsConstant)
				{
					flag = true;
					dictionary[value.Name] = Facet.Create(value.Description, facet.Value);
				}
			}
			else
			{
				flag = true;
				dictionary.Add(facet.Name, facet);
			}
		}
		if (flag)
		{
			targetType = TypeUsage.Create(targetType.EdmType, dictionary.Values);
		}
	}

	private static void PopulateGeneralFacets(StructuredProperty somProperty, ref TypeUsage propertyTypeUsage)
	{
		bool flag = false;
		Dictionary<string, Facet> dictionary = propertyTypeUsage.Facets.ToDictionary((Facet f) => f.Name);
		if (!somProperty.Nullable)
		{
			dictionary["Nullable"] = Facet.Create(MetadataItem.NullableFacetDescription, false);
			flag = true;
		}
		if (somProperty.Default != null)
		{
			dictionary["DefaultValue"] = Facet.Create(MetadataItem.DefaultValueFacetDescription, somProperty.DefaultAsObject);
			flag = true;
		}
		if (somProperty.Schema.SchemaVersion == 1.1)
		{
			Facet facet = Facet.Create(MetadataItem.CollectionKindFacetDescription, somProperty.CollectionKind);
			dictionary.Add(facet.Name, facet);
			flag = true;
		}
		if (flag)
		{
			propertyTypeUsage = TypeUsage.Create(propertyTypeUsage.EdmType, dictionary.Values);
		}
	}

	private static DataSpace GetDataSpace(DbProviderManifest providerManifest)
	{
		if (providerManifest is EdmProviderManifest)
		{
			return DataSpace.CSpace;
		}
		return DataSpace.SSpace;
	}

	private static PrimitiveType GetPrimitiveType(ScalarType scalarType, DbProviderManifest providerManifest)
	{
		PrimitiveType result = null;
		string name = scalarType.Name;
		foreach (PrimitiveType storeType in providerManifest.GetStoreTypes())
		{
			if (storeType.Name == name)
			{
				result = storeType;
				break;
			}
		}
		return result;
	}

	private static void UpdateSentinelValuesInFacets(ref TypeUsage typeUsage)
	{
		PrimitiveType primitiveType = (PrimitiveType)typeUsage.EdmType;
		if ((primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String || primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary) && Helper.IsUnboundedFacetValue(typeUsage.Facets["MaxLength"]))
		{
			typeUsage = typeUsage.ShallowCopy(new FacetValues
			{
				MaxLength = Helper.GetFacet(primitiveType.FacetDescriptions, "MaxLength").MaxValue
			});
		}
	}
}
