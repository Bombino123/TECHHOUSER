using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Mappers;

internal sealed class TypeMapper
{
	private readonly MappingContext _mappingContext;

	private readonly List<Type> _knownTypes = new List<Type>();

	public MappingContext MappingContext => _mappingContext;

	public TypeMapper(MappingContext mappingContext)
	{
		_mappingContext = mappingContext;
		_knownTypes.AddRange(mappingContext.ModelConfiguration.ConfiguredTypes.Select((Type t) => t.Assembly()).Distinct().SelectMany((Assembly a) => from type in a.GetAccessibleTypes()
			where type.IsValidStructuralType()
			select type));
	}

	public EnumType MapEnumType(Type type)
	{
		EnumType enumType = GetExistingEdmType<EnumType>(_mappingContext.Model, type);
		if (enumType == null)
		{
			if (!Enum.GetUnderlyingType(type).IsPrimitiveType(out var primitiveType))
			{
				return null;
			}
			enumType = _mappingContext.Model.AddEnumType(type.Name, _mappingContext.ModelConfiguration.ModelNamespace);
			enumType.IsFlags = type.GetCustomAttributes<FlagsAttribute>(inherit: false).Any();
			enumType.SetClrType(type);
			enumType.UnderlyingType = primitiveType;
			string[] names = Enum.GetNames(type);
			foreach (string text in names)
			{
				enumType.AddMember(new EnumMember(text, Convert.ChangeType(Enum.Parse(type, text), type.GetEnumUnderlyingType(), CultureInfo.InvariantCulture)));
			}
		}
		return enumType;
	}

	public ComplexType MapComplexType(Type type, bool discoverNested = false)
	{
		if (!type.IsValidStructuralType())
		{
			return null;
		}
		_mappingContext.ConventionsConfiguration.ApplyModelConfiguration(type, _mappingContext.ModelConfiguration);
		if (_mappingContext.ModelConfiguration.IsIgnoredType(type) || (!discoverNested && !_mappingContext.ModelConfiguration.IsComplexType(type)))
		{
			return null;
		}
		ComplexType complexType = GetExistingEdmType<ComplexType>(_mappingContext.Model, type);
		if (complexType == null)
		{
			complexType = _mappingContext.Model.AddComplexType(type.Name, _mappingContext.ModelConfiguration.ModelNamespace);
			Func<ComplexTypeConfiguration> complexTypeConfiguration = () => _mappingContext.ModelConfiguration.ComplexType(type);
			_mappingContext.ConventionsConfiguration.ApplyTypeConfiguration(type, complexTypeConfiguration, _mappingContext.ModelConfiguration);
			MapStructuralElements(type, complexType.GetMetadataProperties(), delegate(PropertyMapper m, PropertyInfo p)
			{
				m.Map(p, complexType, complexTypeConfiguration);
			}, complexTypeConfiguration);
		}
		return complexType;
	}

	public EntityType MapEntityType(Type type)
	{
		if (!type.IsValidStructuralType() || _mappingContext.ModelConfiguration.IsIgnoredType(type) || _mappingContext.ModelConfiguration.IsComplexType(type))
		{
			return null;
		}
		EntityType entityType = GetExistingEdmType<EntityType>(_mappingContext.Model, type);
		if (entityType == null)
		{
			_mappingContext.ConventionsConfiguration.ApplyModelConfiguration(type, _mappingContext.ModelConfiguration);
			if (_mappingContext.ModelConfiguration.IsIgnoredType(type) || _mappingContext.ModelConfiguration.IsComplexType(type))
			{
				return null;
			}
			entityType = _mappingContext.Model.AddEntityType(type.Name, _mappingContext.ModelConfiguration.ModelNamespace);
			entityType.Abstract = type.IsAbstract();
			EntityType entityType2 = _mappingContext.Model.GetEntityType(type.BaseType().Name);
			if (entityType2 == null)
			{
				_mappingContext.Model.AddEntitySet(entityType.Name, entityType);
			}
			else if (entityType2 == entityType)
			{
				throw new NotSupportedException(Strings.SimpleNameCollision(type.FullName, type.BaseType().FullName, type.Name));
			}
			entityType.BaseType = entityType2;
			Func<EntityTypeConfiguration> entityTypeConfiguration = () => _mappingContext.ModelConfiguration.Entity(type);
			_mappingContext.ConventionsConfiguration.ApplyTypeConfiguration(type, entityTypeConfiguration, _mappingContext.ModelConfiguration);
			List<PropertyInfo> navigationProperties = new List<PropertyInfo>();
			MapStructuralElements(type, entityType.GetMetadataProperties(), delegate(PropertyMapper m, PropertyInfo p)
			{
				if (!m.MapIfNotNavigationProperty(p, entityType, entityTypeConfiguration))
				{
					navigationProperties.Add(p);
				}
			}, entityTypeConfiguration);
			IEnumerable<PropertyInfo> enumerable = navigationProperties;
			if (_mappingContext.ModelBuilderVersion.IsEF6OrHigher())
			{
				enumerable = enumerable.OrderBy((PropertyInfo p) => p.Name);
			}
			foreach (PropertyInfo item in enumerable)
			{
				new NavigationPropertyMapper(this).Map(item, entityType, entityTypeConfiguration);
			}
			if (entityType.BaseType != null)
			{
				LiftInheritedProperties(type, entityType);
			}
			MapDerivedTypes(type, entityType);
		}
		return entityType;
	}

	private static T GetExistingEdmType<T>(EdmModel model, Type type) where T : EdmType
	{
		EdmType structuralOrEnumType = model.GetStructuralOrEnumType(type.Name);
		if (structuralOrEnumType != null && type != structuralOrEnumType.GetClrType())
		{
			throw new NotSupportedException(Strings.SimpleNameCollision(type.FullName, structuralOrEnumType.GetClrType().FullName, type.Name));
		}
		return structuralOrEnumType as T;
	}

	private void MapStructuralElements<TStructuralTypeConfiguration>(Type type, ICollection<MetadataProperty> annotations, Action<PropertyMapper, PropertyInfo> propertyMappingAction, Func<TStructuralTypeConfiguration> structuralTypeConfiguration) where TStructuralTypeConfiguration : StructuralTypeConfiguration
	{
		annotations.SetClrType(type);
		new AttributeMapper(_mappingContext.AttributeProvider).Map(type, annotations);
		PropertyMapper arg = new PropertyMapper(this);
		List<PropertyInfo> list = new PropertyFilter(_mappingContext.ModelBuilderVersion).GetProperties(type, declaredOnly: false, _mappingContext.ModelConfiguration.GetConfiguredProperties(type), _mappingContext.ModelConfiguration.StructuralTypes).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			PropertyInfo propertyInfo = list[i];
			_mappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, _mappingContext.ModelConfiguration);
			_mappingContext.ConventionsConfiguration.ApplyPropertyTypeConfiguration(propertyInfo, structuralTypeConfiguration, _mappingContext.ModelConfiguration);
			if (!_mappingContext.ModelConfiguration.IsIgnoredProperty(type, propertyInfo))
			{
				propertyMappingAction(arg, propertyInfo);
			}
		}
	}

	private void MapDerivedTypes(Type type, EntityType entityType)
	{
		if (type.IsSealed())
		{
			return;
		}
		if (!_knownTypes.Contains(type))
		{
			_knownTypes.AddRange(from t in type.Assembly().GetAccessibleTypes()
				where t.IsValidStructuralType()
				select t);
		}
		IEnumerable<Type> source = _knownTypes.Where((Type t) => t.BaseType() == type);
		if (_mappingContext.ModelBuilderVersion.IsEF6OrHigher())
		{
			source = source.OrderBy((Type t) => t.FullName);
		}
		List<Type> list = source.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Type type2 = list[i];
			EntityType entityType2 = MapEntityType(type2);
			if (entityType2 != null)
			{
				entityType2.BaseType = entityType;
				LiftDerivedType(type2, entityType2, entityType);
			}
		}
	}

	private void LiftDerivedType(Type derivedType, EntityType derivedEntityType, EntityType entityType)
	{
		_mappingContext.Model.ReplaceEntitySet(derivedEntityType, _mappingContext.Model.GetEntitySet(entityType));
		LiftInheritedProperties(derivedType, derivedEntityType);
	}

	private void LiftInheritedProperties(Type type, EntityType entityType)
	{
		if (_mappingContext.ModelConfiguration.GetStructuralTypeConfiguration(type) is EntityTypeConfiguration entityTypeConfiguration)
		{
			entityTypeConfiguration.ClearKey();
			foreach (PropertyInfo property in type.BaseType().GetInstanceProperties())
			{
				if (!_mappingContext.AttributeProvider.GetAttributes(property).OfType<NotMappedAttribute>().Any() && entityTypeConfiguration.IgnoredProperties.Any((PropertyInfo p) => p.IsSameAs(property)))
				{
					throw Error.CannotIgnoreMappedBaseProperty(property.Name, type, property.DeclaringType);
				}
			}
		}
		List<EdmMember> list = entityType.DeclaredMembers.ToList();
		HashSet<PropertyInfo> hashSet = new HashSet<PropertyInfo>(new PropertyFilter(_mappingContext.ModelBuilderVersion).GetProperties(type, declaredOnly: true, _mappingContext.ModelConfiguration.GetConfiguredProperties(type), _mappingContext.ModelConfiguration.StructuralTypes));
		foreach (EdmMember item in list)
		{
			PropertyInfo clrPropertyInfo = item.GetClrPropertyInfo();
			if (!hashSet.Contains(clrPropertyInfo))
			{
				if (item is NavigationProperty navigationProperty)
				{
					_mappingContext.Model.RemoveAssociationType(navigationProperty.Association);
				}
				entityType.RemoveMember(item);
			}
		}
	}
}
