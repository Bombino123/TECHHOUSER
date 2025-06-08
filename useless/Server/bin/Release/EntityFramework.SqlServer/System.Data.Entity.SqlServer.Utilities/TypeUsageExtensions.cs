using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class TypeUsageExtensions
{
	internal static byte GetPrecision(this TypeUsage type)
	{
		return type.GetFacetValue<byte>("Precision");
	}

	internal static byte GetScale(this TypeUsage type)
	{
		return type.GetFacetValue<byte>("Scale");
	}

	internal static int GetMaxLength(this TypeUsage type)
	{
		return type.GetFacetValue<int>("MaxLength");
	}

	internal static T GetFacetValue<T>(this TypeUsage type, string facetName)
	{
		return (T)type.Facets[facetName].Value;
	}

	internal static bool IsFixedLength(this TypeUsage type)
	{
		Facet val = ((IEnumerable<Facet>)type.Facets).SingleOrDefault((Func<Facet, bool>)((Facet f) => f.Name == "FixedLength"));
		if (val != null && val.Value != null)
		{
			return (bool)val.Value;
		}
		return false;
	}

	internal static bool TryGetPrecision(this TypeUsage type, out byte precision)
	{
		if (!type.IsPrimitiveType((PrimitiveTypeKind)4))
		{
			precision = 0;
			return false;
		}
		return type.TryGetFacetValue<byte>("Precision", out precision);
	}

	internal static bool TryGetScale(this TypeUsage type, out byte scale)
	{
		if (!type.IsPrimitiveType((PrimitiveTypeKind)4))
		{
			scale = 0;
			return false;
		}
		return type.TryGetFacetValue<byte>("Scale", out scale);
	}

	internal static bool TryGetFacetValue<T>(this TypeUsage type, string facetName, out T value)
	{
		value = default(T);
		Facet val = default(Facet);
		if (type.Facets.TryGetValue(facetName, false, ref val) && val.Value is T)
		{
			value = (T)val.Value;
			return true;
		}
		return false;
	}

	internal static bool IsPrimitiveType(this TypeUsage type, PrimitiveTypeKind primitiveTypeKind)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (type.IsPrimitiveType())
		{
			return ((PrimitiveType)type.EdmType).PrimitiveTypeKind == primitiveTypeKind;
		}
		return false;
	}

	internal static bool IsPrimitiveType(this TypeUsage type)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (type != null)
		{
			return (int)((MetadataItem)type.EdmType).BuiltInTypeKind == 26;
		}
		return false;
	}

	internal static bool IsNullable(this TypeUsage type)
	{
		Facet val = ((IEnumerable<Facet>)type.Facets).SingleOrDefault((Func<Facet, bool>)((Facet f) => f.Name == "Nullable"));
		if (val != null && val.Value != null)
		{
			return (bool)val.Value;
		}
		return false;
	}

	internal static PrimitiveTypeKind GetPrimitiveTypeKind(this TypeUsage type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
	}

	internal static bool TryGetIsUnicode(this TypeUsage type, out bool isUnicode)
	{
		if (!type.IsPrimitiveType((PrimitiveTypeKind)12))
		{
			isUnicode = false;
			return false;
		}
		return type.TryGetFacetValue<bool>("Unicode", out isUnicode);
	}

	internal static bool TryGetMaxLength(this TypeUsage type, out int maxLength)
	{
		if (!type.IsPrimitiveType((PrimitiveTypeKind)12) && !type.IsPrimitiveType((PrimitiveTypeKind)0))
		{
			maxLength = 0;
			return false;
		}
		return type.TryGetFacetValue<int>("MaxLength", out maxLength);
	}

	internal static IEnumerable<EdmProperty> GetProperties(this TypeUsage type)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		EdmType edmType = type.EdmType;
		BuiltInTypeKind builtInTypeKind = ((MetadataItem)edmType).BuiltInTypeKind;
		if ((int)builtInTypeKind != 8)
		{
			if ((int)builtInTypeKind != 14)
			{
				if ((int)builtInTypeKind == 36)
				{
					return (IEnumerable<EdmProperty>)((RowType)edmType).Properties;
				}
				return Enumerable.Empty<EdmProperty>();
			}
			return (IEnumerable<EdmProperty>)((EntityType)edmType).Properties;
		}
		return (IEnumerable<EdmProperty>)((ComplexType)edmType).Properties;
	}

	internal static TypeUsage GetElementTypeUsage(this TypeUsage type)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between I4 and Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between I4 and Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		EdmType edmType = type.EdmType;
		if (6 == (int)((MetadataItem)edmType).BuiltInTypeKind)
		{
			return ((CollectionType)edmType).TypeUsage;
		}
		if (31 == (int)((MetadataItem)edmType).BuiltInTypeKind)
		{
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((RefType)edmType).ElementType);
		}
		return null;
	}

	internal static bool MustFacetBeConstant(this TypeUsage type, string facetName)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return ((PrimitiveType)type.EdmType).FacetDescriptions.Single((FacetDescription f) => f.FacetName == facetName).IsConstant;
	}

	internal static bool IsHierarchyIdType(this TypeUsage type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if ((int)((MetadataItem)type.EdmType).BuiltInTypeKind == 26)
		{
			return PrimitiveTypeExtensions.IsHierarchyIdType((PrimitiveType)type.EdmType);
		}
		return false;
	}

	internal static bool IsSpatialType(this TypeUsage type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if ((int)((MetadataItem)type.EdmType).BuiltInTypeKind == 26)
		{
			return PrimitiveTypeExtensions.IsSpatialType((PrimitiveType)type.EdmType);
		}
		return false;
	}

	internal static bool IsSpatialType(this TypeUsage type, out PrimitiveTypeKind spatialType)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected I4, but got Unknown
		if (type.IsSpatialType())
		{
			spatialType = (PrimitiveTypeKind)(int)((PrimitiveType)type.EdmType).PrimitiveTypeKind;
			return true;
		}
		spatialType = (PrimitiveTypeKind)0;
		return false;
	}

	internal static TypeUsage ForceNonUnicode(this TypeUsage typeUsage)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		TypeUsage val = TypeUsage.CreateStringTypeUsage((PrimitiveType)typeUsage.EdmType, false, false);
		return TypeUsage.Create(typeUsage.EdmType, ((IEnumerable<Facet>)typeUsage.Facets).Where((Facet f) => f.Name != "Unicode").Union(((IEnumerable<Facet>)val.Facets).Where((Facet f) => f.Name == "Unicode")));
	}
}
