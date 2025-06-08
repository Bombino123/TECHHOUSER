using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

public class PrimitiveType : SimpleType
{
	private PrimitiveTypeKind _primitiveTypeKind;

	private DbProviderManifest _providerManifest;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.PrimitiveType;

	internal override Type ClrType => ClrEquivalentType;

	[MetadataProperty(BuiltInTypeKind.PrimitiveTypeKind, false)]
	public virtual PrimitiveTypeKind PrimitiveTypeKind
	{
		get
		{
			return _primitiveTypeKind;
		}
		internal set
		{
			_primitiveTypeKind = value;
		}
	}

	internal DbProviderManifest ProviderManifest
	{
		get
		{
			return _providerManifest;
		}
		set
		{
			_providerManifest = value;
		}
	}

	public virtual ReadOnlyCollection<FacetDescription> FacetDescriptions => ProviderManifest.GetFacetDescriptions(this);

	public Type ClrEquivalentType
	{
		get
		{
			switch (PrimitiveTypeKind)
			{
			case PrimitiveTypeKind.Binary:
				return typeof(byte[]);
			case PrimitiveTypeKind.Boolean:
				return typeof(bool);
			case PrimitiveTypeKind.Byte:
				return typeof(byte);
			case PrimitiveTypeKind.DateTime:
				return typeof(DateTime);
			case PrimitiveTypeKind.Time:
				return typeof(TimeSpan);
			case PrimitiveTypeKind.DateTimeOffset:
				return typeof(DateTimeOffset);
			case PrimitiveTypeKind.Decimal:
				return typeof(decimal);
			case PrimitiveTypeKind.Double:
				return typeof(double);
			case PrimitiveTypeKind.Geography:
			case PrimitiveTypeKind.GeographyPoint:
			case PrimitiveTypeKind.GeographyLineString:
			case PrimitiveTypeKind.GeographyPolygon:
			case PrimitiveTypeKind.GeographyMultiPoint:
			case PrimitiveTypeKind.GeographyMultiLineString:
			case PrimitiveTypeKind.GeographyMultiPolygon:
			case PrimitiveTypeKind.GeographyCollection:
				return typeof(DbGeography);
			case PrimitiveTypeKind.Geometry:
			case PrimitiveTypeKind.GeometryPoint:
			case PrimitiveTypeKind.GeometryLineString:
			case PrimitiveTypeKind.GeometryPolygon:
			case PrimitiveTypeKind.GeometryMultiPoint:
			case PrimitiveTypeKind.GeometryMultiLineString:
			case PrimitiveTypeKind.GeometryMultiPolygon:
			case PrimitiveTypeKind.GeometryCollection:
				return typeof(DbGeometry);
			case PrimitiveTypeKind.Guid:
				return typeof(Guid);
			case PrimitiveTypeKind.HierarchyId:
				return typeof(HierarchyId);
			case PrimitiveTypeKind.Single:
				return typeof(float);
			case PrimitiveTypeKind.SByte:
				return typeof(sbyte);
			case PrimitiveTypeKind.Int16:
				return typeof(short);
			case PrimitiveTypeKind.Int32:
				return typeof(int);
			case PrimitiveTypeKind.Int64:
				return typeof(long);
			case PrimitiveTypeKind.String:
				return typeof(string);
			default:
				return null;
			}
		}
	}

	internal PrimitiveType()
	{
	}

	internal PrimitiveType(string name, string namespaceName, DataSpace dataSpace, PrimitiveType baseType, DbProviderManifest providerManifest)
		: base(name, namespaceName, dataSpace)
	{
		Check.NotNull(baseType, "baseType");
		Check.NotNull(providerManifest, "providerManifest");
		BaseType = baseType;
		Initialize(this, baseType.PrimitiveTypeKind, providerManifest);
	}

	internal PrimitiveType(Type clrType, PrimitiveType baseType, DbProviderManifest providerManifest)
		: this(Check.NotNull(clrType, "clrType").Name, clrType.NestingNamespace(), DataSpace.OSpace, baseType, providerManifest)
	{
	}

	internal override IEnumerable<FacetDescription> GetAssociatedFacetDescriptions()
	{
		return base.GetAssociatedFacetDescriptions().Concat(FacetDescriptions);
	}

	internal static void Initialize(PrimitiveType primitiveType, PrimitiveTypeKind primitiveTypeKind, DbProviderManifest providerManifest)
	{
		primitiveType._primitiveTypeKind = primitiveTypeKind;
		primitiveType._providerManifest = providerManifest;
	}

	public EdmType GetEdmPrimitiveType()
	{
		return MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind);
	}

	public static ReadOnlyCollection<PrimitiveType> GetEdmPrimitiveTypes()
	{
		return MetadataItem.EdmProviderManifest.GetStoreTypes();
	}

	public static PrimitiveType GetEdmPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
	{
		return MetadataItem.EdmProviderManifest.GetPrimitiveType(primitiveTypeKind);
	}
}
