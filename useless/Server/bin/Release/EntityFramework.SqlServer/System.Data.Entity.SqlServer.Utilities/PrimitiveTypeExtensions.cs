using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class PrimitiveTypeExtensions
{
	internal static bool IsSpatialType(this PrimitiveType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		PrimitiveTypeKind primitiveTypeKind = type.PrimitiveTypeKind;
		if ((int)primitiveTypeKind >= 15)
		{
			return (int)primitiveTypeKind <= 30;
		}
		return false;
	}

	internal static bool IsHierarchyIdType(this PrimitiveType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return (int)type.PrimitiveTypeKind == 31;
	}
}
