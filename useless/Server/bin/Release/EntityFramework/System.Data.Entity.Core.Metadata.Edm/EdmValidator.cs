using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class EdmValidator
{
	internal bool SkipReadOnlyItems { get; set; }

	public void Validate<T>(IEnumerable<T> items, List<EdmItemError> ospaceErrors) where T : EdmType
	{
		Check.NotNull(items, "items");
		Check.NotNull(items, "items");
		HashSet<MetadataItem> validatedItems = new HashSet<MetadataItem>();
		foreach (T item in items)
		{
			InternalValidate(item, ospaceErrors, validatedItems);
		}
	}

	protected virtual void OnValidationError(ValidationErrorEventArgs e)
	{
	}

	private void AddError(List<EdmItemError> errors, EdmItemError newError)
	{
		ValidationErrorEventArgs validationErrorEventArgs = new ValidationErrorEventArgs(newError);
		OnValidationError(validationErrorEventArgs);
		errors.Add(validationErrorEventArgs.ValidationError);
	}

	protected virtual IEnumerable<EdmItemError> CustomValidate(MetadataItem item)
	{
		return null;
	}

	private void InternalValidate(MetadataItem item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		if ((!item.IsReadOnly || !SkipReadOnlyItems) && !validatedItems.Contains(item))
		{
			validatedItems.Add(item);
			if (string.IsNullOrEmpty(item.Identity))
			{
				AddError(errors, new EdmItemError(Strings.Validator_EmptyIdentity));
			}
			switch (item.BuiltInTypeKind)
			{
			case BuiltInTypeKind.CollectionType:
				ValidateCollectionType((CollectionType)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.ComplexType:
				ValidateComplexType((ComplexType)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.EntityType:
				ValidateEntityType((EntityType)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.Facet:
				ValidateFacet((Facet)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.MetadataProperty:
				ValidateMetadataProperty((MetadataProperty)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.NavigationProperty:
				ValidateNavigationProperty((NavigationProperty)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.PrimitiveType:
				ValidatePrimitiveType((PrimitiveType)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.EdmProperty:
				ValidateEdmProperty((EdmProperty)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.RefType:
				ValidateRefType((RefType)item, errors, validatedItems);
				break;
			case BuiltInTypeKind.TypeUsage:
				ValidateTypeUsage((TypeUsage)item, errors, validatedItems);
				break;
			}
			IEnumerable<EdmItemError> enumerable = CustomValidate(item);
			if (enumerable != null)
			{
				errors.AddRange(enumerable);
			}
		}
	}

	private void ValidateCollectionType(CollectionType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateEdmType(item, errors, validatedItems);
		if (item.BaseType != null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_CollectionTypesCannotHaveBaseType));
		}
		if (item.TypeUsage == null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_CollectionHasNoTypeUsage));
		}
		else
		{
			InternalValidate(item.TypeUsage, errors, validatedItems);
		}
	}

	private void ValidateComplexType(ComplexType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateStructuralType(item, errors, validatedItems);
	}

	private void ValidateEdmType(EdmType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateItem(item, errors, validatedItems);
		if (string.IsNullOrEmpty(item.Name))
		{
			AddError(errors, new EdmItemError(Strings.Validator_TypeHasNoName));
		}
		if (item.NamespaceName == null || (item.DataSpace != 0 && string.Empty == item.NamespaceName))
		{
			AddError(errors, new EdmItemError(Strings.Validator_TypeHasNoNamespace));
		}
		if (item.BaseType != null)
		{
			InternalValidate(item.BaseType, errors, validatedItems);
		}
	}

	private void ValidateEntityType(EntityType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		if (item.BaseType == null)
		{
			if (item.KeyMembers.Count < 1)
			{
				AddError(errors, new EdmItemError(Strings.Validator_NoKeyMembers(item.FullName)));
			}
			else
			{
				foreach (EdmProperty keyMember in item.KeyMembers)
				{
					if (keyMember.Nullable)
					{
						AddError(errors, new EdmItemError(Strings.Validator_NullableEntityKeyProperty(keyMember.Name, item.FullName)));
					}
				}
			}
		}
		ValidateStructuralType(item, errors, validatedItems);
	}

	private void ValidateFacet(Facet item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateItem(item, errors, validatedItems);
		if (string.IsNullOrEmpty(item.Name))
		{
			AddError(errors, new EdmItemError(Strings.Validator_FacetHasNoName));
		}
		if (item.FacetType == null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_FacetTypeIsNull));
		}
		else
		{
			InternalValidate(item.FacetType, errors, validatedItems);
		}
	}

	private void ValidateItem(MetadataItem item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		if (item.RawMetadataProperties == null)
		{
			return;
		}
		foreach (MetadataProperty metadataProperty in item.MetadataProperties)
		{
			InternalValidate(metadataProperty, errors, validatedItems);
		}
	}

	private void ValidateEdmMember(EdmMember item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateItem(item, errors, validatedItems);
		if (string.IsNullOrEmpty(item.Name))
		{
			AddError(errors, new EdmItemError(Strings.Validator_MemberHasNoName));
		}
		if (item.DeclaringType == null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_MemberHasNullDeclaringType));
		}
		else
		{
			InternalValidate(item.DeclaringType, errors, validatedItems);
		}
		if (item.TypeUsage == null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_MemberHasNullTypeUsage));
		}
		else
		{
			InternalValidate(item.TypeUsage, errors, validatedItems);
		}
	}

	private void ValidateMetadataProperty(MetadataProperty item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		if (item.PropertyKind == PropertyKind.Extended)
		{
			ValidateItem(item, errors, validatedItems);
			if (string.IsNullOrEmpty(item.Name))
			{
				AddError(errors, new EdmItemError(Strings.Validator_MetadataPropertyHasNoName));
			}
			if (item.TypeUsage == null)
			{
				AddError(errors, new EdmItemError(Strings.Validator_ItemAttributeHasNullTypeUsage));
			}
			else
			{
				InternalValidate(item.TypeUsage, errors, validatedItems);
			}
		}
	}

	private void ValidateNavigationProperty(NavigationProperty item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateEdmMember(item, errors, validatedItems);
	}

	private void ValidatePrimitiveType(PrimitiveType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateSimpleType(item, errors, validatedItems);
	}

	private void ValidateEdmProperty(EdmProperty item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateEdmMember(item, errors, validatedItems);
	}

	private void ValidateRefType(RefType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateEdmType(item, errors, validatedItems);
		if (item.BaseType != null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_RefTypesCannotHaveBaseType));
		}
		if (item.ElementType == null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_RefTypeHasNullEntityType));
		}
		else
		{
			InternalValidate(item.ElementType, errors, validatedItems);
		}
	}

	private void ValidateSimpleType(SimpleType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateEdmType(item, errors, validatedItems);
	}

	private void ValidateStructuralType(StructuralType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateEdmType(item, errors, validatedItems);
		Dictionary<string, EdmMember> dictionary = new Dictionary<string, EdmMember>();
		foreach (EdmMember member in item.Members)
		{
			EdmMember value = null;
			if (dictionary.TryGetValue(member.Name, out value))
			{
				AddError(errors, new EdmItemError(Strings.Validator_BaseTypeHasMemberOfSameName));
			}
			else
			{
				dictionary.Add(member.Name, member);
			}
			InternalValidate(member, errors, validatedItems);
		}
	}

	private void ValidateTypeUsage(TypeUsage item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
	{
		ValidateItem(item, errors, validatedItems);
		if (item.EdmType == null)
		{
			AddError(errors, new EdmItemError(Strings.Validator_TypeUsageHasNullEdmType));
		}
		else
		{
			InternalValidate(item.EdmType, errors, validatedItems);
		}
		foreach (Facet facet in item.Facets)
		{
			InternalValidate(facet, errors, validatedItems);
		}
	}
}
