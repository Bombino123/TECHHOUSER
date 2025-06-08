using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;

namespace System.Data.Entity.SqlServer;

public static class SqlSpatialFunctions
{
	[DbFunction("SqlServer", "POINTGEOGRAPHY")]
	public static DbGeography PointGeography(double? latitude, double? longitude, int? spatialReferenceId)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ASTEXTZM")]
	public static string AsTextZM(DbGeography geographyValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "BUFFERWITHTOLERANCE")]
	public static DbGeography BufferWithTolerance(DbGeography geographyValue, double? distance, double? tolerance, bool? relative)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ENVELOPEANGLE")]
	public static double? EnvelopeAngle(DbGeography geographyValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ENVELOPECENTER")]
	public static DbGeography EnvelopeCenter(DbGeography geographyValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "FILTER")]
	public static bool? Filter(DbGeography geographyValue, DbGeography geographyOther)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "INSTANCEOF")]
	public static bool? InstanceOf(DbGeography geographyValue, string geometryTypeName)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "NUMRINGS")]
	public static int? NumRings(DbGeography geographyValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "REDUCE")]
	public static DbGeography Reduce(DbGeography geographyValue, double? tolerance)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RINGN")]
	public static DbGeography RingN(DbGeography geographyValue, int? index)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "POINTGEOMETRY")]
	public static DbGeometry PointGeometry(double? xCoordinate, double? yCoordinate, int? spatialReferenceId)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ASTEXTZM")]
	public static string AsTextZM(DbGeometry geometryValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "BUFFERWITHTOLERANCE")]
	public static DbGeometry BufferWithTolerance(DbGeometry geometryValue, double? distance, double? tolerance, bool? relative)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "INSTANCEOF")]
	public static bool? InstanceOf(DbGeometry geometryValue, string geometryTypeName)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "FILTER")]
	public static bool? Filter(DbGeometry geometryValue, DbGeometry geometryOther)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "MAKEVALID")]
	public static DbGeometry MakeValid(DbGeometry geometryValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "REDUCE")]
	public static DbGeometry Reduce(DbGeometry geometryValue, double? tolerance)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}
}
