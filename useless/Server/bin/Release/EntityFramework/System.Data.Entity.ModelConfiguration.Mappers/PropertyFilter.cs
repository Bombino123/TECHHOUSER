using System.Collections.Generic;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Mappers;

internal sealed class PropertyFilter
{
	private readonly DbModelBuilderVersion _modelBuilderVersion;

	public bool EdmV3FeaturesSupported => _modelBuilderVersion.GetEdmVersion() >= 3.0;

	public bool Ef6FeaturesSupported
	{
		get
		{
			if (_modelBuilderVersion != 0)
			{
				return _modelBuilderVersion >= DbModelBuilderVersion.V6_0;
			}
			return true;
		}
	}

	public PropertyFilter(DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest)
	{
		_modelBuilderVersion = modelBuilderVersion;
	}

	public IEnumerable<PropertyInfo> GetProperties(Type type, bool declaredOnly, IEnumerable<PropertyInfo> explicitlyMappedProperties = null, IEnumerable<Type> knownTypes = null, bool includePrivate = false)
	{
		explicitlyMappedProperties = explicitlyMappedProperties ?? Enumerable.Empty<PropertyInfo>();
		knownTypes = knownTypes ?? Enumerable.Empty<Type>();
		ValidatePropertiesForModelVersion(type, explicitlyMappedProperties);
		return from p in declaredOnly ? type.GetDeclaredProperties() : type.GetNonHiddenProperties()
			where !p.IsStatic() && p.IsValidStructuralProperty()
			let m = p.Getter()
			where (includePrivate || m.IsPublic || explicitlyMappedProperties.Contains(p) || knownTypes.Contains(p.PropertyType)) && (!declaredOnly || type.BaseType().GetInstanceProperties().All((PropertyInfo bp) => bp.Name != p.Name)) && (EdmV3FeaturesSupported || (!IsEnumType(p.PropertyType) && !IsSpatialType(p.PropertyType) && !IsHierarchyIdType(p.PropertyType))) && (Ef6FeaturesSupported || !p.PropertyType.IsNested)
			select p;
	}

	public void ValidatePropertiesForModelVersion(Type type, IEnumerable<PropertyInfo> explicitlyMappedProperties)
	{
		if (_modelBuilderVersion != 0 && !EdmV3FeaturesSupported)
		{
			PropertyInfo propertyInfo = explicitlyMappedProperties.FirstOrDefault((PropertyInfo p) => IsEnumType(p.PropertyType) || IsSpatialType(p.PropertyType) || IsHierarchyIdType(p.PropertyType));
			if (propertyInfo != null)
			{
				throw Error.UnsupportedUseOfV3Type(type.Name, propertyInfo.Name);
			}
		}
	}

	private static bool IsEnumType(Type type)
	{
		type.TryUnwrapNullableType(out type);
		return type.IsEnum();
	}

	private static bool IsHierarchyIdType(Type type)
	{
		type.TryUnwrapNullableType(out type);
		return type == typeof(HierarchyId);
	}

	private static bool IsSpatialType(Type type)
	{
		type.TryUnwrapNullableType(out type);
		if (!(type == typeof(DbGeometry)))
		{
			return type == typeof(DbGeography);
		}
		return true;
	}
}
