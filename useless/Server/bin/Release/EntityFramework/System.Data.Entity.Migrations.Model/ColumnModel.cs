using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Reflection;

namespace System.Data.Entity.Migrations.Model;

public class ColumnModel : PropertyModel
{
	private readonly Type _clrType;

	private PropertyInfo _apiPropertyInfo;

	private IDictionary<string, AnnotationValues> _annotations = new Dictionary<string, AnnotationValues>();

	private static readonly Dictionary<PrimitiveTypeKind, int> _typeSize = new Dictionary<PrimitiveTypeKind, int>
	{
		{
			PrimitiveTypeKind.Binary,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.Boolean,
			1
		},
		{
			PrimitiveTypeKind.Byte,
			1
		},
		{
			PrimitiveTypeKind.DateTime,
			8
		},
		{
			PrimitiveTypeKind.DateTimeOffset,
			10
		},
		{
			PrimitiveTypeKind.Decimal,
			17
		},
		{
			PrimitiveTypeKind.Double,
			53
		},
		{
			PrimitiveTypeKind.Guid,
			16
		},
		{
			PrimitiveTypeKind.Int16,
			2
		},
		{
			PrimitiveTypeKind.Int32,
			4
		},
		{
			PrimitiveTypeKind.Int64,
			8
		},
		{
			PrimitiveTypeKind.SByte,
			1
		},
		{
			PrimitiveTypeKind.Single,
			4
		},
		{
			PrimitiveTypeKind.String,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.Time,
			5
		},
		{
			PrimitiveTypeKind.HierarchyId,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.Geometry,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.Geography,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryPoint,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryLineString,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryPolygon,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryMultiPoint,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryMultiLineString,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryMultiPolygon,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeometryCollection,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyPoint,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyLineString,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyPolygon,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyMultiPoint,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyMultiLineString,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyMultiPolygon,
			int.MaxValue
		},
		{
			PrimitiveTypeKind.GeographyCollection,
			int.MaxValue
		}
	};

	public virtual Type ClrType => _clrType;

	public virtual object ClrDefaultValue
	{
		get
		{
			if (_clrType.IsValueType())
			{
				return Activator.CreateInstance(_clrType);
			}
			if (_clrType == typeof(string))
			{
				return string.Empty;
			}
			if (_clrType == typeof(DbGeography))
			{
				return DbGeography.FromText("POINT(0 0)");
			}
			if (_clrType == typeof(DbGeometry))
			{
				return DbGeometry.FromText("POINT(0 0)");
			}
			return new byte[0];
		}
	}

	public virtual bool? IsNullable { get; set; }

	public virtual bool IsIdentity { get; set; }

	public virtual bool IsTimestamp { get; set; }

	public IDictionary<string, AnnotationValues> Annotations
	{
		get
		{
			return _annotations;
		}
		set
		{
			_annotations = value ?? new Dictionary<string, AnnotationValues>();
		}
	}

	internal PropertyInfo ApiPropertyInfo
	{
		get
		{
			return _apiPropertyInfo;
		}
		set
		{
			_apiPropertyInfo = value;
		}
	}

	public ColumnModel(PrimitiveTypeKind type)
		: this(type, null)
	{
	}

	public ColumnModel(PrimitiveTypeKind type, TypeUsage typeUsage)
		: base(type, typeUsage)
	{
		_clrType = PrimitiveType.GetEdmPrimitiveType(type).ClrEquivalentType;
	}

	public bool IsNarrowerThan(ColumnModel column, DbProviderManifest providerManifest)
	{
		Check.NotNull(column, "column");
		Check.NotNull(providerManifest, "providerManifest");
		TypeUsage storeType = providerManifest.GetStoreType(base.TypeUsage);
		TypeUsage storeType2 = providerManifest.GetStoreType(column.TypeUsage);
		if (_typeSize[Type] >= _typeSize[column.Type])
		{
			bool? isUnicode = IsUnicode;
			if (!isUnicode.HasValue || isUnicode.GetValueOrDefault() || ((!column.IsUnicode) ?? false))
			{
				isUnicode = IsNullable;
				if (!isUnicode.HasValue || isUnicode.GetValueOrDefault() || ((!column.IsNullable) ?? false))
				{
					return IsNarrowerThan(storeType, storeType2);
				}
			}
		}
		return true;
	}

	private static bool IsNarrowerThan(TypeUsage typeUsage, TypeUsage other)
	{
		string[] array = new string[3] { "MaxLength", "Precision", "Scale" };
		foreach (string identity in array)
		{
			if (typeUsage.Facets.TryGetValue(identity, ignoreCase: true, out var item) && other.Facets.TryGetValue(item.Name, ignoreCase: true, out var item2) && item.Value != item2.Value)
			{
				int num = Convert.ToInt32(item.Value, CultureInfo.InvariantCulture);
				int num2 = Convert.ToInt32(item2.Value, CultureInfo.InvariantCulture);
				if (num < num2)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal override FacetValues ToFacetValues()
	{
		FacetValues facetValues = base.ToFacetValues();
		if (IsNullable.HasValue)
		{
			facetValues.Nullable = IsNullable.Value;
		}
		if (IsIdentity)
		{
			facetValues.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
		}
		return facetValues;
	}
}
