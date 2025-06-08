using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EdmProperty : EdmMember
{
	private readonly PropertyInfo _propertyInfo;

	private readonly Type _entityDeclaringType;

	private Func<object, object> _memberGetter;

	private Action<object, object> _memberSetter;

	internal PropertyInfo PropertyInfo => _propertyInfo;

	internal Type EntityDeclaringType => _entityDeclaringType;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EdmProperty;

	public bool Nullable
	{
		get
		{
			return (bool)TypeUsage.Facets["Nullable"].Value;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			TypeUsage = TypeUsage.ShallowCopy(new FacetValues
			{
				Nullable = value
			});
		}
	}

	public string TypeName => TypeUsage.EdmType.Name;

	public object DefaultValue
	{
		get
		{
			return TypeUsage.Facets["DefaultValue"].Value;
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			TypeUsage = TypeUsage.ShallowCopy(new FacetValues
			{
				DefaultValue = value
			});
		}
	}

	internal Func<object, object> ValueGetter
	{
		get
		{
			return _memberGetter;
		}
		set
		{
			Interlocked.CompareExchange(ref _memberGetter, value, null);
		}
	}

	internal Action<object, object> ValueSetter
	{
		get
		{
			return _memberSetter;
		}
		set
		{
			Interlocked.CompareExchange(ref _memberSetter, value, null);
		}
	}

	internal bool IsKeyMember
	{
		get
		{
			if (DeclaringType is EntityType entityType)
			{
				return entityType.KeyMembers.Contains(this);
			}
			return false;
		}
	}

	public bool IsCollectionType => TypeUsage.EdmType is CollectionType;

	public bool IsComplexType => TypeUsage.EdmType is ComplexType;

	public bool IsPrimitiveType => TypeUsage.EdmType is PrimitiveType;

	public bool IsEnumType => TypeUsage.EdmType is EnumType;

	public bool IsUnderlyingPrimitiveType
	{
		get
		{
			if (!IsPrimitiveType)
			{
				return IsEnumType;
			}
			return true;
		}
	}

	public ComplexType ComplexType => TypeUsage.EdmType as ComplexType;

	public PrimitiveType PrimitiveType
	{
		get
		{
			return TypeUsage.EdmType as PrimitiveType;
		}
		internal set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			StoreGeneratedPattern storeGeneratedPattern = StoreGeneratedPattern;
			ConcurrencyMode concurrencyMode = ConcurrencyMode;
			List<Facet> list = new List<Facet>();
			foreach (FacetDescription associatedFacetDescription in value.GetAssociatedFacetDescriptions())
			{
				if (TypeUsage.Facets.TryGetValue(associatedFacetDescription.FacetName, ignoreCase: false, out var item) && ((item.Value == null && item.Description.DefaultValue != null) || (item.Value != null && !item.Value.Equals(item.Description.DefaultValue))))
				{
					list.Add(item);
				}
			}
			TypeUsage = TypeUsage.Create(value, FacetValues.Create(list));
			if (storeGeneratedPattern != 0)
			{
				StoreGeneratedPattern = storeGeneratedPattern;
			}
			if (concurrencyMode != 0)
			{
				ConcurrencyMode = concurrencyMode;
			}
		}
	}

	public EnumType EnumType => TypeUsage.EdmType as EnumType;

	public PrimitiveType UnderlyingPrimitiveType
	{
		get
		{
			if (!IsUnderlyingPrimitiveType)
			{
				return null;
			}
			if (!IsEnumType)
			{
				return PrimitiveType;
			}
			return EnumType.UnderlyingType;
		}
	}

	public ConcurrencyMode ConcurrencyMode
	{
		get
		{
			return MetadataHelper.GetConcurrencyMode(this);
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			TypeUsage = TypeUsage.ShallowCopy(Facet.Create(Converter.ConcurrencyModeFacet, value));
		}
	}

	public StoreGeneratedPattern StoreGeneratedPattern
	{
		get
		{
			return MetadataHelper.GetStoreGeneratedPattern(this);
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			TypeUsage = TypeUsage.ShallowCopy(Facet.Create(Converter.StoreGeneratedPatternFacet, value));
		}
	}

	public CollectionKind CollectionKind
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("CollectionKind", ignoreCase: false, out var item))
			{
				return CollectionKind.None;
			}
			return (CollectionKind)item.Value;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			TypeUsage = TypeUsage.ShallowCopy(Facet.Create(MetadataItem.CollectionKindFacetDescription, value));
		}
	}

	public bool IsMaxLengthConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public int? MaxLength
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as int?;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (MaxLength != value)
			{
				TypeUsage = TypeUsage.ShallowCopy(new FacetValues
				{
					MaxLength = value
				});
			}
		}
	}

	public bool IsMaxLength
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item))
			{
				return item.IsUnbounded;
			}
			return false;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (value)
			{
				TypeUsage = TypeUsage.ShallowCopy(new FacetValues
				{
					MaxLength = EdmConstants.UnboundedValue
				});
			}
		}
	}

	public bool IsFixedLengthConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("FixedLength", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public bool? IsFixedLength
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("FixedLength", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as bool?;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (IsFixedLength != value)
			{
				TypeUsage = TypeUsage.ShallowCopy(new FacetValues
				{
					FixedLength = value
				});
			}
		}
	}

	public bool IsUnicodeConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("Unicode", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public bool? IsUnicode
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("Unicode", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as bool?;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (IsUnicode != value)
			{
				TypeUsage = TypeUsage.ShallowCopy(new FacetValues
				{
					Unicode = value
				});
			}
		}
	}

	public bool IsPrecisionConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("Precision", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public byte? Precision
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("Precision", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as byte?;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (Precision != value)
			{
				TypeUsage = TypeUsage.ShallowCopy(new FacetValues
				{
					Precision = value
				});
			}
		}
	}

	public bool IsScaleConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("Scale", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public byte? Scale
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("Scale", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as byte?;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (Scale != value)
			{
				TypeUsage = TypeUsage.ShallowCopy(new FacetValues
				{
					Scale = value
				});
			}
		}
	}

	public static EdmProperty CreatePrimitive(string name, PrimitiveType primitiveType)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(primitiveType, "primitiveType");
		return CreateProperty(name, primitiveType);
	}

	public static EdmProperty CreateEnum(string name, EnumType enumType)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(enumType, "enumType");
		return CreateProperty(name, enumType);
	}

	public static EdmProperty CreateComplex(string name, ComplexType complexType)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(complexType, "complexType");
		EdmProperty edmProperty = CreateProperty(name, complexType);
		edmProperty.Nullable = false;
		return edmProperty;
	}

	public static EdmProperty Create(string name, TypeUsage typeUsage)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(typeUsage, "typeUsage");
		EdmType edmType = typeUsage.EdmType;
		if (!Helper.IsPrimitiveType(edmType) && !Helper.IsEnumType(edmType) && !Helper.IsComplexType(edmType))
		{
			throw new ArgumentException(Strings.EdmProperty_InvalidPropertyType(edmType.FullName));
		}
		return new EdmProperty(name, typeUsage);
	}

	private static EdmProperty CreateProperty(string name, EdmType edmType)
	{
		TypeUsage typeUsage = TypeUsage.Create(edmType, new FacetValues());
		return new EdmProperty(name, typeUsage);
	}

	internal EdmProperty(string name, TypeUsage typeUsage)
		: base(name, typeUsage)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(typeUsage, "typeUsage");
	}

	internal EdmProperty(string name, TypeUsage typeUsage, PropertyInfo propertyInfo, Type entityDeclaringType)
		: this(name, typeUsage)
	{
		_propertyInfo = propertyInfo;
		_entityDeclaringType = entityDeclaringType;
	}

	internal EdmProperty(string name)
		: this(name, TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))
	{
	}

	public void SetMetadataProperties(IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotNull(metadataProperties, "metadataProperties");
		Util.ThrowIfReadOnly(this);
		AddMetadataProperties(metadataProperties);
	}
}
