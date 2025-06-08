using System.Data.Entity.Resources;
using System.Globalization;

namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class Literal : Node
{
	private readonly LiteralKind _literalKind;

	private string _originalValue;

	private bool _wasValueComputed;

	private object _computedValue;

	private Type _type;

	private static readonly byte[] _emptyByteArray = new byte[0];

	private static readonly char[] _numberSuffixes = new char[10] { 'U', 'u', 'L', 'l', 'F', 'f', 'M', 'm', 'D', 'd' };

	private static readonly char[] _floatTokens = new char[3] { '.', 'E', 'e' };

	private static readonly char[] _datetimeSeparators = new char[4] { ' ', ':', '-', '.' };

	private static readonly char[] _datetimeOffsetSeparators = new char[6] { ' ', ':', '-', '.', '+', '-' };

	internal bool IsNumber => _literalKind == LiteralKind.Number;

	internal bool IsSignedNumber
	{
		get
		{
			if (IsNumber)
			{
				if (_originalValue[0] != '-')
				{
					return _originalValue[0] == '+';
				}
				return true;
			}
			return false;
		}
	}

	internal bool IsString
	{
		get
		{
			if (_literalKind != LiteralKind.String)
			{
				return _literalKind == LiteralKind.UnicodeString;
			}
			return true;
		}
	}

	internal bool IsUnicodeString => _literalKind == LiteralKind.UnicodeString;

	internal bool IsNullLiteral => _literalKind == LiteralKind.Null;

	internal string OriginalValue => _originalValue;

	internal object Value
	{
		get
		{
			ComputeValue();
			return _computedValue;
		}
	}

	internal Type Type
	{
		get
		{
			ComputeValue();
			return _type;
		}
	}

	internal Literal(string originalValue, LiteralKind kind, string query, int inputPos)
		: base(query, inputPos)
	{
		_originalValue = originalValue;
		_literalKind = kind;
	}

	internal static Literal NewBooleanLiteral(bool value)
	{
		return new Literal(value);
	}

	private Literal(bool boolLiteral)
		: base(null, 0)
	{
		_wasValueComputed = true;
		_originalValue = string.Empty;
		_computedValue = boolLiteral;
		_type = typeof(bool);
	}

	internal void PrefixSign(string sign)
	{
		_originalValue = sign + _originalValue;
	}

	private void ComputeValue()
	{
		if (!_wasValueComputed)
		{
			_wasValueComputed = true;
			switch (_literalKind)
			{
			case LiteralKind.Number:
				_computedValue = ConvertNumericLiteral(base.ErrCtx, _originalValue);
				break;
			case LiteralKind.String:
				_computedValue = GetStringLiteralValue(_originalValue, isUnicode: false);
				break;
			case LiteralKind.UnicodeString:
				_computedValue = GetStringLiteralValue(_originalValue, isUnicode: true);
				break;
			case LiteralKind.Boolean:
				_computedValue = ConvertBooleanLiteralValue(base.ErrCtx, _originalValue);
				break;
			case LiteralKind.Binary:
				_computedValue = ConvertBinaryLiteralValue(_originalValue);
				break;
			case LiteralKind.DateTime:
				_computedValue = ConvertDateTimeLiteralValue(_originalValue);
				break;
			case LiteralKind.Time:
				_computedValue = ConvertTimeLiteralValue(_originalValue);
				break;
			case LiteralKind.DateTimeOffset:
				_computedValue = ConvertDateTimeOffsetLiteralValue(base.ErrCtx, _originalValue);
				break;
			case LiteralKind.Guid:
				_computedValue = ConvertGuidLiteralValue(_originalValue);
				break;
			case LiteralKind.Null:
				_computedValue = null;
				break;
			default:
				throw new NotSupportedException(Strings.LiteralTypeNotSupported(_literalKind.ToString()));
			}
			_type = (IsNullLiteral ? null : _computedValue.GetType());
		}
	}

	private static object ConvertNumericLiteral(ErrorContext errCtx, string numericString)
	{
		int num = numericString.IndexOfAny(_numberSuffixes);
		if (-1 != num)
		{
			string text = numericString.Substring(num).ToUpperInvariant();
			string s = numericString.Substring(0, numericString.Length - text.Length);
			switch (text)
			{
			case "U":
			{
				if (!uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result5))
				{
					string errorMessage5 = Strings.CannotConvertNumericLiteral(numericString, "unsigned int");
					throw EntitySqlException.Create(errCtx, errorMessage5, null);
				}
				return result5;
			}
			case "L":
			{
				if (!long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3))
				{
					string errorMessage3 = Strings.CannotConvertNumericLiteral(numericString, "long");
					throw EntitySqlException.Create(errCtx, errorMessage3, null);
				}
				return result3;
			}
			case "UL":
			case "LU":
			{
				if (!ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result6))
				{
					string errorMessage6 = Strings.CannotConvertNumericLiteral(numericString, "unsigned long");
					throw EntitySqlException.Create(errCtx, errorMessage6, null);
				}
				return result6;
			}
			case "F":
			{
				if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					string errorMessage2 = Strings.CannotConvertNumericLiteral(numericString, "float");
					throw EntitySqlException.Create(errCtx, errorMessage2, null);
				}
				return result2;
			}
			case "M":
			{
				if (!decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result4))
				{
					string errorMessage4 = Strings.CannotConvertNumericLiteral(numericString, "decimal");
					throw EntitySqlException.Create(errCtx, errorMessage4, null);
				}
				return result4;
			}
			case "D":
			{
				if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					string errorMessage = Strings.CannotConvertNumericLiteral(numericString, "double");
					throw EntitySqlException.Create(errCtx, errorMessage, null);
				}
				return result;
			}
			}
		}
		return DefaultNumericConversion(numericString, errCtx);
	}

	private static object DefaultNumericConversion(string numericString, ErrorContext errCtx)
	{
		if (-1 != numericString.IndexOfAny(_floatTokens))
		{
			if (!double.TryParse(numericString, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				string errorMessage = Strings.CannotConvertNumericLiteral(numericString, "double");
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
			return result;
		}
		if (int.TryParse(numericString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
		{
			return result2;
		}
		if (!long.TryParse(numericString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3))
		{
			string errorMessage2 = Strings.CannotConvertNumericLiteral(numericString, "long");
			throw EntitySqlException.Create(errCtx, errorMessage2, null);
		}
		return result3;
	}

	private static bool ConvertBooleanLiteralValue(ErrorContext errCtx, string booleanLiteralValue)
	{
		bool result = false;
		if (!bool.TryParse(booleanLiteralValue, out result))
		{
			string errorMessage = Strings.InvalidLiteralFormat("Boolean", booleanLiteralValue);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		return result;
	}

	private static string GetStringLiteralValue(string stringLiteralValue, bool isUnicode)
	{
		int num = ((!isUnicode) ? 1 : 2);
		char c = stringLiteralValue[num - 1];
		if (c != '\'' && c != '"')
		{
			throw new EntitySqlException(Strings.MalformedStringLiteralPayload);
		}
		string text = "";
		int num2 = stringLiteralValue.Split(new char[1] { c }).Length - 1;
		if (num2 % 2 != 0)
		{
			throw new EntitySqlException(Strings.MalformedStringLiteralPayload);
		}
		text = stringLiteralValue.Substring(num, stringLiteralValue.Length - (1 + num));
		text = text.Replace(new string(c, 2), new string(c, 1));
		if (text.Split(new char[1] { c }).Length - 1 != (num2 - 2) / 2)
		{
			throw new EntitySqlException(Strings.MalformedStringLiteralPayload);
		}
		return text;
	}

	private static byte[] ConvertBinaryLiteralValue(string binaryLiteralValue)
	{
		if (string.IsNullOrEmpty(binaryLiteralValue))
		{
			return _emptyByteArray;
		}
		int num = 0;
		int num2 = binaryLiteralValue.Length - 1;
		int num3 = num2 - num + 1;
		int num4 = num3 / 2;
		bool num5 = num3 % 2 != 0;
		if (num5)
		{
			num4++;
		}
		byte[] array = new byte[num4];
		int num6 = 0;
		if (num5)
		{
			array[num6++] = (byte)HexDigitToBinaryValue(binaryLiteralValue[num++]);
		}
		while (num < num2)
		{
			array[num6++] = (byte)((HexDigitToBinaryValue(binaryLiteralValue[num++]) << 4) | HexDigitToBinaryValue(binaryLiteralValue[num++]));
		}
		return array;
	}

	private static int HexDigitToBinaryValue(char hexChar)
	{
		if (hexChar >= '0' && hexChar <= '9')
		{
			return hexChar - 48;
		}
		if (hexChar >= 'A' && hexChar <= 'F')
		{
			return hexChar - 65 + 10;
		}
		if (hexChar >= 'a' && hexChar <= 'f')
		{
			return hexChar - 97 + 10;
		}
		throw new ArgumentOutOfRangeException("hexChar");
	}

	private static DateTime ConvertDateTimeLiteralValue(string datetimeLiteralValue)
	{
		string[] datetimeParts = datetimeLiteralValue.Split(_datetimeSeparators, StringSplitOptions.RemoveEmptyEntries);
		GetDateParts(datetimeLiteralValue, datetimeParts, out var year, out var month, out var day);
		GetTimeParts(datetimeLiteralValue, datetimeParts, 3, out var hour, out var minute, out var second, out var ticks);
		return new DateTime(year, month, day, hour, minute, second, 0).AddTicks(ticks);
	}

	private static DateTimeOffset ConvertDateTimeOffsetLiteralValue(ErrorContext errCtx, string datetimeLiteralValue)
	{
		string[] array = datetimeLiteralValue.Split(_datetimeOffsetSeparators, StringSplitOptions.RemoveEmptyEntries);
		GetDateParts(datetimeLiteralValue, array, out var year, out var month, out var day);
		string[] array2 = new string[array.Length - 2];
		Array.Copy(array, array2, array.Length - 2);
		GetTimeParts(datetimeLiteralValue, array2, 3, out var hour, out var minute, out var second, out var ticks);
		int hours = int.Parse(array[^2], NumberStyles.Integer, CultureInfo.InvariantCulture);
		int minutes = int.Parse(array[^1], NumberStyles.Integer, CultureInfo.InvariantCulture);
		TimeSpan offset = new TimeSpan(hours, minutes, 0);
		if (datetimeLiteralValue.IndexOf('+') == -1)
		{
			offset = offset.Negate();
		}
		DateTime dateTime = new DateTime(year, month, day, hour, minute, second, 0).AddTicks(ticks);
		try
		{
			return new DateTimeOffset(dateTime, offset);
		}
		catch (ArgumentOutOfRangeException innerException)
		{
			string errorMessage = Strings.InvalidDateTimeOffsetLiteral(datetimeLiteralValue);
			throw EntitySqlException.Create(errCtx, errorMessage, innerException);
		}
	}

	private static TimeSpan ConvertTimeLiteralValue(string datetimeLiteralValue)
	{
		string[] datetimeParts = datetimeLiteralValue.Split(_datetimeSeparators, StringSplitOptions.RemoveEmptyEntries);
		GetTimeParts(datetimeLiteralValue, datetimeParts, 0, out var hour, out var minute, out var second, out var ticks);
		return new TimeSpan(hour, minute, second).Add(new TimeSpan(ticks));
	}

	private static void GetTimeParts(string datetimeLiteralValue, string[] datetimeParts, int timePartStartIndex, out int hour, out int minute, out int second, out int ticks)
	{
		hour = int.Parse(datetimeParts[timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (hour > 23)
		{
			throw new EntitySqlException(Strings.InvalidHour(datetimeParts[timePartStartIndex], datetimeLiteralValue));
		}
		minute = int.Parse(datetimeParts[++timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (minute > 59)
		{
			throw new EntitySqlException(Strings.InvalidMinute(datetimeParts[timePartStartIndex], datetimeLiteralValue));
		}
		second = 0;
		ticks = 0;
		timePartStartIndex++;
		if (datetimeParts.Length > timePartStartIndex)
		{
			second = int.Parse(datetimeParts[timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
			if (second > 59)
			{
				throw new EntitySqlException(Strings.InvalidSecond(datetimeParts[timePartStartIndex], datetimeLiteralValue));
			}
			timePartStartIndex++;
			if (datetimeParts.Length > timePartStartIndex)
			{
				string s = datetimeParts[timePartStartIndex].PadRight(7, '0');
				ticks = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
			}
		}
	}

	private static void GetDateParts(string datetimeLiteralValue, string[] datetimeParts, out int year, out int month, out int day)
	{
		year = int.Parse(datetimeParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (year < 1 || year > 9999)
		{
			throw new EntitySqlException(Strings.InvalidYear(datetimeParts[0], datetimeLiteralValue));
		}
		month = int.Parse(datetimeParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (month < 1 || month > 12)
		{
			throw new EntitySqlException(Strings.InvalidMonth(datetimeParts[1], datetimeLiteralValue));
		}
		day = int.Parse(datetimeParts[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (day < 1)
		{
			throw new EntitySqlException(Strings.InvalidDay(datetimeParts[2], datetimeLiteralValue));
		}
		if (day > DateTime.DaysInMonth(year, month))
		{
			throw new EntitySqlException(Strings.InvalidDayInMonth(datetimeParts[2], datetimeParts[1], datetimeLiteralValue));
		}
	}

	private static Guid ConvertGuidLiteralValue(string guidLiteralValue)
	{
		return new Guid(guidLiteralValue);
	}
}
