using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class ComplexTypeExtensions
{
	public static EdmProperty AddComplexProperty(this ComplexType complexType, string name, ComplexType targetComplexType)
	{
		EdmProperty edmProperty = EdmProperty.CreateComplex(name, targetComplexType);
		complexType.AddMember(edmProperty);
		return edmProperty;
	}

	public static object GetConfiguration(this ComplexType complexType)
	{
		return complexType.Annotations.GetConfiguration();
	}

	public static Type GetClrType(this ComplexType complexType)
	{
		return complexType.Annotations.GetClrType();
	}

	internal static IEnumerable<ComplexType> ToHierarchy(this ComplexType edmType)
	{
		return EdmType.SafeTraverseHierarchy(edmType);
	}
}
