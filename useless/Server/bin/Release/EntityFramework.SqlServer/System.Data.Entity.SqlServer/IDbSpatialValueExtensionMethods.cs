using System.Data.Entity.Spatial;

namespace System.Data.Entity.SqlServer;

internal static class IDbSpatialValueExtensionMethods
{
	internal static IDbSpatialValue AsSpatialValue(this DbGeography geographyValue)
	{
		return new DbGeographyAdapter(geographyValue);
	}

	internal static IDbSpatialValue AsSpatialValue(this DbGeometry geometryValue)
	{
		return new DbGeometryAdapter(geometryValue);
	}
}
