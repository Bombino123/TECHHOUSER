using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects;

[Obsolete("This class has been replaced by System.Data.Entity.DbFunctions.")]
public static class EntityFunctions
{
	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<decimal> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<decimal?> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<double> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<double?> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<int> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<int?> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<long> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDev")]
	public static double? StandardDeviation(IEnumerable<long?> collection)
	{
		return DbFunctions.StandardDeviation(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<decimal> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<decimal?> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<double> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<double?> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<int> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<int?> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<long> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "StDevP")]
	public static double? StandardDeviationP(IEnumerable<long?> collection)
	{
		return DbFunctions.StandardDeviationP(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<decimal> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<decimal?> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<double> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<double?> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<int> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<int?> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<long> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "Var")]
	public static double? Var(IEnumerable<long?> collection)
	{
		return DbFunctions.Var(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<decimal> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<decimal?> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<double> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<double?> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<int> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<int?> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<long> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "VarP")]
	public static double? VarP(IEnumerable<long?> collection)
	{
		return DbFunctions.VarP(collection);
	}

	[DbFunction("Edm", "Left")]
	public static string Left(string stringArgument, long? length)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "Right")]
	public static string Right(string stringArgument, long? length)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "Reverse")]
	public static string Reverse(string stringArgument)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "GetTotalOffsetMinutes")]
	public static int? GetTotalOffsetMinutes(DateTimeOffset? dateTimeOffsetArgument)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "TruncateTime")]
	public static DateTimeOffset? TruncateTime(DateTimeOffset? dateValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "TruncateTime")]
	public static DateTime? TruncateTime(DateTime? dateValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "CreateDateTime")]
	public static DateTime? CreateDateTime(int? year, int? month, int? day, int? hour, int? minute, double? second)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "CreateDateTimeOffset")]
	public static DateTimeOffset? CreateDateTimeOffset(int? year, int? month, int? day, int? hour, int? minute, double? second, int? timeZoneOffset)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "CreateTime")]
	public static TimeSpan? CreateTime(int? hour, int? minute, double? second)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddYears")]
	public static DateTimeOffset? AddYears(DateTimeOffset? dateValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddYears")]
	public static DateTime? AddYears(DateTime? dateValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMonths")]
	public static DateTimeOffset? AddMonths(DateTimeOffset? dateValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMonths")]
	public static DateTime? AddMonths(DateTime? dateValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddDays")]
	public static DateTimeOffset? AddDays(DateTimeOffset? dateValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddDays")]
	public static DateTime? AddDays(DateTime? dateValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddHours")]
	public static DateTimeOffset? AddHours(DateTimeOffset? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddHours")]
	public static DateTime? AddHours(DateTime? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddHours")]
	public static TimeSpan? AddHours(TimeSpan? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMinutes")]
	public static DateTimeOffset? AddMinutes(DateTimeOffset? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMinutes")]
	public static DateTime? AddMinutes(DateTime? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMinutes")]
	public static TimeSpan? AddMinutes(TimeSpan? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddSeconds")]
	public static DateTimeOffset? AddSeconds(DateTimeOffset? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddSeconds")]
	public static DateTime? AddSeconds(DateTime? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddSeconds")]
	public static TimeSpan? AddSeconds(TimeSpan? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMilliseconds")]
	public static DateTimeOffset? AddMilliseconds(DateTimeOffset? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMilliseconds")]
	public static DateTime? AddMilliseconds(DateTime? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMilliseconds")]
	public static TimeSpan? AddMilliseconds(TimeSpan? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMicroseconds")]
	public static DateTimeOffset? AddMicroseconds(DateTimeOffset? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMicroseconds")]
	public static DateTime? AddMicroseconds(DateTime? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddMicroseconds")]
	public static TimeSpan? AddMicroseconds(TimeSpan? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddNanoseconds")]
	public static DateTimeOffset? AddNanoseconds(DateTimeOffset? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddNanoseconds")]
	public static DateTime? AddNanoseconds(DateTime? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "AddNanoseconds")]
	public static TimeSpan? AddNanoseconds(TimeSpan? timeValue, int? addValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffYears")]
	public static int? DiffYears(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffYears")]
	public static int? DiffYears(DateTime? dateValue1, DateTime? dateValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMonths")]
	public static int? DiffMonths(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMonths")]
	public static int? DiffMonths(DateTime? dateValue1, DateTime? dateValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffDays")]
	public static int? DiffDays(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffDays")]
	public static int? DiffDays(DateTime? dateValue1, DateTime? dateValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffHours")]
	public static int? DiffHours(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffHours")]
	public static int? DiffHours(DateTime? timeValue1, DateTime? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffHours")]
	public static int? DiffHours(TimeSpan? timeValue1, TimeSpan? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMinutes")]
	public static int? DiffMinutes(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMinutes")]
	public static int? DiffMinutes(DateTime? timeValue1, DateTime? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMinutes")]
	public static int? DiffMinutes(TimeSpan? timeValue1, TimeSpan? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffSeconds")]
	public static int? DiffSeconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffSeconds")]
	public static int? DiffSeconds(DateTime? timeValue1, DateTime? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffSeconds")]
	public static int? DiffSeconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMilliseconds")]
	public static int? DiffMilliseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMilliseconds")]
	public static int? DiffMilliseconds(DateTime? timeValue1, DateTime? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMilliseconds")]
	public static int? DiffMilliseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMicroseconds")]
	public static int? DiffMicroseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMicroseconds")]
	public static int? DiffMicroseconds(DateTime? timeValue1, DateTime? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffMicroseconds")]
	public static int? DiffMicroseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffNanoseconds")]
	public static int? DiffNanoseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffNanoseconds")]
	public static int? DiffNanoseconds(DateTime? timeValue1, DateTime? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "DiffNanoseconds")]
	public static int? DiffNanoseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "Truncate")]
	public static double? Truncate(double? value, int? digits)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("Edm", "Truncate")]
	public static decimal? Truncate(decimal? value, int? digits)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	public static bool Like(string searchString, string likeExpression)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	public static bool Like(string searchString, string likeExpression, string escapeCharacter)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	public static string AsUnicode(string value)
	{
		return value;
	}

	public static string AsNonUnicode(string value)
	{
		return value;
	}
}
