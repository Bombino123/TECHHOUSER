namespace System.Data.Entity.SqlServer;

internal interface IDbSpatialValue
{
	bool IsGeography { get; }

	object ProviderValue { get; }

	int? CoordinateSystemId { get; }

	string WellKnownText { get; }

	byte[] WellKnownBinary { get; }

	string GmlString { get; }

	Exception NotSqlCompatible();
}
