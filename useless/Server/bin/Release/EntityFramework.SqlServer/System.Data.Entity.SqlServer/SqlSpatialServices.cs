using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Reflection;

namespace System.Data.Entity.SqlServer;

[Serializable]
public class SqlSpatialServices : DbSpatialServices
{
	internal static readonly SqlSpatialServices Instance = new SqlSpatialServices();

	private static Dictionary<string, SqlSpatialServices> _otherSpatialServices;

	[NonSerialized]
	private readonly SqlTypesAssemblyLoader _loader;

	public override bool NativeTypesAvailable => (_loader ?? SqlTypesAssemblyLoader.DefaultInstance).TryGetSqlTypesAssembly() != null;

	internal SqlTypesAssembly SqlTypes => (_loader ?? SqlTypesAssemblyLoader.DefaultInstance).GetSqlTypesAssembly();

	internal SqlSpatialServices()
	{
	}

	internal SqlSpatialServices(SqlTypesAssemblyLoader loader)
	{
		_loader = loader;
	}

	private static bool TryGetSpatialServiceFromAssembly(Assembly assembly, out SqlSpatialServices services)
	{
		if (_otherSpatialServices == null || !_otherSpatialServices.TryGetValue(assembly.FullName, out services))
		{
			lock (Instance)
			{
				if (_otherSpatialServices == null || !_otherSpatialServices.TryGetValue(assembly.FullName, out services))
				{
					if (SqlTypesAssemblyLoader.DefaultInstance.TryGetSqlTypesAssembly(assembly, out var sqlAssembly))
					{
						if (_otherSpatialServices == null)
						{
							_otherSpatialServices = new Dictionary<string, SqlSpatialServices>(1);
						}
						services = new SqlSpatialServices(new SqlTypesAssemblyLoader(sqlAssembly));
						_otherSpatialServices.Add(assembly.FullName, services);
					}
					else
					{
						services = null;
					}
				}
			}
		}
		return services != null;
	}

	public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
	{
		Check.NotNull<DbGeographyWellKnownValue>(wellKnownValue, "wellKnownValue");
		object obj = null;
		if (wellKnownValue.WellKnownText != null)
		{
			return SqlTypes.SqlTypesGeographyFromText(wellKnownValue.WellKnownText, wellKnownValue.CoordinateSystemId);
		}
		if (wellKnownValue.WellKnownBinary != null)
		{
			return SqlTypes.SqlTypesGeographyFromBinary(wellKnownValue.WellKnownBinary, wellKnownValue.CoordinateSystemId);
		}
		throw new ArgumentException(Strings.Spatial_WellKnownGeographyValueNotValid, "wellKnownValue");
	}

	public override DbGeography GeographyFromProviderValue(object providerValue)
	{
		Check.NotNull(providerValue, "providerValue");
		object obj = NormalizeProviderValue(providerValue, SqlTypes.SqlGeographyType);
		if (!SqlTypes.IsSqlGeographyNull(obj))
		{
			return DbSpatialServices.CreateGeography((DbSpatialServices)(object)this, obj);
		}
		return null;
	}

	private object NormalizeProviderValue(object providerValue, Type expectedSpatialType)
	{
		Type type = providerValue.GetType();
		if (type != expectedSpatialType)
		{
			if (TryGetSpatialServiceFromAssembly(providerValue.GetType().Assembly(), out var services))
			{
				if (expectedSpatialType == SqlTypes.SqlGeographyType)
				{
					if (type == services.SqlTypes.SqlGeographyType)
					{
						return ConvertToSqlValue(((DbSpatialServices)services).GeographyFromProviderValue(providerValue), "providerValue");
					}
				}
				else if (type == services.SqlTypes.SqlGeometryType)
				{
					return ConvertToSqlValue(((DbSpatialServices)services).GeometryFromProviderValue(providerValue), "providerValue");
				}
			}
			throw new ArgumentException(Strings.SqlSpatialServices_ProviderValueNotSqlType(expectedSpatialType.AssemblyQualifiedName), "providerValue");
		}
		return providerValue;
	}

	public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		return CreateWellKnownValue(geographyValue.AsSpatialValue(), (Func<Exception>)(() => new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoSrid, "geographyValue")), (Func<Exception>)(() => new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoWkbOrWkt, "geographyValue")), (Func<int, byte[], string, DbGeographyWellKnownValue>)((int coordinateSystemId, byte[] wkb, string wkt) => new DbGeographyWellKnownValue
		{
			CoordinateSystemId = coordinateSystemId,
			WellKnownBinary = wkb,
			WellKnownText = wkt
		}));
	}

	public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
	{
		Check.NotNull<DbGeometryWellKnownValue>(wellKnownValue, "wellKnownValue");
		object obj = null;
		if (wellKnownValue.WellKnownText != null)
		{
			return SqlTypes.SqlTypesGeometryFromText(wellKnownValue.WellKnownText, wellKnownValue.CoordinateSystemId);
		}
		if (wellKnownValue.WellKnownBinary != null)
		{
			return SqlTypes.SqlTypesGeometryFromBinary(wellKnownValue.WellKnownBinary, wellKnownValue.CoordinateSystemId);
		}
		throw new ArgumentException(Strings.Spatial_WellKnownGeometryValueNotValid, "wellKnownValue");
	}

	public override DbGeometry GeometryFromProviderValue(object providerValue)
	{
		Check.NotNull(providerValue, "providerValue");
		object obj = NormalizeProviderValue(providerValue, SqlTypes.SqlGeometryType);
		if (!SqlTypes.IsSqlGeometryNull(obj))
		{
			return DbSpatialServices.CreateGeometry((DbSpatialServices)(object)this, obj);
		}
		return null;
	}

	public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		return CreateWellKnownValue(geometryValue.AsSpatialValue(), (Func<Exception>)(() => new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoSrid, "geometryValue")), (Func<Exception>)(() => new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoWkbOrWkt, "geometryValue")), (Func<int, byte[], string, DbGeometryWellKnownValue>)((int coordinateSystemId, byte[] wkb, string wkt) => new DbGeometryWellKnownValue
		{
			CoordinateSystemId = coordinateSystemId,
			WellKnownBinary = wkb,
			WellKnownText = wkt
		}));
	}

	private static TValue CreateWellKnownValue<TValue>(IDbSpatialValue spatialValue, Func<Exception> onMissingcoordinateSystemId, Func<Exception> onMissingWkbAndWkt, Func<int, byte[], string, TValue> onValidValue)
	{
		int? coordinateSystemId = spatialValue.CoordinateSystemId;
		if (!coordinateSystemId.HasValue)
		{
			throw onMissingcoordinateSystemId();
		}
		string wellKnownText = spatialValue.WellKnownText;
		if (wellKnownText != null)
		{
			return onValidValue(coordinateSystemId.Value, null, wellKnownText);
		}
		byte[] wellKnownBinary = spatialValue.WellKnownBinary;
		if (wellKnownBinary != null)
		{
			return onValidValue(coordinateSystemId.Value, wellKnownBinary, null);
		}
		throw onMissingWkbAndWkt();
	}

	public override string AsTextIncludingElevationAndMeasure(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		return SqlTypes.GeographyAsTextZM(geographyValue);
	}

	public override string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		return SqlTypes.GeometryAsTextZM(geometryValue);
	}

	private object ConvertToSqlValue(DbGeography geographyValue, string argumentName)
	{
		if (geographyValue == null)
		{
			return null;
		}
		return SqlTypes.ConvertToSqlTypesGeography(geographyValue);
	}

	private object ConvertToSqlValue(DbGeometry geometryValue, string argumentName)
	{
		if (geometryValue == null)
		{
			return null;
		}
		return SqlTypes.ConvertToSqlTypesGeometry(geometryValue);
	}

	private object ConvertToSqlBytes(byte[] binaryValue, string argumentName)
	{
		if (binaryValue == null)
		{
			return null;
		}
		return SqlTypes.SqlBytesFromByteArray(binaryValue);
	}

	private object ConvertToSqlChars(string stringValue, string argumentName)
	{
		if (stringValue == null)
		{
			return null;
		}
		return SqlTypes.SqlCharsFromString(stringValue);
	}

	private object ConvertToSqlString(string stringValue, string argumentName)
	{
		if (stringValue == null)
		{
			return null;
		}
		return SqlTypes.SqlStringFromString(stringValue);
	}

	private object ConvertToSqlXml(string stringValue, string argumentName)
	{
		if (stringValue == null)
		{
			return null;
		}
		return SqlTypes.SqlXmlFromString(stringValue);
	}

	private bool ConvertSqlBooleanToBoolean(object sqlBoolean)
	{
		return SqlTypes.SqlBooleanToBoolean(sqlBoolean);
	}

	private bool? ConvertSqlBooleanToNullableBoolean(object sqlBoolean)
	{
		return SqlTypes.SqlBooleanToNullableBoolean(sqlBoolean);
	}

	private byte[] ConvertSqlBytesToBinary(object sqlBytes)
	{
		return SqlTypes.SqlBytesToByteArray(sqlBytes);
	}

	private string ConvertSqlCharsToString(object sqlCharsValue)
	{
		return SqlTypes.SqlCharsToString(sqlCharsValue);
	}

	private string ConvertSqlStringToString(object sqlCharsValue)
	{
		return SqlTypes.SqlStringToString(sqlCharsValue);
	}

	private double ConvertSqlDoubleToDouble(object sqlDoubleValue)
	{
		return SqlTypes.SqlDoubleToDouble(sqlDoubleValue);
	}

	private double? ConvertSqlDoubleToNullableDouble(object sqlDoubleValue)
	{
		return SqlTypes.SqlDoubleToNullableDouble(sqlDoubleValue);
	}

	private int ConvertSqlInt32ToInt(object sqlInt32Value)
	{
		return SqlTypes.SqlInt32ToInt(sqlInt32Value);
	}

	private int? ConvertSqlInt32ToNullableInt(object sqlInt32Value)
	{
		return SqlTypes.SqlInt32ToNullableInt(sqlInt32Value);
	}

	private string ConvertSqlXmlToString(object sqlXmlValue)
	{
		return SqlTypes.SqlXmlToString(sqlXmlValue);
	}

	public override DbGeography GeographyFromText(string wellKnownText)
	{
		object obj = ConvertToSqlString(wellKnownText, "wellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyParse.Value.Invoke(null, new object[1] { obj });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyFromText(string wellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(wellKnownText, "wellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStGeomFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyPointFromText(string pointWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(pointWellKnownText, "pointWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStPointFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyLineFromText(string lineWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(lineWellKnownText, "lineWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStLineFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyPolygonFromText(string polygonWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(polygonWellKnownText, "polygonWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStPolyFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(multiPointWellKnownText, "multiPointWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStmPointFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(multiLineWellKnownText, "multiLineWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStmLineFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(multiPolygonKnownText, "multiPolygonWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStmPolyFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(geographyCollectionWellKnownText, "geographyCollectionWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeographyStGeomCollFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyFromBinary(byte[] wellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(wellKnownBinary, "wellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStGeomFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyFromBinary(byte[] wellKnownBinary)
	{
		object obj = ConvertToSqlBytes(wellKnownBinary, "wellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStGeomFromWkb.Value.Invoke(null, new object[2] { obj, 4326 });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(pointWellKnownBinary, "pointWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStPointFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(lineWellKnownBinary, "lineWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStLineFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(polygonWellKnownBinary, "polygonWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStPolyFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(multiPointWellKnownBinary, "multiPointWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStmPointFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(multiLineWellKnownBinary, "multiLineWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStmLineFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(multiPolygonWellKnownBinary, "multiPolygonWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStmPolyFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(geographyCollectionWellKnownBinary, "geographyCollectionWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeographyStGeomCollFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyFromGml(string geographyMarkup)
	{
		object obj = ConvertToSqlXml(geographyMarkup, "geographyMarkup");
		object obj2 = SqlTypes.SmiSqlGeographyGeomFromGml.Value.Invoke(null, new object[2] { obj, 4326 });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GeographyFromGml(string geographyMarkup, int coordinateSystemId)
	{
		object obj = ConvertToSqlXml(geographyMarkup, "geographyMarkup");
		object obj2 = SqlTypes.SmiSqlGeographyGeomFromGml.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override int GetCoordinateSystemId(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object value = SqlTypes.IpiSqlGeographyStSrid.Value.GetValue(obj, null);
		return ConvertSqlInt32ToInt(value);
	}

	public override string GetSpatialTypeName(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlCharsValue = SqlTypes.ImiSqlGeographyStGeometryType.Value.Invoke(obj, new object[0]);
		return ConvertSqlStringToString(sqlCharsValue);
	}

	public override int GetDimension(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeographyStDimension.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToInt(sqlInt32Value);
	}

	public override byte[] AsBinary(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlBytes = SqlTypes.ImiSqlGeographyStAsBinary.Value.Invoke(obj, new object[0]);
		return ConvertSqlBytesToBinary(sqlBytes);
	}

	public override string AsGml(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlXmlValue = SqlTypes.ImiSqlGeographyAsGml.Value.Invoke(obj, new object[0]);
		return ConvertSqlXmlToString(sqlXmlValue);
	}

	public override string AsText(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlCharsValue = SqlTypes.ImiSqlGeographyStAsText.Value.Invoke(obj, new object[0]);
		return ConvertSqlCharsToString(sqlCharsValue);
	}

	public override bool GetIsEmpty(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlBoolean = SqlTypes.ImiSqlGeographyStIsEmpty.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object sqlBoolean = SqlTypes.ImiSqlGeographyStEquals.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object sqlBoolean = SqlTypes.ImiSqlGeographyStDisjoint.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object sqlBoolean = SqlTypes.ImiSqlGeographyStIntersects.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override DbGeography Buffer(DbGeography geographyValue, double distance)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = SqlTypes.ImiSqlGeographyStBuffer.Value.Invoke(obj, new object[1] { distance });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object sqlDoubleValue = SqlTypes.ImiSqlGeographyStDistance.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlDoubleToDouble(sqlDoubleValue);
	}

	public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object obj3 = SqlTypes.ImiSqlGeographyStIntersection.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj3);
	}

	public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object obj3 = SqlTypes.ImiSqlGeographyStUnion.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj3);
	}

	public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object obj3 = SqlTypes.ImiSqlGeographyStDifference.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj3);
	}

	public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = ConvertToSqlValue(otherGeography, "otherGeography");
		object obj3 = SqlTypes.ImiSqlGeographyStSymDifference.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj3);
	}

	public override int? GetElementCount(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeographyStNumGeometries.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToNullableInt(sqlInt32Value);
	}

	public override DbGeography ElementAt(DbGeography geographyValue, int index)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = SqlTypes.ImiSqlGeographyStGeometryN.Value.Invoke(obj, new object[1] { index });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override double? GetLatitude(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object value = SqlTypes.IpiSqlGeographyLat.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetLongitude(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object value = SqlTypes.IpiSqlGeographyLong.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetElevation(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object value = SqlTypes.IpiSqlGeographyZ.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetMeasure(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object value = SqlTypes.IpiSqlGeographyM.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetLength(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlDoubleValue = SqlTypes.ImiSqlGeographyStLength.Value.Invoke(obj, new object[0]);
		return ConvertSqlDoubleToNullableDouble(sqlDoubleValue);
	}

	public override DbGeography GetStartPoint(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = SqlTypes.ImiSqlGeographyStStartPoint.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override DbGeography GetEndPoint(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = SqlTypes.ImiSqlGeographyStEndPoint.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override bool? GetIsClosed(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlBoolean = SqlTypes.ImiSqlGeographyStIsClosed.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToNullableBoolean(sqlBoolean);
	}

	public override int? GetPointCount(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeographyStNumPoints.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToNullableInt(sqlInt32Value);
	}

	public override DbGeography PointAt(DbGeography geographyValue, int index)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object obj2 = SqlTypes.ImiSqlGeographyStPointN.Value.Invoke(obj, new object[1] { index });
		return ((DbSpatialServices)this).GeographyFromProviderValue(obj2);
	}

	public override double? GetArea(DbGeography geographyValue)
	{
		Check.NotNull<DbGeography>(geographyValue, "geographyValue");
		object obj = ConvertToSqlValue(geographyValue, "geographyValue");
		object sqlDoubleValue = SqlTypes.ImiSqlGeographyStArea.Value.Invoke(obj, new object[0]);
		return ConvertSqlDoubleToNullableDouble(sqlDoubleValue);
	}

	public override DbGeometry GeometryFromText(string wellKnownText)
	{
		object obj = ConvertToSqlString(wellKnownText, "wellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryParse.Value.Invoke(null, new object[1] { obj });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryFromText(string wellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(wellKnownText, "wellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStGeomFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryPointFromText(string pointWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(pointWellKnownText, "pointWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStPointFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryLineFromText(string lineWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(lineWellKnownText, "lineWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStLineFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryPolygonFromText(string polygonWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(polygonWellKnownText, "polygonWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStPolyFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(multiPointWellKnownText, "multiPointWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStmPointFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(multiLineWellKnownText, "multiLineWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStmLineFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(multiPolygonKnownText, "multiPolygonKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStmPolyFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId)
	{
		object obj = ConvertToSqlChars(geometryCollectionWellKnownText, "geometryCollectionWellKnownText");
		object obj2 = SqlTypes.SmiSqlGeometryStGeomCollFromText.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryFromBinary(byte[] wellKnownBinary)
	{
		object obj = ConvertToSqlBytes(wellKnownBinary, "wellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStGeomFromWkb.Value.Invoke(null, new object[2] { obj, 0 });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryFromBinary(byte[] wellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(wellKnownBinary, "wellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStGeomFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(pointWellKnownBinary, "pointWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStPointFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(lineWellKnownBinary, "lineWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStLineFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(polygonWellKnownBinary, "polygonWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStPolyFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(multiPointWellKnownBinary, "multiPointWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStmPointFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(multiLineWellKnownBinary, "multiLineWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStmLineFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(multiPolygonWellKnownBinary, "multiPolygonWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStmPolyFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId)
	{
		object obj = ConvertToSqlBytes(geometryCollectionWellKnownBinary, "geometryCollectionWellKnownBinary");
		object obj2 = SqlTypes.SmiSqlGeometryStGeomCollFromWkb.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryFromGml(string geometryMarkup)
	{
		object obj = ConvertToSqlXml(geometryMarkup, "geometryMarkup");
		object obj2 = SqlTypes.SmiSqlGeometryGeomFromGml.Value.Invoke(null, new object[2] { obj, 0 });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GeometryFromGml(string geometryMarkup, int coordinateSystemId)
	{
		object obj = ConvertToSqlXml(geometryMarkup, "geometryMarkup");
		object obj2 = SqlTypes.SmiSqlGeometryGeomFromGml.Value.Invoke(null, new object[2] { obj, coordinateSystemId });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override int GetCoordinateSystemId(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object value = SqlTypes.IpiSqlGeometryStSrid.Value.GetValue(obj, null);
		return ConvertSqlInt32ToInt(value);
	}

	public override string GetSpatialTypeName(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlCharsValue = SqlTypes.ImiSqlGeometryStGeometryType.Value.Invoke(obj, new object[0]);
		return ConvertSqlStringToString(sqlCharsValue);
	}

	public override int GetDimension(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeometryStDimension.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToInt(sqlInt32Value);
	}

	public override DbGeometry GetEnvelope(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStEnvelope.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override byte[] AsBinary(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlBytes = SqlTypes.ImiSqlGeometryStAsBinary.Value.Invoke(obj, new object[0]);
		return ConvertSqlBytesToBinary(sqlBytes);
	}

	public override string AsGml(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlXmlValue = SqlTypes.ImiSqlGeometryAsGml.Value.Invoke(obj, new object[0]);
		return ConvertSqlXmlToString(sqlXmlValue);
	}

	public override string AsText(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlCharsValue = SqlTypes.ImiSqlGeometryStAsText.Value.Invoke(obj, new object[0]);
		return ConvertSqlCharsToString(sqlCharsValue);
	}

	public override bool GetIsEmpty(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStIsEmpty.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool GetIsSimple(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStIsSimple.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override DbGeometry GetBoundary(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStBoundary.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override bool GetIsValid(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStIsValid.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStEquals.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStDisjoint.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStIntersects.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStTouches.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStCrosses.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStWithin.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStContains.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStOverlaps.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStRelate.Value.Invoke(obj, new object[2] { obj2, matrix });
		return ConvertSqlBooleanToBoolean(sqlBoolean);
	}

	public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStBuffer.Value.Invoke(obj, new object[1] { distance });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object sqlDoubleValue = SqlTypes.ImiSqlGeometryStDistance.Value.Invoke(obj, new object[1] { obj2 });
		return ConvertSqlDoubleToDouble(sqlDoubleValue);
	}

	public override DbGeometry GetConvexHull(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStConvexHull.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object obj3 = SqlTypes.ImiSqlGeometryStIntersection.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj3);
	}

	public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object obj3 = SqlTypes.ImiSqlGeometryStUnion.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj3);
	}

	public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object obj3 = SqlTypes.ImiSqlGeometryStDifference.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj3);
	}

	public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = ConvertToSqlValue(otherGeometry, "otherGeometry");
		object obj3 = SqlTypes.ImiSqlGeometryStSymDifference.Value.Invoke(obj, new object[1] { obj2 });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj3);
	}

	public override int? GetElementCount(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeometryStNumGeometries.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToNullableInt(sqlInt32Value);
	}

	public override DbGeometry ElementAt(DbGeometry geometryValue, int index)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStGeometryN.Value.Invoke(obj, new object[1] { index });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override double? GetXCoordinate(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object value = SqlTypes.IpiSqlGeometryStx.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetYCoordinate(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object value = SqlTypes.IpiSqlGeometrySty.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetElevation(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object value = SqlTypes.IpiSqlGeometryZ.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetMeasure(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object value = SqlTypes.IpiSqlGeometryM.Value.GetValue(obj, null);
		return ConvertSqlDoubleToNullableDouble(value);
	}

	public override double? GetLength(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlDoubleValue = SqlTypes.ImiSqlGeometryStLength.Value.Invoke(obj, new object[0]);
		return ConvertSqlDoubleToNullableDouble(sqlDoubleValue);
	}

	public override DbGeometry GetStartPoint(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStStartPoint.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GetEndPoint(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStEndPoint.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override bool? GetIsClosed(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStIsClosed.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToNullableBoolean(sqlBoolean);
	}

	public override bool? GetIsRing(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlBoolean = SqlTypes.ImiSqlGeometryStIsRing.Value.Invoke(obj, new object[0]);
		return ConvertSqlBooleanToNullableBoolean(sqlBoolean);
	}

	public override int? GetPointCount(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeometryStNumPoints.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToNullableInt(sqlInt32Value);
	}

	public override DbGeometry PointAt(DbGeometry geometryValue, int index)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStPointN.Value.Invoke(obj, new object[1] { index });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override double? GetArea(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlDoubleValue = SqlTypes.ImiSqlGeometryStArea.Value.Invoke(obj, new object[0]);
		return ConvertSqlDoubleToNullableDouble(sqlDoubleValue);
	}

	public override DbGeometry GetCentroid(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStCentroid.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStPointOnSurface.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStExteriorRing.Value.Invoke(obj, new object[0]);
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}

	public override int? GetInteriorRingCount(DbGeometry geometryValue)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object sqlInt32Value = SqlTypes.ImiSqlGeometryStNumInteriorRing.Value.Invoke(obj, new object[0]);
		return ConvertSqlInt32ToNullableInt(sqlInt32Value);
	}

	public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int index)
	{
		Check.NotNull<DbGeometry>(geometryValue, "geometryValue");
		object obj = ConvertToSqlValue(geometryValue, "geometryValue");
		object obj2 = SqlTypes.ImiSqlGeometryStInteriorRingN.Value.Invoke(obj, new object[1] { index });
		return ((DbSpatialServices)this).GeometryFromProviderValue(obj2);
	}
}
