using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Utilities;

internal static class TypeExtensions
{
	private static readonly Dictionary<Type, PrimitiveType> _primitiveTypesMap;

	static TypeExtensions()
	{
		_primitiveTypesMap = new Dictionary<Type, PrimitiveType>();
		foreach (PrimitiveType edmPrimitiveType in PrimitiveType.GetEdmPrimitiveTypes())
		{
			if (!_primitiveTypesMap.ContainsKey(edmPrimitiveType.ClrEquivalentType))
			{
				_primitiveTypesMap.Add(edmPrimitiveType.ClrEquivalentType, edmPrimitiveType);
			}
		}
	}

	public static bool IsCollection(this Type type)
	{
		return type.IsCollection(out type);
	}

	public static bool IsCollection(this Type type, out Type elementType)
	{
		elementType = type.TryGetElementType(typeof(ICollection<>));
		if (elementType == null || type.IsArray)
		{
			elementType = type;
			return false;
		}
		return true;
	}

	public static IEnumerable<PropertyInfo> GetNonIndexerProperties(this Type type)
	{
		return from p in type.GetRuntimeProperties()
			where p.IsPublic() && !p.GetIndexParameters().Any()
			select p;
	}

	public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
	{
		if (!type.IsGenericTypeDefinition())
		{
			List<Type> list = type.GetGenericTypeImplementations(interfaceOrBaseType).ToList();
			if (list.Count != 1)
			{
				return null;
			}
			return list[0].GetGenericArguments().FirstOrDefault();
		}
		return null;
	}

	public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
	{
		if (!type.IsGenericTypeDefinition())
		{
			IEnumerable<Type> first;
			if (!interfaceOrBaseType.IsInterface())
			{
				first = type.GetBaseTypes();
			}
			else
			{
				IEnumerable<Type> interfaces = type.GetInterfaces();
				first = interfaces;
			}
			return from t in first.Union(new Type[1] { type })
				where t.IsGenericType() && t.GetGenericTypeDefinition() == interfaceOrBaseType
				select t;
		}
		return Enumerable.Empty<Type>();
	}

	public static IEnumerable<Type> GetBaseTypes(this Type type)
	{
		type = type.BaseType();
		while (type != null)
		{
			yield return type;
			type = type.BaseType();
		}
	}

	public static Type GetTargetType(this Type type)
	{
		if (!type.IsCollection(out var elementType))
		{
			return type;
		}
		return elementType;
	}

	public static bool TryUnwrapNullableType(this Type type, out Type underlyingType)
	{
		underlyingType = Nullable.GetUnderlyingType(type) ?? type;
		return underlyingType != type;
	}

	public static bool IsNullable(this Type type)
	{
		if (type.IsValueType())
		{
			return Nullable.GetUnderlyingType(type) != null;
		}
		return true;
	}

	public static bool IsValidStructuralType(this Type type)
	{
		if (!type.IsGenericType() && !type.IsValueType() && !type.IsPrimitive() && !type.IsInterface() && !type.IsArray && !(type == typeof(string)) && !(type == typeof(DbGeography)) && !(type == typeof(DbGeometry)) && !(type == typeof(HierarchyId)))
		{
			return type.IsValidStructuralPropertyType();
		}
		return false;
	}

	public static bool IsValidStructuralPropertyType(this Type type)
	{
		if (!type.IsGenericTypeDefinition() && !type.IsPointer && !(type == typeof(object)) && !typeof(ComplexObject).IsAssignableFrom(type) && !typeof(EntityObject).IsAssignableFrom(type) && !typeof(StructuralObject).IsAssignableFrom(type) && !typeof(EntityKey).IsAssignableFrom(type))
		{
			return !typeof(EntityReference).IsAssignableFrom(type);
		}
		return false;
	}

	public static bool IsPrimitiveType(this Type type, out PrimitiveType primitiveType)
	{
		return _primitiveTypesMap.TryGetValue(type, out primitiveType);
	}

	public static T CreateInstance<T>(this Type type, Func<string, string, string> typeMessageFactory, Func<string, Exception> exceptionFactory = null)
	{
		exceptionFactory = exceptionFactory ?? ((Func<string, Exception>)((string s) => new InvalidOperationException(s)));
		if (!typeof(T).IsAssignableFrom(type))
		{
			throw exceptionFactory(typeMessageFactory(type.ToString(), typeof(T).ToString()));
		}
		return type.CreateInstance<T>(exceptionFactory);
	}

	public static T CreateInstance<T>(this Type type, Func<string, Exception> exceptionFactory = null)
	{
		exceptionFactory = exceptionFactory ?? ((Func<string, Exception>)((string s) => new InvalidOperationException(s)));
		if (type.GetDeclaredConstructor() == null)
		{
			throw exceptionFactory(Strings.CreateInstance_NoParameterlessConstructor(type));
		}
		if (type.IsAbstract())
		{
			throw exceptionFactory(Strings.CreateInstance_AbstractType(type));
		}
		if (type.IsGenericType())
		{
			throw exceptionFactory(Strings.CreateInstance_GenericType(type));
		}
		return (T)Activator.CreateInstance(type, nonPublic: true);
	}

	public static bool IsValidEdmScalarType(this Type type)
	{
		type.TryUnwrapNullableType(out type);
		if (!type.IsPrimitiveType(out var _))
		{
			return type.IsEnum();
		}
		return true;
	}

	public static string NestingNamespace(this Type type)
	{
		if (!type.IsNested)
		{
			return type.Namespace;
		}
		string fullName = type.FullName;
		return fullName.Substring(0, fullName.Length - type.Name.Length - 1).Replace('+', '.');
	}

	public static string FullNameWithNesting(this Type type)
	{
		if (!type.IsNested)
		{
			return type.FullName;
		}
		return type.FullName.Replace('+', '.');
	}

	public static bool OverridesEqualsOrGetHashCode(this Type type)
	{
		while (type != typeof(object))
		{
			if (type.GetDeclaredMethods().Any((MethodInfo m) => (m.Name == "Equals" || m.Name == "GetHashCode") && m.DeclaringType != typeof(object) && m.GetBaseDefinition().DeclaringType == typeof(object)))
			{
				return true;
			}
			type = type.BaseType();
		}
		return false;
	}

	public static bool IsPublic(this Type type)
	{
		TypeInfo typeInfo = type.GetTypeInfo();
		if (!typeInfo.IsPublic)
		{
			if (typeInfo.IsNestedPublic)
			{
				return type.DeclaringType.IsPublic();
			}
			return false;
		}
		return true;
	}

	public static bool IsNotPublic(this Type type)
	{
		return !type.IsPublic();
	}

	public static MethodInfo GetOnlyDeclaredMethod(this Type type, string name)
	{
		return type.GetDeclaredMethods(name).SingleOrDefault();
	}

	public static MethodInfo GetDeclaredMethod(this Type type, string name, params Type[] parameterTypes)
	{
		return type.GetDeclaredMethods(name).SingleOrDefault((MethodInfo m) => (from p in m.GetParameters()
			select p.ParameterType).SequenceEqual(parameterTypes));
	}

	public static MethodInfo GetPublicInstanceMethod(this Type type, string name, params Type[] parameterTypes)
	{
		return type.GetRuntimeMethod(name, (MethodInfo m) => m.IsPublic && !m.IsStatic, parameterTypes);
	}

	public static MethodInfo GetRuntimeMethod(this Type type, string name, Func<MethodInfo, bool> predicate, params Type[][] parameterTypes)
	{
		return parameterTypes.Select((Type[] t) => type.GetRuntimeMethod(name, predicate, t)).FirstOrDefault((MethodInfo m) => m != null);
	}

	private static MethodInfo GetRuntimeMethod(this Type type, string name, Func<MethodInfo, bool> predicate, Type[] parameterTypes)
	{
		MethodInfo[] methods = (from m in type.GetRuntimeMethods()
			where name == m.Name && predicate(m) && (from p in m.GetParameters()
				select p.ParameterType).SequenceEqual(parameterTypes)
			select m).ToArray();
		if (methods.Length == 1)
		{
			return methods[0];
		}
		return methods.SingleOrDefault((MethodInfo m) => !methods.Any((MethodInfo m2) => m2.DeclaringType.IsSubclassOf(m.DeclaringType)));
	}

	public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
	{
		return type.GetTypeInfo().DeclaredMethods;
	}

	public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
	{
		return type.GetTypeInfo().GetDeclaredMethods(name);
	}

	public static PropertyInfo GetDeclaredProperty(this Type type, string name)
	{
		return type.GetTypeInfo().GetDeclaredProperty(name);
	}

	public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
	{
		return type.GetTypeInfo().DeclaredProperties;
	}

	public static IEnumerable<PropertyInfo> GetInstanceProperties(this Type type)
	{
		return from p in type.GetRuntimeProperties()
			where !p.IsStatic()
			select p;
	}

	public static IEnumerable<PropertyInfo> GetNonHiddenProperties(this Type type)
	{
		return from property in type.GetRuntimeProperties()
			group property by property.Name into propertyGroup
			select MostDerived(propertyGroup);
	}

	private static PropertyInfo MostDerived(IEnumerable<PropertyInfo> properties)
	{
		PropertyInfo propertyInfo = null;
		foreach (PropertyInfo property in properties)
		{
			if (propertyInfo == null || (propertyInfo.DeclaringType != null && propertyInfo.DeclaringType.IsAssignableFrom(property.DeclaringType)))
			{
				propertyInfo = property;
			}
		}
		return propertyInfo;
	}

	public static PropertyInfo GetAnyProperty(this Type type, string name)
	{
		List<PropertyInfo> source = (from p in type.GetRuntimeProperties()
			where p.Name == name
			select p).ToList();
		if (source.Count() > 1)
		{
			throw new AmbiguousMatchException();
		}
		return source.SingleOrDefault();
	}

	public static PropertyInfo GetInstanceProperty(this Type type, string name)
	{
		List<PropertyInfo> source = (from p in type.GetRuntimeProperties()
			where p.Name == name && !p.IsStatic()
			select p).ToList();
		if (source.Count() > 1)
		{
			throw new AmbiguousMatchException();
		}
		return source.SingleOrDefault();
	}

	public static PropertyInfo GetStaticProperty(this Type type, string name)
	{
		List<PropertyInfo> source = (from p in type.GetRuntimeProperties()
			where p.Name == name && p.IsStatic()
			select p).ToList();
		if (source.Count() > 1)
		{
			throw new AmbiguousMatchException();
		}
		return source.SingleOrDefault();
	}

	public static PropertyInfo GetTopProperty(this Type type, string name)
	{
		do
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			PropertyInfo declaredProperty = typeInfo.GetDeclaredProperty(name);
			if (declaredProperty != null && !(declaredProperty.GetMethod ?? declaredProperty.SetMethod).IsStatic)
			{
				return declaredProperty;
			}
			type = typeInfo.BaseType;
		}
		while (type != null);
		return null;
	}

	public static Assembly Assembly(this Type type)
	{
		return type.GetTypeInfo().Assembly;
	}

	public static Type BaseType(this Type type)
	{
		return type.GetTypeInfo().BaseType;
	}

	public static bool IsGenericType(this Type type)
	{
		return type.GetTypeInfo().IsGenericType;
	}

	public static bool IsGenericTypeDefinition(this Type type)
	{
		return type.GetTypeInfo().IsGenericTypeDefinition;
	}

	public static TypeAttributes Attributes(this Type type)
	{
		return type.GetTypeInfo().Attributes;
	}

	public static bool IsClass(this Type type)
	{
		return type.GetTypeInfo().IsClass;
	}

	public static bool IsInterface(this Type type)
	{
		return type.GetTypeInfo().IsInterface;
	}

	public static bool IsValueType(this Type type)
	{
		return type.GetTypeInfo().IsValueType;
	}

	public static bool IsAbstract(this Type type)
	{
		return type.GetTypeInfo().IsAbstract;
	}

	public static bool IsSealed(this Type type)
	{
		return type.GetTypeInfo().IsSealed;
	}

	public static bool IsEnum(this Type type)
	{
		return type.GetTypeInfo().IsEnum;
	}

	public static bool IsSerializable(this Type type)
	{
		return type.GetTypeInfo().IsSerializable;
	}

	public static bool IsGenericParameter(this Type type)
	{
		return type.GetTypeInfo().IsGenericParameter;
	}

	public static bool ContainsGenericParameters(this Type type)
	{
		return type.GetTypeInfo().ContainsGenericParameters;
	}

	public static bool IsPrimitive(this Type type)
	{
		return type.GetTypeInfo().IsPrimitive;
	}

	public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
	{
		return type.GetTypeInfo().DeclaredConstructors;
	}

	public static ConstructorInfo GetDeclaredConstructor(this Type type, params Type[] parameterTypes)
	{
		return type.GetDeclaredConstructors().SingleOrDefault((ConstructorInfo c) => !c.IsStatic && (from p in c.GetParameters()
			select p.ParameterType).SequenceEqual(parameterTypes));
	}

	public static ConstructorInfo GetPublicConstructor(this Type type, params Type[] parameterTypes)
	{
		ConstructorInfo declaredConstructor = type.GetDeclaredConstructor(parameterTypes);
		if (!(declaredConstructor != null) || !declaredConstructor.IsPublic)
		{
			return null;
		}
		return declaredConstructor;
	}

	public static ConstructorInfo GetDeclaredConstructor(this Type type, Func<ConstructorInfo, bool> predicate, params Type[][] parameterTypes)
	{
		return parameterTypes.Select((Type[] p) => type.GetDeclaredConstructor(p)).FirstOrDefault((ConstructorInfo c) => c != null && predicate(c));
	}

	public static bool IsSubclassOf(this Type type, Type otherType)
	{
		return type.GetTypeInfo().IsSubclassOf(otherType);
	}
}
