#define TRACE
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SQLite;

public abstract class SQLiteConvert
{
	internal const char EscapeChar = '\\';

	internal const char QuoteChar = '"';

	internal const char AltQuoteChar = '\'';

	internal const char ValueChar = '=';

	internal const char PairChar = ';';

	internal static readonly char[] SpecialChars = new char[5] { '"', '\'', ';', '=', '\\' };

	private const DbType FallbackDefaultDbType = DbType.Object;

	private static readonly string FallbackDefaultTypeName = string.Empty;

	protected static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private static readonly double OleAutomationEpochAsJulianDay = 2415018.5;

	private const string FullFormat = "yyyy-MM-ddTHH:mm:ss.fffffffK";

	private static readonly long MinimumJd = computeJD(DateTime.MinValue);

	private static readonly long MaximumJd = computeJD(DateTime.MaxValue);

	private static string[] _datetimeFormats = new string[31]
	{
		"THHmmssK", "THHmmK", "HH:mm:ss.FFFFFFFK", "HH:mm:ssK", "HH:mmK", "yyyy-MM-dd HH:mm:ss.FFFFFFFK", "yyyy-MM-dd HH:mm:ssK", "yyyy-MM-dd HH:mmK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", "yyyy-MM-ddTHH:mmK",
		"yyyy-MM-ddTHH:mm:ssK", "yyyyMMddHHmmssK", "yyyyMMddHHmmK", "yyyyMMddTHHmmssFFFFFFFK", "THHmmss", "THHmm", "HH:mm:ss.FFFFFFF", "HH:mm:ss", "HH:mm", "yyyy-MM-dd HH:mm:ss.FFFFFFF",
		"yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyyMMddHHmmss", "yyyyMMddHHmm", "yyyyMMddTHHmmssFFFFFFF", "yyyy-MM-dd", "yyyyMMdd",
		"yy-MM-dd"
	};

	private static readonly string _datetimeFormatUtc = _datetimeFormats[5];

	private static readonly string _datetimeFormatLocal = _datetimeFormats[19];

	private static Encoding _utf8 = new UTF8Encoding();

	internal SQLiteDateFormats _datetimeFormat;

	internal DateTimeKind _datetimeKind;

	internal string _datetimeFormatString;

	private static Type[] _affinitytotype = new Type[12]
	{
		typeof(object),
		typeof(long),
		typeof(double),
		typeof(string),
		typeof(byte[]),
		typeof(object),
		null,
		null,
		null,
		null,
		typeof(DateTime),
		typeof(object)
	};

	private static DbType[] _typetodbtype = new DbType[19]
	{
		DbType.Object,
		DbType.Binary,
		DbType.Object,
		DbType.Boolean,
		DbType.SByte,
		DbType.SByte,
		DbType.Byte,
		DbType.Int16,
		DbType.UInt16,
		DbType.Int32,
		DbType.UInt32,
		DbType.Int64,
		DbType.UInt64,
		DbType.Single,
		DbType.Double,
		DbType.Decimal,
		DbType.DateTime,
		DbType.Object,
		DbType.String
	};

	private static int[] _dbtypetocolumnsize = new int[28]
	{
		2147483647, 2147483647, 1, 1, 8, 8, 8, 8, 8, 16,
		2, 4, 8, 2147483647, 1, 4, 2147483647, 8, 2, 4,
		8, 8, 2147483647, 2147483647, 2147483647, 2147483647, 8, 10
	};

	private static object[] _dbtypetonumericprecision = new object[28]
	{
		DBNull.Value,
		DBNull.Value,
		3,
		DBNull.Value,
		19,
		DBNull.Value,
		DBNull.Value,
		53,
		53,
		DBNull.Value,
		5,
		10,
		19,
		DBNull.Value,
		3,
		24,
		DBNull.Value,
		DBNull.Value,
		5,
		10,
		19,
		53,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value
	};

	private static object[] _dbtypetonumericscale = new object[28]
	{
		DBNull.Value,
		DBNull.Value,
		0,
		DBNull.Value,
		4,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		0,
		0,
		0,
		DBNull.Value,
		0,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		0,
		0,
		0,
		0,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value,
		DBNull.Value
	};

	private static Type[] _dbtypeToType = new Type[28]
	{
		typeof(string),
		typeof(byte[]),
		typeof(byte),
		typeof(bool),
		typeof(decimal),
		typeof(DateTime),
		typeof(DateTime),
		typeof(decimal),
		typeof(double),
		typeof(Guid),
		typeof(short),
		typeof(int),
		typeof(long),
		typeof(object),
		typeof(sbyte),
		typeof(float),
		typeof(string),
		typeof(DateTime),
		typeof(ushort),
		typeof(uint),
		typeof(ulong),
		typeof(double),
		typeof(string),
		typeof(string),
		typeof(string),
		typeof(string),
		typeof(DateTime),
		typeof(DateTimeOffset)
	};

	private static TypeAffinity[] _typecodeAffinities = new TypeAffinity[19]
	{
		TypeAffinity.Null,
		TypeAffinity.Blob,
		TypeAffinity.Null,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Int64,
		TypeAffinity.Double,
		TypeAffinity.Double,
		TypeAffinity.Double,
		TypeAffinity.DateTime,
		TypeAffinity.Null,
		TypeAffinity.Text
	};

	private static readonly SQLiteDbTypeMap _typeNames = GetSQLiteDbTypeMap();

	internal SQLiteConvert(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString)
	{
		_datetimeFormat = fmt;
		_datetimeKind = kind;
		_datetimeFormatString = fmtString;
	}

	public static byte[] ToUTF8(string sourceText)
	{
		if (sourceText == null)
		{
			return null;
		}
		int num = _utf8.GetByteCount(sourceText) + 1;
		byte[] array = new byte[num];
		num = _utf8.GetBytes(sourceText, 0, sourceText.Length, array, 0);
		array[num] = 0;
		return array;
	}

	public byte[] ToUTF8(DateTime dateTimeValue)
	{
		return ToUTF8(ToString(dateTimeValue));
	}

	public virtual string ToString(IntPtr nativestring, int nativestringlen)
	{
		return UTF8ToString(nativestring, nativestringlen);
	}

	public static string UTF8ToString(IntPtr nativestring, int nativestringlen)
	{
		if (nativestring == IntPtr.Zero || nativestringlen == 0)
		{
			return string.Empty;
		}
		if (nativestringlen < 0)
		{
			nativestringlen = 0;
			while (Marshal.ReadByte(nativestring, nativestringlen) != 0)
			{
				nativestringlen++;
			}
			if (nativestringlen == 0)
			{
				return string.Empty;
			}
		}
		byte[] array = new byte[nativestringlen];
		Marshal.Copy(nativestring, array, 0, nativestringlen);
		return _utf8.GetString(array, 0, nativestringlen);
	}

	private static bool isValidJd(long jd)
	{
		if (jd >= MinimumJd)
		{
			return jd <= MaximumJd;
		}
		return false;
	}

	private static long DoubleToJd(double julianDay)
	{
		return (long)Math.Round(julianDay * 86400000.0);
	}

	private static double JdToDouble(long jd)
	{
		return (double)jd / 86400000.0;
	}

	private static DateTime computeYMD(long jd, DateTime? badValue)
	{
		if (!isValidJd(jd))
		{
			if (!badValue.HasValue)
			{
				throw new ArgumentException("Not a supported Julian Day value.");
			}
			return badValue.Value;
		}
		int num = (int)((jd + 43200000) / 86400000);
		int num2 = (int)(((double)num - 1867216.25) / 36524.25);
		num2 = num + 1 + num2 - num2 / 4;
		int num3 = num2 + 1524;
		int num4 = (int)(((double)num3 - 122.1) / 365.25);
		int num5 = 36525 * num4 / 100;
		int num6 = (int)((double)(num3 - num5) / 30.6001);
		int num7 = (int)(30.6001 * (double)num6);
		int day = num3 - num5 - num7;
		int num8 = ((num6 < 14) ? (num6 - 1) : (num6 - 13));
		int year = ((num8 > 2) ? (num4 - 4716) : (num4 - 4715));
		try
		{
			return new DateTime(year, num8, day);
		}
		catch
		{
			if (!badValue.HasValue)
			{
				throw;
			}
			return badValue.Value;
		}
	}

	private static DateTime computeHMS(long jd, DateTime? badValue)
	{
		if (!isValidJd(jd))
		{
			if (!badValue.HasValue)
			{
				throw new ArgumentException("Not a supported Julian Day value.");
			}
			return badValue.Value;
		}
		int num = (int)((jd + 43200000) % 86400000);
		decimal num2 = (decimal)num / 1000.0m;
		num = (int)num2;
		int millisecond = (int)((num2 - (decimal)num) * 1000.0m);
		decimal num3 = num2 - (decimal)num;
		int num4 = num / 3600;
		num -= num4 * 3600;
		int num5 = num / 60;
		int second = (int)(num3 + (decimal)(num - num5 * 60));
		try
		{
			DateTime minValue = DateTime.MinValue;
			return new DateTime(minValue.Year, minValue.Month, minValue.Day, num4, num5, second, millisecond);
		}
		catch
		{
			if (!badValue.HasValue)
			{
				throw;
			}
			return badValue.Value;
		}
	}

	private static long computeJD(DateTime dateTime)
	{
		int num = dateTime.Year;
		int num2 = dateTime.Month;
		int day = dateTime.Day;
		if (num2 <= 2)
		{
			num--;
			num2 += 12;
		}
		int num3 = num / 100;
		int num4 = 2 - num3 + num3 / 4;
		int num5 = 36525 * (num + 4716) / 100;
		int num6 = 306001 * (num2 + 1) / 10000;
		return (long)(((double)(num5 + num6 + day + num4) - 1524.5) * 86400000.0) + (dateTime.Hour * 3600000 + dateTime.Minute * 60000 + dateTime.Second * 1000 + dateTime.Millisecond);
	}

	public DateTime ToDateTime(string dateText)
	{
		return ToDateTime(dateText, _datetimeFormat, _datetimeKind, _datetimeFormatString);
	}

	public static DateTime ToDateTime(string dateText, SQLiteDateFormats format, DateTimeKind kind, string formatString)
	{
		switch (format)
		{
		case SQLiteDateFormats.Ticks:
			return TicksToDateTime(Convert.ToInt64(dateText, CultureInfo.InvariantCulture), kind);
		case SQLiteDateFormats.JulianDay:
			return ToDateTime(Convert.ToDouble(dateText, CultureInfo.InvariantCulture), kind);
		case SQLiteDateFormats.UnixEpoch:
			return UnixEpochToDateTime(Convert.ToInt64(dateText, CultureInfo.InvariantCulture), kind);
		case SQLiteDateFormats.InvariantCulture:
			if (formatString != null)
			{
				return DateTime.SpecifyKind(DateTime.ParseExact(dateText, formatString, DateTimeFormatInfo.InvariantInfo, (kind == DateTimeKind.Utc) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), kind);
			}
			return DateTime.SpecifyKind(DateTime.Parse(dateText, DateTimeFormatInfo.InvariantInfo, (kind == DateTimeKind.Utc) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), kind);
		case SQLiteDateFormats.CurrentCulture:
			if (formatString != null)
			{
				return DateTime.SpecifyKind(DateTime.ParseExact(dateText, formatString, DateTimeFormatInfo.CurrentInfo, (kind == DateTimeKind.Utc) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), kind);
			}
			return DateTime.SpecifyKind(DateTime.Parse(dateText, DateTimeFormatInfo.CurrentInfo, (kind == DateTimeKind.Utc) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), kind);
		default:
			if (formatString != null)
			{
				return DateTime.SpecifyKind(DateTime.ParseExact(dateText, formatString, DateTimeFormatInfo.InvariantInfo, (kind == DateTimeKind.Utc) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), kind);
			}
			return DateTime.SpecifyKind(DateTime.ParseExact(dateText, _datetimeFormats, DateTimeFormatInfo.InvariantInfo, (kind == DateTimeKind.Utc) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), kind);
		}
	}

	public DateTime ToDateTime(double julianDay)
	{
		return ToDateTime(julianDay, _datetimeKind);
	}

	public static DateTime ToDateTime(double julianDay, DateTimeKind kind)
	{
		long jd = DoubleToJd(julianDay);
		DateTime dateTime = computeYMD(jd, null);
		DateTime dateTime2 = computeHMS(jd, null);
		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime2.Hour, dateTime2.Minute, dateTime2.Second, dateTime2.Millisecond, kind);
	}

	internal static DateTime UnixEpochToDateTime(long seconds, DateTimeKind kind)
	{
		DateTime unixEpoch = UnixEpoch;
		return DateTime.SpecifyKind(unixEpoch.AddSeconds(seconds), kind);
	}

	internal static DateTime TicksToDateTime(long ticks, DateTimeKind kind)
	{
		return new DateTime(ticks, kind);
	}

	public static double ToJulianDay(DateTime value)
	{
		return JdToDouble(computeJD(value));
	}

	public static long ToUnixEpoch(DateTime value)
	{
		return value.Subtract(UnixEpoch).Ticks / 10000000;
	}

	private static string GetDateTimeKindFormat(DateTimeKind kind, string formatString)
	{
		if (formatString != null)
		{
			return formatString;
		}
		if (kind != DateTimeKind.Utc)
		{
			return _datetimeFormatLocal;
		}
		return _datetimeFormatUtc;
	}

	public string ToString(DateTime dateValue)
	{
		return ToString(dateValue, _datetimeFormat, _datetimeKind, _datetimeFormatString);
	}

	public static string ToString(DateTime dateValue, SQLiteDateFormats format, DateTimeKind kind, string formatString)
	{
		switch (format)
		{
		case SQLiteDateFormats.Ticks:
			return dateValue.Ticks.ToString(CultureInfo.InvariantCulture);
		case SQLiteDateFormats.JulianDay:
			return ToJulianDay(dateValue).ToString(CultureInfo.InvariantCulture);
		case SQLiteDateFormats.UnixEpoch:
			return (dateValue.Subtract(UnixEpoch).Ticks / 10000000).ToString();
		case SQLiteDateFormats.InvariantCulture:
			return dateValue.ToString((formatString != null) ? formatString : "yyyy-MM-ddTHH:mm:ss.fffffffK", CultureInfo.InvariantCulture);
		case SQLiteDateFormats.CurrentCulture:
			return dateValue.ToString((formatString != null) ? formatString : "yyyy-MM-ddTHH:mm:ss.fffffffK", CultureInfo.CurrentCulture);
		default:
			if (dateValue.Kind != 0)
			{
				return dateValue.ToString(GetDateTimeKindFormat(dateValue.Kind, formatString), CultureInfo.InvariantCulture);
			}
			return DateTime.SpecifyKind(dateValue, kind).ToString(GetDateTimeKindFormat(kind, formatString), CultureInfo.InvariantCulture);
		}
	}

	internal DateTime ToDateTime(IntPtr ptr, int len)
	{
		return ToDateTime(ToString(ptr, len));
	}

	public static string[] Split(string source, char separator)
	{
		char[] array = new char[2] { '"', separator };
		char[] array2 = new char[1] { '"' };
		int startIndex = 0;
		List<string> list = new List<string>();
		while (source.Length > 0)
		{
			startIndex = source.IndexOfAny(array, startIndex);
			if (startIndex == -1)
			{
				break;
			}
			if (source[startIndex] == array[0])
			{
				startIndex = source.IndexOfAny(array2, startIndex + 1);
				if (startIndex == -1)
				{
					break;
				}
				startIndex++;
				continue;
			}
			string text = source.Substring(0, startIndex).Trim();
			if (text.Length > 1 && text[0] == array2[0] && text[text.Length - 1] == text[0])
			{
				text = text.Substring(1, text.Length - 2);
			}
			source = source.Substring(startIndex + 1).Trim();
			if (text.Length > 0)
			{
				list.Add(text);
			}
			startIndex = 0;
		}
		if (source.Length > 0)
		{
			string text = source.Trim();
			if (text.Length > 1 && text[0] == array2[0] && text[text.Length - 1] == text[0])
			{
				text = text.Substring(1, text.Length - 2);
			}
			list.Add(text);
		}
		string[] array3 = new string[list.Count];
		list.CopyTo(array3, 0);
		return array3;
	}

	internal static string[] NewSplit(string value, char separator, bool keepQuote, ref string error)
	{
		if (separator == '\\' || separator == '"')
		{
			error = "separator character cannot be the escape or quote characters";
			return null;
		}
		if (value == null)
		{
			error = "string value to split cannot be null";
			return null;
		}
		int length = value.Length;
		if (length == 0)
		{
			return new string[0];
		}
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		while (num < length)
		{
			char c = value[num++];
			if (flag)
			{
				if (c != '\\' && c != '"' && c != separator)
				{
					stringBuilder.Append('\\');
				}
				stringBuilder.Append(c);
				flag = false;
				continue;
			}
			switch (c)
			{
			case '\\':
				flag = true;
				continue;
			case '"':
				if (keepQuote)
				{
					stringBuilder.Append(c);
				}
				flag2 = !flag2;
				continue;
			}
			if (c == separator)
			{
				if (flag2)
				{
					stringBuilder.Append(c);
					continue;
				}
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		if (flag || flag2)
		{
			error = "unbalanced escape or quote character found";
			return null;
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(stringBuilder.ToString());
		}
		return list.ToArray();
	}

	public static string ToStringWithProvider(object obj, IFormatProvider provider)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is string)
		{
			return (string)obj;
		}
		if (obj is IConvertible convertible)
		{
			return convertible.ToString(provider);
		}
		return obj.ToString();
	}

	internal static bool ToBoolean(object obj, IFormatProvider provider, bool viaFramework)
	{
		if (obj == null)
		{
			return false;
		}
		TypeCode typeCode = Type.GetTypeCode(obj.GetType());
		switch (typeCode)
		{
		case TypeCode.Empty:
		case TypeCode.DBNull:
			return false;
		case TypeCode.Boolean:
			return (bool)obj;
		case TypeCode.Char:
			if ((char)obj == '\0')
			{
				return false;
			}
			return true;
		case TypeCode.SByte:
			if ((sbyte)obj == 0)
			{
				return false;
			}
			return true;
		case TypeCode.Byte:
			if ((byte)obj == 0)
			{
				return false;
			}
			return true;
		case TypeCode.Int16:
			if ((short)obj == 0)
			{
				return false;
			}
			return true;
		case TypeCode.UInt16:
			if ((ushort)obj == 0)
			{
				return false;
			}
			return true;
		case TypeCode.Int32:
			if ((int)obj == 0)
			{
				return false;
			}
			return true;
		case TypeCode.UInt32:
			if ((uint)obj == 0)
			{
				return false;
			}
			return true;
		case TypeCode.Int64:
			if ((long)obj == 0L)
			{
				return false;
			}
			return true;
		case TypeCode.UInt64:
			if ((ulong)obj == 0L)
			{
				return false;
			}
			return true;
		case TypeCode.Single:
			if ((float)obj == 0f)
			{
				return false;
			}
			return true;
		case TypeCode.Double:
			if ((double)obj == 0.0)
			{
				return false;
			}
			return true;
		case TypeCode.Decimal:
			if (!((decimal)obj != 0m))
			{
				return false;
			}
			return true;
		case TypeCode.String:
			if (!viaFramework)
			{
				return ToBoolean(ToStringWithProvider(obj, provider));
			}
			return Convert.ToBoolean(obj, provider);
		default:
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Cannot convert type {0} to boolean", typeCode));
		}
	}

	public static bool ToBoolean(object source)
	{
		if (source is bool)
		{
			return (bool)source;
		}
		return ToBoolean(ToStringWithProvider(source, CultureInfo.InvariantCulture));
	}

	internal static string ToString(int value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public static bool ToBoolean(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (string.Compare(source, 0, bool.TrueString, 0, source.Length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		if (string.Compare(source, 0, bool.FalseString, 0, source.Length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return false;
		}
		switch (source.ToLower(CultureInfo.InvariantCulture))
		{
		case "y":
		case "yes":
		case "on":
		case "1":
			return true;
		case "n":
		case "no":
		case "off":
		case "0":
			return false;
		default:
			throw new ArgumentException("source");
		}
	}

	internal static Type SQLiteTypeToType(SQLiteType t)
	{
		if (t.Type == DbType.Object)
		{
			return _affinitytotype[(int)t.Affinity];
		}
		return DbTypeToType(t.Type);
	}

	internal static DbType TypeToDbType(Type typ)
	{
		TypeCode typeCode = Type.GetTypeCode(typ);
		if (typeCode == TypeCode.Object)
		{
			if (typ == typeof(byte[]))
			{
				return DbType.Binary;
			}
			if (typ == typeof(Guid))
			{
				return DbType.Guid;
			}
			return DbType.String;
		}
		return _typetodbtype[(int)typeCode];
	}

	internal static int DbTypeToColumnSize(DbType typ)
	{
		return _dbtypetocolumnsize[(int)typ];
	}

	internal static object DbTypeToNumericPrecision(DbType typ)
	{
		return _dbtypetonumericprecision[(int)typ];
	}

	internal static object DbTypeToNumericScale(DbType typ)
	{
		return _dbtypetonumericscale[(int)typ];
	}

	private static string GetDefaultTypeName(SQLiteConnection connection)
	{
		if (HelperMethods.HasFlags(connection?.Flags ?? SQLiteConnectionFlags.None, SQLiteConnectionFlags.NoConvertSettings))
		{
			return FallbackDefaultTypeName;
		}
		string name = "Use_SQLiteConvert_DefaultTypeName";
		object value = null;
		string @default = null;
		if (connection == null || !connection.TryGetCachedSetting(name, @default, out value))
		{
			try
			{
				value = UnsafeNativeMethods.GetSettingValue(name, @default);
				if (value == null)
				{
					value = FallbackDefaultTypeName;
				}
			}
			finally
			{
				connection?.SetCachedSetting(name, value);
			}
		}
		return SettingValueToString(value);
	}

	private static void DefaultTypeNameWarning(DbType dbType, SQLiteConnectionFlags flags, string typeName)
	{
		if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.TraceWarning))
		{
			Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "WARNING: Type mapping failed, returning default name \"{0}\" for type {1}.", typeName, dbType));
		}
	}

	private static void DefaultDbTypeWarning(string typeName, SQLiteConnectionFlags flags, DbType? dbType)
	{
		if (!string.IsNullOrEmpty(typeName) && HelperMethods.HasFlags(flags, SQLiteConnectionFlags.TraceWarning))
		{
			Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "WARNING: Type mapping failed, returning default type {0} for name \"{1}\".", dbType, typeName));
		}
	}

	internal static string DbTypeToTypeName(SQLiteConnection connection, DbType dbType, SQLiteConnectionFlags flags)
	{
		string text = null;
		if (connection != null)
		{
			flags |= connection.Flags;
			if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.UseConnectionTypes))
			{
				SQLiteDbTypeMap typeNames = connection._typeNames;
				if (typeNames != null && typeNames.TryGetValue(dbType, out var value))
				{
					return value.typeName;
				}
			}
			text = connection.DefaultTypeName;
		}
		if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.NoGlobalTypes))
		{
			if (text != null)
			{
				return text;
			}
			text = GetDefaultTypeName(connection);
			DefaultTypeNameWarning(dbType, flags, text);
			return text;
		}
		if (_typeNames != null && _typeNames.TryGetValue(dbType, out var value2))
		{
			return value2.typeName;
		}
		if (text != null)
		{
			return text;
		}
		text = GetDefaultTypeName(connection);
		DefaultTypeNameWarning(dbType, flags, text);
		return text;
	}

	internal static Type DbTypeToType(DbType typ)
	{
		return _dbtypeToType[(int)typ];
	}

	internal static TypeAffinity TypeToAffinity(Type typ, SQLiteConnectionFlags flags)
	{
		TypeCode typeCode = Type.GetTypeCode(typ);
		switch (typeCode)
		{
		case TypeCode.Object:
			if (typ == typeof(byte[]) || typ == typeof(Guid))
			{
				return TypeAffinity.Blob;
			}
			return TypeAffinity.Text;
		case TypeCode.Decimal:
			if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetDecimalAsText))
			{
				return TypeAffinity.Text;
			}
			break;
		}
		return _typecodeAffinities[(int)typeCode];
	}

	private static SQLiteDbTypeMap GetSQLiteDbTypeMap()
	{
		return new SQLiteDbTypeMap(new SQLiteDbTypeMapping[76]
		{
			new SQLiteDbTypeMapping("BIGINT", DbType.Int64, newPrimary: false),
			new SQLiteDbTypeMapping("BIGUINT", DbType.UInt64, newPrimary: false),
			new SQLiteDbTypeMapping("BINARY", DbType.Binary, newPrimary: false),
			new SQLiteDbTypeMapping("BIT", DbType.Boolean, newPrimary: true),
			new SQLiteDbTypeMapping("BLOB", DbType.Binary, newPrimary: true),
			new SQLiteDbTypeMapping("BOOL", DbType.Boolean, newPrimary: false),
			new SQLiteDbTypeMapping("BOOLEAN", DbType.Boolean, newPrimary: false),
			new SQLiteDbTypeMapping("CHAR", DbType.AnsiStringFixedLength, newPrimary: true),
			new SQLiteDbTypeMapping("CLOB", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("COUNTER", DbType.Int64, newPrimary: false),
			new SQLiteDbTypeMapping("CURRENCY", DbType.Decimal, newPrimary: false),
			new SQLiteDbTypeMapping("DATE", DbType.DateTime, newPrimary: false),
			new SQLiteDbTypeMapping("DATETIME", DbType.DateTime, newPrimary: true),
			new SQLiteDbTypeMapping("DECIMAL", DbType.Decimal, newPrimary: true),
			new SQLiteDbTypeMapping("DECIMALTEXT", DbType.Decimal, newPrimary: false),
			new SQLiteDbTypeMapping("DOUBLE", DbType.Double, newPrimary: false),
			new SQLiteDbTypeMapping("FLOAT", DbType.Double, newPrimary: false),
			new SQLiteDbTypeMapping("GENERAL", DbType.Binary, newPrimary: false),
			new SQLiteDbTypeMapping("GUID", DbType.Guid, newPrimary: false),
			new SQLiteDbTypeMapping("IDENTITY", DbType.Int64, newPrimary: false),
			new SQLiteDbTypeMapping("IMAGE", DbType.Binary, newPrimary: false),
			new SQLiteDbTypeMapping("INT", DbType.Int32, newPrimary: true),
			new SQLiteDbTypeMapping("INT8", DbType.SByte, newPrimary: false),
			new SQLiteDbTypeMapping("INT16", DbType.Int16, newPrimary: false),
			new SQLiteDbTypeMapping("INT32", DbType.Int32, newPrimary: false),
			new SQLiteDbTypeMapping("INT64", DbType.Int64, newPrimary: false),
			new SQLiteDbTypeMapping("INTEGER", DbType.Int64, newPrimary: true),
			new SQLiteDbTypeMapping("INTEGER8", DbType.SByte, newPrimary: false),
			new SQLiteDbTypeMapping("INTEGER16", DbType.Int16, newPrimary: false),
			new SQLiteDbTypeMapping("INTEGER32", DbType.Int32, newPrimary: false),
			new SQLiteDbTypeMapping("INTEGER64", DbType.Int64, newPrimary: false),
			new SQLiteDbTypeMapping("LOGICAL", DbType.Boolean, newPrimary: false),
			new SQLiteDbTypeMapping("LONG", DbType.Int64, newPrimary: false),
			new SQLiteDbTypeMapping("LONGCHAR", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("LONGTEXT", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("LONGVARCHAR", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("MEDIUMINT", DbType.Int32, newPrimary: false),
			new SQLiteDbTypeMapping("MEDIUMUINT", DbType.UInt32, newPrimary: false),
			new SQLiteDbTypeMapping("MEMO", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("MONEY", DbType.Decimal, newPrimary: false),
			new SQLiteDbTypeMapping("NCHAR", DbType.StringFixedLength, newPrimary: true),
			new SQLiteDbTypeMapping("NOTE", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("NTEXT", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("NUMBER", DbType.Decimal, newPrimary: false),
			new SQLiteDbTypeMapping("NUMERIC", DbType.Decimal, newPrimary: false),
			new SQLiteDbTypeMapping("NUMERICTEXT", DbType.Decimal, newPrimary: false),
			new SQLiteDbTypeMapping("NVARCHAR", DbType.String, newPrimary: true),
			new SQLiteDbTypeMapping("OLEOBJECT", DbType.Binary, newPrimary: false),
			new SQLiteDbTypeMapping("RAW", DbType.Binary, newPrimary: false),
			new SQLiteDbTypeMapping("REAL", DbType.Double, newPrimary: true),
			new SQLiteDbTypeMapping("SINGLE", DbType.Single, newPrimary: true),
			new SQLiteDbTypeMapping("SMALLDATE", DbType.DateTime, newPrimary: false),
			new SQLiteDbTypeMapping("SMALLINT", DbType.Int16, newPrimary: true),
			new SQLiteDbTypeMapping("SMALLUINT", DbType.UInt16, newPrimary: true),
			new SQLiteDbTypeMapping("STRING", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("TEXT", DbType.String, newPrimary: false),
			new SQLiteDbTypeMapping("TIME", DbType.DateTime, newPrimary: false),
			new SQLiteDbTypeMapping("TIMESTAMP", DbType.DateTime, newPrimary: false),
			new SQLiteDbTypeMapping("TINYINT", DbType.Byte, newPrimary: true),
			new SQLiteDbTypeMapping("TINYSINT", DbType.SByte, newPrimary: true),
			new SQLiteDbTypeMapping("UINT", DbType.UInt32, newPrimary: true),
			new SQLiteDbTypeMapping("UINT8", DbType.Byte, newPrimary: false),
			new SQLiteDbTypeMapping("UINT16", DbType.UInt16, newPrimary: false),
			new SQLiteDbTypeMapping("UINT32", DbType.UInt32, newPrimary: false),
			new SQLiteDbTypeMapping("UINT64", DbType.UInt64, newPrimary: false),
			new SQLiteDbTypeMapping("ULONG", DbType.UInt64, newPrimary: false),
			new SQLiteDbTypeMapping("UNIQUEIDENTIFIER", DbType.Guid, newPrimary: true),
			new SQLiteDbTypeMapping("UNSIGNEDINTEGER", DbType.UInt64, newPrimary: true),
			new SQLiteDbTypeMapping("UNSIGNEDINTEGER8", DbType.Byte, newPrimary: false),
			new SQLiteDbTypeMapping("UNSIGNEDINTEGER16", DbType.UInt16, newPrimary: false),
			new SQLiteDbTypeMapping("UNSIGNEDINTEGER32", DbType.UInt32, newPrimary: false),
			new SQLiteDbTypeMapping("UNSIGNEDINTEGER64", DbType.UInt64, newPrimary: false),
			new SQLiteDbTypeMapping("VARBINARY", DbType.Binary, newPrimary: false),
			new SQLiteDbTypeMapping("VARCHAR", DbType.AnsiString, newPrimary: true),
			new SQLiteDbTypeMapping("VARCHAR2", DbType.AnsiString, newPrimary: false),
			new SQLiteDbTypeMapping("YESNO", DbType.Boolean, newPrimary: false)
		});
	}

	internal static bool IsStringDbType(DbType type)
	{
		switch (type)
		{
		case DbType.AnsiString:
		case DbType.String:
		case DbType.AnsiStringFixedLength:
		case DbType.StringFixedLength:
			return true;
		default:
			return false;
		}
	}

	private static string SettingValueToString(object value)
	{
		if (value is string)
		{
			return (string)value;
		}
		return value?.ToString();
	}

	private static DbType GetDefaultDbType(SQLiteConnection connection)
	{
		if (HelperMethods.HasFlags(connection?.Flags ?? SQLiteConnectionFlags.None, SQLiteConnectionFlags.NoConvertSettings))
		{
			return DbType.Object;
		}
		bool flag = false;
		string name = "Use_SQLiteConvert_DefaultDbType";
		object value = null;
		string @default = null;
		if (connection == null || !connection.TryGetCachedSetting(name, @default, out value))
		{
			value = UnsafeNativeMethods.GetSettingValue(name, @default);
			if (value == null)
			{
				value = DbType.Object;
			}
		}
		else
		{
			flag = true;
		}
		try
		{
			if (!(value is DbType))
			{
				value = SQLiteConnection.TryParseEnum(typeof(DbType), SettingValueToString(value), ignoreCase: true);
				if (!(value is DbType))
				{
					value = DbType.Object;
				}
			}
			return (DbType)value;
		}
		finally
		{
			if (!flag)
			{
				connection?.SetCachedSetting(name, value);
			}
		}
	}

	public static string GetStringOrNull(object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is string)
		{
			return (string)value;
		}
		if (value == DBNull.Value)
		{
			return null;
		}
		return value.ToString();
	}

	internal static bool LooksLikeNull(string text)
	{
		return text == null;
	}

	internal static bool LooksLikeInt64(string text)
	{
		if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return false;
		}
		return string.Equals(result.ToString(CultureInfo.InvariantCulture), text, StringComparison.Ordinal);
	}

	internal static bool LooksLikeDouble(string text)
	{
		if (!double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
		{
			return false;
		}
		return string.Equals(result.ToString(CultureInfo.InvariantCulture), text, StringComparison.Ordinal);
	}

	internal static bool LooksLikeDateTime(SQLiteConvert convert, string text)
	{
		if (convert == null)
		{
			return false;
		}
		try
		{
			DateTime dateTime = convert.ToDateTime(text);
			if (string.Equals(convert.ToString(dateTime), text, StringComparison.Ordinal))
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	internal static DbType TypeNameToDbType(SQLiteConnection connection, string typeName, SQLiteConnectionFlags flags)
	{
		DbType? dbType = null;
		if (connection != null)
		{
			flags |= connection.Flags;
			if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.UseConnectionTypes))
			{
				SQLiteDbTypeMap typeNames = connection._typeNames;
				if (typeNames != null && typeName != null)
				{
					if (typeNames.TryGetValue(typeName, out var value))
					{
						return value.dataType;
					}
					int num = typeName.IndexOf('(');
					if (num > 0 && typeNames.TryGetValue(typeName.Substring(0, num).TrimEnd(Array.Empty<char>()), out value))
					{
						return value.dataType;
					}
				}
			}
			dbType = connection.DefaultDbType;
		}
		if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.NoGlobalTypes))
		{
			if (dbType.HasValue)
			{
				return dbType.Value;
			}
			dbType = GetDefaultDbType(connection);
			DefaultDbTypeWarning(typeName, flags, dbType);
			return dbType.Value;
		}
		if (_typeNames != null && typeName != null)
		{
			if (_typeNames.TryGetValue(typeName, out var value2))
			{
				return value2.dataType;
			}
			int num2 = typeName.IndexOf('(');
			if (num2 > 0 && _typeNames.TryGetValue(typeName.Substring(0, num2).TrimEnd(Array.Empty<char>()), out value2))
			{
				return value2.dataType;
			}
		}
		if (dbType.HasValue)
		{
			return dbType.Value;
		}
		dbType = GetDefaultDbType(connection);
		DefaultDbTypeWarning(typeName, flags, dbType);
		return dbType.Value;
	}
}
