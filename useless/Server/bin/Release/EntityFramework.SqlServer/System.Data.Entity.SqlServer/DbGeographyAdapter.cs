using System.Data.Entity.Core;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;

namespace System.Data.Entity.SqlServer;

internal class DbGeographyAdapter : IDbSpatialValue
{
	private readonly DbGeography _value;

	public bool IsGeography => true;

	public object ProviderValue => FuncExtensions.NullIfNotImplemented(() => _value.ProviderValue);

	public int? CoordinateSystemId => ((Func<int?>)(() => _value.CoordinateSystemId)).NullIfNotImplemented();

	public string WellKnownText => FuncExtensions.NullIfNotImplemented(() => _value.Provider.AsTextIncludingElevationAndMeasure(_value)) ?? FuncExtensions.NullIfNotImplemented(() => _value.AsText());

	public byte[] WellKnownBinary => FuncExtensions.NullIfNotImplemented(() => _value.AsBinary());

	public string GmlString => FuncExtensions.NullIfNotImplemented(() => _value.AsGml());

	internal DbGeographyAdapter(DbGeography value)
	{
		_value = value;
	}

	public Exception NotSqlCompatible()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return (Exception)new ProviderIncompatibleException(Strings.SqlProvider_GeographyValueNotSqlCompatible);
	}
}
