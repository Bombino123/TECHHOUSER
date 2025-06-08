using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm.Provider;

internal class ClrProviderManifest : DbProviderManifest
{
	private const int s_PrimitiveTypeCount = 32;

	private ReadOnlyCollection<PrimitiveType> _primitiveTypesArray;

	private ReadOnlyCollection<PrimitiveType> _primitiveTypes;

	private static readonly ClrProviderManifest _instance = new ClrProviderManifest();

	internal static ClrProviderManifest Instance => _instance;

	public override string NamespaceName => "System";

	private ClrProviderManifest()
	{
	}

	internal bool TryGetPrimitiveType(Type clrType, out PrimitiveType primitiveType)
	{
		primitiveType = null;
		if (TryGetPrimitiveTypeKind(clrType, out var resolvedPrimitiveTypeKind))
		{
			InitializePrimitiveTypes();
			primitiveType = _primitiveTypesArray[(int)resolvedPrimitiveTypeKind];
			return true;
		}
		return false;
	}

	internal static bool TryGetPrimitiveTypeKind(Type clrType, out PrimitiveTypeKind resolvedPrimitiveTypeKind)
	{
		PrimitiveTypeKind? primitiveTypeKind = null;
		if (!clrType.IsEnum())
		{
			switch (Type.GetTypeCode(clrType))
			{
			case TypeCode.Boolean:
				primitiveTypeKind = PrimitiveTypeKind.Boolean;
				break;
			case TypeCode.Byte:
				primitiveTypeKind = PrimitiveTypeKind.Byte;
				break;
			case TypeCode.DateTime:
				primitiveTypeKind = PrimitiveTypeKind.DateTime;
				break;
			case TypeCode.Decimal:
				primitiveTypeKind = PrimitiveTypeKind.Decimal;
				break;
			case TypeCode.Double:
				primitiveTypeKind = PrimitiveTypeKind.Double;
				break;
			case TypeCode.Int16:
				primitiveTypeKind = PrimitiveTypeKind.Int16;
				break;
			case TypeCode.Int32:
				primitiveTypeKind = PrimitiveTypeKind.Int32;
				break;
			case TypeCode.Int64:
				primitiveTypeKind = PrimitiveTypeKind.Int64;
				break;
			case TypeCode.SByte:
				primitiveTypeKind = PrimitiveTypeKind.SByte;
				break;
			case TypeCode.Single:
				primitiveTypeKind = PrimitiveTypeKind.Single;
				break;
			case TypeCode.String:
				primitiveTypeKind = PrimitiveTypeKind.String;
				break;
			case TypeCode.Object:
				if (typeof(byte[]) == clrType)
				{
					primitiveTypeKind = PrimitiveTypeKind.Binary;
				}
				else if (typeof(DateTimeOffset) == clrType)
				{
					primitiveTypeKind = PrimitiveTypeKind.DateTimeOffset;
				}
				else if (typeof(DbGeography).IsAssignableFrom(clrType))
				{
					primitiveTypeKind = PrimitiveTypeKind.Geography;
				}
				else if (typeof(DbGeometry).IsAssignableFrom(clrType))
				{
					primitiveTypeKind = PrimitiveTypeKind.Geometry;
				}
				else if (typeof(Guid) == clrType)
				{
					primitiveTypeKind = PrimitiveTypeKind.Guid;
				}
				else if (typeof(HierarchyId) == clrType)
				{
					primitiveTypeKind = PrimitiveTypeKind.HierarchyId;
				}
				else if (typeof(TimeSpan) == clrType)
				{
					primitiveTypeKind = PrimitiveTypeKind.Time;
				}
				break;
			}
		}
		if (primitiveTypeKind.HasValue)
		{
			resolvedPrimitiveTypeKind = primitiveTypeKind.Value;
			return true;
		}
		resolvedPrimitiveTypeKind = PrimitiveTypeKind.Binary;
		return false;
	}

	public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
	{
		return Helper.EmptyEdmFunctionReadOnlyCollection;
	}

	public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType type)
	{
		if (Helper.IsPrimitiveType(type) && type.DataSpace == DataSpace.OSpace)
		{
			PrimitiveType primitiveType = (PrimitiveType)type.BaseType;
			return primitiveType.ProviderManifest.GetFacetDescriptions(primitiveType);
		}
		return Helper.EmptyFacetDescriptionEnumerable;
	}

	private void InitializePrimitiveTypes()
	{
		if (_primitiveTypes == null)
		{
			PrimitiveType[] array = new PrimitiveType[32];
			array[0] = CreatePrimitiveType(typeof(byte[]), PrimitiveTypeKind.Binary);
			array[1] = CreatePrimitiveType(typeof(bool), PrimitiveTypeKind.Boolean);
			array[2] = CreatePrimitiveType(typeof(byte), PrimitiveTypeKind.Byte);
			array[3] = CreatePrimitiveType(typeof(DateTime), PrimitiveTypeKind.DateTime);
			array[13] = CreatePrimitiveType(typeof(TimeSpan), PrimitiveTypeKind.Time);
			array[14] = CreatePrimitiveType(typeof(DateTimeOffset), PrimitiveTypeKind.DateTimeOffset);
			array[4] = CreatePrimitiveType(typeof(decimal), PrimitiveTypeKind.Decimal);
			array[5] = CreatePrimitiveType(typeof(double), PrimitiveTypeKind.Double);
			array[16] = CreatePrimitiveType(typeof(DbGeography), PrimitiveTypeKind.Geography);
			array[15] = CreatePrimitiveType(typeof(DbGeometry), PrimitiveTypeKind.Geometry);
			array[6] = CreatePrimitiveType(typeof(Guid), PrimitiveTypeKind.Guid);
			array[31] = CreatePrimitiveType(typeof(HierarchyId), PrimitiveTypeKind.HierarchyId);
			array[9] = CreatePrimitiveType(typeof(short), PrimitiveTypeKind.Int16);
			array[10] = CreatePrimitiveType(typeof(int), PrimitiveTypeKind.Int32);
			array[11] = CreatePrimitiveType(typeof(long), PrimitiveTypeKind.Int64);
			array[8] = CreatePrimitiveType(typeof(sbyte), PrimitiveTypeKind.SByte);
			array[7] = CreatePrimitiveType(typeof(float), PrimitiveTypeKind.Single);
			array[12] = CreatePrimitiveType(typeof(string), PrimitiveTypeKind.String);
			ReadOnlyCollection<PrimitiveType> value = new ReadOnlyCollection<PrimitiveType>(array);
			ReadOnlyCollection<PrimitiveType> value2 = new ReadOnlyCollection<PrimitiveType>(array.Where((PrimitiveType t) => t != null).ToList());
			Interlocked.CompareExchange(ref _primitiveTypesArray, value, null);
			Interlocked.CompareExchange(ref _primitiveTypes, value2, null);
		}
	}

	private PrimitiveType CreatePrimitiveType(Type clrType, PrimitiveTypeKind primitiveTypeKind)
	{
		PrimitiveType primitiveType = MetadataItem.EdmProviderManifest.GetPrimitiveType(primitiveTypeKind);
		PrimitiveType primitiveType2 = new PrimitiveType(clrType, primitiveType, this);
		primitiveType2.SetReadOnly();
		return primitiveType2;
	}

	public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
	{
		InitializePrimitiveTypes();
		return _primitiveTypes;
	}

	public override TypeUsage GetEdmType(TypeUsage storeType)
	{
		Check.NotNull(storeType, "storeType");
		throw new NotImplementedException();
	}

	public override TypeUsage GetStoreType(TypeUsage edmType)
	{
		Check.NotNull(edmType, "edmType");
		throw new NotImplementedException();
	}

	protected override XmlReader GetDbInformation(string informationType)
	{
		throw new NotImplementedException();
	}
}
