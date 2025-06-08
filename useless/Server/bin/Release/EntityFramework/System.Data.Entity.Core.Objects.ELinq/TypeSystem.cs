using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq;

internal static class TypeSystem
{
	internal static readonly MethodInfo GetDefaultMethod = typeof(TypeSystem).GetOnlyDeclaredMethod("GetDefault");

	private static T GetDefault<T>()
	{
		return default(T);
	}

	internal static object GetDefaultValue(Type type)
	{
		if (!type.IsValueType() || (type.IsGenericType() && typeof(Nullable<>) == type.GetGenericTypeDefinition()))
		{
			return null;
		}
		return GetDefaultMethod.MakeGenericMethod(type).Invoke(null, new object[0]);
	}

	internal static bool IsSequenceType(Type seqType)
	{
		return FindIEnumerable(seqType) != null;
	}

	internal static Type GetDelegateType(IEnumerable<Type> inputTypes, Type returnType)
	{
		inputTypes = inputTypes ?? Enumerable.Empty<Type>();
		int num = inputTypes.Count();
		Type[] array = new Type[num + 1];
		int num2 = 0;
		foreach (Type inputType in inputTypes)
		{
			array[num2++] = inputType;
		}
		array[num2] = returnType;
		return (num switch
		{
			0 => typeof(Func<>), 
			1 => typeof(Func<, >), 
			2 => typeof(Func<, , >), 
			3 => typeof(Func<, , , >), 
			4 => typeof(Func<, , , , >), 
			5 => typeof(Func<, , , , , >), 
			6 => typeof(Func<, , , , , , >), 
			7 => typeof(Func<, , , , , , , >), 
			8 => typeof(Func<, , , , , , , , >), 
			9 => typeof(Func<, , , , , , , , , >), 
			10 => typeof(Func<, , , , , , , , , , >), 
			11 => typeof(Func<, , , , , , , , , , , >), 
			12 => typeof(Func<, , , , , , , , , , , , >), 
			13 => typeof(Func<, , , , , , , , , , , , , >), 
			14 => typeof(Func<, , , , , , , , , , , , , , >), 
			15 => typeof(Func<, , , , , , , , , , , , , , , >), 
			_ => null, 
		}).MakeGenericType(array);
	}

	internal static Expression EnsureType(Expression expression, Type requiredType)
	{
		if (expression.Type != requiredType)
		{
			expression = Expression.Convert(expression, requiredType);
		}
		return expression;
	}

	internal static MemberInfo PropertyOrField(MemberInfo member, out string name, out Type type)
	{
		name = null;
		type = null;
		if (member.MemberType == MemberTypes.Field)
		{
			FieldInfo fieldInfo = (FieldInfo)member;
			name = fieldInfo.Name;
			type = fieldInfo.FieldType;
			return fieldInfo;
		}
		if (member.MemberType == MemberTypes.Property)
		{
			PropertyInfo propertyInfo = (PropertyInfo)member;
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new NotSupportedException(Strings.ELinq_PropertyIndexNotSupported);
			}
			name = propertyInfo.Name;
			type = propertyInfo.PropertyType;
			return propertyInfo;
		}
		if (member.MemberType == MemberTypes.Method)
		{
			MethodInfo methodInfo = (MethodInfo)member;
			if (methodInfo.IsSpecialName)
			{
				foreach (PropertyInfo runtimeProperty in methodInfo.DeclaringType.GetRuntimeProperties())
				{
					if (runtimeProperty.CanRead && runtimeProperty.Getter() == methodInfo)
					{
						return PropertyOrField(runtimeProperty, out name, out type);
					}
				}
			}
		}
		throw new NotSupportedException(Strings.ELinq_NotPropertyOrField(member.Name));
	}

	private static Type FindIEnumerable(Type seqType)
	{
		if (seqType == null || seqType == typeof(string) || seqType == typeof(byte[]))
		{
			return null;
		}
		if (seqType.IsArray)
		{
			return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
		}
		if (seqType.IsGenericType())
		{
			Type[] genericArguments = seqType.GetGenericArguments();
			foreach (Type type in genericArguments)
			{
				Type type2 = typeof(IEnumerable<>).MakeGenericType(type);
				if (type2.IsAssignableFrom(seqType))
				{
					return type2;
				}
			}
		}
		Type[] interfaces = seqType.GetInterfaces();
		if (interfaces != null && interfaces.Length != 0)
		{
			Type[] genericArguments = interfaces;
			for (int i = 0; i < genericArguments.Length; i++)
			{
				Type type3 = FindIEnumerable(genericArguments[i]);
				if (type3 != null)
				{
					return type3;
				}
			}
		}
		if (seqType.BaseType() != null && seqType.BaseType() != typeof(object))
		{
			return FindIEnumerable(seqType.BaseType());
		}
		return null;
	}

	internal static Type GetElementType(Type seqType)
	{
		Type type = FindIEnumerable(seqType);
		if (type == null)
		{
			return seqType;
		}
		return type.GetGenericArguments()[0];
	}

	internal static Type GetNonNullableType(Type type)
	{
		if (type != null)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}
		return null;
	}

	internal static bool IsImplementationOfGenericInterfaceMethod(this MethodInfo test, Type match, out Type[] genericTypeArguments)
	{
		genericTypeArguments = null;
		if (null == test || null == match || !match.IsInterface() || !match.IsGenericTypeDefinition() || null == test.DeclaringType)
		{
			return false;
		}
		if (test.DeclaringType.IsInterface() && test.DeclaringType.IsGenericType() && test.DeclaringType.GetGenericTypeDefinition() == match)
		{
			return true;
		}
		Type[] interfaces = test.DeclaringType.GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (type.IsGenericType() && type.GetGenericTypeDefinition() == match && test.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(test))
			{
				genericTypeArguments = type.GetGenericArguments();
				return true;
			}
		}
		return false;
	}

	internal static bool IsImplementationOf(this PropertyInfo propertyInfo, Type interfaceType)
	{
		PropertyInfo declaredProperty = interfaceType.GetDeclaredProperty(propertyInfo.Name);
		if (null == declaredProperty)
		{
			return false;
		}
		if (propertyInfo.DeclaringType.IsInterface())
		{
			return declaredProperty.Equals(propertyInfo);
		}
		bool result = false;
		MethodInfo value = declaredProperty.Getter();
		InterfaceMapping interfaceMap = propertyInfo.DeclaringType.GetInterfaceMap(interfaceType);
		int num = Array.IndexOf(interfaceMap.InterfaceMethods, value);
		MethodInfo[] targetMethods = interfaceMap.TargetMethods;
		if (num > -1 && num < targetMethods.Length)
		{
			MethodInfo methodInfo = propertyInfo.Getter();
			if (methodInfo != null)
			{
				result = methodInfo.Equals(targetMethods[num]);
			}
		}
		return result;
	}
}
