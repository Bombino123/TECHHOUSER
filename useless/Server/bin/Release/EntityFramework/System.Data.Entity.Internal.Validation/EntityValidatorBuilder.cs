using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Internal.Validation;

internal class EntityValidatorBuilder
{
	private readonly AttributeProvider _attributeProvider;

	public EntityValidatorBuilder(AttributeProvider attributeProvider)
	{
		_attributeProvider = attributeProvider;
	}

	public virtual EntityValidator BuildEntityValidator(InternalEntityEntry entityEntry)
	{
		return BuildTypeValidator(entityEntry.EntityType, entityEntry.EdmEntityType.Properties, entityEntry.EdmEntityType.NavigationProperties, (IEnumerable<PropertyValidator> propertyValidators, IEnumerable<IValidator> typeLevelValidators) => new EntityValidator(propertyValidators, typeLevelValidators));
	}

	protected virtual ComplexTypeValidator BuildComplexTypeValidator(Type clrType, ComplexType complexType)
	{
		return BuildTypeValidator(clrType, complexType.Properties, Enumerable.Empty<NavigationProperty>(), (IEnumerable<PropertyValidator> propertyValidators, IEnumerable<IValidator> typeLevelValidators) => new ComplexTypeValidator(propertyValidators, typeLevelValidators));
	}

	private T BuildTypeValidator<T>(Type clrType, IEnumerable<EdmProperty> edmProperties, IEnumerable<NavigationProperty> navigationProperties, Func<IEnumerable<PropertyValidator>, IEnumerable<IValidator>, T> validatorFactoryFunc) where T : TypeValidator
	{
		IList<PropertyValidator> list = BuildValidatorsForProperties(GetPublicInstanceProperties(clrType), edmProperties, navigationProperties);
		IEnumerable<Attribute> attributes = _attributeProvider.GetAttributes(clrType);
		IList<IValidator> list2 = BuildValidationAttributeValidators(attributes);
		if (typeof(IValidatableObject).IsAssignableFrom(clrType))
		{
			list2.Add(new ValidatableObjectValidator(attributes.OfType<DisplayAttribute>().SingleOrDefault()));
		}
		if (!list.Any() && !list2.Any())
		{
			return null;
		}
		return validatorFactoryFunc(list, list2);
	}

	protected virtual IList<PropertyValidator> BuildValidatorsForProperties(IEnumerable<PropertyInfo> clrProperties, IEnumerable<EdmProperty> edmProperties, IEnumerable<NavigationProperty> navigationProperties)
	{
		List<PropertyValidator> list = new List<PropertyValidator>();
		foreach (PropertyInfo property in clrProperties)
		{
			PropertyValidator propertyValidator = null;
			EdmProperty edmProperty = edmProperties.Where((EdmProperty p) => p.Name == property.Name).SingleOrDefault();
			if (edmProperty != null)
			{
				IEnumerable<ReferentialConstraint> source = from navigationProperty in navigationProperties
					let associationType = navigationProperty.RelationshipType as AssociationType
					where associationType != null
					from constraint in associationType.ReferentialConstraints
					where constraint.ToProperties.Contains(edmProperty)
					select constraint;
				propertyValidator = BuildPropertyValidator(property, edmProperty, !source.Any());
			}
			else
			{
				propertyValidator = BuildPropertyValidator(property);
			}
			if (propertyValidator != null)
			{
				list.Add(propertyValidator);
			}
		}
		return list;
	}

	protected virtual PropertyValidator BuildPropertyValidator(PropertyInfo clrProperty, EdmProperty edmProperty, bool buildFacetValidators)
	{
		List<IValidator> list = new List<IValidator>();
		IEnumerable<Attribute> attributes = _attributeProvider.GetAttributes(clrProperty);
		list.AddRange(BuildValidationAttributeValidators(attributes));
		if (edmProperty.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
		{
			ComplexType complexType = (ComplexType)edmProperty.TypeUsage.EdmType;
			ComplexTypeValidator complexTypeValidator = BuildComplexTypeValidator(clrProperty.PropertyType, complexType);
			if (!list.Any() && complexTypeValidator == null)
			{
				return null;
			}
			return new ComplexPropertyValidator(clrProperty.Name, list, complexTypeValidator);
		}
		if (buildFacetValidators)
		{
			list.AddRange(BuildFacetValidators(clrProperty, edmProperty, attributes));
		}
		if (!list.Any())
		{
			return null;
		}
		return new PropertyValidator(clrProperty.Name, list);
	}

	protected virtual PropertyValidator BuildPropertyValidator(PropertyInfo clrProperty)
	{
		IList<IValidator> list = BuildValidationAttributeValidators(_attributeProvider.GetAttributes(clrProperty));
		if (list.Count <= 0)
		{
			return null;
		}
		return new PropertyValidator(clrProperty.Name, list);
	}

	protected virtual IList<IValidator> BuildValidationAttributeValidators(IEnumerable<Attribute> attributes)
	{
		return ((IEnumerable<IValidator>)(from validationAttribute in attributes
			where validationAttribute is ValidationAttribute
			select new ValidationAttributeValidator((ValidationAttribute)validationAttribute, attributes.OfType<DisplayAttribute>().SingleOrDefault()))).ToList();
	}

	protected virtual IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type type)
	{
		return from p in type.GetInstanceProperties()
			where p.IsPublic() && p.GetIndexParameters().Length == 0 && p.Getter() != null
			select p;
	}

	protected virtual IEnumerable<IValidator> BuildFacetValidators(PropertyInfo clrProperty, EdmMember edmProperty, IEnumerable<Attribute> existingAttributes)
	{
		List<ValidationAttribute> list = new List<ValidationAttribute>();
		edmProperty.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern", ignoreCase: false, out var item);
		bool flag = item != null && item.Value != null;
		edmProperty.TypeUsage.Facets.TryGetValue("Nullable", ignoreCase: false, out var item2);
		if (item2 != null && item2.Value != null && !(bool)item2.Value && !flag && clrProperty.PropertyType.IsNullable() && !existingAttributes.Any((Attribute a) => a is RequiredAttribute))
		{
			list.Add(new RequiredAttribute
			{
				AllowEmptyStrings = true
			});
		}
		edmProperty.TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item3);
		if (item3 != null && item3.Value != null && item3.Value is int && !existingAttributes.Any((Attribute a) => a is MaxLengthAttribute) && !existingAttributes.Any((Attribute a) => a is StringLengthAttribute))
		{
			list.Add(new MaxLengthAttribute((int)item3.Value));
		}
		return list.Select((ValidationAttribute attribute) => new ValidationAttributeValidator(attribute, existingAttributes.OfType<DisplayAttribute>().SingleOrDefault()));
	}
}
