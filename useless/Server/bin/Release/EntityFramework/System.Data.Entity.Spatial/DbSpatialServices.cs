using System.Data.Entity.Utilities;

namespace System.Data.Entity.Spatial;

[Serializable]
public abstract class DbSpatialServices
{
	private static readonly Lazy<DbSpatialServices> _defaultServices = new Lazy<DbSpatialServices>(() => new SpatialServicesLoader(DbConfiguration.DependencyResolver).LoadDefaultServices(), isThreadSafe: true);

	public static DbSpatialServices Default => _defaultServices.Value;

	public virtual bool NativeTypesAvailable => true;

	protected static DbGeography CreateGeography(DbSpatialServices spatialServices, object providerValue)
	{
		Check.NotNull(spatialServices, "spatialServices");
		Check.NotNull(providerValue, "providerValue");
		return new DbGeography(spatialServices, providerValue);
	}

	public abstract DbGeography GeographyFromProviderValue(object providerValue);

	public abstract object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue);

	public abstract DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue);

	public abstract DbGeography GeographyFromBinary(byte[] wellKnownBinary);

	public abstract DbGeography GeographyFromBinary(byte[] wellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId);

	public abstract DbGeography GeographyFromText(string wellKnownText);

	public abstract DbGeography GeographyFromText(string wellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyLineFromText(string lineWellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyPointFromText(string pointWellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyPolygonFromText(string polygonWellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId);

	public abstract DbGeography GeographyFromGml(string geographyMarkup);

	public abstract DbGeography GeographyFromGml(string geographyMarkup, int coordinateSystemId);

	public abstract int GetCoordinateSystemId(DbGeography geographyValue);

	public abstract int GetDimension(DbGeography geographyValue);

	public abstract string GetSpatialTypeName(DbGeography geographyValue);

	public abstract bool GetIsEmpty(DbGeography geographyValue);

	public abstract string AsText(DbGeography geographyValue);

	public virtual string AsTextIncludingElevationAndMeasure(DbGeography geographyValue)
	{
		return null;
	}

	public abstract byte[] AsBinary(DbGeography geographyValue);

	public abstract string AsGml(DbGeography geographyValue);

	public abstract bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography);

	public abstract bool Disjoint(DbGeography geographyValue, DbGeography otherGeography);

	public abstract bool Intersects(DbGeography geographyValue, DbGeography otherGeography);

	public abstract DbGeography Buffer(DbGeography geographyValue, double distance);

	public abstract double Distance(DbGeography geographyValue, DbGeography otherGeography);

	public abstract DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography);

	public abstract DbGeography Union(DbGeography geographyValue, DbGeography otherGeography);

	public abstract DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography);

	public abstract DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography);

	public abstract int? GetElementCount(DbGeography geographyValue);

	public abstract DbGeography ElementAt(DbGeography geographyValue, int index);

	public abstract double? GetLatitude(DbGeography geographyValue);

	public abstract double? GetLongitude(DbGeography geographyValue);

	public abstract double? GetElevation(DbGeography geographyValue);

	public abstract double? GetMeasure(DbGeography geographyValue);

	public abstract double? GetLength(DbGeography geographyValue);

	public abstract DbGeography GetStartPoint(DbGeography geographyValue);

	public abstract DbGeography GetEndPoint(DbGeography geographyValue);

	public abstract bool? GetIsClosed(DbGeography geographyValue);

	public abstract int? GetPointCount(DbGeography geographyValue);

	public abstract DbGeography PointAt(DbGeography geographyValue, int index);

	public abstract double? GetArea(DbGeography geographyValue);

	protected static DbGeometry CreateGeometry(DbSpatialServices spatialServices, object providerValue)
	{
		Check.NotNull(spatialServices, "spatialServices");
		Check.NotNull(providerValue, "providerValue");
		return new DbGeometry(spatialServices, providerValue);
	}

	public abstract object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue);

	public abstract DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue);

	public abstract DbGeometry GeometryFromProviderValue(object providerValue);

	public abstract DbGeometry GeometryFromBinary(byte[] wellKnownBinary);

	public abstract DbGeometry GeometryFromBinary(byte[] wellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId);

	public abstract DbGeometry GeometryFromText(string wellKnownText);

	public abstract DbGeometry GeometryFromText(string wellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryLineFromText(string lineWellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryPointFromText(string pointWellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryPolygonFromText(string polygonWellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId);

	public abstract DbGeometry GeometryFromGml(string geometryMarkup);

	public abstract DbGeometry GeometryFromGml(string geometryMarkup, int coordinateSystemId);

	public abstract int GetCoordinateSystemId(DbGeometry geometryValue);

	public abstract DbGeometry GetBoundary(DbGeometry geometryValue);

	public abstract int GetDimension(DbGeometry geometryValue);

	public abstract DbGeometry GetEnvelope(DbGeometry geometryValue);

	public abstract string GetSpatialTypeName(DbGeometry geometryValue);

	public abstract bool GetIsEmpty(DbGeometry geometryValue);

	public abstract bool GetIsSimple(DbGeometry geometryValue);

	public abstract bool GetIsValid(DbGeometry geometryValue);

	public abstract string AsText(DbGeometry geometryValue);

	public virtual string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
	{
		return null;
	}

	public abstract byte[] AsBinary(DbGeometry geometryValue);

	public abstract string AsGml(DbGeometry geometryValue);

	public abstract bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Within(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix);

	public abstract DbGeometry Buffer(DbGeometry geometryValue, double distance);

	public abstract double Distance(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract DbGeometry GetConvexHull(DbGeometry geometryValue);

	public abstract DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry);

	public abstract int? GetElementCount(DbGeometry geometryValue);

	public abstract DbGeometry ElementAt(DbGeometry geometryValue, int index);

	public abstract double? GetXCoordinate(DbGeometry geometryValue);

	public abstract double? GetYCoordinate(DbGeometry geometryValue);

	public abstract double? GetElevation(DbGeometry geometryValue);

	public abstract double? GetMeasure(DbGeometry geometryValue);

	public abstract double? GetLength(DbGeometry geometryValue);

	public abstract DbGeometry GetStartPoint(DbGeometry geometryValue);

	public abstract DbGeometry GetEndPoint(DbGeometry geometryValue);

	public abstract bool? GetIsClosed(DbGeometry geometryValue);

	public abstract bool? GetIsRing(DbGeometry geometryValue);

	public abstract int? GetPointCount(DbGeometry geometryValue);

	public abstract DbGeometry PointAt(DbGeometry geometryValue, int index);

	public abstract double? GetArea(DbGeometry geometryValue);

	public abstract DbGeometry GetCentroid(DbGeometry geometryValue);

	public abstract DbGeometry GetPointOnSurface(DbGeometry geometryValue);

	public abstract DbGeometry GetExteriorRing(DbGeometry geometryValue);

	public abstract int? GetInteriorRingCount(DbGeometry geometryValue);

	public abstract DbGeometry InteriorRingAt(DbGeometry geometryValue, int index);
}
