using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.Entity.Spatial;

[Serializable]
[DataContract]
public class DbGeography
{
	private DbSpatialServices _spatialProvider;

	private object _providerValue;

	public static int DefaultCoordinateSystemId => 4326;

	public object ProviderValue => _providerValue;

	public virtual DbSpatialServices Provider => _spatialProvider;

	[DataMember(Name = "Geography")]
	public DbGeographyWellKnownValue WellKnownValue
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

	public int Dimension => _spatialProvider.GetDimension(this);

	public string SpatialTypeName => _spatialProvider.GetSpatialTypeName(this);

	public bool IsEmpty => _spatialProvider.GetIsEmpty(this);

	public int? ElementCount => _spatialProvider.GetElementCount(this);

	public double? Latitude => _spatialProvider.GetLatitude(this);

	public double? Longitude => _spatialProvider.GetLongitude(this);

	public double? Elevation => _spatialProvider.GetElevation(this);

	public double? Measure => _spatialProvider.GetMeasure(this);

	public double? Length => _spatialProvider.GetLength(this);

	public DbGeography StartPoint => _spatialProvider.GetStartPoint(this);

	public DbGeography EndPoint => _spatialProvider.GetEndPoint(this);

	public bool? IsClosed => _spatialProvider.GetIsClosed(this);

	public int? PointCount => _spatialProvider.GetPointCount(this);

	public double? Area => _spatialProvider.GetArea(this);

	internal DbGeography()
	{
	}

	internal DbGeography(DbSpatialServices spatialServices, object spatialProviderValue)
	{
		_spatialProvider = spatialServices;
		_providerValue = spatialProviderValue;
	}

	public static DbGeography FromBinary(byte[] wellKnownBinary)
	{
		Check.NotNull(wellKnownBinary, "wellKnownBinary");
		return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary);
	}

	public static DbGeography FromBinary(byte[] wellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(wellKnownBinary, "wellKnownBinary");
		return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary, coordinateSystemId);
	}

	public static DbGeography LineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(lineWellKnownBinary, "lineWellKnownBinary");
		return DbSpatialServices.Default.GeographyLineFromBinary(lineWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography PointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(pointWellKnownBinary, "pointWellKnownBinary");
		return DbSpatialServices.Default.GeographyPointFromBinary(pointWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography PolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownBinary, "polygonWellKnownBinary");
		return DbSpatialServices.Default.GeographyPolygonFromBinary(polygonWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography MultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownBinary, "multiLineWellKnownBinary");
		return DbSpatialServices.Default.GeographyMultiLineFromBinary(multiLineWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography MultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownBinary, "multiPointWellKnownBinary");
		return DbSpatialServices.Default.GeographyMultiPointFromBinary(multiPointWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography MultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownBinary, "multiPolygonWellKnownBinary");
		return DbSpatialServices.Default.GeographyMultiPolygonFromBinary(multiPolygonWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
	{
		Check.NotNull(geographyCollectionWellKnownBinary, "geographyCollectionWellKnownBinary");
		return DbSpatialServices.Default.GeographyCollectionFromBinary(geographyCollectionWellKnownBinary, coordinateSystemId);
	}

	public static DbGeography FromGml(string geographyMarkup)
	{
		Check.NotNull(geographyMarkup, "geographyMarkup");
		return DbSpatialServices.Default.GeographyFromGml(geographyMarkup);
	}

	public static DbGeography FromGml(string geographyMarkup, int coordinateSystemId)
	{
		Check.NotNull(geographyMarkup, "geographyMarkup");
		return DbSpatialServices.Default.GeographyFromGml(geographyMarkup, coordinateSystemId);
	}

	public static DbGeography FromText(string wellKnownText)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		return DbSpatialServices.Default.GeographyFromText(wellKnownText);
	}

	public static DbGeography FromText(string wellKnownText, int coordinateSystemId)
	{
		Check.NotNull(wellKnownText, "wellKnownText");
		return DbSpatialServices.Default.GeographyFromText(wellKnownText, coordinateSystemId);
	}

	public static DbGeography LineFromText(string lineWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(lineWellKnownText, "lineWellKnownText");
		return DbSpatialServices.Default.GeographyLineFromText(lineWellKnownText, coordinateSystemId);
	}

	public static DbGeography PointFromText(string pointWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(pointWellKnownText, "pointWellKnownText");
		return DbSpatialServices.Default.GeographyPointFromText(pointWellKnownText, coordinateSystemId);
	}

	public static DbGeography PolygonFromText(string polygonWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
		return DbSpatialServices.Default.GeographyPolygonFromText(polygonWellKnownText, coordinateSystemId);
	}

	public static DbGeography MultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
		return DbSpatialServices.Default.GeographyMultiLineFromText(multiLineWellKnownText, coordinateSystemId);
	}

	public static DbGeography MultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
		return DbSpatialServices.Default.GeographyMultiPointFromText(multiPointWellKnownText, coordinateSystemId);
	}

	public static DbGeography MultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
		return DbSpatialServices.Default.GeographyMultiPolygonFromText(multiPolygonWellKnownText, coordinateSystemId);
	}

	public static DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
	{
		Check.NotNull(geographyCollectionWellKnownText, "geographyCollectionWellKnownText");
		return DbSpatialServices.Default.GeographyCollectionFromText(geographyCollectionWellKnownText, coordinateSystemId);
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

	public bool SpatialEquals(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.SpatialEquals(this, other);
	}

	public bool Disjoint(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Disjoint(this, other);
	}

	public bool Intersects(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Intersects(this, other);
	}

	public DbGeography Buffer(double? distance)
	{
		Check.NotNull(distance, "distance");
		return _spatialProvider.Buffer(this, distance.Value);
	}

	public double? Distance(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Distance(this, other);
	}

	public DbGeography Intersection(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Intersection(this, other);
	}

	public DbGeography Union(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Union(this, other);
	}

	public DbGeography Difference(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.Difference(this, other);
	}

	public DbGeography SymmetricDifference(DbGeography other)
	{
		Check.NotNull(other, "other");
		return _spatialProvider.SymmetricDifference(this, other);
	}

	public DbGeography ElementAt(int index)
	{
		return _spatialProvider.ElementAt(this, index);
	}

	public DbGeography PointAt(int index)
	{
		return _spatialProvider.PointAt(this, index);
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
