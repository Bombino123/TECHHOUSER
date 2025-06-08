using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

[DebuggerDisplay("EdmType={EdmType}, Facets.Count={Facets.Count}")]
public class TypeUsage : MetadataItem
{
	private TypeUsage _modelTypeUsage;

	private readonly EdmType _edmType;

	private ReadOnlyMetadataCollection<Facet> _facets;

	private string _identity;

	private static readonly string[] _identityFacets = new string[8] { "DefaultValue", "FixedLength", "MaxLength", "Nullable", "Precision", "Scale", "Unicode", "SRID" };

	internal static readonly EdmConstants.Unbounded DefaultMaxLengthFacetValue = EdmConstants.UnboundedValue;

	internal static readonly EdmConstants.Unbounded DefaultPrecisionFacetValue = EdmConstants.UnboundedValue;

	internal static readonly EdmConstants.Unbounded DefaultScaleFacetValue = EdmConstants.UnboundedValue;

	internal const bool DefaultUnicodeFacetValue = true;

	internal const bool DefaultFixedLengthFacetValue = false;

	internal static readonly byte? DefaultDateTimePrecisionFacetValue = null;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.TypeUsage;

	[MetadataProperty(BuiltInTypeKind.EdmType, false)]
	public virtual EdmType EdmType => _edmType;

	[MetadataProperty(BuiltInTypeKind.Facet, true)]
	public virtual ReadOnlyMetadataCollection<Facet> Facets
	{
		get
		{
			if (_facets == null)
			{
				MetadataCollection<Facet> metadataCollection = new MetadataCollection<Facet>(GetFacets());
				metadataCollection.SetReadOnly();
				Interlocked.CompareExchange(ref _facets, metadataCollection.AsReadOnlyMetadataCollection(), null);
			}
			return _facets;
		}
	}

	public TypeUsage ModelTypeUsage
	{
		get
		{
			if (_modelTypeUsage == null)
			{
				EdmType edmType = EdmType;
				if (edmType.DataSpace == DataSpace.CSpace || edmType.DataSpace == DataSpace.OSpace)
				{
					return this;
				}
				TypeUsage typeUsage;
				if (Helper.IsRowType(edmType))
				{
					RowType rowType = (RowType)edmType;
					EdmProperty[] array = new EdmProperty[rowType.Properties.Count];
					for (int i = 0; i < array.Length; i++)
					{
						EdmProperty edmProperty = rowType.Properties[i];
						TypeUsage modelTypeUsage = edmProperty.TypeUsage.ModelTypeUsage;
						array[i] = new EdmProperty(edmProperty.Name, modelTypeUsage);
					}
					typeUsage = Create(new RowType(array, rowType.InitializerMetadata), Facets);
				}
				else if (Helper.IsCollectionType(edmType))
				{
					typeUsage = Create(new CollectionType(((CollectionType)edmType).TypeUsage.ModelTypeUsage), Facets);
				}
				else if (Helper.IsPrimitiveType(edmType))
				{
					typeUsage = ((PrimitiveType)edmType).ProviderManifest.GetEdmType(this);
					if (typeUsage == null)
					{
						throw new ProviderIncompatibleException(Strings.Mapping_ProviderReturnsNullType(ToString()));
					}
					if (!TypeSemantics.IsNullable(this))
					{
						typeUsage = Create(typeUsage.EdmType, OverrideFacetValues(typeUsage.Facets, new FacetValues
						{
							Nullable = false
						}));
					}
				}
				else
				{
					if (!Helper.IsEntityTypeBase(edmType) && !Helper.IsComplexType(edmType))
					{
						return null;
					}
					typeUsage = this;
				}
				Interlocked.CompareExchange(ref _modelTypeUsage, typeUsage, null);
			}
			return _modelTypeUsage;
		}
	}

	internal override string Identity
	{
		get
		{
			if (Facets.Count == 0)
			{
				return EdmType.Identity;
			}
			if (_identity == null)
			{
				StringBuilder stringBuilder = new StringBuilder(128);
				BuildIdentity(stringBuilder);
				string value = stringBuilder.ToString();
				Interlocked.CompareExchange(ref _identity, value, null);
			}
			return _identity;
		}
	}

	internal TypeUsage()
	{
	}

	private TypeUsage(EdmType edmType)
		: base(MetadataFlags.Readonly)
	{
		Check.NotNull(edmType, "edmType");
		_edmType = edmType;
	}

	private TypeUsage(EdmType edmType, IEnumerable<Facet> facets)
		: this(edmType)
	{
		MetadataCollection<Facet> metadataCollection = MetadataCollection<Facet>.Wrap(facets.ToList());
		metadataCollection.SetReadOnly();
		_facets = metadataCollection.AsReadOnlyMetadataCollection();
	}

	internal static TypeUsage Create(EdmType edmType)
	{
		return new TypeUsage(edmType);
	}

	internal static TypeUsage Create(EdmType edmType, FacetValues values)
	{
		return new TypeUsage(edmType, GetDefaultFacetDescriptionsAndOverrideFacetValues(edmType, values));
	}

	public static TypeUsage Create(EdmType edmType, IEnumerable<Facet> facets)
	{
		return new TypeUsage(edmType, facets);
	}

	internal TypeUsage ShallowCopy(FacetValues facetValues)
	{
		return Create(_edmType, OverrideFacetValues(Facets, facetValues));
	}

	internal TypeUsage ShallowCopy(params Facet[] facetValues)
	{
		return Create(_edmType, OverrideFacetValues(Facets, facetValues));
	}

	private static IEnumerable<Facet> OverrideFacetValues(IEnumerable<Facet> facets, IEnumerable<Facet> facetValues)
	{
		return facets.Except(facetValues, (Facet f1, Facet f2) => f1.EdmEquals(f2)).Union(facetValues);
	}

	public static TypeUsage CreateDefaultTypeUsage(EdmType edmType)
	{
		Check.NotNull(edmType, "edmType");
		return Create(edmType);
	}

	public static TypeUsage CreateStringTypeUsage(PrimitiveType primitiveType, bool isUnicode, bool isFixedLength, int maxLength)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
		{
			throw new ArgumentException(Strings.NotStringTypeForTypeUsage);
		}
		ValidateMaxLength(maxLength);
		return Create(primitiveType, new FacetValues
		{
			MaxLength = maxLength,
			Unicode = isUnicode,
			FixedLength = isFixedLength
		});
	}

	public static TypeUsage CreateStringTypeUsage(PrimitiveType primitiveType, bool isUnicode, bool isFixedLength)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
		{
			throw new ArgumentException(Strings.NotStringTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			MaxLength = DefaultMaxLengthFacetValue,
			Unicode = isUnicode,
			FixedLength = isFixedLength
		});
	}

	public static TypeUsage CreateBinaryTypeUsage(PrimitiveType primitiveType, bool isFixedLength, int maxLength)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != 0)
		{
			throw new ArgumentException(Strings.NotBinaryTypeForTypeUsage);
		}
		ValidateMaxLength(maxLength);
		return Create(primitiveType, new FacetValues
		{
			MaxLength = maxLength,
			FixedLength = isFixedLength
		});
	}

	public static TypeUsage CreateBinaryTypeUsage(PrimitiveType primitiveType, bool isFixedLength)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != 0)
		{
			throw new ArgumentException(Strings.NotBinaryTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			MaxLength = DefaultMaxLengthFacetValue,
			FixedLength = isFixedLength
		});
	}

	public static TypeUsage CreateDateTimeTypeUsage(PrimitiveType primitiveType, byte? precision)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.DateTime)
		{
			throw new ArgumentException(Strings.NotDateTimeTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			Precision = precision
		});
	}

	public static TypeUsage CreateDateTimeOffsetTypeUsage(PrimitiveType primitiveType, byte? precision)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.DateTimeOffset)
		{
			throw new ArgumentException(Strings.NotDateTimeOffsetTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			Precision = precision
		});
	}

	public static TypeUsage CreateTimeTypeUsage(PrimitiveType primitiveType, byte? precision)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Time)
		{
			throw new ArgumentException(Strings.NotTimeTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			Precision = precision
		});
	}

	public static TypeUsage CreateDecimalTypeUsage(PrimitiveType primitiveType, byte precision, byte scale)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
		{
			throw new ArgumentException(Strings.NotDecimalTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			Precision = precision,
			Scale = scale
		});
	}

	public static TypeUsage CreateDecimalTypeUsage(PrimitiveType primitiveType)
	{
		Check.NotNull(primitiveType, "primitiveType");
		if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
		{
			throw new ArgumentException(Strings.NotDecimalTypeForTypeUsage);
		}
		return Create(primitiveType, new FacetValues
		{
			Precision = DefaultPrecisionFacetValue,
			Scale = DefaultScaleFacetValue
		});
	}

	public bool IsSubtypeOf(TypeUsage typeUsage)
	{
		if (EdmType == null || typeUsage == null)
		{
			return false;
		}
		return EdmType.IsSubtypeOf(typeUsage.EdmType);
	}

	private IEnumerable<Facet> GetFacets()
	{
		return from facetDescription in _edmType.GetAssociatedFacetDescriptions()
			select facetDescription.DefaultValueFacet;
	}

	internal override void SetReadOnly()
	{
		base.SetReadOnly();
	}

	private static IEnumerable<Facet> GetDefaultFacetDescriptionsAndOverrideFacetValues(EdmType type, FacetValues values)
	{
		return OverrideFacetValues(type.GetAssociatedFacetDescriptions(), (FacetDescription fd) => fd, (FacetDescription fd) => fd.DefaultValueFacet, values);
	}

	private static IEnumerable<Facet> OverrideFacetValues(IEnumerable<Facet> facets, FacetValues values)
	{
		return OverrideFacetValues(facets, (Facet f) => f.Description, (Facet f) => f, values);
	}

	private static IEnumerable<Facet> OverrideFacetValues<T>(IEnumerable<T> facetThings, Func<T, FacetDescription> getDescription, Func<T, Facet> getFacet, FacetValues values)
	{
		foreach (T facetThing in facetThings)
		{
			FacetDescription facetDescription = getDescription(facetThing);
			if (!facetDescription.IsConstant && values.TryGetFacet(facetDescription, out var facet))
			{
				yield return facet;
			}
			else
			{
				yield return getFacet(facetThing);
			}
		}
	}

	internal override void BuildIdentity(StringBuilder builder)
	{
		if (_identity != null)
		{
			builder.Append(_identity);
			return;
		}
		builder.Append(EdmType.Identity);
		builder.Append("(");
		bool flag = true;
		for (int i = 0; i < Facets.Count; i++)
		{
			Facet facet = Facets[i];
			if (0 <= Array.BinarySearch(_identityFacets, facet.Name, (IComparer<string>?)StringComparer.Ordinal))
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					builder.Append(",");
				}
				builder.Append(facet.Name);
				builder.Append("=");
				builder.Append(facet.Value ?? string.Empty);
			}
		}
		builder.Append(")");
	}

	public override string ToString()
	{
		return EdmType.ToString();
	}

	internal override bool EdmEquals(MetadataItem item)
	{
		if (this == item)
		{
			return true;
		}
		if (item == null || BuiltInTypeKind.TypeUsage != item.BuiltInTypeKind)
		{
			return false;
		}
		TypeUsage typeUsage = (TypeUsage)item;
		if (!EdmType.EdmEquals(typeUsage.EdmType))
		{
			return false;
		}
		if (_facets == null && typeUsage._facets == null)
		{
			return true;
		}
		if (Facets.Count != typeUsage.Facets.Count)
		{
			return false;
		}
		foreach (Facet facet in Facets)
		{
			if (!typeUsage.Facets.TryGetValue(facet.Name, ignoreCase: false, out var item2))
			{
				return false;
			}
			if (!object.Equals(facet.Value, item2.Value))
			{
				return false;
			}
		}
		return true;
	}

	private static void ValidateMaxLength(int maxLength)
	{
		if (maxLength <= 0)
		{
			throw new ArgumentOutOfRangeException("maxLength", Strings.InvalidMaxLengthSize);
		}
	}
}
