using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Linq;
using System.Text;

namespace System.Data.Entity.SqlServer.SqlGen;

internal static class SqlFunctionCallHandler
{
	private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

	private static readonly Dictionary<string, FunctionHandler> _storeFunctionHandlers = InitializeStoreFunctionHandlers();

	private static readonly Dictionary<string, FunctionHandler> _canonicalFunctionHandlers = InitializeCanonicalFunctionHandlers();

	private static readonly Dictionary<string, string> _functionNameToOperatorDictionary = InitializeFunctionNameToOperatorDictionary();

	private static readonly Dictionary<string, string> _dateAddFunctionNameToDatepartDictionary = InitializeDateAddFunctionNameToDatepartDictionary();

	private static readonly Dictionary<string, string> _dateDiffFunctionNameToDatepartDictionary = InitializeDateDiffFunctionNameToDatepartDictionary();

	private static readonly Dictionary<string, FunctionHandler> _hierarchyIdFunctionNameToStaticMethodHandlerDictionary = InitializeHierarchyIdStaticMethodFunctionsDictionary();

	private static readonly Dictionary<string, FunctionHandler> _geographyFunctionNameToStaticMethodHandlerDictionary = InitializeGeographyStaticMethodFunctionsDictionary();

	private static readonly Dictionary<string, string> _geographyFunctionNameToInstancePropertyNameDictionary = InitializeGeographyInstancePropertyFunctionsDictionary();

	private static readonly Dictionary<string, string> _geographyRenamedInstanceMethodFunctionDictionary = InitializeRenamedGeographyInstanceMethodFunctions();

	private static readonly Dictionary<string, FunctionHandler> _geometryFunctionNameToStaticMethodHandlerDictionary = InitializeGeometryStaticMethodFunctionsDictionary();

	private static readonly Dictionary<string, string> _geometryFunctionNameToInstancePropertyNameDictionary = InitializeGeometryInstancePropertyFunctionsDictionary();

	private static readonly Dictionary<string, string> _geometryRenamedInstanceMethodFunctionDictionary = InitializeRenamedGeometryInstanceMethodFunctions();

	private static readonly ISet<string> _datepartKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"year", "yy", "yyyy", "quarter", "qq", "q", "month", "mm", "m", "dayofyear",
		"dy", "y", "day", "dd", "d", "week", "wk", "ww", "weekday", "dw",
		"w", "hour", "hh", "minute", "mi", "n", "second", "ss", "s", "millisecond",
		"ms", "microsecond", "mcs", "nanosecond", "ns", "tzoffset", "tz", "iso_week", "isoww", "isowk"
	};

	private static readonly ISet<string> _functionRequiresReturnTypeCastToInt64 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "SqlServer.CHARINDEX" };

	private static readonly ISet<string> _functionRequiresReturnTypeCastToInt32 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "SqlServer.LEN", "SqlServer.PATINDEX", "SqlServer.DATALENGTH", "SqlServer.CHARINDEX", "Edm.IndexOf", "Edm.Length" };

	private static readonly ISet<string> _functionRequiresReturnTypeCastToInt16 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Edm.Abs" };

	private static readonly ISet<string> _functionRequiresReturnTypeCastToSingle = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Edm.Abs", "Edm.Round", "Edm.Floor", "Edm.Ceiling" };

	private static readonly ISet<string> _maxTypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar(max)", "nvarchar(max)", "text", "ntext", "varbinary(max)", "image", "xml" };

	private static readonly DbExpression _defaultGeographySridExpression = (DbExpression)(object)DbExpressionBuilder.Constant((object)DbGeography.DefaultCoordinateSystemId);

	private static readonly DbExpression _defaultGeometrySridExpression = (DbExpression)(object)DbExpressionBuilder.Constant((object)DbGeometry.DefaultCoordinateSystemId);

	private static Dictionary<string, FunctionHandler> InitializeStoreFunctionHandlers()
	{
		return new Dictionary<string, FunctionHandler>(19, StringComparer.Ordinal)
		{
			{ "CONCAT", HandleConcatFunction },
			{ "DATEADD", HandleDatepartDateFunction },
			{ "DATEDIFF", HandleDatepartDateFunction },
			{ "DATENAME", HandleDatepartDateFunction },
			{ "DATEPART", HandleDatepartDateFunction },
			{
				"Parse",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "hierarchyid::Parse")
			},
			{
				"GetRoot",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "hierarchyid::GetRoot")
			},
			{
				"POINTGEOGRAPHY",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::Point")
			},
			{
				"POINTGEOMETRY",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::Point")
			},
			{
				"ASTEXTZM",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "AsTextZM", functionExpression, isPropertyAccess: false)
			},
			{
				"BUFFERWITHTOLERANCE",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "BufferWithTolerance", functionExpression, isPropertyAccess: false)
			},
			{
				"ENVELOPEANGLE",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "EnvelopeAngle", functionExpression, isPropertyAccess: false)
			},
			{
				"ENVELOPECENTER",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "EnvelopeCenter", functionExpression, isPropertyAccess: false)
			},
			{
				"INSTANCEOF",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "InstanceOf", functionExpression, isPropertyAccess: false)
			},
			{
				"FILTER",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "Filter", functionExpression, isPropertyAccess: false)
			},
			{
				"MAKEVALID",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "MakeValid", functionExpression, isPropertyAccess: false)
			},
			{
				"REDUCE",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "Reduce", functionExpression, isPropertyAccess: false)
			},
			{
				"NUMRINGS",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "NumRings", functionExpression, isPropertyAccess: false)
			},
			{
				"RINGN",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => WriteInstanceFunctionCall(sqlgen, "RingN", functionExpression, isPropertyAccess: false)
			}
		};
	}

	private static Dictionary<string, FunctionHandler> InitializeCanonicalFunctionHandlers()
	{
		return new Dictionary<string, FunctionHandler>(16, StringComparer.Ordinal)
		{
			{ "IndexOf", HandleCanonicalFunctionIndexOf },
			{ "Length", HandleCanonicalFunctionLength },
			{ "NewGuid", HandleCanonicalFunctionNewGuid },
			{ "Round", HandleCanonicalFunctionRound },
			{ "Truncate", HandleCanonicalFunctionTruncate },
			{ "Abs", HandleCanonicalFunctionAbs },
			{ "ToLower", HandleCanonicalFunctionToLower },
			{ "ToUpper", HandleCanonicalFunctionToUpper },
			{ "Trim", HandleCanonicalFunctionTrim },
			{ "Contains", HandleCanonicalFunctionContains },
			{ "StartsWith", HandleCanonicalFunctionStartsWith },
			{ "EndsWith", HandleCanonicalFunctionEndsWith },
			{ "Year", HandleCanonicalFunctionDatepart },
			{ "Month", HandleCanonicalFunctionDatepart },
			{ "Day", HandleCanonicalFunctionDatepart },
			{ "Hour", HandleCanonicalFunctionDatepart },
			{ "Minute", HandleCanonicalFunctionDatepart },
			{ "Second", HandleCanonicalFunctionDatepart },
			{ "Millisecond", HandleCanonicalFunctionDatepart },
			{ "DayOfYear", HandleCanonicalFunctionDatepart },
			{ "CurrentDateTime", HandleCanonicalFunctionCurrentDateTime },
			{ "CurrentUtcDateTime", HandleCanonicalFunctionCurrentUtcDateTime },
			{ "CurrentDateTimeOffset", HandleCanonicalFunctionCurrentDateTimeOffset },
			{ "GetTotalOffsetMinutes", HandleCanonicalFunctionGetTotalOffsetMinutes },
			{ "LocalDateTime", HandleCanonicalFunctionLocalDateTime },
			{ "UtcDateTime", HandleCanonicalFunctionUtcDateTime },
			{ "TruncateTime", HandleCanonicalFunctionTruncateTime },
			{ "CreateDateTime", HandleCanonicalFunctionCreateDateTime },
			{ "CreateDateTimeOffset", HandleCanonicalFunctionCreateDateTimeOffset },
			{ "CreateTime", HandleCanonicalFunctionCreateTime },
			{ "AddYears", HandleCanonicalFunctionDateAdd },
			{ "AddMonths", HandleCanonicalFunctionDateAdd },
			{ "AddDays", HandleCanonicalFunctionDateAdd },
			{ "AddHours", HandleCanonicalFunctionDateAdd },
			{ "AddMinutes", HandleCanonicalFunctionDateAdd },
			{ "AddSeconds", HandleCanonicalFunctionDateAdd },
			{ "AddMilliseconds", HandleCanonicalFunctionDateAdd },
			{ "AddMicroseconds", HandleCanonicalFunctionDateAddKatmaiOrNewer },
			{ "AddNanoseconds", HandleCanonicalFunctionDateAddKatmaiOrNewer },
			{ "DiffYears", HandleCanonicalFunctionDateDiff },
			{ "DiffMonths", HandleCanonicalFunctionDateDiff },
			{ "DiffDays", HandleCanonicalFunctionDateDiff },
			{ "DiffHours", HandleCanonicalFunctionDateDiff },
			{ "DiffMinutes", HandleCanonicalFunctionDateDiff },
			{ "DiffSeconds", HandleCanonicalFunctionDateDiff },
			{ "DiffMilliseconds", HandleCanonicalFunctionDateDiff },
			{ "DiffMicroseconds", HandleCanonicalFunctionDateDiffKatmaiOrNewer },
			{ "DiffNanoseconds", HandleCanonicalFunctionDateDiffKatmaiOrNewer },
			{ "Concat", HandleConcatFunction },
			{ "BitwiseAnd", HandleCanonicalFunctionBitwise },
			{ "BitwiseNot", HandleCanonicalFunctionBitwise },
			{ "BitwiseOr", HandleCanonicalFunctionBitwise },
			{ "BitwiseXor", HandleCanonicalFunctionBitwise }
		};
	}

	private static Dictionary<string, string> InitializeFunctionNameToOperatorDictionary()
	{
		return new Dictionary<string, string>(5, StringComparer.Ordinal)
		{
			{ "Concat", "+" },
			{ "CONCAT", "+" },
			{ "BitwiseAnd", "&" },
			{ "BitwiseNot", "~" },
			{ "BitwiseOr", "|" },
			{ "BitwiseXor", "^" }
		};
	}

	private static Dictionary<string, string> InitializeDateAddFunctionNameToDatepartDictionary()
	{
		return new Dictionary<string, string>(5, StringComparer.Ordinal)
		{
			{ "AddYears", "year" },
			{ "AddMonths", "month" },
			{ "AddDays", "day" },
			{ "AddHours", "hour" },
			{ "AddMinutes", "minute" },
			{ "AddSeconds", "second" },
			{ "AddMilliseconds", "millisecond" },
			{ "AddMicroseconds", "microsecond" },
			{ "AddNanoseconds", "nanosecond" }
		};
	}

	private static Dictionary<string, string> InitializeDateDiffFunctionNameToDatepartDictionary()
	{
		return new Dictionary<string, string>(5, StringComparer.Ordinal)
		{
			{ "DiffYears", "year" },
			{ "DiffMonths", "month" },
			{ "DiffDays", "day" },
			{ "DiffHours", "hour" },
			{ "DiffMinutes", "minute" },
			{ "DiffSeconds", "second" },
			{ "DiffMilliseconds", "millisecond" },
			{ "DiffMicroseconds", "microsecond" },
			{ "DiffNanoseconds", "nanosecond" }
		};
	}

	private static Dictionary<string, FunctionHandler> InitializeHierarchyIdStaticMethodFunctionsDictionary()
	{
		return new Dictionary<string, FunctionHandler>
		{
			{
				"HierarchyIdGetRoot",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "hierarchyid::GetRoot")
			},
			{
				"HierarchyIdParse",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "hierarchyid::Parse")
			}
		};
	}

	private static Dictionary<string, FunctionHandler> InitializeGeographyStaticMethodFunctionsDictionary()
	{
		return new Dictionary<string, FunctionHandler>
		{
			{ "GeographyFromText", HandleSpatialFromTextFunction },
			{
				"GeographyPointFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPointFromText")
			},
			{
				"GeographyLineFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STLineFromText")
			},
			{
				"GeographyPolygonFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPolyFromText")
			},
			{
				"GeographyMultiPointFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPointFromText")
			},
			{
				"GeographyMultiLineFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMLineFromText")
			},
			{
				"GeographyMultiPolygonFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPolyFromText")
			},
			{
				"GeographyCollectionFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STGeomCollFromText")
			},
			{ "GeographyFromBinary", HandleSpatialFromBinaryFunction },
			{
				"GeographyPointFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPointFromWKB")
			},
			{
				"GeographyLineFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STLineFromWKB")
			},
			{
				"GeographyPolygonFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPolyFromWKB")
			},
			{
				"GeographyMultiPointFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPointFromWKB")
			},
			{
				"GeographyMultiLineFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMLineFromWKB")
			},
			{
				"GeographyMultiPolygonFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPolyFromWKB")
			},
			{
				"GeographyCollectionFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STGeomCollFromWKB")
			},
			{ "GeographyFromGml", HandleSpatialFromGmlFunction }
		};
	}

	private static Dictionary<string, string> InitializeGeographyInstancePropertyFunctionsDictionary()
	{
		return new Dictionary<string, string>
		{
			{ "CoordinateSystemId", "STSrid" },
			{ "Latitude", "Lat" },
			{ "Longitude", "Long" },
			{ "Measure", "M" },
			{ "Elevation", "Z" }
		};
	}

	private static Dictionary<string, string> InitializeRenamedGeographyInstanceMethodFunctions()
	{
		return new Dictionary<string, string>
		{
			{ "AsText", "STAsText" },
			{ "AsBinary", "STAsBinary" },
			{ "SpatialTypeName", "STGeometryType" },
			{ "SpatialDimension", "STDimension" },
			{ "IsEmptySpatial", "STIsEmpty" },
			{ "SpatialEquals", "STEquals" },
			{ "SpatialDisjoint", "STDisjoint" },
			{ "SpatialIntersects", "STIntersects" },
			{ "SpatialBuffer", "STBuffer" },
			{ "Distance", "STDistance" },
			{ "SpatialUnion", "STUnion" },
			{ "SpatialIntersection", "STIntersection" },
			{ "SpatialDifference", "STDifference" },
			{ "SpatialSymmetricDifference", "STSymDifference" },
			{ "SpatialElementCount", "STNumGeometries" },
			{ "SpatialElementAt", "STGeometryN" },
			{ "SpatialLength", "STLength" },
			{ "StartPoint", "STStartPoint" },
			{ "EndPoint", "STEndPoint" },
			{ "IsClosedSpatial", "STIsClosed" },
			{ "PointCount", "STNumPoints" },
			{ "PointAt", "STPointN" },
			{ "Area", "STArea" }
		};
	}

	private static Dictionary<string, FunctionHandler> InitializeGeometryStaticMethodFunctionsDictionary()
	{
		return new Dictionary<string, FunctionHandler>
		{
			{ "GeometryFromText", HandleSpatialFromTextFunction },
			{
				"GeometryPointFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPointFromText")
			},
			{
				"GeometryLineFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STLineFromText")
			},
			{
				"GeometryPolygonFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPolyFromText")
			},
			{
				"GeometryMultiPointFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPointFromText")
			},
			{
				"GeometryMultiLineFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMLineFromText")
			},
			{
				"GeometryMultiPolygonFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPolyFromText")
			},
			{
				"GeometryCollectionFromText",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STGeomCollFromText")
			},
			{ "GeometryFromBinary", HandleSpatialFromBinaryFunction },
			{
				"GeometryPointFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPointFromWKB")
			},
			{
				"GeometryLineFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STLineFromWKB")
			},
			{
				"GeometryPolygonFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPolyFromWKB")
			},
			{
				"GeometryMultiPointFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPointFromWKB")
			},
			{
				"GeometryMultiLineFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMLineFromWKB")
			},
			{
				"GeometryMultiPolygonFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPolyFromWKB")
			},
			{
				"GeometryCollectionFromBinary",
				(SqlGenerator sqlgen, DbFunctionExpression functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STGeomCollFromWKB")
			},
			{ "GeometryFromGml", HandleSpatialFromGmlFunction }
		};
	}

	private static Dictionary<string, string> InitializeGeometryInstancePropertyFunctionsDictionary()
	{
		return new Dictionary<string, string>
		{
			{ "CoordinateSystemId", "STSrid" },
			{ "Measure", "M" },
			{ "XCoordinate", "STX" },
			{ "YCoordinate", "STY" },
			{ "Elevation", "Z" }
		};
	}

	private static Dictionary<string, string> InitializeRenamedGeometryInstanceMethodFunctions()
	{
		return new Dictionary<string, string>
		{
			{ "AsText", "STAsText" },
			{ "AsBinary", "STAsBinary" },
			{ "SpatialTypeName", "STGeometryType" },
			{ "SpatialDimension", "STDimension" },
			{ "IsEmptySpatial", "STIsEmpty" },
			{ "IsSimpleGeometry", "STIsSimple" },
			{ "IsValidGeometry", "STIsValid" },
			{ "SpatialBoundary", "STBoundary" },
			{ "SpatialEnvelope", "STEnvelope" },
			{ "SpatialEquals", "STEquals" },
			{ "SpatialDisjoint", "STDisjoint" },
			{ "SpatialIntersects", "STIntersects" },
			{ "SpatialTouches", "STTouches" },
			{ "SpatialCrosses", "STCrosses" },
			{ "SpatialWithin", "STWithin" },
			{ "SpatialContains", "STContains" },
			{ "SpatialOverlaps", "STOverlaps" },
			{ "SpatialRelate", "STRelate" },
			{ "SpatialBuffer", "STBuffer" },
			{ "SpatialConvexHull", "STConvexHull" },
			{ "Distance", "STDistance" },
			{ "SpatialUnion", "STUnion" },
			{ "SpatialIntersection", "STIntersection" },
			{ "SpatialDifference", "STDifference" },
			{ "SpatialSymmetricDifference", "STSymDifference" },
			{ "SpatialElementCount", "STNumGeometries" },
			{ "SpatialElementAt", "STGeometryN" },
			{ "SpatialLength", "STLength" },
			{ "StartPoint", "STStartPoint" },
			{ "EndPoint", "STEndPoint" },
			{ "IsClosedSpatial", "STIsClosed" },
			{ "IsRing", "STIsRing" },
			{ "PointCount", "STNumPoints" },
			{ "PointAt", "STPointN" },
			{ "Area", "STArea" },
			{ "Centroid", "STCentroid" },
			{ "PointOnSurface", "STPointOnSurface" },
			{ "ExteriorRing", "STExteriorRing" },
			{ "InteriorRingCount", "STNumInteriorRing" },
			{ "InteriorRingAt", "STInteriorRingN" }
		};
	}

	private static ISqlFragment HandleSpatialFromTextFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
	{
		string functionName = (((DbExpression)functionExpression).ResultType.IsPrimitiveType((PrimitiveTypeKind)15) ? "geometry::STGeomFromText" : "geography::STGeomFromText");
		string functionName2 = (((DbExpression)functionExpression).ResultType.IsPrimitiveType((PrimitiveTypeKind)15) ? "geometry::Parse" : "geography::Parse");
		if (functionExpression.Arguments.Count == 2)
		{
			return HandleFunctionDefaultGivenName(sqlgen, functionExpression, functionName);
		}
		return HandleFunctionDefaultGivenName(sqlgen, functionExpression, functionName2);
	}

	private static ISqlFragment HandleSpatialFromGmlFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
	{
		return HandleSpatialStaticMethodFunctionAppendSrid(sqlgen, functionExpression, ((DbExpression)functionExpression).ResultType.IsPrimitiveType((PrimitiveTypeKind)15) ? "geometry::GeomFromGml" : "geography::GeomFromGml");
	}

	private static ISqlFragment HandleSpatialFromBinaryFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
	{
		return HandleSpatialStaticMethodFunctionAppendSrid(sqlgen, functionExpression, ((DbExpression)functionExpression).ResultType.IsPrimitiveType((PrimitiveTypeKind)15) ? "geometry::STGeomFromWKB" : "geography::STGeomFromWKB");
	}

	private static ISqlFragment HandleSpatialStaticMethodFunctionAppendSrid(SqlGenerator sqlgen, DbFunctionExpression functionExpression, string functionName)
	{
		if (functionExpression.Arguments.Count == 2)
		{
			return HandleFunctionDefaultGivenName(sqlgen, functionExpression, functionName);
		}
		DbExpression val = (((DbExpression)functionExpression).ResultType.IsPrimitiveType((PrimitiveTypeKind)15) ? _defaultGeometrySridExpression : _defaultGeographySridExpression);
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(functionName);
		WriteFunctionArguments(sqlgen, functionExpression.Arguments.Concat((IEnumerable<DbExpression>)(object)new DbExpression[1] { val }), sqlBuilder);
		return sqlBuilder;
	}

	internal static ISqlFragment GenerateFunctionCallSql(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (IsSpecialCanonicalFunction(functionExpression))
		{
			return HandleSpecialCanonicalFunction(sqlgen, functionExpression);
		}
		if (IsSpecialStoreFunction(functionExpression))
		{
			return HandleSpecialStoreFunction(sqlgen, functionExpression);
		}
		if (IsSpatialCanonicalFunction(functionExpression, out var spatialTypeKind))
		{
			return HandleSpatialCanonicalFunction(sqlgen, functionExpression, spatialTypeKind);
		}
		if (IsHierarchyCanonicalFunction(functionExpression))
		{
			return HandleHierarchyIdCanonicalFunction(sqlgen, functionExpression);
		}
		return HandleFunctionDefault(sqlgen, functionExpression);
	}

	private static bool IsSpecialStoreFunction(DbFunctionExpression e)
	{
		if (IsStoreFunction(e.Function))
		{
			return _storeFunctionHandlers.ContainsKey(((EdmType)e.Function).Name);
		}
		return false;
	}

	private static bool IsSpecialCanonicalFunction(DbFunctionExpression e)
	{
		if (e.Function.IsCanonicalFunction())
		{
			return _canonicalFunctionHandlers.ContainsKey(((EdmType)e.Function).Name);
		}
		return false;
	}

	private static bool IsHierarchyCanonicalFunction(DbFunctionExpression e)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (e.Function.IsCanonicalFunction())
		{
			if (((DbExpression)e).ResultType.IsHierarchyIdType())
			{
				return true;
			}
			Enumerator<FunctionParameter> enumerator = e.Function.Parameters.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.TypeUsage.IsHierarchyIdType())
					{
						return true;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		return false;
	}

	private static bool IsSpatialCanonicalFunction(DbFunctionExpression e, out PrimitiveTypeKind spatialTypeKind)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (e.Function.IsCanonicalFunction())
		{
			if (((DbExpression)e).ResultType.IsSpatialType(out spatialTypeKind))
			{
				return true;
			}
			Enumerator<FunctionParameter> enumerator = e.Function.Parameters.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.TypeUsage.IsSpatialType(out spatialTypeKind))
					{
						return true;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		spatialTypeKind = (PrimitiveTypeKind)0;
		return false;
	}

	private static ISqlFragment HandleFunctionDefault(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionDefaultGivenName(sqlgen, e, null);
	}

	private static ISqlFragment HandleFunctionDefaultGivenName(SqlGenerator sqlgen, DbFunctionExpression e, string functionName)
	{
		if (CastReturnTypeToInt64(e))
		{
			return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "bigint");
		}
		if (CastReturnTypeToInt32(sqlgen, e))
		{
			return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "int");
		}
		if (CastReturnTypeToInt16(e))
		{
			return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "smallint");
		}
		if (CastReturnTypeToSingle(e))
		{
			return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "real");
		}
		return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, null);
	}

	private static ISqlFragment HandleFunctionDefaultCastReturnValue(SqlGenerator sqlgen, DbFunctionExpression e, string functionName, string returnType)
	{
		return WrapWithCast(returnType, delegate(SqlBuilder result)
		{
			if (functionName == null)
			{
				WriteFunctionName(result, e.Function);
			}
			else
			{
				result.Append(functionName);
			}
			HandleFunctionArgumentsDefault(sqlgen, e, result);
		});
	}

	private static ISqlFragment WrapWithCast(string returnType, Action<SqlBuilder> toWrap)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (returnType != null)
		{
			sqlBuilder.Append(" CAST(");
		}
		toWrap(sqlBuilder);
		if (returnType != null)
		{
			sqlBuilder.Append(" AS ");
			sqlBuilder.Append(returnType);
			sqlBuilder.Append(")");
		}
		return sqlBuilder;
	}

	private static void HandleFunctionArgumentsDefault(SqlGenerator sqlgen, DbFunctionExpression e, SqlBuilder result)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		bool niladicFunctionAttribute = e.Function.NiladicFunctionAttribute;
		if (niladicFunctionAttribute && e.Arguments.Count > 0)
		{
			throw new MetadataException(Strings.SqlGen_NiladicFunctionsCannotHaveParameters);
		}
		if (!niladicFunctionAttribute)
		{
			WriteFunctionArguments(sqlgen, e.Arguments, result);
		}
	}

	private static void WriteFunctionArguments(SqlGenerator sqlgen, IEnumerable<DbExpression> functionArguments, SqlBuilder result)
	{
		result.Append("(");
		string s = "";
		foreach (DbExpression functionArgument in functionArguments)
		{
			result.Append(s);
			result.Append(functionArgument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			s = ", ";
		}
		result.Append(")");
	}

	private static ISqlFragment HandleFunctionGivenNameBasedOnVersion(SqlGenerator sqlgen, DbFunctionExpression e, string preKatmaiName, string katmaiName)
	{
		if (sqlgen.IsPreKatmai)
		{
			return HandleFunctionDefaultGivenName(sqlgen, e, preKatmaiName);
		}
		return HandleFunctionDefaultGivenName(sqlgen, e, katmaiName);
	}

	private static ISqlFragment HandleSpecialStoreFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleSpecialFunction(_storeFunctionHandlers, sqlgen, e);
	}

	private static ISqlFragment HandleHierarchyIdCanonicalFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
	{
		if (_hierarchyIdFunctionNameToStaticMethodHandlerDictionary.TryGetValue(((EdmType)functionExpression.Function).Name, out var value))
		{
			return value(sqlgen, functionExpression);
		}
		string name = ((EdmType)functionExpression.Function).Name;
		return WriteInstanceFunctionCall(sqlgen, name, functionExpression, isPropertyAccess: false);
	}

	private static ISqlFragment HandleSpecialCanonicalFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleSpecialFunction(_canonicalFunctionHandlers, sqlgen, e);
	}

	private static ISqlFragment HandleSpecialFunction(Dictionary<string, FunctionHandler> handlers, SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return handlers[((EdmType)e.Function).Name](sqlgen, e);
	}

	private static ISqlFragment HandleSpatialCanonicalFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression, PrimitiveTypeKind spatialTypeKind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)spatialTypeKind == 16)
		{
			return HandleSpatialCanonicalFunction(sqlgen, functionExpression, _geographyFunctionNameToStaticMethodHandlerDictionary, _geographyFunctionNameToInstancePropertyNameDictionary, _geographyRenamedInstanceMethodFunctionDictionary);
		}
		return HandleSpatialCanonicalFunction(sqlgen, functionExpression, _geometryFunctionNameToStaticMethodHandlerDictionary, _geometryFunctionNameToInstancePropertyNameDictionary, _geometryRenamedInstanceMethodFunctionDictionary);
	}

	private static ISqlFragment HandleSpatialCanonicalFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression, Dictionary<string, FunctionHandler> staticMethodsMap, Dictionary<string, string> instancePropertiesMap, Dictionary<string, string> renamedInstanceMethodsMap)
	{
		if (staticMethodsMap.TryGetValue(((EdmType)functionExpression.Function).Name, out var value))
		{
			return value(sqlgen, functionExpression);
		}
		if (instancePropertiesMap.TryGetValue(((EdmType)functionExpression.Function).Name, out var value2))
		{
			return WriteInstanceFunctionCall(sqlgen, value2, functionExpression, isPropertyAccess: true, null);
		}
		if (!renamedInstanceMethodsMap.TryGetValue(((EdmType)functionExpression.Function).Name, out var value3))
		{
			value3 = ((EdmType)functionExpression.Function).Name;
		}
		string castReturnTypeTo = null;
		if (value3 == "AsGml")
		{
			castReturnTypeTo = "nvarchar(max)";
		}
		return WriteInstanceFunctionCall(sqlgen, value3, functionExpression, isPropertyAccess: false, castReturnTypeTo);
	}

	private static ISqlFragment WriteInstanceFunctionCall(SqlGenerator sqlgen, string functionName, DbFunctionExpression functionExpression, bool isPropertyAccess)
	{
		return WriteInstanceFunctionCall(sqlgen, functionName, functionExpression, isPropertyAccess, null);
	}

	private static ISqlFragment WriteInstanceFunctionCall(SqlGenerator sqlgen, string functionName, DbFunctionExpression functionExpression, bool isPropertyAccess, string castReturnTypeTo)
	{
		return WrapWithCast(castReturnTypeTo, delegate(SqlBuilder result)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			DbExpression val = functionExpression.Arguments[0];
			if ((int)val.ExpressionKind != 17)
			{
				sqlgen.ParenthesizeExpressionIfNeeded(val, result);
			}
			else
			{
				result.Append(val.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			}
			result.Append(".");
			result.Append(functionName);
			if (!isPropertyAccess)
			{
				WriteFunctionArguments(sqlgen, functionExpression.Arguments.Skip(1), result);
			}
		});
	}

	private static ISqlFragment HandleSpecialFunctionToOperator(SqlGenerator sqlgen, DbFunctionExpression e, bool parenthesizeArguments)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (e.Arguments.Count > 1)
		{
			if (parenthesizeArguments)
			{
				sqlBuilder.Append("(");
			}
			sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			if (parenthesizeArguments)
			{
				sqlBuilder.Append(")");
			}
		}
		sqlBuilder.Append(" ");
		sqlBuilder.Append(_functionNameToOperatorDictionary[((EdmType)e.Function).Name]);
		sqlBuilder.Append(" ");
		if (parenthesizeArguments)
		{
			sqlBuilder.Append("(");
		}
		sqlBuilder.Append(e.Arguments[e.Arguments.Count - 1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		if (parenthesizeArguments)
		{
			sqlBuilder.Append(")");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleConcatFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleSpecialFunctionToOperator(sqlgen, e, parenthesizeArguments: false);
	}

	private static ISqlFragment HandleCanonicalFunctionBitwise(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleSpecialFunctionToOperator(sqlgen, e, parenthesizeArguments: true);
	}

	internal static ISqlFragment HandleDatepartDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		DbExpression obj = e.Arguments[0];
		DbConstantExpression val = (DbConstantExpression)(object)((obj is DbConstantExpression) ? obj : null);
		if (val == null)
		{
			throw new InvalidOperationException(Strings.SqlGen_InvalidDatePartArgumentExpression(((EdmType)e.Function).NamespaceName, ((EdmType)e.Function).Name));
		}
		if (!(val.Value is string text))
		{
			throw new InvalidOperationException(Strings.SqlGen_InvalidDatePartArgumentExpression(((EdmType)e.Function).NamespaceName, ((EdmType)e.Function).Name));
		}
		if (!_datepartKeywords.Contains(text))
		{
			throw new InvalidOperationException(Strings.SqlGen_InvalidDatePartArgumentValue(text, ((EdmType)e.Function).NamespaceName, ((EdmType)e.Function).Name));
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		WriteFunctionName(sqlBuilder, e.Function);
		sqlBuilder.Append("(");
		sqlBuilder.Append(text);
		for (int i = 1; i < e.Arguments.Count; i++)
		{
			sqlBuilder.Append(", ");
			sqlBuilder.Append(e.Arguments[i].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		}
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleCanonicalFunctionDatepart(sqlgen, ((EdmType)e.Function).Name.ToLowerInvariant(), e);
	}

	private static ISqlFragment HandleCanonicalFunctionGetTotalOffsetMinutes(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleCanonicalFunctionDatepart(sqlgen, "tzoffset", e);
	}

	private static ISqlFragment HandleCanonicalFunctionLocalDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("CAST (");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(" AS DATETIME2)");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionUtcDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("CONVERT (DATETIME2, ");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", 1)");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, string datepart, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("DATEPART (");
		sqlBuilder.Append(datepart);
		sqlBuilder.Append(", ");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionCurrentDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionGivenNameBasedOnVersion(sqlgen, e, "GetDate", "SysDateTime");
	}

	private static ISqlFragment HandleCanonicalFunctionCurrentUtcDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionGivenNameBasedOnVersion(sqlgen, e, "GetUtcDate", "SysUtcDateTime");
	}

	private static ISqlFragment HandleCanonicalFunctionCurrentDateTimeOffset(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		return HandleFunctionDefaultGivenName(sqlgen, e, "SysDateTimeOffset");
	}

	private static ISqlFragment HandleCanonicalFunctionCreateDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		string typeName = (sqlgen.IsPreKatmai ? "datetime" : "datetime2");
		return HandleCanonicalFunctionDateTimeTypeCreation(sqlgen, typeName, e.Arguments, hasDatePart: true, hasTimeZonePart: false);
	}

	private static ISqlFragment HandleCanonicalFunctionCreateDateTimeOffset(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		return HandleCanonicalFunctionDateTimeTypeCreation(sqlgen, "datetimeoffset", e.Arguments, hasDatePart: true, hasTimeZonePart: true);
	}

	private static ISqlFragment HandleCanonicalFunctionCreateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		return HandleCanonicalFunctionDateTimeTypeCreation(sqlgen, "time", e.Arguments, hasDatePart: false, hasTimeZonePart: false);
	}

	private static ISqlFragment HandleCanonicalFunctionDateTimeTypeCreation(SqlGenerator sqlgen, string typeName, IList<DbExpression> args, bool hasDatePart, bool hasTimeZonePart)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		int index = 0;
		sqlBuilder.Append("convert (");
		sqlBuilder.Append(typeName);
		sqlBuilder.Append(",");
		if (hasDatePart)
		{
			sqlBuilder.Append("right('000' + ");
			AppendConvertToVarchar(sqlgen, sqlBuilder, args[index++]);
			sqlBuilder.Append(", 4)");
			sqlBuilder.Append(" + '-' + ");
			AppendConvertToVarchar(sqlgen, sqlBuilder, args[index++]);
			sqlBuilder.Append(" + '-' + ");
			AppendConvertToVarchar(sqlgen, sqlBuilder, args[index++]);
			sqlBuilder.Append(" + ' ' + ");
		}
		AppendConvertToVarchar(sqlgen, sqlBuilder, args[index++]);
		sqlBuilder.Append(" + ':' + ");
		AppendConvertToVarchar(sqlgen, sqlBuilder, args[index++]);
		sqlBuilder.Append(" + ':' + str(");
		sqlBuilder.Append(args[index++].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		if (sqlgen.IsPreKatmai)
		{
			sqlBuilder.Append(", 6, 3)");
		}
		else
		{
			sqlBuilder.Append(", 10, 7)");
		}
		if (hasTimeZonePart)
		{
			sqlBuilder.Append(" + (CASE WHEN ");
			sqlgen.ParenthesizeExpressionIfNeeded(args[index], sqlBuilder);
			sqlBuilder.Append(" >= 0 THEN '+' ELSE '-' END) + convert(varchar(255), ABS(");
			sqlgen.ParenthesizeExpressionIfNeeded(args[index], sqlBuilder);
			sqlBuilder.Append("/60)) + ':' + convert(varchar(255), ABS(");
			sqlgen.ParenthesizeExpressionIfNeeded(args[index], sqlBuilder);
			sqlBuilder.Append("%60))");
		}
		sqlBuilder.Append(", 121)");
		return sqlBuilder;
	}

	private static void AppendConvertToVarchar(SqlGenerator sqlgen, SqlBuilder result, DbExpression e)
	{
		result.Append("convert(varchar(255), ");
		result.Append(e.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		result.Append(")");
	}

	private static ISqlFragment HandleCanonicalFunctionTruncateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		bool flag = (int)e.Arguments[0].ResultType.GetPrimitiveTypeKind() == 14;
		if (sqlgen.IsPreKatmai && flag)
		{
			throw new NotSupportedException(Strings.SqlGen_CanonicalFunctionNotSupportedPriorSql10(((EdmType)e.Function).Name));
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		ISqlFragment s = e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen);
		if (sqlgen.IsPreKatmai)
		{
			sqlBuilder.Append("dateadd(d, datediff(d, 0, ");
			sqlBuilder.Append(s);
			sqlBuilder.Append("), 0)");
		}
		else if (!flag)
		{
			sqlBuilder.Append("cast(cast(");
			sqlBuilder.Append(s);
			sqlBuilder.Append(" as date) as datetime2)");
		}
		else
		{
			sqlBuilder.Append("todatetimeoffset(cast(");
			sqlBuilder.Append(s);
			sqlBuilder.Append(" as date), datepart(tz, ");
			sqlBuilder.Append(s);
			sqlBuilder.Append("))");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDateAddKatmaiOrNewer(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		return HandleCanonicalFunctionDateAdd(sqlgen, e);
	}

	private static ISqlFragment HandleCanonicalFunctionDateAdd(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("DATEADD (");
		sqlBuilder.Append(_dateAddFunctionNameToDatepartDictionary[((EdmType)e.Function).Name]);
		sqlBuilder.Append(", ");
		sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", ");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDateDiffKatmaiOrNewer(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen.AssertKatmaiOrNewer(e);
		return HandleCanonicalFunctionDateDiff(sqlgen, e);
	}

	private static ISqlFragment HandleCanonicalFunctionDateDiff(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("DATEDIFF (");
		sqlBuilder.Append(_dateDiffFunctionNameToDatepartDictionary[((EdmType)e.Function).Name]);
		sqlBuilder.Append(", ");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", ");
		sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionIndexOf(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionDefaultGivenName(sqlgen, e, "CHARINDEX");
	}

	private static ISqlFragment HandleCanonicalFunctionNewGuid(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionDefaultGivenName(sqlgen, e, "NEWID");
	}

	private static ISqlFragment HandleCanonicalFunctionLength(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionDefaultGivenName(sqlgen, e, "LEN");
	}

	private static ISqlFragment HandleCanonicalFunctionRound(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleCanonicalFunctionRoundOrTruncate(sqlgen, e, round: true);
	}

	private static ISqlFragment HandleCanonicalFunctionTruncate(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleCanonicalFunctionRoundOrTruncate(sqlgen, e, round: false);
	}

	private static ISqlFragment HandleCanonicalFunctionRoundOrTruncate(SqlGenerator sqlgen, DbFunctionExpression e, bool round)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		bool flag = false;
		if (e.Arguments.Count == 1)
		{
			flag = CastReturnTypeToSingle(e);
			if (flag)
			{
				sqlBuilder.Append(" CAST(");
			}
		}
		sqlBuilder.Append("ROUND(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", ");
		if (e.Arguments.Count > 1)
		{
			sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		}
		else
		{
			sqlBuilder.Append("0");
		}
		if (!round)
		{
			sqlBuilder.Append(", 1");
		}
		sqlBuilder.Append(")");
		if (flag)
		{
			sqlBuilder.Append(" AS real)");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionAbs(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		if (e.Arguments[0].ResultType.IsPrimitiveType((PrimitiveTypeKind)2))
		{
			SqlBuilder sqlBuilder = new SqlBuilder();
			sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			return sqlBuilder;
		}
		return HandleFunctionDefault(sqlgen, e);
	}

	private static ISqlFragment HandleCanonicalFunctionTrim(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("LTRIM(RTRIM(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append("))");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionToLower(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionDefaultGivenName(sqlgen, e, "LOWER");
	}

	private static ISqlFragment HandleCanonicalFunctionToUpper(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return HandleFunctionDefaultGivenName(sqlgen, e, "UPPER");
	}

	private static void TranslateConstantParameterForLike(SqlGenerator sqlgen, DbExpression targetExpression, DbConstantExpression constSearchParamExpression, SqlBuilder result, bool insertPercentStart, bool insertPercentEnd)
	{
		result.Append(targetExpression.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		result.Append(" LIKE ");
		StringBuilder stringBuilder = new StringBuilder();
		if (insertPercentStart)
		{
			stringBuilder.Append("%");
		}
		stringBuilder.Append(SqlProviderManifest.EscapeLikeText(constSearchParamExpression.Value as string, alwaysEscapeEscapeChar: false, out var usedEscapeChar));
		if (insertPercentEnd)
		{
			stringBuilder.Append("%");
		}
		DbConstantExpression val = DbExpressionBuilder.Constant(((DbExpression)constSearchParamExpression).ResultType, (object)stringBuilder.ToString());
		result.Append(((DbExpression)val).Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		if (usedEscapeChar)
		{
			result.Append(" ESCAPE '~'");
		}
	}

	private static ISqlFragment HandleCanonicalFunctionContains(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return WrapPredicate(HandleCanonicalFunctionContains, sqlgen, e);
	}

	private static SqlBuilder HandleCanonicalFunctionContains(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
	{
		DbExpression obj = args[1];
		DbConstantExpression val = (DbConstantExpression)(object)((obj is DbConstantExpression) ? obj : null);
		if (val != null && !string.IsNullOrEmpty(val.Value as string))
		{
			TranslateConstantParameterForLike(sqlgen, args[0], val, result, insertPercentStart: true, insertPercentEnd: true);
		}
		else
		{
			result.Append("CHARINDEX( ");
			result.Append(args[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			result.Append(", ");
			result.Append(args[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			result.Append(") > 0");
		}
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionStartsWith(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return WrapPredicate(HandleCanonicalFunctionStartsWith, sqlgen, e);
	}

	private static SqlBuilder HandleCanonicalFunctionStartsWith(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
	{
		DbExpression obj = args[1];
		DbConstantExpression val = (DbConstantExpression)(object)((obj is DbConstantExpression) ? obj : null);
		if (val != null && !string.IsNullOrEmpty(val.Value as string))
		{
			TranslateConstantParameterForLike(sqlgen, args[0], val, result, insertPercentStart: false, insertPercentEnd: true);
		}
		else
		{
			result.Append("CHARINDEX( ");
			result.Append(args[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			result.Append(", ");
			result.Append(args[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			result.Append(") = 1");
		}
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionEndsWith(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return WrapPredicate(HandleCanonicalFunctionEndsWith, sqlgen, e);
	}

	private static SqlBuilder HandleCanonicalFunctionEndsWith(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
	{
		DbExpression obj = args[1];
		DbConstantExpression val = (DbConstantExpression)(object)((obj is DbConstantExpression) ? obj : null);
		DbExpression obj2 = args[0];
		DbPropertyExpression val2 = (DbPropertyExpression)(object)((obj2 is DbPropertyExpression) ? obj2 : null);
		if (val != null && val2 != null && !string.IsNullOrEmpty(val.Value as string))
		{
			TranslateConstantParameterForLike(sqlgen, args[0], val, result, insertPercentStart: true, insertPercentEnd: false);
		}
		else
		{
			result.Append("CHARINDEX( REVERSE(");
			result.Append(args[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			result.Append("), REVERSE(");
			result.Append(args[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			result.Append(")) = 1");
		}
		return result;
	}

	private static ISqlFragment WrapPredicate(Func<SqlGenerator, IList<DbExpression>, SqlBuilder, SqlBuilder> predicateTranslator, SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("CASE WHEN (");
		predicateTranslator(sqlgen, e.Arguments, sqlBuilder);
		sqlBuilder.Append(") THEN cast(1 as bit) WHEN ( NOT (");
		predicateTranslator(sqlgen, e.Arguments, sqlBuilder);
		sqlBuilder.Append(")) THEN cast(0 as bit) END");
		return sqlBuilder;
	}

	internal static void WriteFunctionName(SqlBuilder result, EdmFunction function)
	{
		string text = ((function.StoreFunctionNameAttribute == null) ? ((EdmType)function).Name : function.StoreFunctionNameAttribute);
		if (function.IsCanonicalFunction())
		{
			result.Append(text.ToUpperInvariant());
			return;
		}
		if (IsStoreFunction(function))
		{
			result.Append(text);
			return;
		}
		if (string.IsNullOrEmpty(function.Schema))
		{
			result.Append(SqlGenerator.QuoteIdentifier(((EdmType)function).NamespaceName));
		}
		else
		{
			result.Append(SqlGenerator.QuoteIdentifier(function.Schema));
		}
		result.Append(".");
		result.Append(SqlGenerator.QuoteIdentifier(text));
	}

	internal static bool IsStoreFunction(EdmFunction function)
	{
		if (function.BuiltInAttribute)
		{
			return !function.IsCanonicalFunction();
		}
		return false;
	}

	internal static bool CastReturnTypeToInt64(DbFunctionExpression e)
	{
		return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToInt64, (PrimitiveTypeKind)11);
	}

	internal static bool CastReturnTypeToInt32(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		if (!_functionRequiresReturnTypeCastToInt32.Contains(((EdmType)e.Function).FullName))
		{
			return false;
		}
		return e.Arguments.Select((DbExpression t) => sqlgen.StoreItemCollection.ProviderManifest.GetStoreType(t.ResultType)).Any((TypeUsage storeType) => _maxTypeNames.Contains(storeType.EdmType.Name));
	}

	internal static bool CastReturnTypeToInt16(DbFunctionExpression e)
	{
		return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToInt16, (PrimitiveTypeKind)9);
	}

	internal static bool CastReturnTypeToSingle(DbFunctionExpression e)
	{
		return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToSingle, (PrimitiveTypeKind)7);
	}

	private static bool CastReturnTypeToGivenType(DbFunctionExpression e, ISet<string> functionsRequiringReturnTypeCast, PrimitiveTypeKind type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!functionsRequiringReturnTypeCast.Contains(((EdmType)e.Function).FullName))
		{
			return false;
		}
		return e.Arguments.Any((DbExpression t) => t.ResultType.IsPrimitiveType(type));
	}
}
