using System.Globalization;
using System.Threading;

namespace System.Data.Entity.Migrations.Utilities;

internal static class UtcNowGenerator
{
	public const string MigrationIdFormat = "yyyyMMddHHmmssf";

	private static readonly ThreadLocal<DateTime> _lastNow = new ThreadLocal<DateTime>(() => DateTime.UtcNow);

	public static DateTime UtcNow()
	{
		DateTime dateTime = DateTime.UtcNow;
		DateTime value = _lastNow.Value;
		if (dateTime <= value || dateTime.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture).Equals(value.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture), StringComparison.Ordinal))
		{
			dateTime = value.AddMilliseconds(100.0);
		}
		_lastNow.Value = dateTime;
		return dateTime;
	}

	public static string UtcNowAsMigrationIdTimestamp()
	{
		return UtcNow().ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture);
	}
}
