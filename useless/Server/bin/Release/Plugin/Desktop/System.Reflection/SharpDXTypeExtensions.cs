using System.Collections.Generic;

namespace System.Reflection;

internal static class SharpDXTypeExtensions
{
	public static Type GetTypeInfo(this Type type)
	{
		return type;
	}

	public static T GetCustomAttribute<T>(this Type type) where T : Attribute
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(T), inherit: true);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return (T)customAttributes[0];
	}

	public static T GetCustomAttribute<T>(this MemberInfo memberInfo, bool inherited) where T : Attribute
	{
		object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), inherited);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return (T)customAttributes[0];
	}

	public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherited) where T : Attribute
	{
		object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), inherited);
		object[] array = customAttributes;
		foreach (object obj in array)
		{
			yield return (T)obj;
		}
	}
}
