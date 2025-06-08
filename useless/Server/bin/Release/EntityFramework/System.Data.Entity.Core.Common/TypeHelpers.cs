using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Common;

internal static class TypeHelpers
{
	internal static readonly ReadOnlyMetadataCollection<EdmMember> EmptyArrayEdmMember = new ReadOnlyMetadataCollection<EdmMember>(new MetadataCollection<EdmMember>().SetReadOnly());

	internal static readonly FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember> EmptyArrayEdmProperty = new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(EmptyArrayEdmMember, null);

	[Conditional("DEBUG")]
	internal static void AssertEdmType(TypeUsage typeUsage)
	{
		EdmType edmType = typeUsage.EdmType;
		if (TypeSemantics.IsCollectionType(typeUsage))
		{
			return;
		}
		if (TypeSemantics.IsStructuralType(typeUsage) && !Helper.IsComplexType(typeUsage.EdmType) && !Helper.IsEntityType(typeUsage.EdmType))
		{
			foreach (EdmMember declaredStructuralMember in GetDeclaredStructuralMembers(typeUsage))
			{
				_ = declaredStructuralMember;
			}
			return;
		}
		if (TypeSemantics.IsPrimitiveType(typeUsage) && edmType is PrimitiveType { DataSpace: not DataSpace.CSpace })
		{
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "PrimitiveType must be CSpace '{0}'", new object[1] { typeUsage }));
		}
	}

	[Conditional("DEBUG")]
	internal static void AssertEdmType(DbCommandTree commandTree)
	{
		DbQueryCommandTree dbQueryCommandTree = commandTree as DbQueryCommandTree;
	}

	internal static bool IsValidSortOpKeyType(TypeUsage typeUsage)
	{
		if (TypeSemantics.IsRowType(typeUsage))
		{
			foreach (EdmProperty property in ((RowType)typeUsage.EdmType).Properties)
			{
				if (!IsValidSortOpKeyType(property.TypeUsage))
				{
					return false;
				}
			}
			return true;
		}
		return TypeSemantics.IsOrderComparable(typeUsage);
	}

	internal static bool IsValidGroupKeyType(TypeUsage typeUsage)
	{
		return IsSetComparableOpType(typeUsage);
	}

	internal static bool IsValidDistinctOpType(TypeUsage typeUsage)
	{
		return IsSetComparableOpType(typeUsage);
	}

	internal static bool IsSetComparableOpType(TypeUsage typeUsage)
	{
		if (Helper.IsEntityType(typeUsage.EdmType) || Helper.IsPrimitiveType(typeUsage.EdmType) || Helper.IsEnumType(typeUsage.EdmType) || Helper.IsRefType(typeUsage.EdmType))
		{
			return true;
		}
		if (TypeSemantics.IsRowType(typeUsage))
		{
			foreach (EdmProperty property in ((RowType)typeUsage.EdmType).Properties)
			{
				if (!IsSetComparableOpType(property.TypeUsage))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	internal static bool IsValidIsNullOpType(TypeUsage typeUsage)
	{
		if (!TypeSemantics.IsReferenceType(typeUsage) && !TypeSemantics.IsEntityType(typeUsage) && !TypeSemantics.IsScalarType(typeUsage))
		{
			return TypeSemantics.IsRowType(typeUsage);
		}
		return true;
	}

	internal static bool IsValidInOpType(TypeUsage typeUsage)
	{
		if (!TypeSemantics.IsReferenceType(typeUsage) && !TypeSemantics.IsEntityType(typeUsage))
		{
			return TypeSemantics.IsScalarType(typeUsage);
		}
		return true;
	}

	internal static TypeUsage GetCommonTypeUsage(TypeUsage typeUsage1, TypeUsage typeUsage2)
	{
		return TypeSemantics.GetCommonType(typeUsage1, typeUsage2);
	}

	internal static TypeUsage GetCommonTypeUsage(IEnumerable<TypeUsage> types)
	{
		TypeUsage typeUsage = null;
		foreach (TypeUsage type in types)
		{
			if (type == null)
			{
				return null;
			}
			if (typeUsage == null)
			{
				typeUsage = type;
				continue;
			}
			typeUsage = TypeSemantics.GetCommonType(typeUsage, type);
			if (typeUsage != null)
			{
				continue;
			}
			break;
		}
		return typeUsage;
	}

	internal static bool TryGetClosestPromotableType(TypeUsage fromType, out TypeUsage promotableType)
	{
		promotableType = null;
		if (Helper.IsPrimitiveType(fromType.EdmType))
		{
			PrimitiveType primitiveType = (PrimitiveType)fromType.EdmType;
			IList<PrimitiveType> promotionTypes = EdmProviderManifest.Instance.GetPromotionTypes(primitiveType);
			int num = promotionTypes.IndexOf(primitiveType);
			if (-1 != num && num + 1 < promotionTypes.Count)
			{
				promotableType = TypeUsage.Create(promotionTypes[num + 1]);
			}
		}
		return promotableType != null;
	}

	internal static bool TryGetBooleanFacetValue(TypeUsage type, string facetName, out bool boolValue)
	{
		boolValue = false;
		if (type.Facets.TryGetValue(facetName, ignoreCase: false, out var item) && item.Value != null)
		{
			boolValue = (bool)item.Value;
			return true;
		}
		return false;
	}

	internal static bool TryGetByteFacetValue(TypeUsage type, string facetName, out byte byteValue)
	{
		byteValue = 0;
		if (type.Facets.TryGetValue(facetName, ignoreCase: false, out var item) && item.Value != null && !Helper.IsUnboundedFacetValue(item))
		{
			byteValue = (byte)item.Value;
			return true;
		}
		return false;
	}

	internal static bool TryGetIntFacetValue(TypeUsage type, string facetName, out int intValue)
	{
		intValue = 0;
		if (type.Facets.TryGetValue(facetName, ignoreCase: false, out var item) && item.Value != null && !Helper.IsUnboundedFacetValue(item) && !Helper.IsVariableFacetValue(item))
		{
			intValue = (int)item.Value;
			return true;
		}
		return false;
	}

	internal static bool TryGetIsFixedLength(TypeUsage type, out bool isFixedLength)
	{
		if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String) && !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
		{
			isFixedLength = false;
			return false;
		}
		return TryGetBooleanFacetValue(type, "FixedLength", out isFixedLength);
	}

	internal static bool TryGetIsUnicode(TypeUsage type, out bool isUnicode)
	{
		if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String))
		{
			isUnicode = false;
			return false;
		}
		return TryGetBooleanFacetValue(type, "Unicode", out isUnicode);
	}

	internal static bool IsFacetValueConstant(TypeUsage type, string facetName)
	{
		return Helper.GetFacet(((PrimitiveType)type.EdmType).FacetDescriptions, facetName).IsConstant;
	}

	internal static bool TryGetMaxLength(TypeUsage type, out int maxLength)
	{
		if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String) && !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
		{
			maxLength = 0;
			return false;
		}
		return TryGetIntFacetValue(type, "MaxLength", out maxLength);
	}

	internal static bool TryGetPrecision(TypeUsage type, out byte precision)
	{
		if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
		{
			precision = 0;
			return false;
		}
		return TryGetByteFacetValue(type, "Precision", out precision);
	}

	internal static bool TryGetScale(TypeUsage type, out byte scale)
	{
		if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
		{
			scale = 0;
			return false;
		}
		return TryGetByteFacetValue(type, "Scale", out scale);
	}

	internal static bool TryGetPrimitiveTypeKind(TypeUsage type, out PrimitiveTypeKind typeKind)
	{
		if (type != null && type.EdmType != null && type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
		{
			typeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
			return true;
		}
		typeKind = PrimitiveTypeKind.Binary;
		return false;
	}

	internal static CollectionType CreateCollectionType(TypeUsage elementType)
	{
		return new CollectionType(elementType);
	}

	internal static TypeUsage CreateCollectionTypeUsage(TypeUsage elementType)
	{
		return TypeUsage.Create(new CollectionType(elementType));
	}

	internal static RowType CreateRowType(IEnumerable<KeyValuePair<string, TypeUsage>> columns)
	{
		return CreateRowType(columns, null);
	}

	internal static RowType CreateRowType(IEnumerable<KeyValuePair<string, TypeUsage>> columns, InitializerMetadata initializerMetadata)
	{
		List<EdmProperty> list = new List<EdmProperty>();
		foreach (KeyValuePair<string, TypeUsage> column in columns)
		{
			list.Add(new EdmProperty(column.Key, column.Value));
		}
		return new RowType(list, initializerMetadata);
	}

	internal static TypeUsage CreateRowTypeUsage(IEnumerable<KeyValuePair<string, TypeUsage>> columns)
	{
		return TypeUsage.Create(CreateRowType(columns));
	}

	internal static RefType CreateReferenceType(EntityTypeBase entityType)
	{
		return new RefType((EntityType)entityType);
	}

	internal static TypeUsage CreateReferenceTypeUsage(EntityType entityType)
	{
		return TypeUsage.Create(CreateReferenceType(entityType));
	}

	internal static RowType CreateKeyRowType(EntityTypeBase entityType)
	{
		IEnumerable<EdmMember> keyMembers = entityType.KeyMembers;
		if (keyMembers == null)
		{
			throw new ArgumentException(Strings.Cqt_Metadata_EntityTypeNullKeyMembersInvalid, "entityType");
		}
		List<KeyValuePair<string, TypeUsage>> list = new List<KeyValuePair<string, TypeUsage>>();
		foreach (EdmProperty item in keyMembers)
		{
			list.Add(new KeyValuePair<string, TypeUsage>(item.Name, Helper.GetModelTypeUsage(item)));
		}
		if (list.Count < 1)
		{
			throw new ArgumentException(Strings.Cqt_Metadata_EntityTypeEmptyKeyMembersInvalid, "entityType");
		}
		return CreateRowType(list);
	}

	internal static TypeUsage GetPrimitiveTypeUsageForScalar(TypeUsage scalarType)
	{
		if (!TypeSemantics.IsEnumerationType(scalarType))
		{
			return scalarType;
		}
		return CreateEnumUnderlyingTypeUsage(scalarType);
	}

	internal static TypeUsage CreateEnumUnderlyingTypeUsage(TypeUsage enumTypeUsage)
	{
		return TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(enumTypeUsage.EdmType), enumTypeUsage.Facets);
	}

	internal static TypeUsage CreateSpatialUnionTypeUsage(TypeUsage spatialTypeUsage)
	{
		return TypeUsage.Create(Helper.GetSpatialNormalizedPrimitiveType(spatialTypeUsage.EdmType), spatialTypeUsage.Facets);
	}

	internal static IBaseList<EdmMember> GetAllStructuralMembers(TypeUsage type)
	{
		return GetAllStructuralMembers(type.EdmType);
	}

	internal static IBaseList<EdmMember> GetAllStructuralMembers(EdmType edmType)
	{
		return edmType.BuiltInTypeKind switch
		{
			BuiltInTypeKind.AssociationType => (IBaseList<EdmMember>)((AssociationType)edmType).AssociationEndMembers, 
			BuiltInTypeKind.ComplexType => (IBaseList<EdmMember>)((ComplexType)edmType).Properties, 
			BuiltInTypeKind.EntityType => (IBaseList<EdmMember>)((EntityType)edmType).Properties, 
			BuiltInTypeKind.RowType => (IBaseList<EdmMember>)((RowType)edmType).Properties, 
			_ => EmptyArrayEdmProperty, 
		};
	}

	internal static IEnumerable GetDeclaredStructuralMembers(TypeUsage type)
	{
		return GetDeclaredStructuralMembers(type.EdmType);
	}

	internal static IEnumerable GetDeclaredStructuralMembers(EdmType edmType)
	{
		return edmType.BuiltInTypeKind switch
		{
			BuiltInTypeKind.AssociationType => ((AssociationType)edmType).GetDeclaredOnlyMembers<AssociationEndMember>(), 
			BuiltInTypeKind.ComplexType => ((ComplexType)edmType).GetDeclaredOnlyMembers<EdmProperty>(), 
			BuiltInTypeKind.EntityType => ((EntityType)edmType).GetDeclaredOnlyMembers<EdmProperty>(), 
			BuiltInTypeKind.RowType => ((RowType)edmType).GetDeclaredOnlyMembers<EdmProperty>(), 
			_ => EmptyArrayEdmProperty, 
		};
	}

	internal static ReadOnlyMetadataCollection<EdmProperty> GetProperties(TypeUsage typeUsage)
	{
		return GetProperties(typeUsage.EdmType);
	}

	internal static ReadOnlyMetadataCollection<EdmProperty> GetProperties(EdmType edmType)
	{
		return edmType.BuiltInTypeKind switch
		{
			BuiltInTypeKind.ComplexType => ((ComplexType)edmType).Properties, 
			BuiltInTypeKind.EntityType => ((EntityType)edmType).Properties, 
			BuiltInTypeKind.RowType => ((RowType)edmType).Properties, 
			_ => EmptyArrayEdmProperty, 
		};
	}

	internal static TypeUsage GetElementTypeUsage(TypeUsage type)
	{
		if (TypeSemantics.IsCollectionType(type))
		{
			return ((CollectionType)type.EdmType).TypeUsage;
		}
		if (TypeSemantics.IsReferenceType(type))
		{
			return TypeUsage.Create(((RefType)type.EdmType).ElementType);
		}
		return null;
	}

	internal static RowType GetTvfReturnType(EdmFunction tvf)
	{
		if (tvf.ReturnParameter != null && TypeSemantics.IsCollectionType(tvf.ReturnParameter.TypeUsage))
		{
			TypeUsage typeUsage = ((CollectionType)tvf.ReturnParameter.TypeUsage.EdmType).TypeUsage;
			if (TypeSemantics.IsRowType(typeUsage))
			{
				return (RowType)typeUsage.EdmType;
			}
		}
		return null;
	}

	internal static bool TryGetCollectionElementType(TypeUsage type, out TypeUsage elementType)
	{
		if (TryGetEdmType<CollectionType>(type, out var type2))
		{
			elementType = type2.TypeUsage;
			return elementType != null;
		}
		elementType = null;
		return false;
	}

	internal static bool TryGetRefEntityType(TypeUsage type, out EntityType referencedEntityType)
	{
		if (TryGetEdmType<RefType>(type, out var type2) && Helper.IsEntityType(type2.ElementType))
		{
			referencedEntityType = (EntityType)type2.ElementType;
			return true;
		}
		referencedEntityType = null;
		return false;
	}

	internal static TEdmType GetEdmType<TEdmType>(TypeUsage typeUsage) where TEdmType : EdmType
	{
		return (TEdmType)typeUsage.EdmType;
	}

	internal static bool TryGetEdmType<TEdmType>(TypeUsage typeUsage, out TEdmType type) where TEdmType : EdmType
	{
		type = typeUsage.EdmType as TEdmType;
		return type != null;
	}

	internal static TypeUsage GetReadOnlyType(TypeUsage type)
	{
		if (!type.IsReadOnly)
		{
			type.SetReadOnly();
		}
		return type;
	}

	internal static string GetFullName(string qualifier, string name)
	{
		if (!string.IsNullOrEmpty(qualifier))
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2] { qualifier, name });
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { name });
	}

	internal static DbType ConvertClrTypeToDbType(Type clrType)
	{
		switch (Type.GetTypeCode(clrType))
		{
		case TypeCode.Empty:
			throw new ArgumentException(Strings.ADP_InvalidDataType(TypeCode.Empty.ToString()));
		case TypeCode.Object:
			if (clrType == typeof(byte[]))
			{
				return DbType.Binary;
			}
			if (clrType == typeof(char[]))
			{
				return DbType.String;
			}
			if (clrType == typeof(Guid))
			{
				return DbType.Guid;
			}
			if (clrType == typeof(TimeSpan))
			{
				return DbType.Time;
			}
			if (clrType == typeof(DateTimeOffset))
			{
				return DbType.DateTimeOffset;
			}
			return DbType.Object;
		case TypeCode.DBNull:
			return DbType.Object;
		case TypeCode.Boolean:
			return DbType.Boolean;
		case TypeCode.SByte:
			return DbType.SByte;
		case TypeCode.Byte:
			return DbType.Byte;
		case TypeCode.Char:
			return DbType.String;
		case TypeCode.Int16:
			return DbType.Int16;
		case TypeCode.UInt16:
			return DbType.UInt16;
		case TypeCode.Int32:
			return DbType.Int32;
		case TypeCode.UInt32:
			return DbType.UInt32;
		case TypeCode.Int64:
			return DbType.Int64;
		case TypeCode.UInt64:
			return DbType.UInt64;
		case TypeCode.Single:
			return DbType.Single;
		case TypeCode.Double:
			return DbType.Double;
		case TypeCode.Decimal:
			return DbType.Decimal;
		case TypeCode.DateTime:
			return DbType.DateTime;
		case TypeCode.String:
			return DbType.String;
		default:
			throw new ArgumentException(Strings.ADP_UnknownDataTypeCode(((int)Type.GetTypeCode(clrType)).ToString(CultureInfo.InvariantCulture), clrType.FullName));
		}
	}

	internal static bool IsIntegerConstant(TypeUsage valueType, object value, long expectedValue)
	{
		if (!TypeSemantics.IsIntegerNumericType(valueType))
		{
			return false;
		}
		if (value == null)
		{
			return false;
		}
		return ((PrimitiveType)valueType.EdmType).PrimitiveTypeKind switch
		{
			PrimitiveTypeKind.Byte => expectedValue == (byte)value, 
			PrimitiveTypeKind.Int16 => expectedValue == (short)value, 
			PrimitiveTypeKind.Int32 => expectedValue == (int)value, 
			PrimitiveTypeKind.Int64 => expectedValue == (long)value, 
			PrimitiveTypeKind.SByte => expectedValue == (sbyte)value, 
			_ => false, 
		};
	}

	internal static TypeUsage GetLiteralTypeUsage(PrimitiveTypeKind primitiveTypeKind)
	{
		return GetLiteralTypeUsage(primitiveTypeKind, isUnicode: true);
	}

	internal static TypeUsage GetLiteralTypeUsage(PrimitiveTypeKind primitiveTypeKind, bool isUnicode)
	{
		PrimitiveType primitiveType = EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
		if (primitiveTypeKind == PrimitiveTypeKind.String)
		{
			return TypeUsage.Create(primitiveType, new FacetValues
			{
				Unicode = isUnicode,
				MaxLength = TypeUsage.DefaultMaxLengthFacetValue,
				FixedLength = false,
				Nullable = false
			});
		}
		return TypeUsage.Create(primitiveType, new FacetValues
		{
			Nullable = false
		});
	}

	internal static bool IsCanonicalFunction(EdmFunction function)
	{
		if (function.DataSpace == DataSpace.CSpace)
		{
			return function.NamespaceName == "Edm";
		}
		return false;
	}
}
