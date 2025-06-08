using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EnumTypeExtensions
{
	public static Type GetClrType(this EnumType enumType)
	{
		return enumType.Annotations.GetClrType();
	}

	public static void SetClrType(this EnumType enumType, Type type)
	{
		enumType.GetMetadataProperties().SetClrType(type);
	}
}
