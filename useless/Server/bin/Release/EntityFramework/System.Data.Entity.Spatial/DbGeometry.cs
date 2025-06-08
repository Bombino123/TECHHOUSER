using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.Entity.Spatial;

[Serializable]
[DataContract]
public class DbGeometry
{
	private DbSpatialServices _spatialProvider;

	private object _providerValue;

	public static int DefaultCoordinateSystemId => 0;

	public object ProviderValue => _providerValue;

	public virtual DbSpatialServices Provider => _spatialProvider;

	[DataMember(Name = "Geometry")]
	public DbGeometryWellKnownValue WellKnownValue
	{
		get
		{
			return _spatialProvider.CreateWellKnownValue(this);
		}
		set
		{
			if (_spatialProvider != null)
			{
				throw new InvalidOperationException(Strings.Spatial_WellKnownValueSerializationPropertyNotDirectlySettable);
			}
			DbSpatialServices @default = DbSpatialServices.Default;
			_providerValue = @default.CreateProviderValue(value);
			_spatialProvider = @default;
		}
	}

	public int CoordinateSystemId => _spatialProvider.GetCoordinateSystemId(this);

	public DbGeometry Boundary => _spatialProvider.GetBoundary(this);

	public int Dimension => _spatialProvider.GetDimension(this);

	public DbGeometry Envelope => _spatialProvider.GetEnvelope(this);

	public string SpatialTypeName => _spatialProvider.GetSpatialTypeName(this);

	public bool IsEmpty => _spatialProvider.GetIsEmpty(this);

	public bool IsSimple => _spatialProvider.GetIsSimple(this);

	public bool IsValid => _spatialProvider.GetIsValid(this);

	public DbGeometry ConvexHull => _spatialProvider.GetConvexHull(this);

	public int? ElementCount => _spatialProvider.GetElementCount(this);

	public double? XCoordinate => _spatialProvider.GetXCoordinate(this);

	public double? YCoordinate => _spatialProvider.GetYCoordinate(this);

	public double? Elevation => _spatialProvider.GetElevation(this);

	public double? Measure => _spatialProvider.GetMeasure(this);

	public double? Length => _spatialProvider.GetLength(this);

	public DbGeometry StartPoint => _spatialProvider.GetStartPoint(this);

	public DbGeometry EndPoint => _spatialProvider.GetEndPoint(this);

	public bool? IsClosed => _spatialProvider.GetIsClosed(this);

	public bool? IsRing => _spatialProvider.GetIsRing(this);

	public int? PointCount => _spatialProvider.GetPointCount(this);

	public double? Area => _spatialProvider.GetArea(this);

	public DbGeometry Centroid => _spatialProvider.GetCentroid(this);

	public DbGeometry PointOnSurface => _spatialProvider.GetPointOnSurface(this);

	public DbGeometry ExteriorRing => _spatialProvider.GetExteriorRing(this);

	public int? InteriorRingCount => _spatialProvider.GetInteriorRingCount(this);

	internal DbGeometry()
	{
	}

	internal DbGeometry(DbSpatialServices spatialServices, object spatialProviderValue)
	{
		_spatialProvider = spatialServices;
		_providerValue = spatialProviderValue;
	}

	public static DbGeometry FromBinary(byte[] wellKnownBinary)
	{
		Check.NotNull(wellKnownBinary, "wellKnownBinary");
		return DbSpatialServices.Default.GeometryFromBinary(wellKnownBinary);
	}

	public static DbGeometry FromBinary(byte[] wellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(wellKnownBinary, "wellKnownBinary");
		return DbSpatialServices.Default.GeometryFromBinary(wellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry LineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(lineWellKnownBinary, "lineWellKnownBinary");
		return DbSpatialServices.Default.GeometryLineFromBinary(lineWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry PointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(pointWellKnownBinary, "pointWellKnownBinary");
		return DbSpatialServices.Default.GeometryPointFromBinary(pointWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry PolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownBinary, "polygonWellKnownBinary");
		return DbSpatialServices.Default.GeometryPolygonFromBinary(polygonWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry MultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownBinary, "multiLineWellKnownBinary");
		return DbSpatialServices.Default.GeometryMultiLineFromBinary(multiLineWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry MultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownBinary, "multiPointWellKnownBinary");
		return DbSpatialServices.Default.GeometryMultiPointFromBinary(multiPointWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry MultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownBinary, "multiPolygonWellKnownBinary");
		return DbSpatialServices.Default.GeometryMultiPolygonFromBinary(multiPolygonWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(geometryCollectionWellKnownBinary, "geometryCollectionWellKnownBinary");
		return DbSpatialServices.Default.GeometryCollectionFromBinary(geometryCollectionWellKnownBinary, coordinateSystemId);
	}

	public static DbGeometry FromGml(string geometryMarkup)
	{
		Check.NotNull(geometryMarkup, "geometryMarkup");
		return DbSpatialServices.Default.GeometryFromGml(geometryMarkup);
	}

	public static DbGeometry FromGml(string geometryMarkup, int coordinateSystemId)
	{
		Check.NotNull(geometryMarkup, "geometryMarkup");
		return DbSpatialServices.Default.GeometryFromGml(geometryMarkup, coordinateSystemId);
	}

	public static DbGeometry FromText(string wellKnownText)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		return DbSpatialServices.Default.GeometryFromText(wellKnownText);
	}

	public static DbGeometry FromText(string wellKnownText, int coordinateSystemId)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		return DbSpatialServices.Default.GeometryFromText(wellKnownText, coordinateSystemId);
	}

	public static DbGeometry LineFromText(string lineWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(lineWellKnownText, "lineWellKnownText");
		return DbSpatialServices.Default.GeometryLineFromText(lineWellKnownText, coordinateSystemId);
	}

	public static DbGeometry PointFromText(string pointWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(pointWellKnownText, "pointWellKnownText");
		return DbSpatialServices.Default.GeometryPointFromText(pointWellKnownText, coordinateSystemId);
	}

	public static DbGeometry PolygonFromText(string polygonWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
		return DbSpatialServices.Default.GeometryPolygonFromText(polygonWellKnownText, coordinateSystemId);
	}

	public static DbGeometry MultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
		return DbSpatialServices.Default.GeometryMultiLineFromText(multiLineWellKnownText, coordinateSystemId);
	}

	public static DbGeometry MultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
		return DbSpatialServices.Default.GeometryMultiPointFromText(multiPointWellKnownText, coordinateSystemId);
	}

	public static DbGeometry MultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
		return DbSpatialServices.Default.GeometryMultiPolygonFromText(multiPolygonWellKnownText, coordinateSystemId);
	}

	public static DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(geometryCollectionWellKnownText, "geometryCollectionWellKnownText");
		return DbSpatialServices.Default.GeometryCollectionFromText(geometryCollectionWellKnownText, coordinateSystemId);
	}

	public virtual string AsText()
	{
		return _spatialProvider.AsText(this);
	}

	internal string AsTextIncludingElevationAndMeasure()
	{
		return _spatialProvider.AsTextIncludingElevationAndMeasure(this);
	}

	public byte[] AsBinary()
	{
		return _spatialProvider.AsBinary(this);
	}

	public string AsGml()
	{
		return _spatialProvider.AsGml(this);
	}

	public bool SpatialEquals(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.SpatialEquals(this, other);
	}

	public bool Disjoint(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Disjoint(this, other);
	}

	public bool Intersects(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Intersects(this, other);
	}

	public bool Touches(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Touches(this, other);
	}

	public bool Crosses(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Crosses(this, other);
	}

	public bool Within(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Within(this, other);
	}

	public bool Contains(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Contains(this, other);
	}

	public bool Overlaps(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Overlaps(this, other);
	}

	public bool Relate(DbGeometry other, string matrix)
	{
		Check.NotNull(other, "other");
		Check.NotNull(matrix, "matrix");
		return _spatialProvider.Relate(this, other, matrix);
	}

	public DbGeometry Buffer(double? distance)
	{
		Check.NotNull(distance, "distance");
		return _spatialProvider.Buffer(this, distance.Value);
	}

	public double? Distance(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Distance(this, other);
	}

	public DbGeometry Intersection(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Intersection(this, other);
	}

	public DbGeometry Union(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Union(this, other);
	}

	public DbGeometry Difference(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Difference(this, other);
	}

	public DbGeometry SymmetricDifference(DbGeometry other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.SymmetricDifference(this, other);
	}

	public DbGeometry ElementAt(int index)
	{
		return _spatialProvider.ElementAt(this, index);
	}

	public DbGeometry PointAt(int index)
	{
		return _spatialProvider.PointAt(this, index);
	}

	public DbGeometry InteriorRingAt(int index)
	{
		return _spatialProvider.InteriorRingAt(this, index);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "SRID={1};{0}", new object[2]
		{
			WellKnownValue.WellKnownText ?? base.ToString(),
			CoordinateSystemId
		});
	}
}
