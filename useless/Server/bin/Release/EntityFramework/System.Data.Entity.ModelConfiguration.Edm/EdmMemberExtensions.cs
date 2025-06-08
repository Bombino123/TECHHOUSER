using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EdmMemberExtensions
{
	public static PropertyInfo GetClrPropertyInfo(this EdmMember property)
	{
		return property.Annotations.GetClrPropertyInfo();
	}

	public static void SetClrPropertyInfo(this EdmMember property, PropertyInfo propertyInfo)
	{
		property.GetMetadataProperties().SetClrPropertyInfo(propertyInfo);
	}

	public static IEnumerable<T> GetClrAttributes<T>(this EdmMember property) where T : Attribute
	{
		IList<Attribute> clrAttributes = property.Annotations.GetClrAttributes();
		if (clrAttributes == null)
		{
			return Enumerable.Empty<T>();
		}
		return clrAttributes.OfType<T>();
	}
}
