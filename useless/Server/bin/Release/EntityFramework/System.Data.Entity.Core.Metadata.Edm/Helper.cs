using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class Helper
{
	internal static readonly EdmMember[] EmptyArrayEdmProperty = new EdmMember[0];

	private static readonly Dictionary<PrimitiveTypeKind, long[]> _enumUnderlyingTypeRanges = new Dictionary<PrimitiveTypeKind, long[]>
	{
		{
			PrimitiveTypeKind.Byte,
			new long[2] { 0L, 255L }
		},
		{
			PrimitiveTypeKind.SByte,
			new long[2] { -128L, 127L }
		},
		{
			PrimitiveTypeKind.Int16,
			new long[2] { -32768L, 32767L }
		},
		{
			PrimitiveTypeKind.Int32,
			new long[2] { -2147483648L, 2147483647L }
		},
		{
			PrimitiveTypeKind.Int64,
			new long[2] { -9223372036854775808L, 9223372036854775807L }
		}
	};

	internal static readonly ReadOnlyCollection<KeyValuePair<string, object>> EmptyKeyValueStringObjectList = new ReadOnlyCollection<KeyValuePair<string, object>>(new KeyValuePair<string, object>[0]);

	internal static readonly ReadOnlyCollection<string> EmptyStringList = new ReadOnlyCollection<string>(new string[0]);

	internal static readonly ReadOnlyCollection<FacetDescription> EmptyFacetDescriptionEnumerable = new ReadOnlyCollection<FacetDescription>(new FacetDescription[0]);

	internal static readonly ReadOnlyCollection<EdmFunction> EmptyEdmFunctionReadOnlyCollection = new ReadOnlyCollection<EdmFunction>(new EdmFunction[0]);

	internal static readonly ReadOnlyCollection<PrimitiveType> EmptyPrimitiveTypeReadOnlyCollection = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[0]);

	internal static readonly KeyValuePair<string, object>[] EmptyKeyValueStringObjectArray = new KeyValuePair<string, object>[0];

	internal const char PeriodSymbol = '.';

	internal const char CommaSymbol = ',';

	internal static string GetAttributeValue(XPathNavigator nav, string attributeName)
	{
		nav = nav.Clone();
		string result = null;
		if (nav.MoveToAttribute(attributeName, string.Empty))
		{
			result = nav.Value;
		}
		return result;
	}

	internal static object GetTypedAttributeValue(XPathNavigator nav, string attributeName, Type clrType)
	{
		nav = nav.Clone();
		object result = null;
		if (nav.MoveToAttribute(attributeName, string.Empty))
		{
			result = nav.ValueAs(clrType);
		}
		return result;
	}

	internal static FacetDescription GetFacet(IEnumerable<FacetDescription> facetCollection, string facetName)
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

	internal static bool IsAssignableFrom(EdmType firstType, EdmType secondType)
	{
		if (secondType == null)
		{
			return false;
		}
		if (!firstType.Equals(secondType))
		{
			return IsSubtypeOf(secondType, firstType);
		}
		return true;
	}

	internal static bool IsSubtypeOf(EdmType firstType, EdmType secondType)
	{
		if (secondType == null)
		{
			return false;
		}
		for (EdmType baseType = firstType.BaseType; baseType != null; baseType = baseType.BaseType)
		{
			if (baseType == secondType)
			{
				return true;
			}
		}
		return false;
	}

	internal static IList GetAllStructuralMembers(EdmType edmType)
	{
		return edmType.BuiltInTypeKind switch
		{
			BuiltInTypeKind.AssociationType => ((AssociationType)edmType).AssociationEndMembers, 
			BuiltInTypeKind.ComplexType => ((ComplexType)edmType).Properties, 
			BuiltInTypeKind.EntityType => ((EntityType)edmType).Properties, 
			BuiltInTypeKind.RowType => ((RowType)edmType).Properties, 
			_ => EmptyArrayEdmProperty, 
		};
	}

	internal static AssociationEndMember GetEndThatShouldBeMappedToKey(AssociationType associationType)
	{
		if (associationType.AssociationEndMembers.Any((AssociationEndMember it) => it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One)))
		{
			return associationType.AssociationEndMembers.SingleOrDefault((AssociationEndMember it) => it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.Many) || it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.ZeroOrOne));
		}
		if (associationType.AssociationEndMembers.Any((AssociationEndMember it) => it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.ZeroOrOne)))
		{
			return associationType.AssociationEndMembers.SingleOrDefault((AssociationEndMember it) => it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.Many));
		}
		return null;
	}

	internal static string GetCommaDelimitedString(IEnumerable<string> stringList)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (string @string in stringList)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = false;
			}
			stringBuilder.Append(@string);
		}
		return stringBuilder.ToString();
	}

	internal static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)
	{
		foreach (IEnumerable<T> enumerable in sources)
		{
			if (enumerable == null)
			{
				continue;
			}
			foreach (T item in enumerable)
			{
				yield return item;
			}
		}
	}

	internal static void DisposeXmlReaders(IEnumerable<XmlReader> xmlReaders)
	{
		foreach (XmlReader xmlReader in xmlReaders)
		{
			((IDisposable)xmlReader).Dispose();
		}
	}

	internal static bool IsStructuralType(EdmType type)
	{
		if (!IsComplexType(type) && !IsEntityType(type) && !IsRelationshipType(type))
		{
			return IsRowType(type);
		}
		return true;
	}

	internal static bool IsCollectionType(GlobalItem item)
	{
		return BuiltInTypeKind.CollectionType == item.BuiltInTypeKind;
	}

	internal static bool IsEntityType(EdmType type)
	{
		return BuiltInTypeKind.EntityType == type.BuiltInTypeKind;
	}

	internal static bool IsComplexType(EdmType type)
	{
		return BuiltInTypeKind.ComplexType == type.BuiltInTypeKind;
	}

	internal static bool IsPrimitiveType(EdmType type)
	{
		return BuiltInTypeKind.PrimitiveType == type.BuiltInTypeKind;
	}

	internal static bool IsRefType(GlobalItem item)
	{
		return BuiltInTypeKind.RefType == item.BuiltInTypeKind;
	}

	internal static bool IsRowType(GlobalItem item)
	{
		return BuiltInTypeKind.RowType == item.BuiltInTypeKind;
	}

	internal static bool IsAssociationType(EdmType type)
	{
		return BuiltInTypeKind.AssociationType == type.BuiltInTypeKind;
	}

	internal static bool IsRelationshipType(EdmType type)
	{
		return BuiltInTypeKind.AssociationType == type.BuiltInTypeKind;
	}

	internal static bool IsEdmProperty(EdmMember member)
	{
		return BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind;
	}

	internal static bool IsRelationshipEndMember(EdmMember member)
	{
		return member.BuiltInTypeKind == BuiltInTypeKind.AssociationEndMember;
	}

	internal static bool IsAssociationEndMember(EdmMember member)
	{
		return member.BuiltInTypeKind == BuiltInTypeKind.AssociationEndMember;
	}

	internal static bool IsNavigationProperty(EdmMember member)
	{
		return BuiltInTypeKind.NavigationProperty == member.BuiltInTypeKind;
	}

	internal static bool IsEntityTypeBase(EdmType edmType)
	{
		if (!IsEntityType(edmType))
		{
			return IsRelationshipType(edmType);
		}
		return true;
	}

	internal static bool IsTransientType(EdmType edmType)
	{
		if (!IsCollectionType(edmType) && !IsRefType(edmType))
		{
			return IsRowType(edmType);
		}
		return true;
	}

	internal static bool IsAssociationSet(EntitySetBase entitySetBase)
	{
		return BuiltInTypeKind.AssociationSet == entitySetBase.BuiltInTypeKind;
	}

	internal static bool IsEntitySet(EntitySetBase entitySetBase)
	{
		return BuiltInTypeKind.EntitySet == entitySetBase.BuiltInTypeKind;
	}

	internal static bool IsRelationshipSet(EntitySetBase entitySetBase)
	{
		return BuiltInTypeKind.AssociationSet == entitySetBase.BuiltInTypeKind;
	}

	internal static bool IsEntityContainer(GlobalItem item)
	{
		return BuiltInTypeKind.EntityContainer == item.BuiltInTypeKind;
	}

	internal static bool IsEdmFunction(GlobalItem item)
	{
		return BuiltInTypeKind.EdmFunction == item.BuiltInTypeKind;
	}

	internal static string GetFileNameFromUri(Uri uri)
	{
		Check.NotNull(uri, "uri");
		if (uri.IsFile)
		{
			return uri.LocalPath;
		}
		if (uri.IsAbsoluteUri)
		{
			return uri.AbsolutePath;
		}
		throw new ArgumentException(Strings.UnacceptableUri(uri), "uri");
	}

	internal static bool IsEnumType(EdmType edmType)
	{
		return BuiltInTypeKind.EnumType == edmType.BuiltInTypeKind;
	}

	internal static bool IsUnboundedFacetValue(Facet facet)
	{
		return facet.Value == EdmConstants.UnboundedValue;
	}

	internal static bool IsVariableFacetValue(Facet facet)
	{
		return facet.Value == EdmConstants.VariableValue;
	}

	internal static bool IsScalarType(EdmType edmType)
	{
		if (!IsEnumType(edmType))
		{
			return IsPrimitiveType(edmType);
		}
		return true;
	}

	internal static bool IsHierarchyIdType(PrimitiveType type)
	{
		return type.PrimitiveTypeKind == PrimitiveTypeKind.HierarchyId;
	}

	internal static bool IsSpatialType(PrimitiveType type)
	{
		if (!IsGeographicType(type))
		{
			return IsGeometricType(type);
		}
		return true;
	}

	internal static bool IsSpatialType(EdmType type, out bool isGeographic)
	{
		if (!(type is PrimitiveType type2))
		{
			isGeographic = false;
			return false;
		}
		isGeographic = IsGeographicType(type2);
		if (!isGeographic)
		{
			return IsGeometricType(type2);
		}
		return true;
	}

	internal static bool IsGeographicType(PrimitiveType type)
	{
		return IsGeographicTypeKind(type.PrimitiveTypeKind);
	}

	internal static bool AreSameSpatialUnionType(PrimitiveType firstType, PrimitiveType secondType)
	{
		if (IsGeographicTypeKind(firstType.PrimitiveTypeKind) && IsGeographicTypeKind(secondType.PrimitiveTypeKind))
		{
			return true;
		}
		if (IsGeometricTypeKind(firstType.PrimitiveTypeKind) && IsGeometricTypeKind(secondType.PrimitiveTypeKind))
		{
			return true;
		}
		return false;
	}

	internal static bool IsGeographicTypeKind(PrimitiveTypeKind kind)
	{
		if (kind != PrimitiveTypeKind.Geography)
		{
			return IsStrongGeographicTypeKind(kind);
		}
		return true;
	}

	internal static bool IsGeometricType(PrimitiveType type)
	{
		return IsGeometricTypeKind(type.PrimitiveTypeKind);
	}

	internal static bool IsGeometricTypeKind(PrimitiveTypeKind kind)
	{
		if (kind != PrimitiveTypeKind.Geometry)
		{
			return IsStrongGeometricTypeKind(kind);
		}
		return true;
	}

	internal static bool IsStrongSpatialTypeKind(PrimitiveTypeKind kind)
	{
		if (!IsStrongGeometricTypeKind(kind))
		{
			return IsStrongGeographicTypeKind(kind);
		}
		return true;
	}

	private static bool IsStrongGeometricTypeKind(PrimitiveTypeKind kind)
	{
		if (kind >= PrimitiveTypeKind.GeometryPoint)
		{
			return kind <= PrimitiveTypeKind.GeometryCollection;
		}
		return false;
	}

	private static bool IsStrongGeographicTypeKind(PrimitiveTypeKind kind)
	{
		if (kind >= PrimitiveTypeKind.GeographyPoint)
		{
			return kind <= PrimitiveTypeKind.GeographyCollection;
		}
		return false;
	}

	internal static bool IsHierarchyIdType(TypeUsage type)
	{
		if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
		{
			return ((PrimitiveType)type.EdmType).PrimitiveTypeKind == PrimitiveTypeKind.HierarchyId;
		}
		return false;
	}

	internal static bool IsSpatialType(TypeUsage type)
	{
		if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
		{
			return IsSpatialType((PrimitiveType)type.EdmType);
		}
		return false;
	}

	internal static bool IsSpatialType(TypeUsage type, out PrimitiveTypeKind spatialType)
	{
		if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
		{
			PrimitiveType primitiveType = (PrimitiveType)type.EdmType;
			if (IsGeographicTypeKind(primitiveType.PrimitiveTypeKind) || IsGeometricTypeKind(primitiveType.PrimitiveTypeKind))
			{
				spatialType = primitiveType.PrimitiveTypeKind;
				return true;
			}
		}
		spatialType = PrimitiveTypeKind.Binary;
		return false;
	}

	internal static string ToString(ParameterDirection value)
	{
		return value switch
		{
			ParameterDirection.Input => "Input", 
			ParameterDirection.Output => "Output", 
			ParameterDirection.InputOutput => "InputOutput", 
			ParameterDirection.ReturnValue => "ReturnValue", 
			_ => value.ToString(), 
		};
	}

	internal static string ToString(ParameterMode value)
	{
		return value switch
		{
			ParameterMode.In => "In", 
			ParameterMode.Out => "Out", 
			ParameterMode.InOut => "InOut", 
			ParameterMode.ReturnValue => "ReturnValue", 
			_ => value.ToString(), 
		};
	}

	internal static bool IsSupportedEnumUnderlyingType(PrimitiveTypeKind typeKind)
	{
		if (typeKind != PrimitiveTypeKind.Byte && typeKind != PrimitiveTypeKind.SByte && typeKind != PrimitiveTypeKind.Int16 && typeKind != PrimitiveTypeKind.Int32)
		{
			return typeKind == PrimitiveTypeKind.Int64;
		}
		return true;
	}

	internal static bool IsEnumMemberValueInRange(PrimitiveTypeKind underlyingTypeKind, long value)
	{
		if (value >= _enumUnderlyingTypeRanges[underlyingTypeKind][0])
		{
			return value <= _enumUnderlyingTypeRanges[underlyingTypeKind][1];
		}
		return false;
	}

	internal static PrimitiveType AsPrimitive(EdmType type)
	{
		if (!IsEnumType(type))
		{
			return (PrimitiveType)type;
		}
		return GetUnderlyingEdmTypeForEnumType(type);
	}

	internal static PrimitiveType GetUnderlyingEdmTypeForEnumType(EdmType type)
	{
		return ((EnumType)type).UnderlyingType;
	}

	internal static PrimitiveType GetSpatialNormalizedPrimitiveType(EdmType type)
	{
		PrimitiveType primitiveType = (PrimitiveType)type;
		if (IsGeographicType(primitiveType) && primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Geography)
		{
			return PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geography);
		}
		if (IsGeometricType(primitiveType) && primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Geometry)
		{
			return PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geometry);
		}
		return primitiveType;
	}

	internal static string CombineErrorMessage(IEnumerable<EdmSchemaError> errors)
	{
		StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
		int num = 0;
		foreach (EdmSchemaError error in errors)
		{
			if (num++ != 0)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(error);
		}
		return stringBuilder.ToString();
	}

	internal static string CombineErrorMessage(IEnumerable<EdmItemError> errors)
	{
		StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
		int num = 0;
		foreach (EdmItemError error in errors)
		{
			if (num++ != 0)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(error.Message);
		}
		return stringBuilder.ToString();
	}

	internal static IEnumerable<KeyValuePair<T, S>> PairEnumerations<T, S>(IBaseList<T> left, IEnumerable<S> right)
	{
		IEnumerator leftEnumerator = left.GetEnumerator();
		IEnumerator<S> rightEnumerator = right.GetEnumerator();
		while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
		{
			yield return new KeyValuePair<T, S>((T)leftEnumerator.Current, rightEnumerator.Current);
		}
	}

	internal static TypeUsage GetModelTypeUsage(TypeUsage typeUsage)
	{
		return typeUsage.ModelTypeUsage;
	}

	internal static TypeUsage GetModelTypeUsage(EdmMember member)
	{
		return GetModelTypeUsage(member.TypeUsage);
	}

	internal static TypeUsage ValidateAndConvertTypeUsage(EdmProperty edmProperty, EdmProperty columnProperty)
	{
		return ValidateAndConvertTypeUsage(edmProperty.TypeUsage, columnProperty.TypeUsage);
	}

	internal static TypeUsage ValidateAndConvertTypeUsage(TypeUsage cspaceType, TypeUsage sspaceType)
	{
		TypeUsage typeUsage = sspaceType;
		if (sspaceType.EdmType.DataSpace == DataSpace.SSpace)
		{
			typeUsage = sspaceType.ModelTypeUsage;
		}
		if (ValidateScalarTypesAreCompatible(cspaceType, typeUsage))
		{
			return typeUsage;
		}
		return null;
	}

	private static bool ValidateScalarTypesAreCompatible(TypeUsage cspaceType, TypeUsage storeType)
	{
		if (IsEnumType(cspaceType.EdmType))
		{
			return TypeSemantics.IsSubTypeOf(TypeUsage.Create(GetUnderlyingEdmTypeForEnumType(cspaceType.EdmType)), storeType);
		}
		return TypeSemantics.IsSubTypeOf(cspaceType, storeType);
	}
}
