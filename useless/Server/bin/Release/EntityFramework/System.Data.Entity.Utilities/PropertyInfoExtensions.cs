using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Utilities;

internal static class PropertyInfoExtensions
{
	public static bool IsSameAs(this PropertyInfo propertyInfo, PropertyInfo otherPropertyInfo)
	{
		if (!(propertyInfo == otherPropertyInfo))
		{
			if (propertyInfo.Name == otherPropertyInfo.Name)
			{
				if (!(propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType) && !propertyInfo.DeclaringType.IsSubclassOf(otherPropertyInfo.DeclaringType) && !otherPropertyInfo.DeclaringType.IsSubclassOf(propertyInfo.DeclaringType) && !propertyInfo.DeclaringType.GetInterfaces().Contains<Type>(otherPropertyInfo.DeclaringType))
				{
					return otherPropertyInfo.DeclaringType.GetInterfaces().Contains<Type>(propertyInfo.DeclaringType);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool ContainsSame(this IEnumerable<PropertyInfo> enumerable, PropertyInfo propertyInfo)
	{
		return enumerable.Any(propertyInfo.IsSameAs);
	}

	public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.IsValidInterfaceStructuralProperty())
		{
			return !propertyInfo.Getter().IsAbstract;
		}
		return false;
	}

	public static bool IsValidInterfaceStructuralProperty(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.CanRead && (propertyInfo.CanWriteExtended() || propertyInfo.PropertyType.IsCollection()) && propertyInfo.GetIndexParameters().Length == 0)
		{
			return propertyInfo.PropertyType.IsValidStructuralPropertyType();
		}
		return false;
	}

	public static bool IsValidEdmScalarProperty(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.IsValidInterfaceStructuralProperty())
		{
			return propertyInfo.PropertyType.IsValidEdmScalarType();
		}
		return false;
	}

	public static bool IsValidEdmNavigationProperty(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.IsValidInterfaceStructuralProperty())
		{
			if (!propertyInfo.PropertyType.IsCollection(out var elementType) || !elementType.IsValidStructuralType())
			{
				return propertyInfo.PropertyType.IsValidStructuralType();
			}
			return true;
		}
		return false;
	}

	public static EdmProperty AsEdmPrimitiveProperty(this PropertyInfo propertyInfo)
	{
		Type underlyingType = propertyInfo.PropertyType;
		bool nullable = underlyingType.TryUnwrapNullableType(out underlyingType) || !underlyingType.IsValueType();
		if (underlyingType.IsPrimitiveType(out var primitiveType))
		{
			EdmProperty edmProperty = EdmProperty.CreatePrimitive(propertyInfo.Name, primitiveType);
			edmProperty.Nullable = nullable;
			return edmProperty;
		}
		return null;
	}

	public static bool CanWriteExtended(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.CanWrite)
		{
			return true;
		}
		PropertyInfo declaredProperty = GetDeclaredProperty(propertyInfo);
		if (declaredProperty != null)
		{
			return declaredProperty.CanWrite;
		}
		return false;
	}

	public static PropertyInfo GetPropertyInfoForSet(this PropertyInfo propertyInfo)
	{
		PropertyInfo propertyInfo2;
		if (!propertyInfo.CanWrite)
		{
			propertyInfo2 = GetDeclaredProperty(propertyInfo);
			if ((object)propertyInfo2 == null)
			{
				return propertyInfo;
			}
		}
		else
		{
			propertyInfo2 = propertyInfo;
		}
		return propertyInfo2;
	}

	private static PropertyInfo GetDeclaredProperty(PropertyInfo propertyInfo)
	{
		if (!(propertyInfo.DeclaringType == propertyInfo.ReflectedType))
		{
			return propertyInfo.DeclaringType.GetInstanceProperties().SingleOrDefault((PropertyInfo p) => p.Name == propertyInfo.Name && p.DeclaringType == propertyInfo.DeclaringType && !p.GetIndexParameters().Any() && p.PropertyType == propertyInfo.PropertyType);
		}
		return propertyInfo;
	}

	public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this PropertyInfo property)
	{
		List<PropertyInfo> list = new List<PropertyInfo> { property };
		CollectProperties(property, list);
		return list.Distinct();
	}

	private static void CollectProperties(PropertyInfo property, IList<PropertyInfo> collection)
	{
		FindNextProperty(property, collection, getter: true);
		FindNextProperty(property, collection, getter: false);
	}

	private static void FindNextProperty(PropertyInfo property, IList<PropertyInfo> collection, bool getter)
	{
		MethodInfo methodInfo = (getter ? property.Getter() : property.Setter());
		if (!(methodInfo != null))
		{
			return;
		}
		Type type = methodInfo.DeclaringType.BaseType();
		if (type != null && type != typeof(object))
		{
			MethodInfo baseMethod = methodInfo.GetBaseDefinition();
			PropertyInfo propertyInfo = (from p in type.GetInstanceProperties()
				let candidateMethod = getter ? p.Getter() : p.Setter()
				where candidateMethod != null && candidateMethod.GetBaseDefinition() == baseMethod
				select p).FirstOrDefault();
			if (propertyInfo != null)
			{
				collection.Add(propertyInfo);
				CollectProperties(propertyInfo, collection);
			}
		}
	}

	public static MethodInfo Getter(this PropertyInfo property)
	{
		return property.GetMethod;
	}

	public static MethodInfo Setter(this PropertyInfo property)
	{
		return property.SetMethod;
	}

	public static bool IsStatic(this PropertyInfo property)
	{
		return (property.Getter() ?? property.Setter()).IsStatic;
	}

	public static bool IsPublic(this PropertyInfo property)
	{
		MethodInfo methodInfo = property.Getter();
		MethodAttributes methodAttributes = ((methodInfo == null) ? MethodAttributes.Private : (methodInfo.Attributes & MethodAttributes.MemberAccessMask));
		MethodInfo methodInfo2 = property.Setter();
		MethodAttributes methodAttributes2 = ((methodInfo2 == null) ? MethodAttributes.Private : (methodInfo2.Attributes & MethodAttributes.MemberAccessMask));
		return ((methodAttributes > methodAttributes2) ? methodAttributes : methodAttributes2) == MethodAttributes.Public;
	}
}
