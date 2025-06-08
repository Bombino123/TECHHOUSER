using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Mappers;

internal sealed class PropertyMapper
{
	private readonly TypeMapper _typeMapper;

	public PropertyMapper(TypeMapper typeMapper)
	{
		_typeMapper = typeMapper;
	}

	public void Map(PropertyInfo propertyInfo, ComplexType complexType, Func<ComplexTypeConfiguration> complexTypeConfiguration)
	{
		EdmProperty edmProperty = MapPrimitiveOrComplexOrEnumProperty(propertyInfo, complexTypeConfiguration, discoverComplexTypes: true);
		if (edmProperty != null)
		{
			complexType.AddMember(edmProperty);
		}
	}

	public void Map(PropertyInfo propertyInfo, EntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
	{
		EdmProperty edmProperty = MapPrimitiveOrComplexOrEnumProperty(propertyInfo, entityTypeConfiguration);
		if (edmProperty != null)
		{
			entityType.AddMember(edmProperty);
		}
		else
		{
			new NavigationPropertyMapper(_typeMapper).Map(propertyInfo, entityType, entityTypeConfiguration);
		}
	}

	internal bool MapIfNotNavigationProperty(PropertyInfo propertyInfo, EntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
	{
		EdmProperty edmProperty = MapPrimitiveOrComplexOrEnumProperty(propertyInfo, entityTypeConfiguration);
		if (edmProperty != null)
		{
			entityType.AddMember(edmProperty);
			return true;
		}
		return false;
	}

	private EdmProperty MapPrimitiveOrComplexOrEnumProperty(PropertyInfo propertyInfo, Func<StructuralTypeConfiguration> structuralTypeConfiguration, bool discoverComplexTypes = false)
	{
		EdmProperty edmProperty = propertyInfo.AsEdmPrimitiveProperty();
		if (edmProperty == null)
		{
			Type underlyingType = propertyInfo.PropertyType;
			ComplexType complexType = _typeMapper.MapComplexType(underlyingType, discoverComplexTypes);
			if (complexType != null)
			{
				edmProperty = EdmProperty.CreateComplex(propertyInfo.Name, complexType);
			}
			else
			{
				bool nullable = underlyingType.TryUnwrapNullableType(out underlyingType);
				if (underlyingType.IsEnum())
				{
					EnumType enumType = _typeMapper.MapEnumType(underlyingType);
					if (enumType != null)
					{
						edmProperty = EdmProperty.CreateEnum(propertyInfo.Name, enumType);
						edmProperty.Nullable = nullable;
					}
				}
			}
		}
		if (edmProperty != null)
		{
			edmProperty.SetClrPropertyInfo(propertyInfo);
			new AttributeMapper(_typeMapper.MappingContext.AttributeProvider).Map(propertyInfo, edmProperty.GetMetadataProperties());
			if (!edmProperty.IsComplexType)
			{
				_typeMapper.MappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, () => structuralTypeConfiguration().Property(new PropertyPath(propertyInfo)), _typeMapper.MappingContext.ModelConfiguration);
			}
		}
		return edmProperty;
	}
}
