using System;
using System.Reflection;

namespace SharpDX;

[AttributeUsage(AttributeTargets.Interface)]
internal class ShadowAttribute : Attribute
{
	private Type type;

	public Type Type => type;

	public ShadowAttribute(Type typeOfTheAssociatedShadow)
	{
		type = typeOfTheAssociatedShadow;
	}

	public static ShadowAttribute Get(Type type)
	{
		return SharpDXTypeExtensions.GetTypeInfo(type).GetCustomAttribute<ShadowAttribute>();
	}
}
