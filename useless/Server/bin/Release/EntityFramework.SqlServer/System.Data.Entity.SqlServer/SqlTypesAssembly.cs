using System.Data.Entity.Hierarchy;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Utilities;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

namespace System.Data.Entity.SqlServer;

internal class SqlTypesAssembly
{
	private readonly Func<object, bool> sqlBooleanToBoolean;

	private readonly Func<object, bool?> sqlBooleanToNullableBoolean;

	private readonly Func<byte[], object> sqlBytesFromByteArray;

	private readonly Func<object, byte[]> sqlBytesToByteArray;

	private readonly Func<string, object> sqlStringFromString;

	private readonly Func<string, object> sqlCharsFromString;

	private readonly Func<object, string> sqlCharsToString;

	private readonly Func<object, string> sqlStringToString;

	private readonly Func<object, double> sqlDoubleToDouble;

	private readonly Func<object, double?> sqlDoubleToNullableDouble;

	private readonly Func<object, int> sqlInt32ToInt;

	private readonly Func<object, int?> sqlInt32ToNullableInt;

	private readonly Func<XmlReader, object> sqlXmlFromXmlReader;

	private readonly Func<object, string> sqlXmlToString;

	private readonly Func<object, bool> isSqlGeographyNull;

	private readonly Func<object, bool> isSqlGeometryNull;

	private readonly Func<object, object> geographyAsTextZMAsSqlChars;

	private readonly Func<object, object> geometryAsTextZMAsSqlChars;

	private readonly Func<string, object> sqlHierarchyIdParse;

	private readonly Func<string, int, object> sqlGeographyFromWKTString;

	private readonly Func<byte[], int, object> sqlGeographyFromWKBByteArray;

	private readonly Func<XmlReader, int, object> sqlGeographyFromGMLReader;

	private readonly Func<string, int, object> sqlGeometryFromWKTString;

	private readonly Func<byte[], int, object> sqlGeometryFromWKBByteArray;

	private readonly Func<XmlReader, int, object> sqlGeometryFromGMLReader;

	private readonly Lazy<MethodInfo> _smiSqlGeographyParse;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStPointFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStLineFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStPolyFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStmPointFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStmLineFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStmPolyFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomCollFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStPointFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStLineFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStPolyFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStmPointFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStmLineFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStmPolyFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomCollFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeographyGeomFromGml;

	private readonly Lazy<PropertyInfo> _ipiSqlGeographyStSrid;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStGeometryType;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStDimension;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStAsBinary;

	private readonly Lazy<MethodInfo> _imiSqlGeographyAsGml;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStAsText;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStIsEmpty;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStEquals;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStDisjoint;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStIntersects;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStBuffer;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStDistance;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStIntersection;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStUnion;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStDifference;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStSymDifference;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStNumGeometries;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStGeometryN;

	private readonly Lazy<PropertyInfo> _ipiSqlGeographyLat;

	private readonly Lazy<PropertyInfo> _ipiSqlGeographyLong;

	private readonly Lazy<PropertyInfo> _ipiSqlGeographyZ;

	private readonly Lazy<PropertyInfo> _ipiSqlGeographyM;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStLength;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStStartPoint;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStEndPoint;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStIsClosed;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStNumPoints;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStPointN;

	private readonly Lazy<MethodInfo> _imiSqlGeographyStArea;

	private readonly Lazy<MethodInfo> _smiSqlGeometryParse;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStPointFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStLineFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStPolyFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStmPointFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStmLineFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStmPolyFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomCollFromText;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStPointFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStLineFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStPolyFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStmPointFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStmLineFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStmPolyFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomCollFromWkb;

	private readonly Lazy<MethodInfo> _smiSqlGeometryGeomFromGml;

	private readonly Lazy<PropertyInfo> _ipiSqlGeometryStSrid;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStGeometryType;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStDimension;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStEnvelope;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStAsBinary;

	private readonly Lazy<MethodInfo> _imiSqlGeometryAsGml;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStAsText;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIsEmpty;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIsSimple;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStBoundary;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIsValid;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStEquals;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStDisjoint;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIntersects;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStTouches;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStCrosses;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStWithin;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStContains;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStOverlaps;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStRelate;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStBuffer;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStDistance;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStConvexHull;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIntersection;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStUnion;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStDifference;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStSymDifference;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStNumGeometries;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStGeometryN;

	private readonly Lazy<PropertyInfo> _ipiSqlGeometryStx;

	private readonly Lazy<PropertyInfo> _ipiSqlGeometrySty;

	private readonly Lazy<PropertyInfo> _ipiSqlGeometryZ;

	private readonly Lazy<PropertyInfo> _ipiSqlGeometryM;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStLength;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStStartPoint;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStEndPoint;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIsClosed;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStIsRing;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStNumPoints;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStPointN;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStArea;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStCentroid;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStPointOnSurface;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStExteriorRing;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStNumInteriorRing;

	private readonly Lazy<MethodInfo> _imiSqlGeometryStInteriorRingN;

	internal Type SqlBooleanType { get; private set; }

	internal Type SqlBytesType { get; private set; }

	internal Type SqlCharsType { get; private set; }

	internal Type SqlStringType { get; private set; }

	internal Type SqlDoubleType { get; private set; }

	internal Type SqlInt32Type { get; private set; }

	internal Type SqlXmlType { get; private set; }

	internal Type SqlHierarchyIdType { get; private set; }

	internal Type SqlGeographyType { get; private set; }

	internal Type SqlGeometryType { get; private set; }

	public Lazy<MethodInfo> SmiSqlGeographyParse => _smiSqlGeographyParse;

	public Lazy<MethodInfo> SmiSqlGeographyStGeomFromText => _smiSqlGeographyStGeomFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStPointFromText => _smiSqlGeographyStPointFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStLineFromText => _smiSqlGeographyStLineFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStPolyFromText => _smiSqlGeographyStPolyFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStmPointFromText => _smiSqlGeographyStmPointFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStmLineFromText => _smiSqlGeographyStmLineFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStmPolyFromText => _smiSqlGeographyStmPolyFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStGeomCollFromText => _smiSqlGeographyStGeomCollFromText;

	public Lazy<MethodInfo> SmiSqlGeographyStGeomFromWkb => _smiSqlGeographyStGeomFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStPointFromWkb => _smiSqlGeographyStPointFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStLineFromWkb => _smiSqlGeographyStLineFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStPolyFromWkb => _smiSqlGeographyStPolyFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStmPointFromWkb => _smiSqlGeographyStmPointFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStmLineFromWkb => _smiSqlGeographyStmLineFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStmPolyFromWkb => _smiSqlGeographyStmPolyFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyStGeomCollFromWkb => _smiSqlGeographyStGeomCollFromWkb;

	public Lazy<MethodInfo> SmiSqlGeographyGeomFromGml => _smiSqlGeographyGeomFromGml;

	public Lazy<PropertyInfo> IpiSqlGeographyStSrid => _ipiSqlGeographyStSrid;

	public Lazy<MethodInfo> ImiSqlGeographyStGeometryType => _imiSqlGeographyStGeometryType;

	public Lazy<MethodInfo> ImiSqlGeographyStDimension => _imiSqlGeographyStDimension;

	public Lazy<MethodInfo> ImiSqlGeographyStAsBinary => _imiSqlGeographyStAsBinary;

	public Lazy<MethodInfo> ImiSqlGeographyAsGml => _imiSqlGeographyAsGml;

	public Lazy<MethodInfo> ImiSqlGeographyStAsText => _imiSqlGeographyStAsText;

	public Lazy<MethodInfo> ImiSqlGeographyStIsEmpty => _imiSqlGeographyStIsEmpty;

	public Lazy<MethodInfo> ImiSqlGeographyStEquals => _imiSqlGeographyStEquals;

	public Lazy<MethodInfo> ImiSqlGeographyStDisjoint => _imiSqlGeographyStDisjoint;

	public Lazy<MethodInfo> ImiSqlGeographyStIntersects => _imiSqlGeographyStIntersects;

	public Lazy<MethodInfo> ImiSqlGeographyStBuffer => _imiSqlGeographyStBuffer;

	public Lazy<MethodInfo> ImiSqlGeographyStDistance => _imiSqlGeographyStDistance;

	public Lazy<MethodInfo> ImiSqlGeographyStIntersection => _imiSqlGeographyStIntersection;

	public Lazy<MethodInfo> ImiSqlGeographyStUnion => _imiSqlGeographyStUnion;

	public Lazy<MethodInfo> ImiSqlGeographyStDifference => _imiSqlGeographyStDifference;

	public Lazy<MethodInfo> ImiSqlGeographyStSymDifference => _imiSqlGeographyStSymDifference;

	public Lazy<MethodInfo> ImiSqlGeographyStNumGeometries => _imiSqlGeographyStNumGeometries;

	public Lazy<MethodInfo> ImiSqlGeographyStGeometryN => _imiSqlGeographyStGeometryN;

	public Lazy<PropertyInfo> IpiSqlGeographyLat => _ipiSqlGeographyLat;

	public Lazy<PropertyInfo> IpiSqlGeographyLong => _ipiSqlGeographyLong;

	public Lazy<PropertyInfo> IpiSqlGeographyZ => _ipiSqlGeographyZ;

	public Lazy<PropertyInfo> IpiSqlGeographyM => _ipiSqlGeographyM;

	public Lazy<MethodInfo> ImiSqlGeographyStLength => _imiSqlGeographyStLength;

	public Lazy<MethodInfo> ImiSqlGeographyStStartPoint => _imiSqlGeographyStStartPoint;

	public Lazy<MethodInfo> ImiSqlGeographyStEndPoint => _imiSqlGeographyStEndPoint;

	public Lazy<MethodInfo> ImiSqlGeographyStIsClosed => _imiSqlGeographyStIsClosed;

	public Lazy<MethodInfo> ImiSqlGeographyStNumPoints => _imiSqlGeographyStNumPoints;

	public Lazy<MethodInfo> ImiSqlGeographyStPointN => _imiSqlGeographyStPointN;

	public Lazy<MethodInfo> ImiSqlGeographyStArea => _imiSqlGeographyStArea;

	public Lazy<MethodInfo> SmiSqlGeometryParse => _smiSqlGeometryParse;

	public Lazy<MethodInfo> SmiSqlGeometryStGeomFromText => _smiSqlGeometryStGeomFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStPointFromText => _smiSqlGeometryStPointFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStLineFromText => _smiSqlGeometryStLineFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStPolyFromText => _smiSqlGeometryStPolyFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStmPointFromText => _smiSqlGeometryStmPointFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStmLineFromText => _smiSqlGeometryStmLineFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStmPolyFromText => _smiSqlGeometryStmPolyFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStGeomCollFromText => _smiSqlGeometryStGeomCollFromText;

	public Lazy<MethodInfo> SmiSqlGeometryStGeomFromWkb => _smiSqlGeometryStGeomFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStPointFromWkb => _smiSqlGeometryStPointFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStLineFromWkb => _smiSqlGeometryStLineFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStPolyFromWkb => _smiSqlGeometryStPolyFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStmPointFromWkb => _smiSqlGeometryStmPointFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStmLineFromWkb => _smiSqlGeometryStmLineFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStmPolyFromWkb => _smiSqlGeometryStmPolyFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryStGeomCollFromWkb => _smiSqlGeometryStGeomCollFromWkb;

	public Lazy<MethodInfo> SmiSqlGeometryGeomFromGml => _smiSqlGeometryGeomFromGml;

	public Lazy<PropertyInfo> IpiSqlGeometryStSrid => _ipiSqlGeometryStSrid;

	public Lazy<MethodInfo> ImiSqlGeometryStGeometryType => _imiSqlGeometryStGeometryType;

	public Lazy<MethodInfo> ImiSqlGeometryStDimension => _imiSqlGeometryStDimension;

	public Lazy<MethodInfo> ImiSqlGeometryStEnvelope => _imiSqlGeometryStEnvelope;

	public Lazy<MethodInfo> ImiSqlGeometryStAsBinary => _imiSqlGeometryStAsBinary;

	public Lazy<MethodInfo> ImiSqlGeometryAsGml => _imiSqlGeometryAsGml;

	public Lazy<MethodInfo> ImiSqlGeometryStAsText => _imiSqlGeometryStAsText;

	public Lazy<MethodInfo> ImiSqlGeometryStIsEmpty => _imiSqlGeometryStIsEmpty;

	public Lazy<MethodInfo> ImiSqlGeometryStIsSimple => _imiSqlGeometryStIsSimple;

	public Lazy<MethodInfo> ImiSqlGeometryStBoundary => _imiSqlGeometryStBoundary;

	public Lazy<MethodInfo> ImiSqlGeometryStIsValid => _imiSqlGeometryStIsValid;

	public Lazy<MethodInfo> ImiSqlGeometryStEquals => _imiSqlGeometryStEquals;

	public Lazy<MethodInfo> ImiSqlGeometryStDisjoint => _imiSqlGeometryStDisjoint;

	public Lazy<MethodInfo> ImiSqlGeometryStIntersects => _imiSqlGeometryStIntersects;

	public Lazy<MethodInfo> ImiSqlGeometryStTouches => _imiSqlGeometryStTouches;

	public Lazy<MethodInfo> ImiSqlGeometryStCrosses => _imiSqlGeometryStCrosses;

	public Lazy<MethodInfo> ImiSqlGeometryStWithin => _imiSqlGeometryStWithin;

	public Lazy<MethodInfo> ImiSqlGeometryStContains => _imiSqlGeometryStContains;

	public Lazy<MethodInfo> ImiSqlGeometryStOverlaps => _imiSqlGeometryStOverlaps;

	public Lazy<MethodInfo> ImiSqlGeometryStRelate => _imiSqlGeometryStRelate;

	public Lazy<MethodInfo> ImiSqlGeometryStBuffer => _imiSqlGeometryStBuffer;

	public Lazy<MethodInfo> ImiSqlGeometryStDistance => _imiSqlGeometryStDistance;

	public Lazy<MethodInfo> ImiSqlGeometryStConvexHull => _imiSqlGeometryStConvexHull;

	public Lazy<MethodInfo> ImiSqlGeometryStIntersection => _imiSqlGeometryStIntersection;

	public Lazy<MethodInfo> ImiSqlGeometryStUnion => _imiSqlGeometryStUnion;

	public Lazy<MethodInfo> ImiSqlGeometryStDifference => _imiSqlGeometryStDifference;

	public Lazy<MethodInfo> ImiSqlGeometryStSymDifference => _imiSqlGeometryStSymDifference;

	public Lazy<MethodInfo> ImiSqlGeometryStNumGeometries => _imiSqlGeometryStNumGeometries;

	public Lazy<MethodInfo> ImiSqlGeometryStGeometryN => _imiSqlGeometryStGeometryN;

	public Lazy<PropertyInfo> IpiSqlGeometryStx => _ipiSqlGeometryStx;

	public Lazy<PropertyInfo> IpiSqlGeometrySty => _ipiSqlGeometrySty;

	public Lazy<PropertyInfo> IpiSqlGeometryZ => _ipiSqlGeometryZ;

	public Lazy<PropertyInfo> IpiSqlGeometryM => _ipiSqlGeometryM;

	public Lazy<MethodInfo> ImiSqlGeometryStLength => _imiSqlGeometryStLength;

	public Lazy<MethodInfo> ImiSqlGeometryStStartPoint => _imiSqlGeometryStStartPoint;

	public Lazy<MethodInfo> ImiSqlGeometryStEndPoint => _imiSqlGeometryStEndPoint;

	public Lazy<MethodInfo> ImiSqlGeometryStIsClosed => _imiSqlGeometryStIsClosed;

	public Lazy<MethodInfo> ImiSqlGeometryStIsRing => _imiSqlGeometryStIsRing;

	public Lazy<MethodInfo> ImiSqlGeometryStNumPoints => _imiSqlGeometryStNumPoints;

	public Lazy<MethodInfo> ImiSqlGeometryStPointN => _imiSqlGeometryStPointN;

	public Lazy<MethodInfo> ImiSqlGeometryStArea => _imiSqlGeometryStArea;

	public Lazy<MethodInfo> ImiSqlGeometryStCentroid => _imiSqlGeometryStCentroid;

	public Lazy<MethodInfo> ImiSqlGeometryStPointOnSurface => _imiSqlGeometryStPointOnSurface;

	public Lazy<MethodInfo> ImiSqlGeometryStExteriorRing => _imiSqlGeometryStExteriorRing;

	public Lazy<MethodInfo> ImiSqlGeometryStNumInteriorRing => _imiSqlGeometryStNumInteriorRing;

	public Lazy<MethodInfo> ImiSqlGeometryStInteriorRingN => _imiSqlGeometryStInteriorRingN;

	public SqlTypesAssembly()
	{
	}

	public SqlTypesAssembly(Assembly sqlSpatialAssembly)
	{
		Type type = sqlSpatialAssembly.GetType("Microsoft.SqlServer.Types.SqlHierarchyId", throwOnError: true);
		Type type2 = sqlSpatialAssembly.GetType("Microsoft.SqlServer.Types.SqlGeography", throwOnError: true);
		Type type3 = sqlSpatialAssembly.GetType("Microsoft.SqlServer.Types.SqlGeometry", throwOnError: true);
		SqlHierarchyIdType = type;
		sqlHierarchyIdParse = CreateStaticConstructorDelegateHierarchyId<string>(type, "Parse");
		SqlGeographyType = type2;
		sqlGeographyFromWKTString = CreateStaticConstructorDelegate<string>(type2, "STGeomFromText");
		sqlGeographyFromWKBByteArray = CreateStaticConstructorDelegate<byte[]>(type2, "STGeomFromWKB");
		sqlGeographyFromGMLReader = CreateStaticConstructorDelegate<XmlReader>(type2, "GeomFromGml");
		SqlGeometryType = type3;
		sqlGeometryFromWKTString = CreateStaticConstructorDelegate<string>(type3, "STGeomFromText");
		sqlGeometryFromWKBByteArray = CreateStaticConstructorDelegate<byte[]>(type3, "STGeomFromWKB");
		sqlGeometryFromGMLReader = CreateStaticConstructorDelegate<XmlReader>(type3, "GeomFromGml");
		MethodInfo publicInstanceMethod = SqlGeometryType.GetPublicInstanceMethod("STAsText");
		SqlCharsType = publicInstanceMethod.ReturnType;
		SqlStringType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
		SqlBooleanType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlBoolean", throwOnError: true);
		SqlBytesType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlBytes", throwOnError: true);
		SqlDoubleType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlDouble", throwOnError: true);
		SqlInt32Type = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlInt32", throwOnError: true);
		SqlXmlType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlXml", throwOnError: true);
		sqlBytesFromByteArray = Expressions.Lambda<byte[], object>("binaryValue", (ParameterExpression bytesVal) => BuildConvertToSqlBytes(bytesVal, SqlBytesType)).Compile();
		sqlStringFromString = Expressions.Lambda<string, object>("stringValue", (ParameterExpression stringVal) => BuildConvertToSqlString(stringVal, SqlStringType)).Compile();
		sqlCharsFromString = Expressions.Lambda<string, object>("stringValue", (ParameterExpression stringVal) => BuildConvertToSqlChars(stringVal, SqlCharsType)).Compile();
		sqlXmlFromXmlReader = Expressions.Lambda<XmlReader, object>("readerVaue", (ParameterExpression readerVal) => BuildConvertToSqlXml(readerVal, SqlXmlType)).Compile();
		sqlBooleanToBoolean = Expressions.Lambda<object, bool>("sqlBooleanValue", (ParameterExpression sqlBoolVal) => sqlBoolVal.ConvertTo(SqlBooleanType).ConvertTo<bool>()).Compile();
		sqlBooleanToNullableBoolean = Expressions.Lambda<object, bool?>("sqlBooleanValue", (ParameterExpression sqlBoolVal) => sqlBoolVal.ConvertTo(SqlBooleanType).Property<bool>("IsNull").IfTrueThen(Expressions.Null<bool?>())
			.Else(sqlBoolVal.ConvertTo(SqlBooleanType).ConvertTo<bool>().ConvertTo<bool?>())).Compile();
		sqlBytesToByteArray = Expressions.Lambda<object, byte[]>("sqlBytesValue", (ParameterExpression sqlBytesVal) => sqlBytesVal.ConvertTo(SqlBytesType).Property<byte[]>("Value")).Compile();
		sqlCharsToString = Expressions.Lambda<object, string>("sqlCharsValue", (ParameterExpression sqlCharsVal) => sqlCharsVal.ConvertTo(SqlCharsType).Call("ToSqlString").Property<string>("Value")).Compile();
		sqlStringToString = Expressions.Lambda<object, string>("sqlStringValue", (ParameterExpression sqlStringVal) => sqlStringVal.ConvertTo(SqlStringType).Property<string>("Value")).Compile();
		sqlDoubleToDouble = Expressions.Lambda<object, double>("sqlDoubleValue", (ParameterExpression sqlDoubleVal) => sqlDoubleVal.ConvertTo(SqlDoubleType).ConvertTo<double>()).Compile();
		sqlDoubleToNullableDouble = Expressions.Lambda<object, double?>("sqlDoubleValue", (ParameterExpression sqlDoubleVal) => sqlDoubleVal.ConvertTo(SqlDoubleType).Property<bool>("IsNull").IfTrueThen(Expressions.Null<double?>())
			.Else(sqlDoubleVal.ConvertTo(SqlDoubleType).ConvertTo<double>().ConvertTo<double?>())).Compile();
		sqlInt32ToInt = Expressions.Lambda<object, int>("sqlInt32Value", (ParameterExpression sqlInt32Val) => sqlInt32Val.ConvertTo(SqlInt32Type).ConvertTo<int>()).Compile();
		sqlInt32ToNullableInt = Expressions.Lambda<object, int?>("sqlInt32Value", (ParameterExpression sqlInt32Val) => sqlInt32Val.ConvertTo(SqlInt32Type).Property<bool>("IsNull").IfTrueThen(Expressions.Null<int?>())
			.Else(sqlInt32Val.ConvertTo(SqlInt32Type).ConvertTo<int>().ConvertTo<int?>())).Compile();
		sqlXmlToString = Expressions.Lambda<object, string>("sqlXmlValue", (ParameterExpression sqlXmlVal) => sqlXmlVal.ConvertTo(SqlXmlType).Property<string>("Value")).Compile();
		isSqlGeographyNull = Expressions.Lambda<object, bool>("sqlGeographyValue", (ParameterExpression sqlGeographyValue) => sqlGeographyValue.ConvertTo(SqlGeographyType).Property<bool>("IsNull")).Compile();
		isSqlGeometryNull = Expressions.Lambda<object, bool>("sqlGeometryValue", (ParameterExpression sqlGeometryValue) => sqlGeometryValue.ConvertTo(SqlGeometryType).Property<bool>("IsNull")).Compile();
		geographyAsTextZMAsSqlChars = Expressions.Lambda<object, object>("sqlGeographyValue", (ParameterExpression sqlGeographyValue) => sqlGeographyValue.ConvertTo(SqlGeographyType).Call("AsTextZM")).Compile();
		geometryAsTextZMAsSqlChars = Expressions.Lambda<object, object>("sqlGeometryValue", (ParameterExpression sqlGeometryValue) => sqlGeometryValue.ConvertTo(SqlGeometryType).Call("AsTextZM")).Compile();
		_smiSqlGeographyParse = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("Parse", SqlStringType), isThreadSafe: true);
		_smiSqlGeographyStGeomFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STGeomFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStPointFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STPointFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStLineFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STLineFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStPolyFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STPolyFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStmPointFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STMPointFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStmLineFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STMLineFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStmPolyFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STMPolyFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStGeomCollFromText = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STGeomCollFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStGeomFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STGeomFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStPointFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STPointFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStLineFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STLineFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStPolyFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STPolyFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStmPointFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STMPointFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStmLineFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STMLineFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStmPolyFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STMPolyFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyStGeomCollFromWkb = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("STGeomCollFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeographyGeomFromGml = new Lazy<MethodInfo>(() => FindSqlGeographyStaticMethod("GeomFromGml", SqlXmlType, typeof(int)), isThreadSafe: true);
		_ipiSqlGeographyStSrid = new Lazy<PropertyInfo>(() => FindSqlGeographyProperty("STSrid"), isThreadSafe: true);
		_imiSqlGeographyStGeometryType = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STGeometryType"), isThreadSafe: true);
		_imiSqlGeographyStDimension = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STDimension"), isThreadSafe: true);
		_imiSqlGeographyStAsBinary = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STAsBinary"), isThreadSafe: true);
		_imiSqlGeographyAsGml = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("AsGml"), isThreadSafe: true);
		_imiSqlGeographyStAsText = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STAsText"), isThreadSafe: true);
		_imiSqlGeographyStIsEmpty = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STIsEmpty"), isThreadSafe: true);
		_imiSqlGeographyStEquals = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STEquals", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStDisjoint = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STDisjoint", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStIntersects = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STIntersects", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStBuffer = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STBuffer", typeof(double)), isThreadSafe: true);
		_imiSqlGeographyStDistance = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STDistance", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStIntersection = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STIntersection", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStUnion = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STUnion", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStDifference = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STDifference", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStSymDifference = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STSymDifference", SqlGeographyType), isThreadSafe: true);
		_imiSqlGeographyStNumGeometries = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STNumGeometries"), isThreadSafe: true);
		_imiSqlGeographyStGeometryN = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STGeometryN", typeof(int)), isThreadSafe: true);
		_ipiSqlGeographyLat = new Lazy<PropertyInfo>(() => FindSqlGeographyProperty("Lat"), isThreadSafe: true);
		_ipiSqlGeographyLong = new Lazy<PropertyInfo>(() => FindSqlGeographyProperty("Long"), isThreadSafe: true);
		_ipiSqlGeographyZ = new Lazy<PropertyInfo>(() => FindSqlGeographyProperty("Z"), isThreadSafe: true);
		_ipiSqlGeographyM = new Lazy<PropertyInfo>(() => FindSqlGeographyProperty("M"), isThreadSafe: true);
		_imiSqlGeographyStLength = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STLength"), isThreadSafe: true);
		_imiSqlGeographyStStartPoint = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STStartPoint"), isThreadSafe: true);
		_imiSqlGeographyStEndPoint = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STEndPoint"), isThreadSafe: true);
		_imiSqlGeographyStIsClosed = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STIsClosed"), isThreadSafe: true);
		_imiSqlGeographyStNumPoints = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STNumPoints"), isThreadSafe: true);
		_imiSqlGeographyStPointN = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STPointN", typeof(int)), isThreadSafe: true);
		_imiSqlGeographyStArea = new Lazy<MethodInfo>(() => FindSqlGeographyMethod("STArea"), isThreadSafe: true);
		_smiSqlGeometryParse = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("Parse", SqlStringType), isThreadSafe: true);
		_smiSqlGeometryStGeomFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STGeomFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStPointFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STPointFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStLineFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STLineFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStPolyFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STPolyFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStmPointFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STMPointFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStmLineFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STMLineFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStmPolyFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STMPolyFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStGeomCollFromText = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STGeomCollFromText", SqlCharsType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStGeomFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STGeomFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStPointFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STPointFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStLineFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STLineFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStPolyFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STPolyFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStmPointFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STMPointFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStmLineFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STMLineFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStmPolyFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STMPolyFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryStGeomCollFromWkb = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("STGeomCollFromWKB", SqlBytesType, typeof(int)), isThreadSafe: true);
		_smiSqlGeometryGeomFromGml = new Lazy<MethodInfo>(() => FindSqlGeometryStaticMethod("GeomFromGml", SqlXmlType, typeof(int)), isThreadSafe: true);
		_ipiSqlGeometryStSrid = new Lazy<PropertyInfo>(() => FindSqlGeometryProperty("STSrid"), isThreadSafe: true);
		_imiSqlGeometryStGeometryType = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STGeometryType"), isThreadSafe: true);
		_imiSqlGeometryStDimension = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STDimension"), isThreadSafe: true);
		_imiSqlGeometryStEnvelope = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STEnvelope"), isThreadSafe: true);
		_imiSqlGeometryStAsBinary = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STAsBinary"), isThreadSafe: true);
		_imiSqlGeometryAsGml = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("AsGml"), isThreadSafe: true);
		_imiSqlGeometryStAsText = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STAsText"), isThreadSafe: true);
		_imiSqlGeometryStIsEmpty = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIsEmpty"), isThreadSafe: true);
		_imiSqlGeometryStIsSimple = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIsSimple"), isThreadSafe: true);
		_imiSqlGeometryStBoundary = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STBoundary"), isThreadSafe: true);
		_imiSqlGeometryStIsValid = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIsValid"), isThreadSafe: true);
		_imiSqlGeometryStEquals = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STEquals", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStDisjoint = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STDisjoint", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStIntersects = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIntersects", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStTouches = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STTouches", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStCrosses = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STCrosses", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStWithin = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STWithin", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStContains = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STContains", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStOverlaps = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STOverlaps", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStRelate = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STRelate", SqlGeometryType, typeof(string)), isThreadSafe: true);
		_imiSqlGeometryStBuffer = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STBuffer", typeof(double)), isThreadSafe: true);
		_imiSqlGeometryStDistance = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STDistance", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStConvexHull = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STConvexHull"), isThreadSafe: true);
		_imiSqlGeometryStIntersection = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIntersection", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStUnion = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STUnion", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStDifference = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STDifference", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStSymDifference = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STSymDifference", SqlGeometryType), isThreadSafe: true);
		_imiSqlGeometryStNumGeometries = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STNumGeometries"), isThreadSafe: true);
		_imiSqlGeometryStGeometryN = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STGeometryN", typeof(int)), isThreadSafe: true);
		_ipiSqlGeometryStx = new Lazy<PropertyInfo>(() => FindSqlGeometryProperty("STX"), isThreadSafe: true);
		_ipiSqlGeometrySty = new Lazy<PropertyInfo>(() => FindSqlGeometryProperty("STY"), isThreadSafe: true);
		_ipiSqlGeometryZ = new Lazy<PropertyInfo>(() => FindSqlGeometryProperty("Z"), isThreadSafe: true);
		_ipiSqlGeometryM = new Lazy<PropertyInfo>(() => FindSqlGeometryProperty("M"), isThreadSafe: true);
		_imiSqlGeometryStLength = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STLength"), isThreadSafe: true);
		_imiSqlGeometryStStartPoint = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STStartPoint"), isThreadSafe: true);
		_imiSqlGeometryStEndPoint = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STEndPoint"), isThreadSafe: true);
		_imiSqlGeometryStIsClosed = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIsClosed"), isThreadSafe: true);
		_imiSqlGeometryStIsRing = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STIsRing"), isThreadSafe: true);
		_imiSqlGeometryStNumPoints = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STNumPoints"), isThreadSafe: true);
		_imiSqlGeometryStPointN = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STPointN", typeof(int)), isThreadSafe: true);
		_imiSqlGeometryStArea = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STArea"), isThreadSafe: true);
		_imiSqlGeometryStCentroid = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STCentroid"), isThreadSafe: true);
		_imiSqlGeometryStPointOnSurface = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STPointOnSurface"), isThreadSafe: true);
		_imiSqlGeometryStExteriorRing = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STExteriorRing"), isThreadSafe: true);
		_imiSqlGeometryStNumInteriorRing = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STNumInteriorRing"), isThreadSafe: true);
		_imiSqlGeometryStInteriorRingN = new Lazy<MethodInfo>(() => FindSqlGeometryMethod("STInteriorRingN", typeof(int)), isThreadSafe: true);
	}

	internal bool SqlBooleanToBoolean(object sqlBooleanValue)
	{
		return sqlBooleanToBoolean(sqlBooleanValue);
	}

	internal bool? SqlBooleanToNullableBoolean(object sqlBooleanValue)
	{
		if (sqlBooleanToBoolean == null)
		{
			return null;
		}
		return sqlBooleanToNullableBoolean(sqlBooleanValue);
	}

	internal object SqlBytesFromByteArray(byte[] binaryValue)
	{
		return sqlBytesFromByteArray(binaryValue);
	}

	internal byte[] SqlBytesToByteArray(object sqlBytesValue)
	{
		if (sqlBytesValue == null)
		{
			return null;
		}
		return sqlBytesToByteArray(sqlBytesValue);
	}

	internal object SqlStringFromString(string stringValue)
	{
		return sqlStringFromString(stringValue);
	}

	internal object SqlCharsFromString(string stringValue)
	{
		return sqlCharsFromString(stringValue);
	}

	internal string SqlCharsToString(object sqlCharsValue)
	{
		if (sqlCharsValue == null)
		{
			return null;
		}
		return sqlCharsToString(sqlCharsValue);
	}

	internal string SqlStringToString(object sqlStringValue)
	{
		if (sqlStringValue == null)
		{
			return null;
		}
		return sqlStringToString(sqlStringValue);
	}

	internal double SqlDoubleToDouble(object sqlDoubleValue)
	{
		return sqlDoubleToDouble(sqlDoubleValue);
	}

	internal double? SqlDoubleToNullableDouble(object sqlDoubleValue)
	{
		if (sqlDoubleValue == null)
		{
			return null;
		}
		return sqlDoubleToNullableDouble(sqlDoubleValue);
	}

	internal int SqlInt32ToInt(object sqlInt32Value)
	{
		return sqlInt32ToInt(sqlInt32Value);
	}

	internal int? SqlInt32ToNullableInt(object sqlInt32Value)
	{
		if (sqlInt32Value == null)
		{
			return null;
		}
		return sqlInt32ToNullableInt(sqlInt32Value);
	}

	internal object SqlXmlFromString(string stringValue)
	{
		XmlReader arg = XmlReaderFromString(stringValue);
		return sqlXmlFromXmlReader(arg);
	}

	internal string SqlXmlToString(object sqlXmlValue)
	{
		if (sqlXmlValue == null)
		{
			return null;
		}
		return sqlXmlToString(sqlXmlValue);
	}

	internal bool IsSqlGeographyNull(object sqlGeographyValue)
	{
		if (sqlGeographyValue == null)
		{
			return true;
		}
		return isSqlGeographyNull(sqlGeographyValue);
	}

	internal bool IsSqlGeometryNull(object sqlGeometryValue)
	{
		if (sqlGeometryValue == null)
		{
			return true;
		}
		return isSqlGeometryNull(sqlGeometryValue);
	}

	internal string GeographyAsTextZM(DbGeography geographyValue)
	{
		if (geographyValue == null)
		{
			return null;
		}
		object arg = ConvertToSqlTypesGeography(geographyValue);
		object sqlCharsValue = geographyAsTextZMAsSqlChars(arg);
		return SqlCharsToString(sqlCharsValue);
	}

	internal string GeometryAsTextZM(DbGeometry geometryValue)
	{
		if (geometryValue == null)
		{
			return null;
		}
		object arg = ConvertToSqlTypesGeometry(geometryValue);
		object sqlCharsValue = geometryAsTextZMAsSqlChars(arg);
		return SqlCharsToString(sqlCharsValue);
	}

	internal object ConvertToSqlTypesHierarchyId(HierarchyId hierarchyIdValue)
	{
		return GetSqlTypesHierarchyIdValue(((object)hierarchyIdValue).ToString());
	}

	internal object ConvertToSqlTypesGeography(DbGeography geographyValue)
	{
		return GetSqlTypesSpatialValue(geographyValue.AsSpatialValue(), SqlGeographyType);
	}

	internal object SqlTypesGeographyFromBinary(byte[] wellKnownBinary, int srid)
	{
		return sqlGeographyFromWKBByteArray(wellKnownBinary, srid);
	}

	internal object SqlTypesGeographyFromText(string wellKnownText, int srid)
	{
		return sqlGeographyFromWKTString(wellKnownText, srid);
	}

	internal object ConvertToSqlTypesGeometry(DbGeometry geometryValue)
	{
		return GetSqlTypesSpatialValue(geometryValue.AsSpatialValue(), SqlGeometryType);
	}

	internal object SqlTypesGeometryFromBinary(byte[] wellKnownBinary, int srid)
	{
		return sqlGeometryFromWKBByteArray(wellKnownBinary, srid);
	}

	internal object SqlTypesGeometryFromText(string wellKnownText, int srid)
	{
		return sqlGeometryFromWKTString(wellKnownText, srid);
	}

	private object GetSqlTypesHierarchyIdValue(string hierarchyIdValue)
	{
		return sqlHierarchyIdParse(hierarchyIdValue);
	}

	private object GetSqlTypesSpatialValue(IDbSpatialValue spatialValue, Type requiredProviderValueType)
	{
		object providerValue = spatialValue.ProviderValue;
		if (providerValue != null && providerValue.GetType() == requiredProviderValueType)
		{
			return providerValue;
		}
		int? coordinateSystemId = spatialValue.CoordinateSystemId;
		if (coordinateSystemId.HasValue)
		{
			byte[] wellKnownBinary = spatialValue.WellKnownBinary;
			if (wellKnownBinary != null)
			{
				if (!spatialValue.IsGeography)
				{
					return sqlGeometryFromWKBByteArray(wellKnownBinary, coordinateSystemId.Value);
				}
				return sqlGeographyFromWKBByteArray(wellKnownBinary, coordinateSystemId.Value);
			}
			string wellKnownText = spatialValue.WellKnownText;
			if (wellKnownText != null)
			{
				if (!spatialValue.IsGeography)
				{
					return sqlGeometryFromWKTString(wellKnownText, coordinateSystemId.Value);
				}
				return sqlGeographyFromWKTString(wellKnownText, coordinateSystemId.Value);
			}
			string gmlString = spatialValue.GmlString;
			if (gmlString != null)
			{
				XmlReader arg = XmlReaderFromString(gmlString);
				if (!spatialValue.IsGeography)
				{
					return sqlGeometryFromGMLReader(arg, coordinateSystemId.Value);
				}
				return sqlGeographyFromGMLReader(arg, coordinateSystemId.Value);
			}
		}
		throw spatialValue.NotSqlCompatible();
	}

	private static XmlReader XmlReaderFromString(string stringValue)
	{
		return XmlReader.Create(new StringReader(stringValue));
	}

	private static Func<TArg, object> CreateStaticConstructorDelegateHierarchyId<TArg>(Type hierarchyIdType, string methodName)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(TArg));
		MethodInfo method = hierarchyIdType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
		Expression expression = BuildSqlString(parameterExpression, method.GetParameters()[0].ParameterType);
		return Expression.Lambda<Func<TArg, object>>(Expression.Convert(Expression.Call(null, method, expression), typeof(object)), new ParameterExpression[1] { parameterExpression }).Compile();
	}

	private static Func<TArg, int, object> CreateStaticConstructorDelegate<TArg>(Type spatialType, string methodName)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(TArg));
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(int));
		MethodInfo onlyDeclaredMethod = spatialType.GetOnlyDeclaredMethod(methodName);
		Expression arg = BuildConvertToSqlType(parameterExpression, onlyDeclaredMethod.GetParameters()[0].ParameterType);
		return Expression.Lambda<Func<TArg, int, object>>(Expression.Call(null, onlyDeclaredMethod, arg, parameterExpression2), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	private static Expression BuildConvertToSqlType(Expression toConvert, Type convertTo)
	{
		if (toConvert.Type == typeof(byte[]))
		{
			return BuildConvertToSqlBytes(toConvert, convertTo);
		}
		if (toConvert.Type == typeof(string))
		{
			if (convertTo.Name == "SqlString")
			{
				return BuildConvertToSqlString(toConvert, convertTo);
			}
			return BuildConvertToSqlChars(toConvert, convertTo);
		}
		if (toConvert.Type == typeof(XmlReader))
		{
			return BuildConvertToSqlXml(toConvert, convertTo);
		}
		return toConvert;
	}

	private static Expression BuildConvertToSqlBytes(Expression toConvert, Type sqlBytesType)
	{
		return Expression.New(sqlBytesType.GetDeclaredConstructor(toConvert.Type), toConvert);
	}

	private static Expression BuildConvertToSqlChars(Expression toConvert, Type sqlCharsType)
	{
		Type type = sqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
		ConstructorInfo declaredConstructor = sqlCharsType.GetDeclaredConstructor(type);
		ConstructorInfo declaredConstructor2 = type.GetDeclaredConstructor(typeof(string));
		return Expression.New(declaredConstructor, Expression.New(declaredConstructor2, toConvert));
	}

	private static Expression BuildSqlString(Expression toConvert, Type sqlStringType)
	{
		return Expression.New(sqlStringType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(string) }, null), toConvert);
	}

	private static Expression BuildConvertToSqlString(Expression toConvert, Type sqlStringType)
	{
		return Expression.Convert(Expression.New(sqlStringType.GetDeclaredConstructor(typeof(string)), toConvert), typeof(object));
	}

	private static Expression BuildConvertToSqlXml(Expression toConvert, Type sqlXmlType)
	{
		return Expression.New(sqlXmlType.GetDeclaredConstructor(toConvert.Type), toConvert);
	}

	private MethodInfo FindSqlGeographyMethod(string methodName, params Type[] argTypes)
	{
		return SqlGeographyType.GetDeclaredMethod(methodName, argTypes);
	}

	private MethodInfo FindSqlGeographyStaticMethod(string methodName, params Type[] argTypes)
	{
		return SqlGeographyType.GetDeclaredMethod(methodName, argTypes);
	}

	private PropertyInfo FindSqlGeographyProperty(string propertyName)
	{
		return SqlGeographyType.GetRuntimeProperty(propertyName);
	}

	private MethodInfo FindSqlGeometryStaticMethod(string methodName, params Type[] argTypes)
	{
		return SqlGeometryType.GetDeclaredMethod(methodName, argTypes);
	}

	private MethodInfo FindSqlGeometryMethod(string methodName, params Type[] argTypes)
	{
		return SqlGeometryType.GetDeclaredMethod(methodName, argTypes);
	}

	private PropertyInfo FindSqlGeometryProperty(string propertyName)
	{
		return SqlGeometryType.GetRuntimeProperty(propertyName);
	}
}
