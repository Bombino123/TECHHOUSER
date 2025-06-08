using System.Collections.Generic;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

public static class EdmFunctions
{
	private static EdmFunction ResolveCanonicalFunction(string functionName, TypeUsage[] argumentTypes)
	{
		List<EdmFunction> list = new List<EdmFunction>(from func in EdmProviderManifest.Instance.GetStoreFunctions()
			where string.Equals(func.Name, functionName, StringComparison.Ordinal)
			select func);
		EdmFunction edmFunction = null;
		bool isAmbiguous = false;
		if (list.Count > 0)
		{
			edmFunction = FunctionOverloadResolver.ResolveFunctionOverloads(list, argumentTypes, isGroupAggregateFunction: false, out isAmbiguous);
			if (isAmbiguous)
			{
				throw new ArgumentException(Strings.Cqt_Function_CanonicalFunction_AmbiguousMatch(functionName));
			}
		}
		if (edmFunction == null)
		{
			throw new ArgumentException(Strings.Cqt_Function_CanonicalFunction_NotFound(functionName));
		}
		return edmFunction;
	}

	internal static DbFunctionExpression InvokeCanonicalFunction(string functionName, params DbExpression[] arguments)
	{
		TypeUsage[] array = new TypeUsage[arguments.Length];
		for (int i = 0; i < arguments.Length; i++)
		{
			array[i] = arguments[i].ResultType;
		}
		return ResolveCanonicalFunction(functionName, array).Invoke(arguments);
	}

	public static DbFunctionExpression Average(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("Avg", collection);
	}

	public static DbFunctionExpression Count(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("Count", collection);
	}

	public static DbFunctionExpression LongCount(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("BigCount", collection);
	}

	public static DbFunctionExpression Max(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("Max", collection);
	}

	public static DbFunctionExpression Min(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("Min", collection);
	}

	public static DbFunctionExpression Sum(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("Sum", collection);
	}

	public static DbFunctionExpression StDev(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("StDev", collection);
	}

	public static DbFunctionExpression StDevP(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("StDevP", collection);
	}

	public static DbFunctionExpression Var(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("Var", collection);
	}

	public static DbFunctionExpression VarP(this DbExpression collection)
	{
		Check.NotNull(collection, "collection");
		return InvokeCanonicalFunction("VarP", collection);
	}

	public static DbFunctionExpression Concat(this DbExpression string1, DbExpression string2)
	{
		Check.NotNull(string1, "string1");
		Check.NotNull(string2, "string2");
		return InvokeCanonicalFunction("Concat", string1, string2);
	}

	public static DbExpression Contains(this DbExpression searchedString, DbExpression searchedForString)
	{
		Check.NotNull(searchedString, "searchedString");
		Check.NotNull(searchedForString, "searchedForString");
		return InvokeCanonicalFunction("Contains", searchedString, searchedForString);
	}

	public static DbFunctionExpression EndsWith(this DbExpression stringArgument, DbExpression suffix)
	{
		Check.NotNull(stringArgument, "stringArgument");
		Check.NotNull(suffix, "suffix");
		return InvokeCanonicalFunction("EndsWith", stringArgument, suffix);
	}

	public static DbFunctionExpression IndexOf(this DbExpression searchString, DbExpression stringToFind)
	{
		Check.NotNull(searchString, "searchString");
		Check.NotNull(stringToFind, "stringToFind");
		return InvokeCanonicalFunction("IndexOf", stringToFind, searchString);
	}

	public static DbFunctionExpression Left(this DbExpression stringArgument, DbExpression length)
	{
		Check.NotNull(stringArgument, "stringArgument");
		Check.NotNull(length, "length");
		return InvokeCanonicalFunction("Left", stringArgument, length);
	}

	public static DbFunctionExpression Length(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("Length", stringArgument);
	}

	public static DbFunctionExpression Replace(this DbExpression stringArgument, DbExpression toReplace, DbExpression replacement)
	{
		Check.NotNull(stringArgument, "stringArgument");
		Check.NotNull(toReplace, "toReplace");
		Check.NotNull(replacement, "replacement");
		return InvokeCanonicalFunction("Replace", stringArgument, toReplace, replacement);
	}

	public static DbFunctionExpression Reverse(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("Reverse", stringArgument);
	}

	public static DbFunctionExpression Right(this DbExpression stringArgument, DbExpression length)
	{
		Check.NotNull(stringArgument, "stringArgument");
		Check.NotNull(length, "length");
		return InvokeCanonicalFunction("Right", stringArgument, length);
	}

	public static DbFunctionExpression StartsWith(this DbExpression stringArgument, DbExpression prefix)
	{
		Check.NotNull(stringArgument, "stringArgument");
		Check.NotNull(prefix, "prefix");
		return InvokeCanonicalFunction("StartsWith", stringArgument, prefix);
	}

	public static DbFunctionExpression Substring(this DbExpression stringArgument, DbExpression start, DbExpression length)
	{
		Check.NotNull(stringArgument, "stringArgument");
		Check.NotNull(start, "start");
		Check.NotNull(length, "length");
		return InvokeCanonicalFunction("Substring", stringArgument, start, length);
	}

	public static DbFunctionExpression ToLower(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("ToLower", stringArgument);
	}

	public static DbFunctionExpression ToUpper(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("ToUpper", stringArgument);
	}

	public static DbFunctionExpression Trim(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("Trim", stringArgument);
	}

	public static DbFunctionExpression TrimEnd(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("RTrim", stringArgument);
	}

	public static DbFunctionExpression TrimStart(this DbExpression stringArgument)
	{
		Check.NotNull(stringArgument, "stringArgument");
		return InvokeCanonicalFunction("LTrim", stringArgument);
	}

	public static DbFunctionExpression Year(this DbExpression dateValue)
	{
		Check.NotNull(dateValue, "dateValue");
		return InvokeCanonicalFunction("Year", dateValue);
	}

	public static DbFunctionExpression Month(this DbExpression dateValue)
	{
		Check.NotNull(dateValue, "dateValue");
		return InvokeCanonicalFunction("Month", dateValue);
	}

	public static DbFunctionExpression Day(this DbExpression dateValue)
	{
		Check.NotNull(dateValue, "dateValue");
		return InvokeCanonicalFunction("Day", dateValue);
	}

	public static DbFunctionExpression DayOfYear(this DbExpression dateValue)
	{
		Check.NotNull(dateValue, "dateValue");
		return InvokeCanonicalFunction("DayOfYear", dateValue);
	}

	public static DbFunctionExpression Hour(this DbExpression timeValue)
	{
		Check.NotNull(timeValue, "timeValue");
		return InvokeCanonicalFunction("Hour", timeValue);
	}

	public static DbFunctionExpression Minute(this DbExpression timeValue)
	{
		Check.NotNull(timeValue, "timeValue");
		return InvokeCanonicalFunction("Minute", timeValue);
	}

	public static DbFunctionExpression Second(this DbExpression timeValue)
	{
		Check.NotNull(timeValue, "timeValue");
		return InvokeCanonicalFunction("Second", timeValue);
	}

	public static DbFunctionExpression Millisecond(this DbExpression timeValue)
	{
		Check.NotNull(timeValue, "timeValue");
		return InvokeCanonicalFunction("Millisecond", timeValue);
	}

	public static DbFunctionExpression GetTotalOffsetMinutes(this DbExpression dateTimeOffsetArgument)
	{
		Check.NotNull(dateTimeOffsetArgument, "dateTimeOffsetArgument");
		return InvokeCanonicalFunction("GetTotalOffsetMinutes", dateTimeOffsetArgument);
	}

	public static DbFunctionExpression LocalDateTime(this DbExpression dateTimeOffsetArgument)
	{
		Check.NotNull(dateTimeOffsetArgument, "dateTimeOffsetArgument");
		return InvokeCanonicalFunction("LocalDateTime", dateTimeOffsetArgument);
	}

	public static DbFunctionExpression UtcDateTime(this DbExpression dateTimeOffsetArgument)
	{
		Check.NotNull(dateTimeOffsetArgument, "dateTimeOffsetArgument");
		return InvokeCanonicalFunction("UtcDateTime", dateTimeOffsetArgument);
	}

	public static DbFunctionExpression CurrentDateTime()
	{
		return InvokeCanonicalFunction("CurrentDateTime");
	}

	public static DbFunctionExpression CurrentDateTimeOffset()
	{
		return InvokeCanonicalFunction("CurrentDateTimeOffset");
	}

	public static DbFunctionExpression CurrentUtcDateTime()
	{
		return InvokeCanonicalFunction("CurrentUtcDateTime");
	}

	public static DbFunctionExpression TruncateTime(this DbExpression dateValue)
	{
		Check.NotNull(dateValue, "dateValue");
		return InvokeCanonicalFunction("TruncateTime", dateValue);
	}

	public static DbFunctionExpression CreateDateTime(DbExpression year, DbExpression month, DbExpression day, DbExpression hour, DbExpression minute, DbExpression second)
	{
		Check.NotNull(year, "year");
		Check.NotNull(month, "month");
		Check.NotNull(day, "day");
		Check.NotNull(hour, "hour");
		Check.NotNull(minute, "minute");
		Check.NotNull(second, "second");
		return InvokeCanonicalFunction("CreateDateTime", year, month, day, hour, minute, second);
	}

	public static DbFunctionExpression CreateDateTimeOffset(DbExpression year, DbExpression month, DbExpression day, DbExpression hour, DbExpression minute, DbExpression second, DbExpression timeZoneOffset)
	{
		Check.NotNull(year, "year");
		Check.NotNull(month, "month");
		Check.NotNull(day, "day");
		Check.NotNull(hour, "hour");
		Check.NotNull(minute, "minute");
		Check.NotNull(second, "second");
		Check.NotNull(timeZoneOffset, "timeZoneOffset");
		return InvokeCanonicalFunction("CreateDateTimeOffset", year, month, day, hour, minute, second, timeZoneOffset);
	}

	public static DbFunctionExpression CreateTime(DbExpression hour, DbExpression minute, DbExpression second)
	{
		Check.NotNull(hour, "hour");
		Check.NotNull(minute, "minute");
		Check.NotNull(second, "second");
		return InvokeCanonicalFunction("CreateTime", hour, minute, second);
	}

	public static DbFunctionExpression AddYears(this DbExpression dateValue, DbExpression addValue)
	{
		Check.NotNull(dateValue, "dateValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddYears", dateValue, addValue);
	}

	public static DbFunctionExpression AddMonths(this DbExpression dateValue, DbExpression addValue)
	{
		Check.NotNull(dateValue, "dateValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddMonths", dateValue, addValue);
	}

	public static DbFunctionExpression AddDays(this DbExpression dateValue, DbExpression addValue)
	{
		Check.NotNull(dateValue, "dateValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddDays", dateValue, addValue);
	}

	public static DbFunctionExpression AddHours(this DbExpression timeValue, DbExpression addValue)
	{
		Check.NotNull(timeValue, "timeValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddHours", timeValue, addValue);
	}

	public static DbFunctionExpression AddMinutes(this DbExpression timeValue, DbExpression addValue)
	{
		Check.NotNull(timeValue, "timeValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddMinutes", timeValue, addValue);
	}

	public static DbFunctionExpression AddSeconds(this DbExpression timeValue, DbExpression addValue)
	{
		Check.NotNull(timeValue, "timeValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddSeconds", timeValue, addValue);
	}

	public static DbFunctionExpression AddMilliseconds(this DbExpression timeValue, DbExpression addValue)
	{
		Check.NotNull(timeValue, "timeValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddMilliseconds", timeValue, addValue);
	}

	public static DbFunctionExpression AddMicroseconds(this DbExpression timeValue, DbExpression addValue)
	{
		Check.NotNull(timeValue, "timeValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddMicroseconds", timeValue, addValue);
	}

	public static DbFunctionExpression AddNanoseconds(this DbExpression timeValue, DbExpression addValue)
	{
		Check.NotNull(timeValue, "timeValue");
		Check.NotNull(addValue, "addValue");
		return InvokeCanonicalFunction("AddNanoseconds", timeValue, addValue);
	}

	public static DbFunctionExpression DiffYears(this DbExpression dateValue1, DbExpression dateValue2)
	{
		Check.NotNull(dateValue1, "dateValue1");
		Check.NotNull(dateValue2, "dateValue2");
		return InvokeCanonicalFunction("DiffYears", dateValue1, dateValue2);
	}

	public static DbFunctionExpression DiffMonths(this DbExpression dateValue1, DbExpression dateValue2)
	{
		Check.NotNull(dateValue1, "dateValue1");
		Check.NotNull(dateValue2, "dateValue2");
		return InvokeCanonicalFunction("DiffMonths", dateValue1, dateValue2);
	}

	public static DbFunctionExpression DiffDays(this DbExpression dateValue1, DbExpression dateValue2)
	{
		Check.NotNull(dateValue1, "dateValue1");
		Check.NotNull(dateValue2, "dateValue2");
		return InvokeCanonicalFunction("DiffDays", dateValue1, dateValue2);
	}

	public static DbFunctionExpression DiffHours(this DbExpression timeValue1, DbExpression timeValue2)
	{
		Check.NotNull(timeValue1, "timeValue1");
		Check.NotNull(timeValue2, "timeValue2");
		return InvokeCanonicalFunction("DiffHours", timeValue1, timeValue2);
	}

	public static DbFunctionExpression DiffMinutes(this DbExpression timeValue1, DbExpression timeValue2)
	{
		Check.NotNull(timeValue1, "timeValue1");
		Check.NotNull(timeValue2, "timeValue2");
		return InvokeCanonicalFunction("DiffMinutes", timeValue1, timeValue2);
	}

	public static DbFunctionExpression DiffSeconds(this DbExpression timeValue1, DbExpression timeValue2)
	{
		Check.NotNull(timeValue1, "timeValue1");
		Check.NotNull(timeValue2, "timeValue2");
		return InvokeCanonicalFunction("DiffSeconds", timeValue1, timeValue2);
	}

	public static DbFunctionExpression DiffMilliseconds(this DbExpression timeValue1, DbExpression timeValue2)
	{
		Check.NotNull(timeValue1, "timeValue1");
		Check.NotNull(timeValue2, "timeValue2");
		return InvokeCanonicalFunction("DiffMilliseconds", timeValue1, timeValue2);
	}

	public static DbFunctionExpression DiffMicroseconds(this DbExpression timeValue1, DbExpression timeValue2)
	{
		Check.NotNull(timeValue1, "timeValue1");
		Check.NotNull(timeValue2, "timeValue2");
		return InvokeCanonicalFunction("DiffMicroseconds", timeValue1, timeValue2);
	}

	public static DbFunctionExpression DiffNanoseconds(this DbExpression timeValue1, DbExpression timeValue2)
	{
		Check.NotNull(timeValue1, "timeValue1");
		Check.NotNull(timeValue2, "timeValue2");
		return InvokeCanonicalFunction("DiffNanoseconds", timeValue1, timeValue2);
	}

	public static DbFunctionExpression Round(this DbExpression value)
	{
		Check.NotNull(value, "value");
		return InvokeCanonicalFunction("Round", value);
	}

	public static DbFunctionExpression Round(this DbExpression value, DbExpression digits)
	{
		Check.NotNull(value, "value");
		Check.NotNull(digits, "digits");
		return InvokeCanonicalFunction("Round", value, digits);
	}

	public static DbFunctionExpression Floor(this DbExpression value)
	{
		Check.NotNull(value, "value");
		return InvokeCanonicalFunction("Floor", value);
	}

	public static DbFunctionExpression Ceiling(this DbExpression value)
	{
		Check.NotNull(value, "value");
		return InvokeCanonicalFunction("Ceiling", value);
	}

	public static DbFunctionExpression Abs(this DbExpression value)
	{
		Check.NotNull(value, "value");
		return InvokeCanonicalFunction("Abs", value);
	}

	public static DbFunctionExpression Truncate(this DbExpression value, DbExpression digits)
	{
		Check.NotNull(value, "value");
		Check.NotNull(digits, "digits");
		return InvokeCanonicalFunction("Truncate", value, digits);
	}

	public static DbFunctionExpression Power(this DbExpression baseArgument, DbExpression exponent)
	{
		Check.NotNull(baseArgument, "baseArgument");
		Check.NotNull(exponent, "exponent");
		return InvokeCanonicalFunction("Power", baseArgument, exponent);
	}

	public static DbFunctionExpression BitwiseAnd(this DbExpression value1, DbExpression value2)
	{
		Check.NotNull(value1, "value1");
		Check.NotNull(value2, "value2");
		return InvokeCanonicalFunction("BitwiseAnd", value1, value2);
	}

	public static DbFunctionExpression BitwiseOr(this DbExpression value1, DbExpression value2)
	{
		Check.NotNull(value1, "value1");
		Check.NotNull(value2, "value2");
		return InvokeCanonicalFunction("BitwiseOr", value1, value2);
	}

	public static DbFunctionExpression BitwiseNot(this DbExpression value)
	{
		Check.NotNull(value, "value");
		return InvokeCanonicalFunction("BitwiseNot", value);
	}

	public static DbFunctionExpression BitwiseXor(this DbExpression value1, DbExpression value2)
	{
		Check.NotNull(value1, "value1");
		Check.NotNull(value2, "value2");
		return InvokeCanonicalFunction("BitwiseXor", value1, value2);
	}

	public static DbFunctionExpression NewGuid()
	{
		return InvokeCanonicalFunction("NewGuid");
	}
}
