using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class EdmModelSemanticValidationRules
{
	internal static readonly EdmModelValidationRule<EdmFunction> EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2 = new EdmModelValidationRule<EdmFunction>(delegate(EdmModelValidationContext context, EdmFunction function)
	{
		if (function.IsFunctionImport && function.IsComposableAttribute)
		{
			context.AddError(function, null, Strings.EdmModel_Validator_Semantic_ComposableFunctionImportsNotSupportedForSchemaVersion);
		}
	});

	internal static readonly EdmModelValidationRule<EdmFunction> EdmFunction_DuplicateParameterName = new EdmModelValidationRule<EdmFunction>(delegate(EdmModelValidationContext context, EdmFunction function)
	{
		HashSet<string> memberNameList6 = new HashSet<string>();
		foreach (FunctionParameter parameter in function.Parameters)
		{
			if (parameter != null && !string.IsNullOrWhiteSpace(parameter.Name))
			{
				AddMemberNameToHashSet(parameter, memberNameList6, context, Strings.ParameterNameAlreadyDefinedDuplicate);
			}
		}
	});

	internal static readonly EdmModelValidationRule<EdmType> EdmType_SystemNamespaceEncountered = new EdmModelValidationRule<EdmType>(delegate(EdmModelValidationContext context, EdmType edmType)
	{
		if (IsEdmSystemNamespace(edmType.NamespaceName) && edmType.BuiltInTypeKind != BuiltInTypeKind.RowType && edmType.BuiltInTypeKind != BuiltInTypeKind.CollectionType && edmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
		{
			context.AddError(edmType, null, Strings.EdmModel_Validator_Semantic_SystemNamespaceEncountered(edmType.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EntityContainer> EdmEntityContainer_SimilarRelationshipEnd = new EdmModelValidationRule<EntityContainer>(delegate(EdmModelValidationContext context, EntityContainer edmEntityContainer)
	{
		List<KeyValuePair<AssociationSet, EntitySet>> list8 = new List<KeyValuePair<AssociationSet, EntitySet>>();
		List<KeyValuePair<AssociationSet, EntitySet>> list9 = new List<KeyValuePair<AssociationSet, EntitySet>>();
		foreach (AssociationSet associationSet in edmEntityContainer.AssociationSets)
		{
			KeyValuePair<AssociationSet, EntitySet> sourceEnd = new KeyValuePair<AssociationSet, EntitySet>(associationSet, associationSet.SourceSet);
			KeyValuePair<AssociationSet, EntitySet> targetEnd = new KeyValuePair<AssociationSet, EntitySet>(associationSet, associationSet.TargetSet);
			KeyValuePair<AssociationSet, EntitySet> keyValuePair = list8.FirstOrDefault((KeyValuePair<AssociationSet, EntitySet> e) => AreRelationshipEndsEqual(e, sourceEnd));
			KeyValuePair<AssociationSet, EntitySet> keyValuePair2 = list9.FirstOrDefault((KeyValuePair<AssociationSet, EntitySet> e) => AreRelationshipEndsEqual(e, targetEnd));
			if (!keyValuePair.Equals(default(KeyValuePair<AssociationSet, EntitySet>)))
			{
				context.AddError(edmEntityContainer, null, Strings.EdmModel_Validator_Semantic_SimilarRelationshipEnd(keyValuePair.Key.ElementType.SourceEnd.Name, keyValuePair.Key.Name, associationSet.Name, keyValuePair.Value.Name, edmEntityContainer.Name));
			}
			else
			{
				list8.Add(sourceEnd);
			}
			if (!keyValuePair2.Equals(default(KeyValuePair<AssociationSet, EntitySet>)))
			{
				context.AddError(edmEntityContainer, null, Strings.EdmModel_Validator_Semantic_SimilarRelationshipEnd(keyValuePair2.Key.ElementType.TargetEnd.Name, keyValuePair2.Key.Name, associationSet.Name, keyValuePair2.Value.Name, edmEntityContainer.Name));
			}
			else
			{
				list9.Add(targetEnd);
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityContainer> EdmEntityContainer_InvalidEntitySetNameReference = new EdmModelValidationRule<EntityContainer>(delegate(EdmModelValidationContext context, EntityContainer edmEntityContainer)
	{
		if (edmEntityContainer.AssociationSets != null)
		{
			foreach (AssociationSet associationSet2 in edmEntityContainer.AssociationSets)
			{
				if (associationSet2.SourceSet != null && associationSet2.ElementType != null && associationSet2.ElementType.SourceEnd != null && !edmEntityContainer.EntitySets.Contains(associationSet2.SourceSet))
				{
					context.AddError(associationSet2.SourceSet, null, Strings.EdmModel_Validator_Semantic_InvalidEntitySetNameReference(associationSet2.SourceSet.Name, associationSet2.ElementType.SourceEnd.Name));
				}
				if (associationSet2.TargetSet != null && associationSet2.ElementType != null && associationSet2.ElementType.TargetEnd != null && !edmEntityContainer.EntitySets.Contains(associationSet2.TargetSet))
				{
					context.AddError(associationSet2.TargetSet, null, Strings.EdmModel_Validator_Semantic_InvalidEntitySetNameReference(associationSet2.TargetSet.Name, associationSet2.ElementType.TargetEnd.Name));
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityContainer> EdmEntityContainer_ConcurrencyRedefinedOnSubTypeOfEntitySetType = new EdmModelValidationRule<EntityContainer>(delegate(EdmModelValidationContext context, EntityContainer edmEntityContainer)
	{
		Dictionary<EntityType, EntitySet> dictionary = new Dictionary<EntityType, EntitySet>();
		foreach (EntitySet entitySet in edmEntityContainer.EntitySets)
		{
			if (entitySet != null && entitySet.ElementType != null && !dictionary.ContainsKey(entitySet.ElementType))
			{
				dictionary.Add(entitySet.ElementType, entitySet);
			}
		}
		foreach (EntityType entityType4 in context.Model.EntityTypes)
		{
			if (TypeIsSubTypeOf(entityType4, dictionary, out var set) && IsTypeDefinesNewConcurrencyProperties(entityType4))
			{
				context.AddError(entityType4, null, Strings.EdmModel_Validator_Semantic_ConcurrencyRedefinedOnSubTypeOfEntitySetType(GetQualifiedName(entityType4, entityType4.NamespaceName), GetQualifiedName(set.ElementType, set.ElementType.NamespaceName), GetQualifiedName(set, set.EntityContainer.Name)));
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityContainer> EdmEntityContainer_DuplicateEntityContainerMemberName = new EdmModelValidationRule<EntityContainer>(delegate(EdmModelValidationContext context, EntityContainer edmEntityContainer)
	{
		HashSet<string> memberNameList5 = new HashSet<string>();
		foreach (EntitySetBase baseEntitySet in edmEntityContainer.BaseEntitySets)
		{
			AddMemberNameToHashSet(baseEntitySet, memberNameList5, context, Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName);
		}
	});

	internal static readonly EdmModelValidationRule<EntityContainer> EdmEntityContainer_DuplicateEntitySetTable = new EdmModelValidationRule<EntityContainer>(delegate(EdmModelValidationContext context, EntityContainer edmEntityContainer)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (EntitySetBase baseEntitySet2 in edmEntityContainer.BaseEntitySets)
		{
			if (!string.IsNullOrWhiteSpace(baseEntitySet2.Table) && !hashSet.Add(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2] { baseEntitySet2.Schema, baseEntitySet2.Table })))
			{
				context.AddError(baseEntitySet2, "Name", Strings.DuplicateEntitySetTable(baseEntitySet2.Name, baseEntitySet2.Schema, baseEntitySet2.Table));
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntitySet> EdmEntitySet_EntitySetTypeHasNoKeys = new EdmModelValidationRule<EntitySet>(delegate(EdmModelValidationContext context, EntitySet edmEntitySet)
	{
		if (edmEntitySet.ElementType != null && !edmEntitySet.ElementType.GetValidKey().Any())
		{
			context.AddError(edmEntitySet, "EntityType", Strings.EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys(edmEntitySet.Name, edmEntitySet.ElementType.Name));
		}
	});

	internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_DuplicateEndName = new EdmModelValidationRule<AssociationSet>(delegate(EdmModelValidationContext context, AssociationSet edmAssociationSet)
	{
		if (edmAssociationSet.ElementType != null && edmAssociationSet.ElementType.SourceEnd != null && edmAssociationSet.ElementType.TargetEnd != null && edmAssociationSet.ElementType.SourceEnd.Name == edmAssociationSet.ElementType.TargetEnd.Name)
		{
			context.AddError(edmAssociationSet.SourceSet, "Name", Strings.EdmModel_Validator_Semantic_DuplicateEndName(edmAssociationSet.ElementType.SourceEnd.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_DuplicatePropertyNameSpecifiedInEntityKey = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		List<EdmProperty> list6 = edmEntityType.GetKeyProperties().ToList();
		if (list6.Count > 0)
		{
			List<EdmProperty> list7 = new List<EdmProperty>();
			foreach (EdmProperty key2 in list6)
			{
				if (key2 != null && !list7.Contains(key2))
				{
					if (list6.Count((EdmProperty p) => key2.Equals(p)) > 1)
					{
						context.AddError(key2, null, Strings.EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey(edmEntityType.Name, key2.Name));
					}
					list7.Add(key2);
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidKeyNullablePart = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		foreach (EdmProperty item in edmEntityType.GetValidKey())
		{
			if (item.IsPrimitiveType && item.Nullable)
			{
				context.AddError(item, "Nullable", Strings.EdmModel_Validator_Semantic_InvalidKeyNullablePart(item.Name, edmEntityType.Name));
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_EntityKeyMustBeScalar = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		foreach (EdmProperty item2 in edmEntityType.GetValidKey())
		{
			if (!item2.IsUnderlyingPrimitiveType)
			{
				context.AddError(item2, null, Strings.EdmModel_Validator_Semantic_EntityKeyMustBeScalar(edmEntityType.Name, item2.Name));
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidKeyKeyDefinedInBaseClass = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		if (edmEntityType.BaseType != null && edmEntityType.KeyProperties.Where((EdmProperty key) => edmEntityType.DeclaredMembers.Contains(key)).Any())
		{
			context.AddError(edmEntityType.BaseType, null, Strings.EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass(edmEntityType.Name, edmEntityType.BaseType.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_KeyMissingOnEntityType = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		if (edmEntityType.BaseType == null && edmEntityType.KeyProperties.Count == 0)
		{
			context.AddError(edmEntityType, null, Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType(edmEntityType.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidMemberNameMatchesTypeName = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		List<EdmProperty> list5 = edmEntityType.Properties.ToList();
		if (!string.IsNullOrWhiteSpace(edmEntityType.Name) && list5.Count > 0)
		{
			foreach (EdmProperty item3 in list5)
			{
				if (item3 != null && context.IsCSpace && item3.Name.EqualsOrdinal(edmEntityType.Name))
				{
					context.AddError(item3, "Name", Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(item3.Name, GetQualifiedName(edmEntityType, edmEntityType.NamespaceName)));
				}
			}
			if (edmEntityType.DeclaredNavigationProperties.Any())
			{
				foreach (NavigationProperty declaredNavigationProperty in edmEntityType.DeclaredNavigationProperties)
				{
					if (declaredNavigationProperty != null && declaredNavigationProperty.Name.EqualsOrdinal(edmEntityType.Name))
					{
						context.AddError(declaredNavigationProperty, "Name", Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(declaredNavigationProperty.Name, GetQualifiedName(edmEntityType, edmEntityType.NamespaceName)));
					}
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_PropertyNameAlreadyDefinedDuplicate = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		HashSet<string> memberNameList4 = new HashSet<string>();
		foreach (EdmProperty property in edmEntityType.Properties)
		{
			if (property != null && !string.IsNullOrWhiteSpace(property.Name))
			{
				AddMemberNameToHashSet(property, memberNameList4, context, Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
			}
		}
		if (edmEntityType.DeclaredNavigationProperties.Any())
		{
			foreach (NavigationProperty declaredNavigationProperty2 in edmEntityType.DeclaredNavigationProperties)
			{
				if (declaredNavigationProperty2 != null && !string.IsNullOrWhiteSpace(declaredNavigationProperty2.Name))
				{
					AddMemberNameToHashSet(declaredNavigationProperty2, memberNameList4, context, Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_CycleInTypeHierarchy = new EdmModelValidationRule<EntityType>(delegate(EdmModelValidationContext context, EntityType edmEntityType)
	{
		if (CheckForInheritanceCycle(edmEntityType, (EntityType et) => (EntityType)et.BaseType))
		{
			context.AddError(edmEntityType, "BaseType", Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(GetQualifiedName(edmEntityType, edmEntityType.NamespaceName)));
		}
	});

	internal static readonly EdmModelValidationRule<NavigationProperty> EdmNavigationProperty_BadNavigationPropertyUndefinedRole = new EdmModelValidationRule<NavigationProperty>(delegate(EdmModelValidationContext context, NavigationProperty edmNavigationProperty)
	{
		if (edmNavigationProperty.Association != null && edmNavigationProperty.Association.SourceEnd != null && edmNavigationProperty.Association.TargetEnd != null && edmNavigationProperty.Association.SourceEnd.Name != null && edmNavigationProperty.Association.TargetEnd.Name != null && edmNavigationProperty.ToEndMember != edmNavigationProperty.Association.SourceEnd && edmNavigationProperty.ToEndMember != edmNavigationProperty.Association.TargetEnd)
		{
			context.AddError(edmNavigationProperty, null, Strings.EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole(edmNavigationProperty.Association.SourceEnd.Name, edmNavigationProperty.Association.TargetEnd.Name, edmNavigationProperty.Association.Name));
		}
	});

	internal static readonly EdmModelValidationRule<NavigationProperty> EdmNavigationProperty_BadNavigationPropertyRolesCannotBeTheSame = new EdmModelValidationRule<NavigationProperty>(delegate(EdmModelValidationContext context, NavigationProperty edmNavigationProperty)
	{
		if (edmNavigationProperty.Association != null && edmNavigationProperty.Association.SourceEnd != null && edmNavigationProperty.Association.TargetEnd != null && edmNavigationProperty.ToEndMember == edmNavigationProperty.GetFromEnd())
		{
			context.AddError(edmNavigationProperty, "ToRole", Strings.EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame);
		}
	});

	internal static readonly EdmModelValidationRule<NavigationProperty> EdmNavigationProperty_BadNavigationPropertyBadFromRoleType = new EdmModelValidationRule<NavigationProperty>(delegate(EdmModelValidationContext context, NavigationProperty edmNavigationProperty)
	{
		AssociationEndMember fromEnd;
		if (edmNavigationProperty.Association != null && (fromEnd = edmNavigationProperty.GetFromEnd()) != null)
		{
			EntityType entityType = null;
			IList<EntityType> list4 = (context.Model.EntityTypes as IList<EntityType>) ?? context.Model.EntityTypes.ToList();
			for (int j = 0; j < list4.Count; j++)
			{
				EntityType entityType2 = list4[j];
				if (entityType2.DeclaredNavigationProperties.Contains(edmNavigationProperty))
				{
					entityType = entityType2;
					break;
				}
			}
			EntityType entityType3 = fromEnd.GetEntityType();
			if (entityType != entityType3)
			{
				context.AddError(edmNavigationProperty, "FromRole", Strings.BadNavigationPropertyBadFromRoleType(edmNavigationProperty.Name, entityType3.Name, fromEnd.Name, edmNavigationProperty.Association.Name, entityType.Name));
			}
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_InvalidOperationMultipleEndsInAssociation = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (edmAssociationType.SourceEnd != null && edmAssociationType.SourceEnd.DeleteBehavior != 0 && edmAssociationType.TargetEnd != null && edmAssociationType.TargetEnd.DeleteBehavior != 0)
		{
			context.AddError(edmAssociationType, null, Strings.EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation);
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_EndWithManyMultiplicityCannotHaveOperationsSpecified = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (edmAssociationType.SourceEnd != null && edmAssociationType.SourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many && edmAssociationType.SourceEnd.DeleteBehavior != 0)
		{
			context.AddError(edmAssociationType.SourceEnd, "OnDelete", Strings.EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified(edmAssociationType.SourceEnd.Name, edmAssociationType.Name));
		}
		if (edmAssociationType.TargetEnd != null && edmAssociationType.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many && edmAssociationType.TargetEnd.DeleteBehavior != 0)
		{
			context.AddError(edmAssociationType.TargetEnd, "OnDelete", Strings.EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified(edmAssociationType.TargetEnd.Name, edmAssociationType.Name));
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_EndNameAlreadyDefinedDuplicate = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (edmAssociationType.SourceEnd != null && edmAssociationType.TargetEnd != null && edmAssociationType.SourceEnd.Name == edmAssociationType.TargetEnd.Name)
		{
			context.AddError(edmAssociationType.SourceEnd, "Name", Strings.EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate(edmAssociationType.SourceEnd.Name));
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_SameRoleReferredInReferentialConstraint = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (IsReferentialConstraintReadyForValidation(edmAssociationType) && edmAssociationType.Constraint.FromRole.Name == edmAssociationType.Constraint.ToRole.Name)
		{
			context.AddError(edmAssociationType.Constraint.ToRole, null, Strings.EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint(edmAssociationType.Name));
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_ValidateReferentialConstraint = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (IsReferentialConstraintReadyForValidation(edmAssociationType))
		{
			ReferentialConstraint constraint = edmAssociationType.Constraint;
			RelationshipEndMember fromRole = constraint.FromRole;
			RelationshipEndMember toRole = constraint.ToRole;
			IsKeyProperty(constraint.ToProperties.ToList(), toRole, out var isKeyProperty, out var areAllPropertiesNullable, out var isAnyPropertyNullable, out var isSubsetOfKeyProperties);
			IsKeyProperty(constraint.FromRole.GetEntityType().GetValidKey().ToList(), fromRole, out var _, out var _, out var _, out var _);
			bool flag = context.Model.SchemaVersion <= 1.1;
			if (fromRole.RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				context.AddError(fromRole, null, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleUpperBoundMustBeOne(fromRole.Name, edmAssociationType.Name));
			}
			else if (areAllPropertiesNullable && fromRole.RelationshipMultiplicity == RelationshipMultiplicity.One)
			{
				string errorMessage = Strings.EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNullableV1(fromRole.Name, edmAssociationType.Name);
				context.AddError(edmAssociationType, null, errorMessage);
			}
			else if (((flag && !areAllPropertiesNullable) || (!flag && !isAnyPropertyNullable)) && fromRole.RelationshipMultiplicity != RelationshipMultiplicity.One)
			{
				string errorMessage2 = ((!flag) ? Strings.EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV2(fromRole.Name, edmAssociationType.Name) : Strings.EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV1(fromRole.Name, edmAssociationType.Name));
				context.AddError(edmAssociationType, null, errorMessage2);
			}
			if (!isSubsetOfKeyProperties && !edmAssociationType.IsForeignKey(context.Model.SchemaVersion) && context.IsCSpace)
			{
				context.AddError(toRole, null, Strings.EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint(toRole.Name, GetQualifiedName(toRole.GetEntityType(), toRole.GetEntityType().NamespaceName), GetQualifiedName(edmAssociationType, edmAssociationType.NamespaceName)));
			}
			if (isKeyProperty)
			{
				if (toRole.RelationshipMultiplicity == RelationshipMultiplicity.Many)
				{
					context.AddError(toRole, null, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeOne(toRole.Name, edmAssociationType.Name));
				}
			}
			else if (toRole.RelationshipMultiplicity != RelationshipMultiplicity.Many)
			{
				context.AddError(toRole, null, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeMany(toRole.Name, edmAssociationType.Name));
			}
			List<EdmProperty> list2 = fromRole.GetEntityType().GetValidKey().ToList();
			List<EdmProperty> list3 = constraint.ToProperties.ToList();
			if (list3.Count != list2.Count)
			{
				context.AddError(constraint, null, Strings.EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint);
			}
			else
			{
				List<EdmProperty> principalProperties = constraint.FromProperties.ToList();
				int count = list3.Count;
				int i;
				for (i = 0; i < count; i++)
				{
					EdmProperty edmProperty2 = list3[i];
					EdmProperty edmProperty3 = list2.SingleOrDefault((EdmProperty p) => p.Name == principalProperties[i].Name);
					if (edmProperty3 != null && edmProperty2 != null && edmProperty3.TypeUsage != null && edmProperty2.TypeUsage != null && edmProperty3.IsPrimitiveType && edmProperty2.IsPrimitiveType && !IsPrimitiveTypesEqual(edmProperty2, edmProperty3))
					{
						context.AddError(constraint, null, Strings.EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint(constraint.ToProperties.ToList()[i].Name, toRole.GetEntityType().Name, edmProperty3.Name, fromRole.GetEntityType().Name, edmAssociationType.Name));
					}
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_InvalidPropertyInRelationshipConstraint = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (edmAssociationType.Constraint != null && edmAssociationType.Constraint.ToRole != null && edmAssociationType.Constraint.ToRole.GetEntityType() != null)
		{
			List<EdmProperty> list = edmAssociationType.Constraint.ToRole.GetEntityType().Properties.ToList();
			foreach (EdmProperty toProperty in edmAssociationType.Constraint.ToProperties)
			{
				if (toProperty != null && !list.Contains(toProperty))
				{
					context.AddError(toProperty, null, Strings.EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraint(toProperty.Name, edmAssociationType.Constraint.ToRole.Name));
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidIsAbstract = new EdmModelValidationRule<ComplexType>(delegate(EdmModelValidationContext context, ComplexType edmComplexType)
	{
		if (edmComplexType.Abstract)
		{
			context.AddError(edmComplexType, "Abstract", Strings.EdmModel_Validator_Semantic_InvalidComplexTypeAbstract(GetQualifiedName(edmComplexType, edmComplexType.NamespaceName)));
		}
	});

	internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidIsPolymorphic = new EdmModelValidationRule<ComplexType>(delegate(EdmModelValidationContext context, ComplexType edmComplexType)
	{
		if (edmComplexType.BaseType != null)
		{
			context.AddError(edmComplexType, "BaseType", Strings.EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic(GetQualifiedName(edmComplexType, edmComplexType.NamespaceName)));
		}
	});

	internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidMemberNameMatchesTypeName = new EdmModelValidationRule<ComplexType>(delegate(EdmModelValidationContext context, ComplexType edmComplexType)
	{
		if (!string.IsNullOrWhiteSpace(edmComplexType.Name) && edmComplexType.Properties.Any())
		{
			foreach (EdmProperty property2 in edmComplexType.Properties)
			{
				if (property2 != null && property2.Name.EqualsOrdinal(edmComplexType.Name))
				{
					context.AddError(property2, "Name", Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(property2.Name, GetQualifiedName(edmComplexType, edmComplexType.NamespaceName)));
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_PropertyNameAlreadyDefinedDuplicate = new EdmModelValidationRule<ComplexType>(delegate(EdmModelValidationContext context, ComplexType edmComplexType)
	{
		if (edmComplexType.Properties.Any())
		{
			HashSet<string> memberNameList3 = new HashSet<string>();
			foreach (EdmProperty property3 in edmComplexType.Properties)
			{
				if (!string.IsNullOrWhiteSpace(property3.Name))
				{
					AddMemberNameToHashSet(property3, memberNameList3, context, Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_PropertyNameAlreadyDefinedDuplicate_V1_1 = new EdmModelValidationRule<ComplexType>(delegate(EdmModelValidationContext context, ComplexType edmComplexType)
	{
		if (edmComplexType.Properties.Any())
		{
			HashSet<string> memberNameList2 = new HashSet<string>();
			foreach (EdmProperty property4 in edmComplexType.Properties)
			{
				if (property4 != null && !string.IsNullOrWhiteSpace(property4.Name))
				{
					AddMemberNameToHashSet(property4, memberNameList2, context, Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
				}
			}
		}
	});

	internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_CycleInTypeHierarchy_V1_1 = new EdmModelValidationRule<ComplexType>(delegate(EdmModelValidationContext context, ComplexType edmComplexType)
	{
		if (CheckForInheritanceCycle(edmComplexType, (ComplexType ct) => (ComplexType)ct.BaseType))
		{
			context.AddError(edmComplexType, "BaseType", Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(GetQualifiedName(edmComplexType, edmComplexType.NamespaceName)));
		}
	});

	internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidCollectionKind = new EdmModelValidationRule<EdmProperty>(delegate(EdmModelValidationContext context, EdmProperty edmProperty)
	{
		if (edmProperty.CollectionKind != 0)
		{
			context.AddError(edmProperty, "CollectionKind", Strings.EdmModel_Validator_Semantic_InvalidCollectionKindNotV1_1(edmProperty.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidCollectionKind_V1_1 = new EdmModelValidationRule<EdmProperty>(delegate(EdmModelValidationContext context, EdmProperty edmProperty)
	{
		if (edmProperty.CollectionKind != 0 && edmProperty.TypeUsage != null && !edmProperty.IsCollectionType)
		{
			context.AddError(edmProperty, "CollectionKind", Strings.EdmModel_Validator_Semantic_InvalidCollectionKindNotCollection(edmProperty.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_NullableComplexType = new EdmModelValidationRule<EdmProperty>(delegate(EdmModelValidationContext context, EdmProperty edmProperty)
	{
		if (edmProperty.TypeUsage != null && edmProperty.ComplexType != null && edmProperty.Nullable)
		{
			context.AddError(edmProperty, "Nullable", Strings.EdmModel_Validator_Semantic_NullableComplexType(edmProperty.Name));
		}
	});

	internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType = new EdmModelValidationRule<EdmProperty>(delegate(EdmModelValidationContext context, EdmProperty edmProperty)
	{
		if (edmProperty.TypeUsage.EdmType != null && !edmProperty.IsPrimitiveType && !edmProperty.IsComplexType)
		{
			context.AddError(edmProperty, "Type", Strings.EdmModel_Validator_Semantic_InvalidPropertyType(edmProperty.IsCollectionType ? "CollectionType" : edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()));
		}
	});

	internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType_V1_1 = new EdmModelValidationRule<EdmProperty>(delegate(EdmModelValidationContext context, EdmProperty edmProperty)
	{
		if (edmProperty.TypeUsage != null && edmProperty.TypeUsage.EdmType != null && !edmProperty.IsPrimitiveType && !edmProperty.IsComplexType && !edmProperty.IsCollectionType)
		{
			context.AddError(edmProperty, "Type", Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V1_1(edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()));
		}
	});

	internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType_V3 = new EdmModelValidationRule<EdmProperty>(delegate(EdmModelValidationContext context, EdmProperty edmProperty)
	{
		if (edmProperty.TypeUsage != null && edmProperty.TypeUsage.EdmType != null && !edmProperty.IsPrimitiveType && !edmProperty.IsComplexType && !edmProperty.IsEnumType)
		{
			context.AddError(edmProperty, "Type", Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V3(edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()));
		}
	});

	internal static readonly EdmModelValidationRule<EdmModel> EdmNamespace_TypeNameAlreadyDefinedDuplicate = new EdmModelValidationRule<EdmModel>(delegate(EdmModelValidationContext context, EdmModel model)
	{
		HashSet<string> memberNameList = new HashSet<string>();
		foreach (EdmType namespaceItem in model.NamespaceItems)
		{
			AddMemberNameToHashSet(namespaceItem, memberNameList, context, Strings.EdmModel_Validator_Semantic_TypeNameAlreadyDefinedDuplicate);
		}
	});

	private static string GetQualifiedName(INamedDataModelItem item, string qualifiedPrefix)
	{
		return qualifiedPrefix + "." + item.Name;
	}

	private static bool AreRelationshipEndsEqual(KeyValuePair<AssociationSet, EntitySet> left, KeyValuePair<AssociationSet, EntitySet> right)
	{
		if (left.Value == right.Value && left.Key.ElementType == right.Key.ElementType)
		{
			return true;
		}
		return false;
	}

	private static bool IsReferentialConstraintReadyForValidation(AssociationType association)
	{
		ReferentialConstraint constraint = association.Constraint;
		if (constraint == null)
		{
			return false;
		}
		if (constraint.FromRole == null || constraint.ToRole == null)
		{
			return false;
		}
		if (constraint.FromRole.GetEntityType() == null || constraint.ToRole.GetEntityType() == null)
		{
			return false;
		}
		if (constraint.ToProperties.Any())
		{
			foreach (EdmProperty toProperty in constraint.ToProperties)
			{
				if (toProperty == null)
				{
					return false;
				}
				if (toProperty.TypeUsage == null || toProperty.TypeUsage.EdmType == null)
				{
					return false;
				}
			}
			IEnumerable<EdmProperty> validKey = constraint.FromRole.GetEntityType().GetValidKey();
			if (validKey.Any())
			{
				return validKey.All((EdmProperty propRef) => propRef != null && propRef.TypeUsage != null && propRef.TypeUsage.EdmType != null);
			}
			return false;
		}
		return false;
	}

	private static void IsKeyProperty(List<EdmProperty> roleProperties, RelationshipEndMember roleElement, out bool isKeyProperty, out bool areAllPropertiesNullable, out bool isAnyPropertyNullable, out bool isSubsetOfKeyProperties)
	{
		isKeyProperty = true;
		areAllPropertiesNullable = true;
		isAnyPropertyNullable = false;
		isSubsetOfKeyProperties = true;
		if (roleElement.GetEntityType().GetValidKey().Count() != roleProperties.Count())
		{
			isKeyProperty = false;
		}
		for (int i = 0; i < roleProperties.Count(); i++)
		{
			if (isSubsetOfKeyProperties && !roleElement.GetEntityType().GetValidKey().ToList()
				.Contains(roleProperties[i]))
			{
				isKeyProperty = false;
				isSubsetOfKeyProperties = false;
			}
			bool nullable = roleProperties[i].Nullable;
			areAllPropertiesNullable &= nullable;
			isAnyPropertyNullable |= nullable;
		}
	}

	private static void AddMemberNameToHashSet(INamedDataModelItem item, HashSet<string> memberNameList, EdmModelValidationContext context, Func<string, string> getErrorString)
	{
		if (!string.IsNullOrWhiteSpace(item.Name) && !memberNameList.Add(item.Name))
		{
			context.AddError((MetadataItem)item, "Name", getErrorString(item.Name));
		}
	}

	private static bool CheckForInheritanceCycle<T>(T type, Func<T, T> getBaseType) where T : class
	{
		T val = getBaseType(type);
		if (val != null)
		{
			T val2 = val;
			T val3 = val;
			do
			{
				val3 = getBaseType(val3);
				if (val2 == val3)
				{
					return true;
				}
				if (val2 == null)
				{
					return false;
				}
				val2 = getBaseType(val2);
				if (val3 != null)
				{
					val3 = getBaseType(val3);
				}
			}
			while (val3 != null);
		}
		return false;
	}

	private static bool IsPrimitiveTypesEqual(EdmProperty primitiveType1, EdmProperty primitiveType2)
	{
		return primitiveType1.PrimitiveType.PrimitiveTypeKind == primitiveType2.PrimitiveType.PrimitiveTypeKind;
	}

	private static bool IsEdmSystemNamespace(string namespaceName)
	{
		if (!(namespaceName == "Transient") && !(namespaceName == "Edm"))
		{
			return namespaceName == "System";
		}
		return true;
	}

	private static bool IsTypeDefinesNewConcurrencyProperties(EntityType entityType)
	{
		return entityType.DeclaredProperties.Where((EdmProperty property) => property.TypeUsage != null).Any((EdmProperty property) => property.PrimitiveType != null && property.ConcurrencyMode != ConcurrencyMode.None);
	}

	private static bool TypeIsSubTypeOf(EntityType entityType, Dictionary<EntityType, EntitySet> baseEntitySetTypes, out EntitySet set)
	{
		if (entityType.IsTypeHierarchyRoot())
		{
			set = null;
			return false;
		}
		foreach (EntityType item in entityType.ToHierarchy())
		{
			if (baseEntitySetTypes.ContainsKey(item))
			{
				set = baseEntitySetTypes[item];
				return true;
			}
		}
		set = null;
		return false;
	}

	private static bool IsTypeHierarchyRoot(this EntityType entityType)
	{
		return entityType.BaseType == null;
	}

	private static bool IsForeignKey(this AssociationType association, double version)
	{
		if (version >= 2.0 && association.Constraint != null)
		{
			return true;
		}
		return false;
	}
}
