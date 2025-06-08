using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class ScalarType : SchemaType
{
	internal const string DateTimeFormat = "yyyy-MM-dd HH\\:mm\\:ss.fffZ";

	internal const string TimeFormat = "HH\\:mm\\:ss.fffffffZ";

	internal const string DateTimeOffsetFormat = "yyyy-MM-dd HH\\:mm\\:ss.fffffffz";

	private static readonly Regex _binaryValueValidator = new Regex("^0[xX][0-9a-fA-F]+$", RegexOptions.Compiled);

	private static readonly Regex _guidValueValidator = new Regex("[0-9a-fA-F]{8,8}(-[0-9a-fA-F]{4,4}){3,3}-[0-9a-fA-F]{12,12}", RegexOptions.Compiled);

	private readonly PrimitiveType _primitiveType;

	public PrimitiveTypeKind TypeKind => _primitiveType.PrimitiveTypeKind;

	public PrimitiveType Type => _primitiveType;

	internal ScalarType(Schema parentElement, string typeName, PrimitiveType primitiveType)
		: base(parentElement)
	{
		Name = typeName;
		_primitiveType = primitiveType;
	}

	public bool TryParse(string text, out object value)
	{
		return _primitiveType.PrimitiveTypeKind switch
		{
			PrimitiveTypeKind.Binary => TryParseBinary(text, out value), 
			PrimitiveTypeKind.Boolean => TryParseBoolean(text, out value), 
			PrimitiveTypeKind.Byte => TryParseByte(text, out value), 
			PrimitiveTypeKind.DateTime => TryParseDateTime(text, out value), 
			PrimitiveTypeKind.Time => TryParseTime(text, out value), 
			PrimitiveTypeKind.DateTimeOffset => TryParseDateTimeOffset(text, out value), 
			PrimitiveTypeKind.Decimal => TryParseDecimal(text, out value), 
			PrimitiveTypeKind.Double => TryParseDouble(text, out value), 
			PrimitiveTypeKind.Guid => TryParseGuid(text, out value), 
			PrimitiveTypeKind.Int16 => TryParseInt16(text, out value), 
			PrimitiveTypeKind.Int32 => TryParseInt32(text, out value), 
			PrimitiveTypeKind.Int64 => TryParseInt64(text, out value), 
			PrimitiveTypeKind.Single => TryParseSingle(text, out value), 
			PrimitiveTypeKind.String => TryParseString(text, out value), 
			PrimitiveTypeKind.SByte => TryParseSByte(text, out value), 
			_ => throw new NotSupportedException(_primitiveType.FullName), 
		};
	}

	private static bool TryParseBoolean(string text, out object value)
	{
		if (!bool.TryParse(text, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseByte(string text, out object value)
	{
		if (!byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseSByte(string text, out object value)
	{
		if (!sbyte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseInt16(string text, out object value)
	{
		if (!short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseInt32(string text, out object value)
	{
		if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseInt64(string text, out object value)
	{
		if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseDouble(string text, out object value)
	{
		if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseDecimal(string text, out object value)
	{
		if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseDateTime(string text, out object value)
	{
		if (!DateTime.TryParseExact(text, "yyyy-MM-dd HH\\:mm\\:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseTime(string text, out object value)
	{
		if (!DateTime.TryParseExact(text, "HH\\:mm\\:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result))
		{
			value = null;
			return false;
		}
		value = new TimeSpan(result.Ticks);
		return true;
	}

	private static bool TryParseDateTimeOffset(string text, out object value)
	{
		if (!DateTimeOffset.TryParse(text, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}

	private static bool TryParseGuid(string text, out object value)
	{
		if (!_guidValueValidator.IsMatch(text))
		{
			value = null;
			return false;
		}
		value = new Guid(text);
		return true;
	}

	private static bool TryParseString(string text, out object value)
	{
		value = text;
		return true;
	}

	private static bool TryParseBinary(string text, out object value)
	{
		if (!_binaryValueValidator.IsMatch(text))
		{
			value = null;
			return false;
		}
		string text2 = text.Substring(2);
		value = ConvertToByteArray(text2);
		return true;
	}

	internal static byte[] ConvertToByteArray(string text)
	{
		int num = 2;
		int num2 = text.Length / 2;
		if (text.Length % 2 == 1)
		{
			num = 1;
			num2++;
		}
		byte[] array = new byte[num2];
		int num3 = 0;
		int num4 = 0;
		while (num3 < text.Length)
		{
			array[num4] = byte.Parse(text.Substring(num3, num), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			num3 += num;
			num = 2;
			num4++;
		}
		return array;
	}

	private static bool TryParseSingle(string text, out object value)
	{
		if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			value = null;
			return false;
		}
		value = result;
		return true;
	}
}
