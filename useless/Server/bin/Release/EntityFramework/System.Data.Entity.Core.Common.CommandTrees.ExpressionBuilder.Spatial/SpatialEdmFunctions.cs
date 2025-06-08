using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Spatial;

public static class SpatialEdmFunctions
{
	public static DbFunctionExpression GeometryFromText(DbExpression wellKnownText)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		return EdmFunctions.InvokeCanonicalFunction("GeometryFromText", wellKnownText);
	}

	public static DbFunctionExpression GeometryFromText(DbExpression wellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryFromText", wellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryPointFromText(DbExpression pointWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(pointWellKnownText, "pointWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryPointFromText", pointWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryLineFromText(DbExpression lineWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(lineWellKnownText, "lineWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryLineFromText", lineWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryPolygonFromText(DbExpression polygonWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryPolygonFromText", polygonWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryMultiPointFromText(DbExpression multiPointWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPointFromText", multiPointWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryMultiLineFromText(DbExpression multiLineWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryMultiLineFromText", multiLineWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryMultiPolygonFromText(DbExpression multiPolygonWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPolygonFromText", multiPolygonWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryCollectionFromText(DbExpression geometryCollectionWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(geometryCollectionWellKnownText, "geometryCollectionWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryCollectionFromText", geometryCollectionWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryFromBinary(DbExpression wellKnownBinaryValue)
	{
		Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
		return EdmFunctions.InvokeCanonicalFunction("GeometryFromBinary", wellKnownBinaryValue);
	}

	public static DbFunctionExpression GeometryFromBinary(DbExpression wellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryFromBinary", wellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryPointFromBinary(DbExpression pointWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(pointWellKnownBinaryValue, "pointWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryPointFromBinary", pointWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryLineFromBinary(DbExpression lineWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(lineWellKnownBinaryValue, "lineWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryLineFromBinary", lineWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryPolygonFromBinary(DbExpression polygonWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownBinaryValue, "polygonWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryPolygonFromBinary", polygonWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryMultiPointFromBinary(DbExpression multiPointWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownBinaryValue, "multiPointWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPointFromBinary", multiPointWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryMultiLineFromBinary(DbExpression multiLineWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownBinaryValue, "multiLineWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryMultiLineFromBinary", multiLineWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryMultiPolygonFromBinary(DbExpression multiPolygonWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownBinaryValue, "multiPolygonWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPolygonFromBinary", multiPolygonWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryCollectionFromBinary(DbExpression geometryCollectionWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(geometryCollectionWellKnownBinaryValue, "geometryCollectionWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryCollectionFromBinary", geometryCollectionWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeometryFromGml(DbExpression geometryMarkup)
	{
		Check.NotNull(geometryMarkup, "geometryMarkup");
		return EdmFunctions.InvokeCanonicalFunction("GeometryFromGml", geometryMarkup);
	}

	public static DbFunctionExpression GeometryFromGml(DbExpression geometryMarkup, DbExpression coordinateSystemId)
	{
		Check.NotNull(geometryMarkup, "geometryMarkup");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeometryFromGml", geometryMarkup, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyFromText(DbExpression wellKnownText)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		return EdmFunctions.InvokeCanonicalFunction("GeographyFromText", wellKnownText);
	}

	public static DbFunctionExpression GeographyFromText(DbExpression wellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyFromText", wellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyPointFromText(DbExpression pointWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(pointWellKnownText, "pointWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyPointFromText", pointWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyLineFromText(DbExpression lineWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(lineWellKnownText, "lineWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyLineFromText", lineWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyPolygonFromText(DbExpression polygonWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyPolygonFromText", polygonWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyMultiPointFromText(DbExpression multiPointWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPointFromText", multiPointWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyMultiLineFromText(DbExpression multiLineWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyMultiLineFromText", multiLineWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyMultiPolygonFromText(DbExpression multiPolygonWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPolygonFromText", multiPolygonWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyCollectionFromText(DbExpression geographyCollectionWellKnownText, DbExpression coordinateSystemId)
	{
		Check.NotNull(geographyCollectionWellKnownText, "geographyCollectionWellKnownText");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyCollectionFromText", geographyCollectionWellKnownText, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyFromBinary(DbExpression wellKnownBinaryValue)
	{
		Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
		return EdmFunctions.InvokeCanonicalFunction("GeographyFromBinary", wellKnownBinaryValue);
	}

	public static DbFunctionExpression GeographyFromBinary(DbExpression wellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyFromBinary", wellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyPointFromBinary(DbExpression pointWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(pointWellKnownBinaryValue, "pointWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyPointFromBinary", pointWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyLineFromBinary(DbExpression lineWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(lineWellKnownBinaryValue, "lineWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyLineFromBinary", lineWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyPolygonFromBinary(DbExpression polygonWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownBinaryValue, "polygonWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyPolygonFromBinary", polygonWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyMultiPointFromBinary(DbExpression multiPointWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownBinaryValue, "multiPointWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPointFromBinary", multiPointWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyMultiLineFromBinary(DbExpression multiLineWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownBinaryValue, "multiLineWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyMultiLineFromBinary", multiLineWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyMultiPolygonFromBinary(DbExpression multiPolygonWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownBinaryValue, "multiPolygonWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPolygonFromBinary", multiPolygonWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyCollectionFromBinary(DbExpression geographyCollectionWellKnownBinaryValue, DbExpression coordinateSystemId)
	{
		Check.NotNull(geographyCollectionWellKnownBinaryValue, "geographyCollectionWellKnownBinaryValue");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyCollectionFromBinary", geographyCollectionWellKnownBinaryValue, coordinateSystemId);
	}

	public static DbFunctionExpression GeographyFromGml(DbExpression geographyMarkup)
	{
		Check.NotNull(geographyMarkup, "geographyMarkup");
		return EdmFunctions.InvokeCanonicalFunction("GeographyFromGml", geographyMarkup);
	}

	public static DbFunctionExpression GeographyFromGml(DbExpression geographyMarkup, DbExpression coordinateSystemId)
	{
		Check.NotNull(geographyMarkup, "geographyMarkup");
		Check.NotNull(coordinateSystemId, "coordinateSystemId");
		return EdmFunctions.InvokeCanonicalFunction("GeographyFromGml", geographyMarkup, coordinateSystemId);
	}

	public static DbFunctionExpression CoordinateSystemId(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("CoordinateSystemId", spatialValue);
	}

	public static DbFunctionExpression SpatialTypeName(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialTypeName", spatialValue);
	}

	public static DbFunctionExpression SpatialDimension(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialDimension", spatialValue);
	}

	public static DbFunctionExpression SpatialEnvelope(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialEnvelope", geometryValue);
	}

	public static DbFunctionExpression AsBinary(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("AsBinary", spatialValue);
	}

	public static DbFunctionExpression AsGml(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("AsGml", spatialValue);
	}

	public static DbFunctionExpression AsText(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("AsText", spatialValue);
	}

	public static DbFunctionExpression IsEmptySpatial(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("IsEmptySpatial", spatialValue);
	}

	public static DbFunctionExpression IsSimpleGeometry(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("IsSimpleGeometry", geometryValue);
	}

	public static DbFunctionExpression SpatialBoundary(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialBoundary", geometryValue);
	}

	public static DbFunctionExpression IsValidGeometry(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("IsValidGeometry", geometryValue);
	}

	public static DbFunctionExpression SpatialEquals(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialEquals", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialDisjoint(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialDisjoint", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialIntersects(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialIntersects", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialTouches(this DbExpression geometryValue1, DbExpression geometryValue2)
	{
		Check.NotNull(geometryValue1, "geometryValue1");
		Check.NotNull(geometryValue2, "geometryValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialTouches", geometryValue1, geometryValue2);
	}

	public static DbFunctionExpression SpatialCrosses(this DbExpression geometryValue1, DbExpression geometryValue2)
	{
		Check.NotNull(geometryValue1, "geometryValue1");
		Check.NotNull(geometryValue2, "geometryValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialCrosses", geometryValue1, geometryValue2);
	}

	public static DbFunctionExpression SpatialWithin(this DbExpression geometryValue1, DbExpression geometryValue2)
	{
		Check.NotNull(geometryValue1, "geometryValue1");
		Check.NotNull(geometryValue2, "geometryValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialWithin", geometryValue1, geometryValue2);
	}

	public static DbFunctionExpression SpatialContains(this DbExpression geometryValue1, DbExpression geometryValue2)
	{
		Check.NotNull(geometryValue1, "geometryValue1");
		Check.NotNull(geometryValue2, "geometryValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialContains", geometryValue1, geometryValue2);
	}

	public static DbFunctionExpression SpatialOverlaps(this DbExpression geometryValue1, DbExpression geometryValue2)
	{
		Check.NotNull(geometryValue1, "geometryValue1");
		Check.NotNull(geometryValue2, "geometryValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialOverlaps", geometryValue1, geometryValue2);
	}

	public static DbFunctionExpression SpatialRelate(this DbExpression geometryValue1, DbExpression geometryValue2, DbExpression intersectionPatternMatrix)
	{
		Check.NotNull(geometryValue1, "geometryValue1");
		Check.NotNull(geometryValue2, "geometryValue2");
		Check.NotNull(intersectionPatternMatrix, "intersectionPatternMatrix");
		return EdmFunctions.InvokeCanonicalFunction("SpatialRelate", geometryValue1, geometryValue2, intersectionPatternMatrix);
	}

	public static DbFunctionExpression SpatialBuffer(this DbExpression spatialValue, DbExpression distance)
	{
		Check.NotNull(spatialValue, "spatialValue");
		Check.NotNull(distance, "distance");
		return EdmFunctions.InvokeCanonicalFunction("SpatialBuffer", spatialValue, distance);
	}

	public static DbFunctionExpression Distance(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("Distance", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialConvexHull(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialConvexHull", geometryValue);
	}

	public static DbFunctionExpression SpatialIntersection(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialIntersection", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialUnion(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialUnion", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialDifference(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialDifference", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialSymmetricDifference(this DbExpression spatialValue1, DbExpression spatialValue2)
	{
		Check.NotNull(spatialValue1, "spatialValue1");
		Check.NotNull(spatialValue2, "spatialValue2");
		return EdmFunctions.InvokeCanonicalFunction("SpatialSymmetricDifference", spatialValue1, spatialValue2);
	}

	public static DbFunctionExpression SpatialElementCount(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialElementCount", spatialValue);
	}

	public static DbFunctionExpression SpatialElementAt(this DbExpression spatialValue, DbExpression indexValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		Check.NotNull(indexValue, "indexValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialElementAt", spatialValue, indexValue);
	}

	public static DbFunctionExpression XCoordinate(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("XCoordinate", geometryValue);
	}

	public static DbFunctionExpression YCoordinate(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("YCoordinate", geometryValue);
	}

	public static DbFunctionExpression Elevation(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("Elevation", spatialValue);
	}

	public static DbFunctionExpression Measure(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("Measure", spatialValue);
	}

	public static DbFunctionExpression Latitude(this DbExpression geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		return EdmFunctions.InvokeCanonicalFunction("Latitude", geographyValue);
	}

	public static DbFunctionExpression Longitude(this DbExpression geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		return EdmFunctions.InvokeCanonicalFunction("Longitude", geographyValue);
	}

	public static DbFunctionExpression SpatialLength(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("SpatialLength", spatialValue);
	}

	public static DbFunctionExpression StartPoint(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("StartPoint", spatialValue);
	}

	public static DbFunctionExpression EndPoint(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("EndPoint", spatialValue);
	}

	public static DbFunctionExpression IsClosedSpatial(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("IsClosedSpatial", spatialValue);
	}

	public static DbFunctionExpression IsRing(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("IsRing", geometryValue);
	}

	public static DbFunctionExpression PointCount(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("PointCount", spatialValue);
	}

	public static DbFunctionExpression PointAt(this DbExpression spatialValue, DbExpression indexValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		Check.NotNull(indexValue, "indexValue");
		return EdmFunctions.InvokeCanonicalFunction("PointAt", spatialValue, indexValue);
	}

	public static DbFunctionExpression Area(this DbExpression spatialValue)
	{
		Check.NotNull(spatialValue, "spatialValue");
		return EdmFunctions.InvokeCanonicalFunction("Area", spatialValue);
	}

	public static DbFunctionExpression Centroid(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("Centroid", geometryValue);
	}

	public static DbFunctionExpression PointOnSurface(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("PointOnSurface", geometryValue);
	}

	public static DbFunctionExpression ExteriorRing(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("ExteriorRing", geometryValue);
	}

	public static DbFunctionExpression InteriorRingCount(this DbExpression geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return EdmFunctions.InvokeCanonicalFunction("InteriorRingCount", geometryValue);
	}

	public static DbFunctionExpression InteriorRingAt(this DbExpression geometryValue, DbExpression indexValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		Check.NotNull(indexValue, "indexValue");
		return EdmFunctions.InvokeCanonicalFunction("InteriorRingAt", geometryValue, indexValue);
	}
}
