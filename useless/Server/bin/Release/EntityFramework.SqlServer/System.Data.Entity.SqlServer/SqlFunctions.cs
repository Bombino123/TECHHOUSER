using System.Collections.Generic;
using System.Data.Entity.SqlServer.Resources;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.SqlServer;

public static class SqlFunctions
{
	[DbFunction("SqlServer", "CHECKSUM_AGG")]
	public static int? ChecksumAggregate(IEnumerable<int> arg)
	{
		return BootstrapFunction((IEnumerable<int> a) => ChecksumAggregate(a), arg);
	}

	[DbFunction("SqlServer", "CHECKSUM_AGG")]
	public static int? ChecksumAggregate(IEnumerable<int?> arg)
	{
		return BootstrapFunction((IEnumerable<int?> a) => ChecksumAggregate(a), arg);
	}

	private static TOut BootstrapFunction<TIn, TOut>(Expression<Func<IEnumerable<TIn>, TOut>> methodExpression, IEnumerable<TIn> arg)
	{
		if (arg is IQueryable queryable)
		{
			return queryable.Provider.Execute<TOut>(Expression.Call(((MethodCallExpression)methodExpression.Body).Method, Expression.Constant(arg)));
		}
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ASCII")]
	public static int? Ascii(string arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHAR")]
	public static string Char(int? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHARINDEX")]
	public static int? CharIndex(string toFind, string toSearch)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHARINDEX")]
	public static int? CharIndex(byte[] toFind, byte[] toSearch)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHARINDEX")]
	public static int? CharIndex(string toFind, string toSearch, int? startLocation)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHARINDEX")]
	public static int? CharIndex(byte[] toFind, byte[] toSearch, int? startLocation)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHARINDEX")]
	public static long? CharIndex(string toFind, string toSearch, long? startLocation)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHARINDEX")]
	public static long? CharIndex(byte[] toFind, byte[] toSearch, long? startLocation)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DIFFERENCE")]
	public static int? Difference(string string1, string string2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "NCHAR")]
	public static string NChar(int? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "PATINDEX")]
	public static int? PatIndex(string stringPattern, string target)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "QUOTENAME")]
	public static string QuoteName(string stringArg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "QUOTENAME")]
	public static string QuoteName(string stringArg, string quoteCharacter)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "REPLICATE")]
	public static string Replicate(string target, int? count)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SOUNDEX")]
	public static string SoundCode(string arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SPACE")]
	public static string Space(int? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STR")]
	public static string StringConvert(double? number)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STR")]
	public static string StringConvert(decimal? number)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STR")]
	public static string StringConvert(double? number, int? length)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STR")]
	public static string StringConvert(decimal? number, int? length)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STR")]
	public static string StringConvert(double? number, int? length, int? decimalArg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STR")]
	public static string StringConvert(decimal? number, int? length, int? decimalArg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "STUFF")]
	public static string Stuff(string stringInput, int? start, int? length, string stringReplacement)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "UNICODE")]
	public static int? Unicode(string arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ACOS")]
	public static double? Acos(double? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ACOS")]
	public static double? Acos(decimal? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ASIN")]
	public static double? Asin(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ASIN")]
	public static double? Asin(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ATAN")]
	public static double? Atan(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ATAN")]
	public static double? Atan(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ATN2")]
	public static double? Atan2(double? arg1, double? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ATN2")]
	public static double? Atan2(decimal? arg1, decimal? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "COS")]
	public static double? Cos(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "COS")]
	public static double? Cos(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "COT")]
	public static double? Cot(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "COT")]
	public static double? Cot(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DEGREES")]
	public static int? Degrees(int? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DEGREES")]
	public static long? Degrees(long? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DEGREES")]
	public static decimal? Degrees(decimal? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DEGREES")]
	public static double? Degrees(double? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "EXP")]
	public static double? Exp(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "EXP")]
	public static double? Exp(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "LOG")]
	public static double? Log(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "LOG")]
	public static double? Log(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "LOG10")]
	public static double? Log10(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "LOG10")]
	public static double? Log10(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "PI")]
	public static double? Pi()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RADIANS")]
	public static int? Radians(int? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RADIANS")]
	public static long? Radians(long? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RADIANS")]
	public static decimal? Radians(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RADIANS")]
	public static double? Radians(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RAND")]
	public static double? Rand()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "RAND")]
	public static double? Rand(int? seed)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SIGN")]
	public static int? Sign(int? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SIGN")]
	public static long? Sign(long? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SIGN")]
	public static decimal? Sign(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SIGN")]
	public static double? Sign(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SIN")]
	public static double? Sin(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SIN")]
	public static double? Sin(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SQRT")]
	public static double? SquareRoot(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SQRT")]
	public static double? SquareRoot(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SQUARE")]
	public static double? Square(double? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "SQUARE")]
	public static double? Square(decimal? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "TAN")]
	public static double? Tan(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "TAN")]
	public static double? Tan(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEADD")]
	public static DateTime? DateAdd(string datePartArg, double? number, DateTime? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEADD")]
	public static TimeSpan? DateAdd(string datePartArg, double? number, TimeSpan? time)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEADD")]
	public static DateTimeOffset? DateAdd(string datePartArg, double? number, DateTimeOffset? dateTimeOffsetArg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEADD")]
	public static DateTime? DateAdd(string datePartArg, double? number, string date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTime? startDate, DateTime? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTimeOffset? startDate, DateTimeOffset? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, TimeSpan? startDate, TimeSpan? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, string startDate, DateTime? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, string startDate, DateTimeOffset? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, string startDate, TimeSpan? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, TimeSpan? startDate, string endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTime? startDate, string endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTimeOffset? startDate, string endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, string startDate, string endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, TimeSpan? startDate, DateTime? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, TimeSpan? startDate, DateTimeOffset? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTime? startDate, TimeSpan? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTimeOffset? startDate, TimeSpan? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTime? startDate, DateTimeOffset? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEDIFF")]
	public static int? DateDiff(string datePartArg, DateTimeOffset? startDate, DateTime? endDate)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATENAME")]
	public static string DateName(string datePartArg, DateTime? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATENAME")]
	public static string DateName(string datePartArg, string date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATENAME")]
	public static string DateName(string datePartArg, TimeSpan? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATENAME")]
	public static string DateName(string datePartArg, DateTimeOffset? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEPART")]
	public static int? DatePart(string datePartArg, DateTime? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEPART")]
	public static int? DatePart(string datePartArg, DateTimeOffset? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEPART")]
	public static int? DatePart(string datePartArg, string date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATEPART")]
	public static int? DatePart(string datePartArg, TimeSpan? date)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "GETDATE")]
	public static DateTime? GetDate()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "GETUTCDATE")]
	public static DateTime? GetUtcDate()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(bool? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(double? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(decimal? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(DateTime? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(TimeSpan? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(DateTimeOffset? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(string arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(byte[] arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "DATALENGTH")]
	public static int? DataLength(Guid? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(bool? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(double? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(decimal? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(string arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(DateTime? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(TimeSpan? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(DateTimeOffset? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(byte[] arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(Guid? arg1)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(bool? arg1, bool? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(double? arg1, double? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(decimal? arg1, decimal? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(string arg1, string arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(DateTime? arg1, DateTime? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(TimeSpan? arg1, TimeSpan? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(DateTimeOffset? arg1, DateTimeOffset? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(byte[] arg1, byte[] arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(Guid? arg1, Guid? arg2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(bool? arg1, bool? arg2, bool? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(double? arg1, double? arg2, double? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(decimal? arg1, decimal? arg2, decimal? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(string arg1, string arg2, string arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(DateTime? arg1, DateTime? arg2, DateTime? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(DateTimeOffset? arg1, DateTimeOffset? arg2, DateTimeOffset? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(TimeSpan? arg1, TimeSpan? arg2, TimeSpan? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(byte[] arg1, byte[] arg2, byte[] arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CHECKSUM")]
	public static int? Checksum(Guid? arg1, Guid? arg2, Guid? arg3)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CURRENT_TIMESTAMP")]
	public static DateTime? CurrentTimestamp()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "CURRENT_USER")]
	public static string CurrentUser()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "HOST_NAME")]
	public static string HostName()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "USER_NAME")]
	public static string UserName(int? arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "USER_NAME")]
	public static string UserName()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ISNUMERIC")]
	public static int? IsNumeric(string arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "ISDATE")]
	public static int? IsDate(string arg)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}
}
