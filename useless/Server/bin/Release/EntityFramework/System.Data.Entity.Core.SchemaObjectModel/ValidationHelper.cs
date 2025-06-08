using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal static class ValidationHelper
{
	internal static void ValidateFacets(SchemaElement element, SchemaType type, TypeUsageBuilder typeUsageBuilder)
	{
		if (type != null)
		{
			if (type is SchemaEnumType schemaEnumType)
			{
				typeUsageBuilder.ValidateEnumFacets(schemaEnumType);
			}
			else if (!(type is ScalarType) && typeUsageBuilder.HasUserDefinedFacets)
			{
				element.AddError(ErrorCode.FacetOnNonScalarType, EdmSchemaErrorSeverity.Error, Strings.FacetsOnNonScalarType(type.FQName));
			}
		}
		else if (typeUsageBuilder.HasUserDefinedFacets)
		{
			element.AddError(ErrorCode.IncorrectlyPlacedFacet, EdmSchemaErrorSeverity.Error, Strings.FacetDeclarationRequiresTypeAttribute);
		}
	}

	internal static void ValidateTypeDeclaration(SchemaElement element, SchemaType type, SchemaElement typeSubElement)
	{
		if (type == null && typeSubElement == null)
		{
			element.AddError(ErrorCode.TypeNotDeclared, EdmSchemaErrorSeverity.Error, Strings.TypeMustBeDeclared);
		}
		if (type != null && typeSubElement != null)
		{
			element.AddError(ErrorCode.TypeDeclaredAsAttributeAndElement, EdmSchemaErrorSeverity.Error, Strings.TypeDeclaredAsAttributeAndElement);
		}
	}

	internal static void ValidateRefType(SchemaElement element, SchemaType type)
	{
		if (type != null && !(type is SchemaEntityType))
		{
			element.AddError(ErrorCode.ReferenceToNonEntityType, EdmSchemaErrorSeverity.Error, Strings.ReferenceToNonEntityType(type.FQName));
		}
	}
}
