using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class EdmModelSyntacticValidationRules
{
	internal static readonly EdmModelValidationRule<INamedDataModelItem> EdmModel_NameMustNotBeEmptyOrWhiteSpace = new EdmModelValidationRule<INamedDataModelItem>(delegate(EdmModelValidationContext context, INamedDataModelItem item)
	{
		if (string.IsNullOrWhiteSpace(item.Name))
		{
			context.AddError((MetadataItem)item, "Name", Strings.EdmModel_Validator_Syntactic_MissingName);
		}
	});

	internal static readonly EdmModelValidationRule<INamedDataModelItem> EdmModel_NameIsTooLong = new EdmModelValidationRule<INamedDataModelItem>(delegate(EdmModelValidationContext context, INamedDataModelItem item)
	{
		if (!string.IsNullOrWhiteSpace(item.Name) && item.Name.Length > 480 && !(item is RowType) && !(item is CollectionType))
		{
			context.AddError((MetadataItem)item, "Name", Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong(item.Name));
		}
	});

	internal static readonly EdmModelValidationRule<INamedDataModelItem> EdmModel_NameIsNotAllowed = new EdmModelValidationRule<INamedDataModelItem>(delegate(EdmModelValidationContext context, INamedDataModelItem item)
	{
		if (!string.IsNullOrWhiteSpace(item.Name) && !(item is RowType) && !(item is CollectionType) && (context.IsCSpace || !(item is EdmProperty)) && (item.Name.Contains(".") || (context.IsCSpace && !item.Name.IsValidUndottedName())))
		{
			context.AddError((MetadataItem)item, "Name", Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed(item.Name));
		}
	});

	internal static readonly EdmModelValidationRule<AssociationType> EdmAssociationType_AssociationEndMustNotBeNull = new EdmModelValidationRule<AssociationType>(delegate(EdmModelValidationContext context, AssociationType edmAssociationType)
	{
		if (edmAssociationType.SourceEnd == null || edmAssociationType.TargetEnd == null)
		{
			context.AddError(edmAssociationType, "End", Strings.EdmModel_Validator_Syntactic_EdmAssociationType_AssociationEndMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<ReferentialConstraint> EdmAssociationConstraint_DependentEndMustNotBeNull = new EdmModelValidationRule<ReferentialConstraint>(delegate(EdmModelValidationContext context, ReferentialConstraint edmAssociationConstraint)
	{
		if (edmAssociationConstraint.ToRole == null)
		{
			context.AddError(edmAssociationConstraint, "Dependent", Strings.EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentEndMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<ReferentialConstraint> EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty = new EdmModelValidationRule<ReferentialConstraint>(delegate(EdmModelValidationContext context, ReferentialConstraint edmAssociationConstraint)
	{
		if (edmAssociationConstraint.ToProperties == null || !edmAssociationConstraint.ToProperties.Any())
		{
			context.AddError(edmAssociationConstraint, "Dependent", Strings.EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty);
		}
	});

	internal static readonly EdmModelValidationRule<NavigationProperty> EdmNavigationProperty_AssociationMustNotBeNull = new EdmModelValidationRule<NavigationProperty>(delegate(EdmModelValidationContext context, NavigationProperty edmNavigationProperty)
	{
		if (edmNavigationProperty.Association == null)
		{
			context.AddError(edmNavigationProperty, "Relationship", Strings.EdmModel_Validator_Syntactic_EdmNavigationProperty_AssociationMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<NavigationProperty> EdmNavigationProperty_ResultEndMustNotBeNull = new EdmModelValidationRule<NavigationProperty>(delegate(EdmModelValidationContext context, NavigationProperty edmNavigationProperty)
	{
		if (edmNavigationProperty.ToEndMember == null)
		{
			context.AddError(edmNavigationProperty, "ToRole", Strings.EdmModel_Validator_Syntactic_EdmNavigationProperty_ResultEndMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<AssociationEndMember> EdmAssociationEnd_EntityTypeMustNotBeNull = new EdmModelValidationRule<AssociationEndMember>(delegate(EdmModelValidationContext context, AssociationEndMember edmAssociationEnd)
	{
		if (edmAssociationEnd.GetEntityType() == null)
		{
			context.AddError(edmAssociationEnd, "Type", Strings.EdmModel_Validator_Syntactic_EdmAssociationEnd_EntityTypeMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<EntitySet> EdmEntitySet_ElementTypeMustNotBeNull = new EdmModelValidationRule<EntitySet>(delegate(EdmModelValidationContext context, EntitySet edmEntitySet)
	{
		if (edmEntitySet.ElementType == null)
		{
			context.AddError(edmEntitySet, "ElementType", Strings.EdmModel_Validator_Syntactic_EdmEntitySet_ElementTypeMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_ElementTypeMustNotBeNull = new EdmModelValidationRule<AssociationSet>(delegate(EdmModelValidationContext context, AssociationSet edmAssociationSet)
	{
		if (edmAssociationSet.ElementType == null)
		{
			context.AddError(edmAssociationSet, "ElementType", Strings.EdmModel_Validator_Syntactic_EdmAssociationSet_ElementTypeMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_SourceSetMustNotBeNull = new EdmModelValidationRule<AssociationSet>(delegate(EdmModelValidationContext context, AssociationSet edmAssociationSet)
	{
		if (context.IsCSpace && edmAssociationSet.SourceSet == null)
		{
			context.AddError(edmAssociationSet, "FromRole", Strings.EdmModel_Validator_Syntactic_EdmAssociationSet_SourceSetMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_TargetSetMustNotBeNull = new EdmModelValidationRule<AssociationSet>(delegate(EdmModelValidationContext context, AssociationSet edmAssociationSet)
	{
		if (context.IsCSpace && edmAssociationSet.TargetSet == null)
		{
			context.AddError(edmAssociationSet, "ToRole", Strings.EdmModel_Validator_Syntactic_EdmAssociationSet_TargetSetMustNotBeNull);
		}
	});

	internal static readonly EdmModelValidationRule<TypeUsage> EdmTypeReference_TypeNotValid = new EdmModelValidationRule<TypeUsage>(delegate(EdmModelValidationContext context, TypeUsage edmTypeReference)
	{
		if (!IsEdmTypeUsageValid(edmTypeReference))
		{
			context.AddError(edmTypeReference, null, Strings.EdmModel_Validator_Syntactic_EdmTypeReferenceNotValid);
		}
	});

	private static bool IsEdmTypeUsageValid(TypeUsage typeUsage)
	{
		HashSet<TypeUsage> visitedValidTypeUsages = new HashSet<TypeUsage>();
		return IsEdmTypeUsageValid(typeUsage, visitedValidTypeUsages);
	}

	private static bool IsEdmTypeUsageValid(TypeUsage typeUsage, HashSet<TypeUsage> visitedValidTypeUsages)
	{
		if (visitedValidTypeUsages.Contains(typeUsage))
		{
			return false;
		}
		visitedValidTypeUsages.Add(typeUsage);
		return true;
	}
}
