using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.SQLite.EF6;

internal static class MetadataHelpers
{
	internal static readonly int UnicodeStringMaxMaxLength = int.MaxValue;

	internal static readonly int AsciiStringMaxMaxLength = int.MaxValue;

	internal static readonly int BinaryMaxMaxLength = int.MaxValue;

	public static readonly string MaxLengthFacetName = "MaxLength";

	public static readonly string UnicodeFacetName = "Unicode";

	public static readonly string FixedLengthFacetName = "FixedLength";

	public static readonly string PreserveSecondsFacetName = "PreserveSeconds";

	public static readonly string PrecisionFacetName = "Precision";

	public static readonly string ScaleFacetName = "Scale";

	public static readonly string DefaultValueFacetName = "DefaultValue";

	internal const string NullableFacetName = "Nullable";

	internal static TEdmType GetEdmType<TEdmType>(TypeUsage typeUsage) where TEdmType : EdmType
	{
		return (TEdmType)(object)typeUsage.EdmType;
	}

	internal static TypeUsage GetElementTypeUsage(TypeUsage type)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (IsCollectionType(type))
		{
			return ((CollectionType)type.EdmType).TypeUsage;
		}
		return null;
	}

	internal static IList<EdmProperty> GetProperties(TypeUsage typeUsage)
	{
		return GetProperties(typeUsage.EdmType);
	}

	internal static IList<EdmProperty> GetProperties(EdmType edmType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		BuiltInTypeKind builtInTypeKind = ((MetadataItem)edmType).BuiltInTypeKind;
		if ((int)builtInTypeKind != 8)
		{
			if ((int)builtInTypeKind != 14)
			{
				if ((int)builtInTypeKind == 36)
				{
					return (IList<EdmProperty>)((RowType)edmType).Properties;
				}
				return new List<EdmProperty>();
			}
			return (IList<EdmProperty>)((EntityType)edmType).Properties;
		}
		return (IList<EdmProperty>)((ComplexType)edmType).Properties;
	}

	internal static bool IsCollectionType(TypeUsage typeUsage)
	{
		return IsCollectionType(typeUsage.EdmType);
	}

	internal static bool IsCollectionType(EdmType type)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between I4 and Unknown
		return 6 == (int)((MetadataItem)type).BuiltInTypeKind;
	}

	internal static bool IsPrimitiveType(TypeUsage type)
	{
		return IsPrimitiveType(type.EdmType);
	}

	internal static bool IsPrimitiveType(EdmType type)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between I4 and Unknown
		return 26 == (int)((MetadataItem)type).BuiltInTypeKind;
	}

	internal static bool IsRowType(TypeUsage type)
	{
		return IsRowType(type.EdmType);
	}

	internal static bool IsRowType(EdmType type)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between I4 and Unknown
		return 36 == (int)((MetadataItem)type).BuiltInTypeKind;
	}

	internal static bool TryGetPrimitiveTypeKind(TypeUsage type, out PrimitiveTypeKind typeKind)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected I4, but got Unknown
		if (type != null && type.EdmType != null && (int)((MetadataItem)type.EdmType).BuiltInTypeKind == 26)
		{
			typeKind = (PrimitiveTypeKind)(int)((PrimitiveType)type.EdmType).PrimitiveTypeKind;
			return true;
		}
		typeKind = (PrimitiveTypeKind)0;
		return false;
	}

	internal static PrimitiveTypeKind GetPrimitiveTypeKind(TypeUsage type)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			throw new NotSupportedException("Cannot create parameter of non-primitive type");
		}
		return typeKind;
	}

	internal static T TryGetValueForMetadataProperty<T>(MetadataItem item, string propertyName)
	{
		MetadataProperty val = default(MetadataProperty);
		if (!item.MetadataProperties.TryGetValue(propertyName, true, ref val))
		{
			return default(T);
		}
		return (T)val.Value;
	}

	internal static bool IsPrimitiveType(TypeUsage type, PrimitiveTypeKind primitiveType)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			return typeKind == primitiveType;
		}
		return false;
	}

	internal static DbType GetDbType(PrimitiveTypeKind primitiveType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		return (int)primitiveType switch
		{
			0 => DbType.Binary, 
			1 => DbType.Boolean, 
			2 => DbType.Byte, 
			3 => DbType.DateTime, 
			4 => DbType.Decimal, 
			5 => DbType.Double, 
			7 => DbType.Single, 
			6 => DbType.Guid, 
			9 => DbType.Int16, 
			10 => DbType.Int32, 
			11 => DbType.Int64, 
			8 => DbType.SByte, 
			12 => DbType.String, 
			_ => throw new InvalidOperationException($"Unknown PrimitiveTypeKind {primitiveType}"), 
		};
	}

	internal static T GetFacetValueOrDefault<T>(TypeUsage type, string facetName, T defaultValue)
	{
		Facet val = default(Facet);
		if (type.Facets.TryGetValue(facetName, false, ref val) && val.Value != null && !val.IsUnbounded)
		{
			return (T)val.Value;
		}
		return defaultValue;
	}

	internal static bool IsFacetValueConstant(TypeUsage type, string facetName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetFacet(((PrimitiveType)type.EdmType).FacetDescriptions, facetName).IsConstant;
	}

	private static FacetDescription GetFacet(IEnumerable<FacetDescription> facetCollection, string facetName)
	{
		foreach (FacetDescription item in facetCollection)
		{
			if (item.FacetName == facetName)
			{
				return item;
			}
		}
		return null;
	}

	internal static bool TryGetTypeFacetDescriptionByName(EdmType edmType, string facetName, out FacetDescription facetDescription)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		facetDescription = null;
		if (IsPrimitiveType(edmType))
		{
			foreach (FacetDescription facetDescription2 in ((PrimitiveType)edmType).FacetDescriptions)
			{
				if (facetName.Equals(facetDescription2.FacetName, StringComparison.OrdinalIgnoreCase))
				{
					facetDescription = facetDescription2;
					return true;
				}
			}
		}
		return false;
	}

	internal static bool IsNullable(TypeUsage type)
	{
		Facet val = default(Facet);
		if (type.Facets.TryGetValue("Nullable", false, ref val))
		{
			return (bool)val.Value;
		}
		return false;
	}

	internal static bool TryGetMaxLength(TypeUsage type, out int maxLength)
	{
		if (!IsPrimitiveType(type, (PrimitiveTypeKind)12) && !IsPrimitiveType(type, (PrimitiveTypeKind)0))
		{
			maxLength = 0;
			return false;
		}
		return TryGetIntFacetValue(type, MaxLengthFacetName, out maxLength);
	}

	internal static bool TryGetIntFacetValue(TypeUsage type, string facetName, out int intValue)
	{
		intValue = 0;
		Facet val = default(Facet);
		if (type.Facets.TryGetValue(facetName, false, ref val) && val.Value != null && !val.IsUnbounded)
		{
			intValue = (int)val.Value;
			return true;
		}
		return false;
	}

	internal static bool TryGetIsFixedLength(TypeUsage type, out bool isFixedLength)
	{
		if (!IsPrimitiveType(type, (PrimitiveTypeKind)12) && !IsPrimitiveType(type, (PrimitiveTypeKind)0))
		{
			isFixedLength = false;
			return false;
		}
		return TryGetBooleanFacetValue(type, FixedLengthFacetName, out isFixedLength);
	}

	internal static bool TryGetBooleanFacetValue(TypeUsage type, string facetName, out bool boolValue)
	{
		boolValue = false;
		Facet val = default(Facet);
		if (type.Facets.TryGetValue(facetName, false, ref val) && val.Value != null)
		{
			boolValue = (bool)val.Value;
			return true;
		}
		return false;
	}

	internal static bool TryGetIsUnicode(TypeUsage type, out bool isUnicode)
	{
		if (!IsPrimitiveType(type, (PrimitiveTypeKind)12))
		{
			isUnicode = false;
			return false;
		}
		return TryGetBooleanFacetValue(type, UnicodeFacetName, out isUnicode);
	}

	internal static bool IsCanonicalFunction(EdmFunction function)
	{
		return ((EdmType)function).NamespaceName == "Edm";
	}

	internal static bool IsStoreFunction(EdmFunction function)
	{
		return !IsCanonicalFunction(function);
	}

	internal static ParameterDirection ParameterModeToParameterDirection(ParameterMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		return (int)mode switch
		{
			0 => ParameterDirection.Input, 
			2 => ParameterDirection.InputOutput, 
			1 => ParameterDirection.Output, 
			3 => ParameterDirection.ReturnValue, 
			_ => (ParameterDirection)0, 
		};
	}
}
