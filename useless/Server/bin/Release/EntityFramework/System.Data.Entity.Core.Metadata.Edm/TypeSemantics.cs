using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class TypeSemantics
{
	private static ReadOnlyCollection<PrimitiveType>[,] _commonTypeClosure;

	internal static bool IsEqual(TypeUsage type1, TypeUsage type2)
	{
		return CompareTypes(type1, type2, equivalenceOnly: false);
	}

	internal static bool IsStructurallyEqual(TypeUsage fromType, TypeUsage toType)
	{
		return CompareTypes(fromType, toType, equivalenceOnly: true);
	}

	internal static bool IsStructurallyEqualOrPromotableTo(TypeUsage fromType, TypeUsage toType)
	{
		if (!IsStructurallyEqual(fromType, toType))
		{
			return IsPromotableTo(fromType, toType);
		}
		return true;
	}

	internal static bool IsStructurallyEqualOrPromotableTo(EdmType fromType, EdmType toType)
	{
		return IsStructurallyEqualOrPromotableTo(TypeUsage.Create(fromType), TypeUsage.Create(toType));
	}

	internal static bool IsSubTypeOf(TypeUsage subType, TypeUsage superType)
	{
		if (subType.EdmEquals(superType))
		{
			return true;
		}
		if (Helper.IsPrimitiveType(subType.EdmType) && Helper.IsPrimitiveType(superType.EdmType))
		{
			return IsPrimitiveTypeSubTypeOf(subType, superType);
		}
		return subType.IsSubtypeOf(superType);
	}

	internal static bool IsSubTypeOf(EdmType subEdmType, EdmType superEdmType)
	{
		return subEdmType.IsSubtypeOf(superEdmType);
	}

	internal static bool IsPromotableTo(TypeUsage fromType, TypeUsage toType)
	{
		if (toType.EdmType.EdmEquals(fromType.EdmType))
		{
			return true;
		}
		if (Helper.IsPrimitiveType(fromType.EdmType) && Helper.IsPrimitiveType(toType.EdmType))
		{
			return IsPrimitiveTypePromotableTo(fromType, toType);
		}
		if (Helper.IsCollectionType(fromType.EdmType) && Helper.IsCollectionType(toType.EdmType))
		{
			return IsPromotableTo(TypeHelpers.GetElementTypeUsage(fromType), TypeHelpers.GetElementTypeUsage(toType));
		}
		if (Helper.IsEntityTypeBase(fromType.EdmType) && Helper.IsEntityTypeBase(toType.EdmType))
		{
			return fromType.EdmType.IsSubtypeOf(toType.EdmType);
		}
		if (Helper.IsRefType(fromType.EdmType) && Helper.IsRefType(toType.EdmType))
		{
			return IsPromotableTo(TypeHelpers.GetElementTypeUsage(fromType), TypeHelpers.GetElementTypeUsage(toType));
		}
		if (Helper.IsRowType(fromType.EdmType) && Helper.IsRowType(toType.EdmType))
		{
			return IsPromotableTo((RowType)fromType.EdmType, (RowType)toType.EdmType);
		}
		return false;
	}

	internal static IEnumerable<TypeUsage> FlattenType(TypeUsage type)
	{
		Func<TypeUsage, bool> isLeaf = (TypeUsage t) => !Helper.IsTransientType(t.EdmType);
		Func<TypeUsage, IEnumerable<TypeUsage>> getImmediateSubNodes = delegate(TypeUsage t)
		{
			if (Helper.IsCollectionType(t.EdmType) || Helper.IsRefType(t.EdmType))
			{
				return new TypeUsage[1] { TypeHelpers.GetElementTypeUsage(t) };
			}
			return Helper.IsRowType(t.EdmType) ? ((RowType)t.EdmType).Properties.Select((EdmProperty p) => p.TypeUsage) : new TypeUsage[0];
		};
		return Helpers.GetLeafNodes(type, isLeaf, getImmediateSubNodes);
	}

	internal static bool IsCastAllowed(TypeUsage fromType, TypeUsage toType)
	{
		if ((!Helper.IsPrimitiveType(fromType.EdmType) || !Helper.IsPrimitiveType(toType.EdmType)) && (!Helper.IsPrimitiveType(fromType.EdmType) || !Helper.IsEnumType(toType.EdmType)) && (!Helper.IsEnumType(fromType.EdmType) || !Helper.IsPrimitiveType(toType.EdmType)))
		{
			if (Helper.IsEnumType(fromType.EdmType) && Helper.IsEnumType(toType.EdmType))
			{
				return fromType.EdmType.Equals(toType.EdmType);
			}
			return false;
		}
		return true;
	}

	internal static bool TryGetCommonType(TypeUsage type1, TypeUsage type2, out TypeUsage commonType)
	{
		commonType = null;
		if (type1.EdmEquals(type2))
		{
			commonType = ForgetConstraints(type2);
			return true;
		}
		if (Helper.IsPrimitiveType(type1.EdmType) && Helper.IsPrimitiveType(type2.EdmType))
		{
			return TryGetCommonPrimitiveType(type1, type2, out commonType);
		}
		if (TryGetCommonType(type1.EdmType, type2.EdmType, out var commonEdmType))
		{
			commonType = ForgetConstraints(TypeUsage.Create(commonEdmType));
			return true;
		}
		commonType = null;
		return false;
	}

	internal static TypeUsage GetCommonType(TypeUsage type1, TypeUsage type2)
	{
		TypeUsage commonType = null;
		if (TryGetCommonType(type1, type2, out commonType))
		{
			return commonType;
		}
		return null;
	}

	internal static bool IsAggregateFunction(EdmFunction function)
	{
		return function.AggregateAttribute;
	}

	internal static bool IsValidPolymorphicCast(TypeUsage fromType, TypeUsage toType)
	{
		if (!IsPolymorphicType(fromType) || !IsPolymorphicType(toType))
		{
			return false;
		}
		if (!IsStructurallyEqual(fromType, toType) && !IsSubTypeOf(fromType, toType))
		{
			return IsSubTypeOf(toType, fromType);
		}
		return true;
	}

	internal static bool IsValidPolymorphicCast(EdmType fromEdmType, EdmType toEdmType)
	{
		return IsValidPolymorphicCast(TypeUsage.Create(fromEdmType), TypeUsage.Create(toEdmType));
	}

	internal static bool IsNominalType(TypeUsage type)
	{
		if (!IsEntityType(type))
		{
			return IsComplexType(type);
		}
		return true;
	}

	internal static bool IsCollectionType(TypeUsage type)
	{
		return Helper.IsCollectionType(type.EdmType);
	}

	internal static bool IsComplexType(TypeUsage type)
	{
		return BuiltInTypeKind.ComplexType == type.EdmType.BuiltInTypeKind;
	}

	internal static bool IsEntityType(TypeUsage type)
	{
		return Helper.IsEntityType(type.EdmType);
	}

	internal static bool IsRelationshipType(TypeUsage type)
	{
		return BuiltInTypeKind.AssociationType == type.EdmType.BuiltInTypeKind;
	}

	internal static bool IsEnumerationType(TypeUsage type)
	{
		return Helper.IsEnumType(type.EdmType);
	}

	internal static bool IsScalarType(TypeUsage type)
	{
		return IsScalarType(type.EdmType);
	}

	internal static bool IsScalarType(EdmType type)
	{
		if (!Helper.IsPrimitiveType(type))
		{
			return Helper.IsEnumType(type);
		}
		return true;
	}

	internal static bool IsNumericType(TypeUsage type)
	{
		if (!IsIntegerNumericType(type) && !IsFixedPointNumericType(type))
		{
			return IsFloatPointNumericType(type);
		}
		return true;
	}

	internal static bool IsIntegerNumericType(TypeUsage type)
	{
		if (TypeHelpers.TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			if (typeKind == PrimitiveTypeKind.Byte || (uint)(typeKind - 8) <= 3u)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	internal static bool IsFixedPointNumericType(TypeUsage type)
	{
		if (TypeHelpers.TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			return typeKind == PrimitiveTypeKind.Decimal;
		}
		return false;
	}

	internal static bool IsFloatPointNumericType(TypeUsage type)
	{
		if (TypeHelpers.TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			if (typeKind != PrimitiveTypeKind.Double)
			{
				return typeKind == PrimitiveTypeKind.Single;
			}
			return true;
		}
		return false;
	}

	internal static bool IsUnsignedNumericType(TypeUsage type)
	{
		if (TypeHelpers.TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			if (typeKind == PrimitiveTypeKind.Byte)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	internal static bool IsPolymorphicType(TypeUsage type)
	{
		if (!IsEntityType(type))
		{
			return IsComplexType(type);
		}
		return true;
	}

	internal static bool IsBooleanType(TypeUsage type)
	{
		return IsPrimitiveType(type, PrimitiveTypeKind.Boolean);
	}

	internal static bool IsPrimitiveType(TypeUsage type)
	{
		return Helper.IsPrimitiveType(type.EdmType);
	}

	internal static bool IsPrimitiveType(TypeUsage type, PrimitiveTypeKind primitiveTypeKind)
	{
		if (TypeHelpers.TryGetPrimitiveTypeKind(type, out var typeKind))
		{
			return typeKind == primitiveTypeKind;
		}
		return false;
	}

	internal static bool IsRowType(TypeUsage type)
	{
		return Helper.IsRowType(type.EdmType);
	}

	internal static bool IsReferenceType(TypeUsage type)
	{
		return Helper.IsRefType(type.EdmType);
	}

	internal static bool IsSpatialType(TypeUsage type)
	{
		return Helper.IsSpatialType(type);
	}

	internal static bool IsStrongSpatialType(TypeUsage type)
	{
		if (IsPrimitiveType(type))
		{
			return Helper.IsStrongSpatialTypeKind(((PrimitiveType)type.EdmType).PrimitiveTypeKind);
		}
		return false;
	}

	internal static bool IsStructuralType(TypeUsage type)
	{
		return Helper.IsStructuralType(type.EdmType);
	}

	internal static bool IsPartOfKey(EdmMember edmMember)
	{
		if (Helper.IsRelationshipEndMember(edmMember))
		{
			return ((RelationshipType)edmMember.DeclaringType).KeyMembers.Contains(edmMember);
		}
		if (!Helper.IsEdmProperty(edmMember))
		{
			return false;
		}
		if (Helper.IsEntityTypeBase(edmMember.DeclaringType))
		{
			return ((EntityTypeBase)edmMember.DeclaringType).KeyMembers.Contains(edmMember);
		}
		return false;
	}

	internal static bool IsNullable(TypeUsage type)
	{
		if (type.Facets.TryGetValue("Nullable", ignoreCase: false, out var item))
		{
			return (bool)item.Value;
		}
		return true;
	}

	internal static bool IsNullable(EdmMember edmMember)
	{
		return IsNullable(edmMember.TypeUsage);
	}

	internal static bool IsEqualComparable(TypeUsage type)
	{
		return IsEqualComparable(type.EdmType);
	}

	internal static bool IsEqualComparableTo(TypeUsage type1, TypeUsage type2)
	{
		if (IsEqualComparable(type1) && IsEqualComparable(type2))
		{
			return HasCommonType(type1, type2);
		}
		return false;
	}

	internal static bool IsOrderComparable(TypeUsage type)
	{
		return IsOrderComparable(type.EdmType);
	}

	internal static bool IsOrderComparableTo(TypeUsage type1, TypeUsage type2)
	{
		if (IsOrderComparable(type1) && IsOrderComparable(type2))
		{
			return HasCommonType(type1, type2);
		}
		return false;
	}

	internal static TypeUsage ForgetConstraints(TypeUsage type)
	{
		if (Helper.IsPrimitiveType(type.EdmType))
		{
			return EdmProviderManifest.Instance.ForgetScalarConstraints(type);
		}
		return type;
	}

	[Conditional("DEBUG")]
	internal static void AssertTypeInvariant(string message, Func<bool> assertPredicate)
	{
	}

	private static bool IsPrimitiveTypeSubTypeOf(TypeUsage fromType, TypeUsage toType)
	{
		if (!IsSubTypeOf((PrimitiveType)fromType.EdmType, (PrimitiveType)toType.EdmType))
		{
			return false;
		}
		return true;
	}

	private static bool IsSubTypeOf(PrimitiveType subPrimitiveType, PrimitiveType superPrimitiveType)
	{
		if (subPrimitiveType == superPrimitiveType)
		{
			return true;
		}
		if (Helper.AreSameSpatialUnionType(subPrimitiveType, superPrimitiveType))
		{
			return true;
		}
		ReadOnlyCollection<PrimitiveType> promotionTypes = EdmProviderManifest.Instance.GetPromotionTypes(subPrimitiveType);
		return -1 != promotionTypes.IndexOf(superPrimitiveType);
	}

	private static bool IsPromotableTo(RowType fromRowType, RowType toRowType)
	{
		if (fromRowType.Properties.Count != toRowType.Properties.Count)
		{
			return false;
		}
		for (int i = 0; i < fromRowType.Properties.Count; i++)
		{
			if (!IsPromotableTo(fromRowType.Properties[i].TypeUsage, toRowType.Properties[i].TypeUsage))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsPrimitiveTypePromotableTo(TypeUsage fromType, TypeUsage toType)
	{
		if (!IsSubTypeOf((PrimitiveType)fromType.EdmType, (PrimitiveType)toType.EdmType))
		{
			return false;
		}
		return true;
	}

	private static bool TryGetCommonType(EdmType edmType1, EdmType edmType2, out EdmType commonEdmType)
	{
		if (edmType2 == edmType1)
		{
			commonEdmType = edmType1;
			return true;
		}
		if (Helper.IsPrimitiveType(edmType1) && Helper.IsPrimitiveType(edmType2))
		{
			return TryGetCommonType((PrimitiveType)edmType1, (PrimitiveType)edmType2, out commonEdmType);
		}
		if (Helper.IsCollectionType(edmType1) && Helper.IsCollectionType(edmType2))
		{
			return TryGetCommonType((CollectionType)edmType1, (CollectionType)edmType2, out commonEdmType);
		}
		if (Helper.IsEntityTypeBase(edmType1) && Helper.IsEntityTypeBase(edmType2))
		{
			return TryGetCommonBaseType(edmType1, edmType2, out commonEdmType);
		}
		if (Helper.IsRefType(edmType1) && Helper.IsRefType(edmType2))
		{
			return TryGetCommonType((RefType)edmType1, (RefType)edmType2, out commonEdmType);
		}
		if (Helper.IsRowType(edmType1) && Helper.IsRowType(edmType2))
		{
			return TryGetCommonType((RowType)edmType1, (RowType)edmType2, out commonEdmType);
		}
		commonEdmType = null;
		return false;
	}

	private static bool TryGetCommonPrimitiveType(TypeUsage type1, TypeUsage type2, out TypeUsage commonType)
	{
		commonType = null;
		if (IsPromotableTo(type1, type2))
		{
			commonType = ForgetConstraints(type2);
			return true;
		}
		if (IsPromotableTo(type2, type1))
		{
			commonType = ForgetConstraints(type1);
			return true;
		}
		ReadOnlyCollection<PrimitiveType> primitiveCommonSuperTypes = GetPrimitiveCommonSuperTypes((PrimitiveType)type1.EdmType, (PrimitiveType)type2.EdmType);
		if (primitiveCommonSuperTypes.Count == 0)
		{
			return false;
		}
		commonType = TypeUsage.CreateDefaultTypeUsage(primitiveCommonSuperTypes[0]);
		return commonType != null;
	}

	private static bool TryGetCommonType(PrimitiveType primitiveType1, PrimitiveType primitiveType2, out EdmType commonType)
	{
		commonType = null;
		if (IsSubTypeOf(primitiveType1, primitiveType2))
		{
			commonType = primitiveType2;
			return true;
		}
		if (IsSubTypeOf(primitiveType2, primitiveType1))
		{
			commonType = primitiveType1;
			return true;
		}
		ReadOnlyCollection<PrimitiveType> primitiveCommonSuperTypes = GetPrimitiveCommonSuperTypes(primitiveType1, primitiveType2);
		if (primitiveCommonSuperTypes.Count > 0)
		{
			commonType = primitiveCommonSuperTypes[0];
			return true;
		}
		return false;
	}

	private static bool TryGetCommonType(CollectionType collectionType1, CollectionType collectionType2, out EdmType commonType)
	{
		TypeUsage commonType2 = null;
		if (!TryGetCommonType(collectionType1.TypeUsage, collectionType2.TypeUsage, out commonType2))
		{
			commonType = null;
			return false;
		}
		commonType = new CollectionType(commonType2);
		return true;
	}

	private static bool TryGetCommonType(RefType refType1, RefType reftype2, out EdmType commonType)
	{
		if (!TryGetCommonType(refType1.ElementType, reftype2.ElementType, out commonType))
		{
			return false;
		}
		commonType = new RefType((EntityType)commonType);
		return true;
	}

	private static bool TryGetCommonType(RowType rowType1, RowType rowType2, out EdmType commonRowType)
	{
		if (rowType1.Properties.Count != rowType2.Properties.Count || rowType1.InitializerMetadata != rowType2.InitializerMetadata)
		{
			commonRowType = null;
			return false;
		}
		List<EdmProperty> list = new List<EdmProperty>();
		for (int i = 0; i < rowType1.Properties.Count; i++)
		{
			if (!TryGetCommonType(rowType1.Properties[i].TypeUsage, rowType2.Properties[i].TypeUsage, out var commonType))
			{
				commonRowType = null;
				return false;
			}
			list.Add(new EdmProperty(rowType1.Properties[i].Name, commonType));
		}
		commonRowType = new RowType(list, rowType1.InitializerMetadata);
		return true;
	}

	internal static bool TryGetCommonBaseType(EdmType type1, EdmType type2, out EdmType commonBaseType)
	{
		Dictionary<EdmType, byte> dictionary = new Dictionary<EdmType, byte>();
		for (EdmType edmType = type2; edmType != null; edmType = edmType.BaseType)
		{
			dictionary.Add(edmType, 0);
		}
		for (EdmType edmType2 = type1; edmType2 != null; edmType2 = edmType2.BaseType)
		{
			if (dictionary.ContainsKey(edmType2))
			{
				commonBaseType = edmType2;
				return true;
			}
		}
		commonBaseType = null;
		return false;
	}

	private static bool HasCommonType(TypeUsage type1, TypeUsage type2)
	{
		return TypeHelpers.GetCommonTypeUsage(type1, type2) != null;
	}

	private static bool IsEqualComparable(EdmType edmType)
	{
		if (Helper.IsPrimitiveType(edmType) || Helper.IsRefType(edmType) || Helper.IsEntityType(edmType) || Helper.IsEnumType(edmType))
		{
			return true;
		}
		if (Helper.IsRowType(edmType))
		{
			foreach (EdmProperty property in ((RowType)edmType).Properties)
			{
				if (!IsEqualComparable(property.TypeUsage))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private static bool IsOrderComparable(EdmType edmType)
	{
		return Helper.IsScalarType(edmType);
	}

	private static bool CompareTypes(TypeUsage fromType, TypeUsage toType, bool equivalenceOnly)
	{
		if (fromType == toType)
		{
			return true;
		}
		if (fromType.EdmType.BuiltInTypeKind != toType.EdmType.BuiltInTypeKind)
		{
			return false;
		}
		if (fromType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
		{
			return CompareTypes(((CollectionType)fromType.EdmType).TypeUsage, ((CollectionType)toType.EdmType).TypeUsage, equivalenceOnly);
		}
		if (fromType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType)
		{
			return ((RefType)fromType.EdmType).ElementType.EdmEquals(((RefType)toType.EdmType).ElementType);
		}
		if (fromType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
		{
			RowType rowType = (RowType)fromType.EdmType;
			RowType rowType2 = (RowType)toType.EdmType;
			if (rowType.Properties.Count != rowType2.Properties.Count)
			{
				return false;
			}
			for (int i = 0; i < rowType.Properties.Count; i++)
			{
				EdmProperty edmProperty = rowType.Properties[i];
				EdmProperty edmProperty2 = rowType2.Properties[i];
				if (!equivalenceOnly && edmProperty.Name != edmProperty2.Name)
				{
					return false;
				}
				if (!CompareTypes(edmProperty.TypeUsage, edmProperty2.TypeUsage, equivalenceOnly))
				{
					return false;
				}
			}
			return true;
		}
		return fromType.EdmType.EdmEquals(toType.EdmType);
	}

	private static void ComputeCommonTypeClosure()
	{
		if (_commonTypeClosure != null)
		{
			return;
		}
		ReadOnlyCollection<PrimitiveType>[,] array = new ReadOnlyCollection<PrimitiveType>[32, 32];
		for (int i = 0; i < 32; i++)
		{
			array[i, i] = Helper.EmptyPrimitiveTypeReadOnlyCollection;
		}
		ReadOnlyCollection<PrimitiveType> storeTypes = EdmProviderManifest.Instance.GetStoreTypes();
		for (int j = 0; j < 32; j++)
		{
			for (int k = 0; k < j; k++)
			{
				array[j, k] = Intersect(EdmProviderManifest.Instance.GetPromotionTypes(storeTypes[j]), EdmProviderManifest.Instance.GetPromotionTypes(storeTypes[k]));
				array[k, j] = array[j, k];
			}
		}
		Interlocked.CompareExchange(ref _commonTypeClosure, array, null);
	}

	private static ReadOnlyCollection<PrimitiveType> Intersect(IList<PrimitiveType> types1, IList<PrimitiveType> types2)
	{
		List<PrimitiveType> list = new List<PrimitiveType>();
		for (int i = 0; i < types1.Count; i++)
		{
			if (types2.Contains(types1[i]))
			{
				list.Add(types1[i]);
			}
		}
		if (list.Count == 0)
		{
			return Helper.EmptyPrimitiveTypeReadOnlyCollection;
		}
		return new ReadOnlyCollection<PrimitiveType>(list);
	}

	private static ReadOnlyCollection<PrimitiveType> GetPrimitiveCommonSuperTypes(PrimitiveType primitiveType1, PrimitiveType primitiveType2)
	{
		ComputeCommonTypeClosure();
		return _commonTypeClosure[(int)primitiveType1.PrimitiveTypeKind, (int)primitiveType2.PrimitiveTypeKind];
	}
}
