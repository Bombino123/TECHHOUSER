using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Spatial;

[Serializable]
internal sealed class DefaultSpatialServices : DbSpatialServices
{
	[Serializable]
	private sealed class ReadOnlySpatialValues
	{
		private readonly int srid;

		private readonly byte[] wkb;

		private readonly string wkt;

		private readonly string gml;

		internal int CoordinateSystemId => srid;

		internal string Text => wkt;

		internal string GML => gml;

		internal ReadOnlySpatialValues(int spatialRefSysId, string textValue, byte[] binaryValue, string gmlValue)
		{
			srid = spatialRefSysId;
			wkb = ((binaryValue == null) ? null : ((byte[])binaryValue.Clone()));
			wkt = textValue;
			gml = gmlValue;
		}

		internal byte[] CloneBinary()
		{
			if (wkb != null)
			{
				return (byte[])wkb.Clone();
			}
			return null;
		}
	}

	internal static readonly DefaultSpatialServices Instance = new DefaultSpatialServices();

	private DefaultSpatialServices()
	{
	}

	private static Exception SpatialServicesUnavailable()
	{
		return new NotImplementedException(Strings.SpatialProviderNotUsable);
	}

	private static ReadOnlySpatialValues CheckProviderValue(object providerValue)
	{
		return (providerValue as ReadOnlySpatialValues) ?? throw new ArgumentException(Strings.Spatial_ProviderValueNotCompatibleWithSpatialServices, "providerValue");
	}

	private static ReadOnlySpatialValues CheckCompatible(DbGeography geographyValue)
	{
		if (geographyValue != null && geographyValue.ProviderValue is ReadOnlySpatialValues result)
		{
			return result;
		}
		throw new ArgumentException(Strings.Spatial_GeographyValueNotCompatibleWithSpatialServices, "geographyValue");
	}

	private static ReadOnlySpatialValues CheckCompatible(DbGeometry geometryValue)
	{
		if (geometryValue != null && geometryValue.ProviderValue is ReadOnlySpatialValues result)
		{
			return result;
		}
		throw new ArgumentException(Strings.Spatial_GeometryValueNotCompatibleWithSpatialServices, "geometryValue");
	}

	public override DbGeography GeographyFromProviderValue(object providerValue)
	{
		Check.NotNull(providerValue, "providerValue");
		ReadOnlySpatialValues providerValue2 = CheckProviderValue(providerValue);
		return DbSpatialServices.CreateGeography(this, providerValue2);
	}

	public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
	{
		Check.NotNull(wellKnownValue, "wellKnownValue");
		return new ReadOnlySpatialValues(wellKnownValue.CoordinateSystemId, wellKnownValue.WellKnownText, wellKnownValue.WellKnownBinary, null);
	}

	public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		ReadOnlySpatialValues readOnlySpatialValues = CheckCompatible(geographyValue);
		return new DbGeographyWellKnownValue
		{
			CoordinateSystemId = readOnlySpatialValues.CoordinateSystemId,
			WellKnownBinary = readOnlySpatialValues.CloneBinary(),
			WellKnownText = readOnlySpatialValues.Text
		};
	}

	public override DbGeography GeographyFromBinary(byte[] geographyBinary)
	{
		Check.NotNull(geographyBinary, "geographyBinary");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(DbGeography.DefaultCoordinateSystemId, null, geographyBinary, null);
		return DbSpatialServices.CreateGeography(this, providerValue);
	}

	public override DbGeography GeographyFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		Check.NotNull(geographyBinary, "geographyBinary");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(spatialReferenceSystemId, null, geographyBinary, null);
		return DbSpatialServices.CreateGeography(this, providerValue);
	}

	public override DbGeography GeographyLineFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyPointFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyPolygonFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyMultiLineFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyMultiPointFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyMultiPolygonFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyCollectionFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyFromText(string geographyText)
	{
		Check.NotNull(geographyText, "geographyText");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(DbGeography.DefaultCoordinateSystemId, geographyText, null, null);
		return DbSpatialServices.CreateGeography(this, providerValue);
	}

	public override DbGeography GeographyFromText(string geographyText, int spatialReferenceSystemId)
	{
		Check.NotNull(geographyText, "geographyText");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(spatialReferenceSystemId, geographyText, null, null);
		return DbSpatialServices.CreateGeography(this, providerValue);
	}

	public override DbGeography GeographyLineFromText(string geographyText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyPointFromText(string geographyText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyPolygonFromText(string geographyText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyMultiLineFromText(string geographyText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyMultiPointFromText(string geographyText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyMultiPolygonFromText(string multiPolygonKnownText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyCollectionFromText(string geographyText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GeographyFromGml(string geographyMarkup)
	{
		Check.NotNull(geographyMarkup, "geographyMarkup");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(DbGeography.DefaultCoordinateSystemId, null, null, geographyMarkup);
		return DbSpatialServices.CreateGeography(this, providerValue);
	}

	public override DbGeography GeographyFromGml(string geographyMarkup, int spatialReferenceSystemId)
	{
		Check.NotNull(geographyMarkup, "geographyMarkup");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(spatialReferenceSystemId, null, null, geographyMarkup);
		return DbSpatialServices.CreateGeography(this, providerValue);
	}

	public override int GetCoordinateSystemId(DbGeography geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		return CheckCompatible(geographyValue).CoordinateSystemId;
	}

	public override int GetDimension(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override string GetSpatialTypeName(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool GetIsEmpty(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override string AsText(DbGeography geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		return CheckCompatible(geographyValue).Text;
	}

	public override byte[] AsBinary(DbGeography geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		return CheckCompatible(geographyValue).CloneBinary();
	}

	public override string AsGml(DbGeography geographyValue)
	{
		Check.NotNull(geographyValue, "geographyValue");
		return CheckCompatible(geographyValue).GML;
	}

	public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography Buffer(DbGeography geographyValue, double distance)
	{
		throw SpatialServicesUnavailable();
	}

	public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
	{
		throw SpatialServicesUnavailable();
	}

	public override int? GetElementCount(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography ElementAt(DbGeography geographyValue, int index)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetLatitude(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetLongitude(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetElevation(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetMeasure(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetLength(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GetEndPoint(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography GetStartPoint(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool? GetIsClosed(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override int? GetPointCount(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeography PointAt(DbGeography geographyValue, int index)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetArea(DbGeography geographyValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
	{
		Check.NotNull(wellKnownValue, "wellKnownValue");
		return new ReadOnlySpatialValues(wellKnownValue.CoordinateSystemId, wellKnownValue.WellKnownText, wellKnownValue.WellKnownBinary, null);
	}

	public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		ReadOnlySpatialValues readOnlySpatialValues = CheckCompatible(geometryValue);
		return new DbGeometryWellKnownValue
		{
			CoordinateSystemId = readOnlySpatialValues.CoordinateSystemId,
			WellKnownBinary = readOnlySpatialValues.CloneBinary(),
			WellKnownText = readOnlySpatialValues.Text
		};
	}

	public override DbGeometry GeometryFromProviderValue(object providerValue)
	{
		Check.NotNull(providerValue, "providerValue");
		ReadOnlySpatialValues providerValue2 = CheckProviderValue(providerValue);
		return DbSpatialServices.CreateGeometry(this, providerValue2);
	}

	public override DbGeometry GeometryFromBinary(byte[] geometryBinary)
	{
		Check.NotNull(geometryBinary, "geometryBinary");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(DbGeometry.DefaultCoordinateSystemId, null, geometryBinary, null);
		return DbSpatialServices.CreateGeometry(this, providerValue);
	}

	public override DbGeometry GeometryFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		Check.NotNull(geometryBinary, "geometryBinary");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(spatialReferenceSystemId, null, geometryBinary, null);
		return DbSpatialServices.CreateGeometry(this, providerValue);
	}

	public override DbGeometry GeometryLineFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryPointFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryPolygonFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryMultiLineFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryMultiPointFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryMultiPolygonFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryCollectionFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryFromText(string geometryText)
	{
		Check.NotNull(geometryText, "geometryText");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(DbGeometry.DefaultCoordinateSystemId, geometryText, null, null);
		return DbSpatialServices.CreateGeometry(this, providerValue);
	}

	public override DbGeometry GeometryFromText(string geometryText, int spatialReferenceSystemId)
	{
		Check.NotNull(geometryText, "geometryText");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(spatialReferenceSystemId, geometryText, null, null);
		return DbSpatialServices.CreateGeometry(this, providerValue);
	}

	public override DbGeometry GeometryLineFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryPointFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryPolygonFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryMultiLineFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryMultiPointFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryMultiPolygonFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryCollectionFromText(string geometryText, int spatialReferenceSystemId)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GeometryFromGml(string geometryMarkup)
	{
		Check.NotNull(geometryMarkup, "geometryMarkup");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(DbGeometry.DefaultCoordinateSystemId, null, null, geometryMarkup);
		return DbSpatialServices.CreateGeometry(this, providerValue);
	}

	public override DbGeometry GeometryFromGml(string geometryMarkup, int spatialReferenceSystemId)
	{
		Check.NotNull(geometryMarkup, "geometryMarkup");
		ReadOnlySpatialValues providerValue = new ReadOnlySpatialValues(spatialReferenceSystemId, null, null, geometryMarkup);
		return DbSpatialServices.CreateGeometry(this, providerValue);
	}

	public override int GetCoordinateSystemId(DbGeometry geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return CheckCompatible(geometryValue).CoordinateSystemId;
	}

	public override DbGeometry GetBoundary(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override int GetDimension(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetEnvelope(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override string GetSpatialTypeName(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool GetIsEmpty(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool GetIsSimple(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool GetIsValid(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override string AsText(DbGeometry geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return CheckCompatible(geometryValue).Text;
	}

	public override byte[] AsBinary(DbGeometry geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return CheckCompatible(geometryValue).CloneBinary();
	}

	public override string AsGml(DbGeometry geometryValue)
	{
		Check.NotNull(geometryValue, "geometryValue");
		return CheckCompatible(geometryValue).GML;
	}

	public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
	{
		throw SpatialServicesUnavailable();
	}

	public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetConvexHull(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
	{
		throw SpatialServicesUnavailable();
	}

	public override int? GetElementCount(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry ElementAt(DbGeometry geometryValue, int index)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetXCoordinate(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetYCoordinate(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetElevation(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetMeasure(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetLength(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetEndPoint(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetStartPoint(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool? GetIsClosed(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override bool? GetIsRing(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override int? GetPointCount(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry PointAt(DbGeometry geometryValue, int index)
	{
		throw SpatialServicesUnavailable();
	}

	public override double? GetArea(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetCentroid(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override int? GetInteriorRingCount(DbGeometry geometryValue)
	{
		throw SpatialServicesUnavailable();
	}

	public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int index)
	{
		throw SpatialServicesUnavailable();
	}
}
