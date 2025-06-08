using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EdmTypeExtensions
{
	public static Type GetClrType(this EdmType item)
	{
		if (item is EntityType entityType)
		{
			return entityType.GetClrType();
		}
		if (item is EnumType enumType)
		{
			return enumType.GetClrType();
		}
		if (item is ComplexType complexType)
		{
			return complexType.GetClrType();
		}
		return null;
	}
}
