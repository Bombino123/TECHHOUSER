using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm.Provider;

internal class EdmProviderManifest : DbProviderManifest
{
	internal const string ConcurrencyModeFacetName = "ConcurrencyMode";

	internal const string StoreGeneratedPatternFacetName = "StoreGeneratedPattern";

	private Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>> _facetDescriptions;

	private ReadOnlyCollection<PrimitiveType> _primitiveTypes;

	private ReadOnlyCollection<EdmFunction> _functions;

	private static readonly EdmProviderManifest _instance = new EdmProviderManifest();

	private ReadOnlyCollection<PrimitiveType>[] _promotionTypes;

	private static TypeUsage[] _canonicalModelTypes;

	internal const byte MaximumDecimalPrecision = byte.MaxValue;

	internal const byte MaximumDateTimePrecision = byte.MaxValue;

	internal static EdmProviderManifest Instance => _instance;

	public override string NamespaceName => "Edm";

	internal virtual string Token => string.Empty;

	private EdmProviderManifest()
	{
	}

	public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
	{
		InitializeCanonicalFunctions();
		return _functions;
	}

	public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType type)
	{
		InitializeFacetDescriptions();
		ReadOnlyCollection<FacetDescription> value = null;
		if (_facetDescriptions.TryGetValue(type as PrimitiveType, out value))
		{
			return value;
		}
		return Helper.EmptyFacetDescriptionEnumerable;
	}

	public PrimitiveType GetPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
	{
		InitializePrimitiveTypes();
		return _primitiveTypes[(int)primitiveTypeKind];
	}

	private void InitializePrimitiveTypes()
	{
		if (_primitiveTypes == null)
		{
			PrimitiveType[] array = new PrimitiveType[32];
			array[0] = new PrimitiveType();
			array[1] = new PrimitiveType();
			array[2] = new PrimitiveType();
			array[3] = new PrimitiveType();
			array[4] = new PrimitiveType();
			array[5] = new PrimitiveType();
			array[7] = new PrimitiveType();
			array[6] = new PrimitiveType();
			array[31] = new PrimitiveType();
			array[9] = new PrimitiveType();
			array[10] = new PrimitiveType();
			array[11] = new PrimitiveType();
			array[8] = new PrimitiveType();
			array[12] = new PrimitiveType();
			array[13] = new PrimitiveType();
			array[14] = new PrimitiveType();
			array[15] = new PrimitiveType();
			array[17] = new PrimitiveType();
			array[18] = new PrimitiveType();
			array[19] = new PrimitiveType();
			array[20] = new PrimitiveType();
			array[21] = new PrimitiveType();
			array[22] = new PrimitiveType();
			array[23] = new PrimitiveType();
			array[16] = new PrimitiveType();
			array[24] = new PrimitiveType();
			array[25] = new PrimitiveType();
			array[26] = new PrimitiveType();
			array[27] = new PrimitiveType();
			array[28] = new PrimitiveType();
			array[29] = new PrimitiveType();
			array[30] = new PrimitiveType();
			InitializePrimitiveType(array[0], PrimitiveTypeKind.Binary, "Binary", typeof(byte[]));
			InitializePrimitiveType(array[1], PrimitiveTypeKind.Boolean, "Boolean", typeof(bool));
			InitializePrimitiveType(array[2], PrimitiveTypeKind.Byte, "Byte", typeof(byte));
			InitializePrimitiveType(array[3], PrimitiveTypeKind.DateTime, "DateTime", typeof(DateTime));
			InitializePrimitiveType(array[4], PrimitiveTypeKind.Decimal, "Decimal", typeof(decimal));
			InitializePrimitiveType(array[5], PrimitiveTypeKind.Double, "Double", typeof(double));
			InitializePrimitiveType(array[7], PrimitiveTypeKind.Single, "Single", typeof(float));
			InitializePrimitiveType(array[6], PrimitiveTypeKind.Guid, "Guid", typeof(Guid));
			InitializePrimitiveType(array[31], PrimitiveTypeKind.HierarchyId, "HierarchyId", typeof(HierarchyId));
			InitializePrimitiveType(array[9], PrimitiveTypeKind.Int16, "Int16", typeof(short));
			InitializePrimitiveType(array[10], PrimitiveTypeKind.Int32, "Int32", typeof(int));
			InitializePrimitiveType(array[11], PrimitiveTypeKind.Int64, "Int64", typeof(long));
			InitializePrimitiveType(array[8], PrimitiveTypeKind.SByte, "SByte", typeof(sbyte));
			InitializePrimitiveType(array[12], PrimitiveTypeKind.String, "String", typeof(string));
			InitializePrimitiveType(array[13], PrimitiveTypeKind.Time, "Time", typeof(TimeSpan));
			InitializePrimitiveType(array[14], PrimitiveTypeKind.DateTimeOffset, "DateTimeOffset", typeof(DateTimeOffset));
			InitializePrimitiveType(array[16], PrimitiveTypeKind.Geography, "Geography", typeof(DbGeography));
			InitializePrimitiveType(array[24], PrimitiveTypeKind.GeographyPoint, "GeographyPoint", typeof(DbGeography));
			InitializePrimitiveType(array[25], PrimitiveTypeKind.GeographyLineString, "GeographyLineString", typeof(DbGeography));
			InitializePrimitiveType(array[26], PrimitiveTypeKind.GeographyPolygon, "GeographyPolygon", typeof(DbGeography));
			InitializePrimitiveType(array[27], PrimitiveTypeKind.GeographyMultiPoint, "GeographyMultiPoint", typeof(DbGeography));
			InitializePrimitiveType(array[28], PrimitiveTypeKind.GeographyMultiLineString, "GeographyMultiLineString", typeof(DbGeography));
			InitializePrimitiveType(array[29], PrimitiveTypeKind.GeographyMultiPolygon, "GeographyMultiPolygon", typeof(DbGeography));
			InitializePrimitiveType(array[30], PrimitiveTypeKind.GeographyCollection, "GeographyCollection", typeof(DbGeography));
			InitializePrimitiveType(array[15], PrimitiveTypeKind.Geometry, "Geometry", typeof(DbGeometry));
			InitializePrimitiveType(array[17], PrimitiveTypeKind.GeometryPoint, "GeometryPoint", typeof(DbGeometry));
			InitializePrimitiveType(array[18], PrimitiveTypeKind.GeometryLineString, "GeometryLineString", typeof(DbGeometry));
			InitializePrimitiveType(array[19], PrimitiveTypeKind.GeometryPolygon, "GeometryPolygon", typeof(DbGeometry));
			InitializePrimitiveType(array[20], PrimitiveTypeKind.GeometryMultiPoint, "GeometryMultiPoint", typeof(DbGeometry));
			InitializePrimitiveType(array[21], PrimitiveTypeKind.GeometryMultiLineString, "GeometryMultiLineString", typeof(DbGeometry));
			InitializePrimitiveType(array[22], PrimitiveTypeKind.GeometryMultiPolygon, "GeometryMultiPolygon", typeof(DbGeometry));
			InitializePrimitiveType(array[23], PrimitiveTypeKind.GeometryCollection, "GeometryCollection", typeof(DbGeometry));
			PrimitiveType[] array2 = array;
			foreach (PrimitiveType obj in array2)
			{
				obj.ProviderManifest = this;
				obj.SetReadOnly();
			}
			ReadOnlyCollection<PrimitiveType> value = new ReadOnlyCollection<PrimitiveType>(array);
			Interlocked.CompareExchange(ref _primitiveTypes, value, null);
		}
	}

	private void InitializePrimitiveType(PrimitiveType primitiveType, PrimitiveTypeKind primitiveTypeKind, string name, Type clrType)
	{
		EdmType.Initialize(primitiveType, name, "Edm", DataSpace.CSpace, isAbstract: true, null);
		PrimitiveType.Initialize(primitiveType, primitiveTypeKind, this);
	}

	private void InitializeFacetDescriptions()
	{
		if (_facetDescriptions == null)
		{
			InitializePrimitiveTypes();
			Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>> dictionary = new Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>>();
			FacetDescription[] initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.String);
			PrimitiveType key = _primitiveTypes[12];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.Binary);
			key = _primitiveTypes[0];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.DateTime);
			key = _primitiveTypes[3];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.Time);
			key = _primitiveTypes[13];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.DateTimeOffset);
			key = _primitiveTypes[14];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.Decimal);
			key = _primitiveTypes[4];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.Geography);
			key = _primitiveTypes[16];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyPoint);
			key = _primitiveTypes[24];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyLineString);
			key = _primitiveTypes[25];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyPolygon);
			key = _primitiveTypes[26];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyMultiPoint);
			key = _primitiveTypes[27];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyMultiLineString);
			key = _primitiveTypes[28];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyMultiPolygon);
			key = _primitiveTypes[29];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyCollection);
			key = _primitiveTypes[30];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.Geometry);
			key = _primitiveTypes[15];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryPoint);
			key = _primitiveTypes[17];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryLineString);
			key = _primitiveTypes[18];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryPolygon);
			key = _primitiveTypes[19];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryMultiPoint);
			key = _primitiveTypes[20];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryMultiLineString);
			key = _primitiveTypes[21];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryMultiPolygon);
			key = _primitiveTypes[22];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			initialFacetDescriptions = GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryCollection);
			key = _primitiveTypes[23];
			dictionary.Add(key, new ReadOnlyCollection<FacetDescription>(initialFacetDescriptions));
			Interlocked.CompareExchange(ref _facetDescriptions, dictionary, null);
		}
	}

	internal static FacetDescription[] GetInitialFacetDescriptions(PrimitiveTypeKind primitiveTypeKind)
	{
		switch (primitiveTypeKind)
		{
		case PrimitiveTypeKind.String:
			return new FacetDescription[3]
			{
				new FacetDescription("MaxLength", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32), 0, int.MaxValue, null),
				new FacetDescription("Unicode", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean), null, null, null),
				new FacetDescription("FixedLength", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean), null, null, null)
			};
		case PrimitiveTypeKind.Binary:
			return new FacetDescription[2]
			{
				new FacetDescription("MaxLength", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32), 0, int.MaxValue, null),
				new FacetDescription("FixedLength", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean), null, null, null)
			};
		case PrimitiveTypeKind.DateTime:
			return new FacetDescription[1]
			{
				new FacetDescription("Precision", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte), 0, 255, null)
			};
		case PrimitiveTypeKind.Time:
			return new FacetDescription[1]
			{
				new FacetDescription("Precision", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte), 0, 255, TypeUsage.DefaultDateTimePrecisionFacetValue)
			};
		case PrimitiveTypeKind.DateTimeOffset:
			return new FacetDescription[1]
			{
				new FacetDescription("Precision", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte), 0, 255, TypeUsage.DefaultDateTimePrecisionFacetValue)
			};
		case PrimitiveTypeKind.Decimal:
			return new FacetDescription[2]
			{
				new FacetDescription("Precision", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte), 1, 255, null),
				new FacetDescription("Scale", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte), 0, 255, null)
			};
		case PrimitiveTypeKind.Geometry:
		case PrimitiveTypeKind.GeometryPoint:
		case PrimitiveTypeKind.GeometryLineString:
		case PrimitiveTypeKind.GeometryPolygon:
		case PrimitiveTypeKind.GeometryMultiPoint:
		case PrimitiveTypeKind.GeometryMultiLineString:
		case PrimitiveTypeKind.GeometryMultiPolygon:
		case PrimitiveTypeKind.GeometryCollection:
			return new FacetDescription[2]
			{
				new FacetDescription("SRID", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32), 0, int.MaxValue, DbGeometry.DefaultCoordinateSystemId),
				new FacetDescription("IsStrict", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean), null, null, true)
			};
		case PrimitiveTypeKind.Geography:
		case PrimitiveTypeKind.GeographyPoint:
		case PrimitiveTypeKind.GeographyLineString:
		case PrimitiveTypeKind.GeographyPolygon:
		case PrimitiveTypeKind.GeographyMultiPoint:
		case PrimitiveTypeKind.GeographyMultiLineString:
		case PrimitiveTypeKind.GeographyMultiPolygon:
		case PrimitiveTypeKind.GeographyCollection:
			return new FacetDescription[2]
			{
				new FacetDescription("SRID", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32), 0, int.MaxValue, DbGeography.DefaultCoordinateSystemId),
				new FacetDescription("IsStrict", MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean), null, null, true)
			};
		default:
			return null;
		}
	}

	private void InitializeCanonicalFunctions()
	{
		if (_functions != null)
		{
			return;
		}
		InitializePrimitiveTypes();
		EdmProviderManifestFunctionBuilder functions = new EdmProviderManifestFunctionBuilder(_primitiveTypes);
		PrimitiveTypeKind[] typeKinds = new PrimitiveTypeKind[13]
		{
			PrimitiveTypeKind.Byte,
			PrimitiveTypeKind.DateTime,
			PrimitiveTypeKind.Decimal,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Int16,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64,
			PrimitiveTypeKind.SByte,
			PrimitiveTypeKind.Single,
			PrimitiveTypeKind.String,
			PrimitiveTypeKind.Binary,
			PrimitiveTypeKind.Time,
			PrimitiveTypeKind.DateTimeOffset
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate("Max", type);
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate("Min", type);
		});
		PrimitiveTypeKind[] typeKinds2 = new PrimitiveTypeKind[4]
		{
			PrimitiveTypeKind.Decimal,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds2, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate("Avg", type);
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds2, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate("Sum", type);
		});
		PrimitiveTypeKind[] typeKinds3 = new PrimitiveTypeKind[4]
		{
			PrimitiveTypeKind.Decimal,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds3, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate(PrimitiveTypeKind.Double, "StDev", type);
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds3, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate(PrimitiveTypeKind.Double, "StDevP", type);
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds3, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate(PrimitiveTypeKind.Double, "Var", type);
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds3, delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate(PrimitiveTypeKind.Double, "VarP", type);
		});
		EdmProviderManifestFunctionBuilder.ForAllBasePrimitiveTypes(delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate(PrimitiveTypeKind.Int32, "Count", type);
		});
		EdmProviderManifestFunctionBuilder.ForAllBasePrimitiveTypes(delegate(PrimitiveTypeKind type)
		{
			functions.AddAggregate(PrimitiveTypeKind.Int64, "BigCount", type);
		});
		functions.AddFunction(PrimitiveTypeKind.String, "Trim", PrimitiveTypeKind.String, "stringArgument");
		functions.AddFunction(PrimitiveTypeKind.String, "RTrim", PrimitiveTypeKind.String, "stringArgument");
		functions.AddFunction(PrimitiveTypeKind.String, "LTrim", PrimitiveTypeKind.String, "stringArgument");
		functions.AddFunction(PrimitiveTypeKind.String, "Concat", PrimitiveTypeKind.String, "string1", PrimitiveTypeKind.String, "string2");
		functions.AddFunction(PrimitiveTypeKind.Int32, "Length", PrimitiveTypeKind.String, "stringArgument");
		PrimitiveTypeKind[] typeKinds4 = new PrimitiveTypeKind[5]
		{
			PrimitiveTypeKind.Byte,
			PrimitiveTypeKind.Int16,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64,
			PrimitiveTypeKind.SByte
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds4, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.String, "Substring", PrimitiveTypeKind.String, "stringArgument", type, "start", type, "length");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds4, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.String, "Left", PrimitiveTypeKind.String, "stringArgument", type, "length");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds4, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.String, "Right", PrimitiveTypeKind.String, "stringArgument", type, "length");
		});
		functions.AddFunction(PrimitiveTypeKind.String, "Replace", PrimitiveTypeKind.String, "stringArgument", PrimitiveTypeKind.String, "toReplace", PrimitiveTypeKind.String, "replacement");
		functions.AddFunction(PrimitiveTypeKind.Int32, "IndexOf", PrimitiveTypeKind.String, "searchString", PrimitiveTypeKind.String, "stringToFind");
		functions.AddFunction(PrimitiveTypeKind.String, "ToUpper", PrimitiveTypeKind.String, "stringArgument");
		functions.AddFunction(PrimitiveTypeKind.String, "ToLower", PrimitiveTypeKind.String, "stringArgument");
		functions.AddFunction(PrimitiveTypeKind.String, "Reverse", PrimitiveTypeKind.String, "stringArgument");
		functions.AddFunction(PrimitiveTypeKind.Boolean, "Contains", PrimitiveTypeKind.String, "searchedString", PrimitiveTypeKind.String, "searchedForString");
		functions.AddFunction(PrimitiveTypeKind.Boolean, "StartsWith", PrimitiveTypeKind.String, "stringArgument", PrimitiveTypeKind.String, "prefix");
		functions.AddFunction(PrimitiveTypeKind.Boolean, "EndsWith", PrimitiveTypeKind.String, "stringArgument", PrimitiveTypeKind.String, "suffix");
		PrimitiveTypeKind[] typeKinds5 = new PrimitiveTypeKind[2]
		{
			PrimitiveTypeKind.DateTimeOffset,
			PrimitiveTypeKind.DateTime
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Year", type, "dateValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Month", type, "dateValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Day", type, "dateValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DayOfYear", type, "dateValue");
		});
		PrimitiveTypeKind[] typeKinds6 = new PrimitiveTypeKind[3]
		{
			PrimitiveTypeKind.DateTimeOffset,
			PrimitiveTypeKind.DateTime,
			PrimitiveTypeKind.Time
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Hour", type, "timeValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Minute", type, "timeValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Second", type, "timeValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "Millisecond", type, "timeValue");
		});
		functions.AddFunction(PrimitiveTypeKind.DateTime, "CurrentDateTime");
		functions.AddFunction(PrimitiveTypeKind.DateTimeOffset, "CurrentDateTimeOffset");
		functions.AddFunction(PrimitiveTypeKind.Int32, "GetTotalOffsetMinutes", PrimitiveTypeKind.DateTimeOffset, "dateTimeOffsetArgument");
		functions.AddFunction(PrimitiveTypeKind.DateTime, "LocalDateTime", PrimitiveTypeKind.DateTimeOffset, "dateTimeOffsetArgument");
		functions.AddFunction(PrimitiveTypeKind.DateTime, "UtcDateTime", PrimitiveTypeKind.DateTimeOffset, "dateTimeOffsetArgument");
		functions.AddFunction(PrimitiveTypeKind.DateTime, "CurrentUtcDateTime");
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "TruncateTime", type, "dateValue");
		});
		functions.AddFunction(PrimitiveTypeKind.DateTime, "CreateDateTime", PrimitiveTypeKind.Int32, "year", PrimitiveTypeKind.Int32, "month", PrimitiveTypeKind.Int32, "day", PrimitiveTypeKind.Int32, "hour", PrimitiveTypeKind.Int32, "minute", PrimitiveTypeKind.Double, "second");
		functions.AddFunction(PrimitiveTypeKind.DateTimeOffset, "CreateDateTimeOffset", PrimitiveTypeKind.Int32, "year", PrimitiveTypeKind.Int32, "month", PrimitiveTypeKind.Int32, "day", PrimitiveTypeKind.Int32, "hour", PrimitiveTypeKind.Int32, "minute", PrimitiveTypeKind.Double, "second", PrimitiveTypeKind.Int32, "timeZoneOffset");
		functions.AddFunction(PrimitiveTypeKind.Time, "CreateTime", PrimitiveTypeKind.Int32, "hour", PrimitiveTypeKind.Int32, "minute", PrimitiveTypeKind.Double, "second");
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddYears", type, "dateValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddMonths", type, "dateValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddDays", type, "dateValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddHours", type, "timeValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddMinutes", type, "timeValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddSeconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddMilliseconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddMicroseconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "AddNanoseconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffYears", type, "dateValue1", type, "dateValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMonths", type, "dateValue1", type, "dateValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds5, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffDays", type, "dateValue1", type, "dateValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffHours", type, "timeValue1", type, "timeValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMinutes", type, "timeValue1", type, "timeValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffSeconds", type, "timeValue1", type, "timeValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMilliseconds", type, "timeValue1", type, "timeValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMicroseconds", type, "timeValue1", type, "timeValue2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds6, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(PrimitiveTypeKind.Int32, "DiffNanoseconds", type, "timeValue1", type, "timeValue2");
		});
		PrimitiveTypeKind[] typeKinds7 = new PrimitiveTypeKind[3]
		{
			PrimitiveTypeKind.Single,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Decimal
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds7, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "Round", type, "value");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds7, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "Floor", type, "value");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds7, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "Ceiling", type, "value");
		});
		PrimitiveTypeKind[] typeKinds8 = new PrimitiveTypeKind[2]
		{
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Decimal
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds8, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "Round", type, "value", PrimitiveTypeKind.Int32, "digits");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds8, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "Truncate", type, "value", PrimitiveTypeKind.Int32, "digits");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(new PrimitiveTypeKind[7]
		{
			PrimitiveTypeKind.Decimal,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Int16,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64,
			PrimitiveTypeKind.Byte,
			PrimitiveTypeKind.Single
		}, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "Abs", type, "value");
		});
		PrimitiveTypeKind[] array = new PrimitiveTypeKind[4]
		{
			PrimitiveTypeKind.Decimal,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64
		};
		PrimitiveTypeKind[] array2 = new PrimitiveTypeKind[3]
		{
			PrimitiveTypeKind.Decimal,
			PrimitiveTypeKind.Double,
			PrimitiveTypeKind.Int64
		};
		PrimitiveTypeKind[] array3 = array;
		foreach (PrimitiveTypeKind primitiveTypeKind in array3)
		{
			PrimitiveTypeKind[] array4 = array2;
			foreach (PrimitiveTypeKind argument2TypeKind in array4)
			{
				functions.AddFunction(primitiveTypeKind, "Power", primitiveTypeKind, "baseArgument", argument2TypeKind, "exponent");
			}
		}
		PrimitiveTypeKind[] typeKinds9 = new PrimitiveTypeKind[4]
		{
			PrimitiveTypeKind.Int16,
			PrimitiveTypeKind.Int32,
			PrimitiveTypeKind.Int64,
			PrimitiveTypeKind.Byte
		};
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds9, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "BitwiseAnd", type, "value1", type, "value2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds9, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "BitwiseOr", type, "value1", type, "value2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds9, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "BitwiseXor", type, "value1", type, "value2");
		});
		EdmProviderManifestFunctionBuilder.ForTypes(typeKinds9, delegate(PrimitiveTypeKind type)
		{
			functions.AddFunction(type, "BitwiseNot", type, "value");
		});
		functions.AddFunction(PrimitiveTypeKind.Guid, "NewGuid");
		EdmProviderManifestSpatialFunctions.AddFunctions(functions);
		EdmProviderManifestHierarchyIdFunctions.AddFunctions(functions);
		ReadOnlyCollection<EdmFunction> value = functions.ToFunctionCollection();
		Interlocked.CompareExchange(ref _functions, value, null);
	}

	internal ReadOnlyCollection<PrimitiveType> GetPromotionTypes(PrimitiveType primitiveType)
	{
		InitializePromotableTypes();
		return _promotionTypes[(int)primitiveType.PrimitiveTypeKind];
	}

	private void InitializePromotableTypes()
	{
		if (_promotionTypes == null)
		{
			ReadOnlyCollection<PrimitiveType>[] array = new ReadOnlyCollection<PrimitiveType>[32];
			for (int i = 0; i < 32; i++)
			{
				array[i] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[1] { _primitiveTypes[i] });
			}
			array[2] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[7]
			{
				_primitiveTypes[2],
				_primitiveTypes[9],
				_primitiveTypes[10],
				_primitiveTypes[11],
				_primitiveTypes[4],
				_primitiveTypes[7],
				_primitiveTypes[5]
			});
			array[9] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[6]
			{
				_primitiveTypes[9],
				_primitiveTypes[10],
				_primitiveTypes[11],
				_primitiveTypes[4],
				_primitiveTypes[7],
				_primitiveTypes[5]
			});
			array[10] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[5]
			{
				_primitiveTypes[10],
				_primitiveTypes[11],
				_primitiveTypes[4],
				_primitiveTypes[7],
				_primitiveTypes[5]
			});
			array[11] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[4]
			{
				_primitiveTypes[11],
				_primitiveTypes[4],
				_primitiveTypes[7],
				_primitiveTypes[5]
			});
			array[7] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[2]
			{
				_primitiveTypes[7],
				_primitiveTypes[5]
			});
			InitializeSpatialPromotionGroup(array, new PrimitiveTypeKind[7]
			{
				PrimitiveTypeKind.GeographyPoint,
				PrimitiveTypeKind.GeographyLineString,
				PrimitiveTypeKind.GeographyPolygon,
				PrimitiveTypeKind.GeographyMultiPoint,
				PrimitiveTypeKind.GeographyMultiLineString,
				PrimitiveTypeKind.GeographyMultiPolygon,
				PrimitiveTypeKind.GeographyCollection
			}, PrimitiveTypeKind.Geography);
			InitializeSpatialPromotionGroup(array, new PrimitiveTypeKind[7]
			{
				PrimitiveTypeKind.GeometryPoint,
				PrimitiveTypeKind.GeometryLineString,
				PrimitiveTypeKind.GeometryPolygon,
				PrimitiveTypeKind.GeometryMultiPoint,
				PrimitiveTypeKind.GeometryMultiLineString,
				PrimitiveTypeKind.GeometryMultiPolygon,
				PrimitiveTypeKind.GeometryCollection
			}, PrimitiveTypeKind.Geometry);
			Interlocked.CompareExchange(ref _promotionTypes, array, null);
		}
	}

	private void InitializeSpatialPromotionGroup(ReadOnlyCollection<PrimitiveType>[] promotionTypes, PrimitiveTypeKind[] promotableKinds, PrimitiveTypeKind baseKind)
	{
		foreach (PrimitiveTypeKind primitiveTypeKind in promotableKinds)
		{
			promotionTypes[(int)primitiveTypeKind] = new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[2]
			{
				_primitiveTypes[(int)primitiveTypeKind],
				_primitiveTypes[(int)baseKind]
			});
		}
	}

	internal TypeUsage GetCanonicalModelTypeUsage(PrimitiveTypeKind primitiveTypeKind)
	{
		if (_canonicalModelTypes == null)
		{
			InitializeCanonicalModelTypes();
		}
		return _canonicalModelTypes[(int)primitiveTypeKind];
	}

	private void InitializeCanonicalModelTypes()
	{
		InitializePrimitiveTypes();
		TypeUsage[] array = new TypeUsage[32];
		for (int i = 0; i < 32; i++)
		{
			TypeUsage typeUsage = TypeUsage.CreateDefaultTypeUsage(_primitiveTypes[i]);
			array[i] = typeUsage;
		}
		Interlocked.CompareExchange(ref _canonicalModelTypes, array, null);
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

	internal TypeUsage ForgetScalarConstraints(TypeUsage type)
	{
		if (type.EdmType is PrimitiveType primitiveType)
		{
			return GetCanonicalModelTypeUsage(primitiveType.PrimitiveTypeKind);
		}
		return type;
	}

	protected override XmlReader GetDbInformation(string informationType)
	{
		throw new NotImplementedException();
	}
}
