using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace dnlib.DotNet;

[DebuggerDisplay("{String}")]
public sealed class UTF8String : IEquatable<UTF8String>, IComparable<UTF8String>
{
	public static readonly UTF8String Empty = new UTF8String(string.Empty);

	private readonly byte[] data;

	private string asString;

	public string String
	{
		get
		{
			if (asString == null)
			{
				asString = ConvertFromUTF8(data);
			}
			return asString;
		}
	}

	public byte[] Data => data;

	public int Length => String.Length;

	public int DataLength
	{
		get
		{
			if (data != null)
			{
				return data.Length;
			}
			return 0;
		}
	}

	public static bool IsNull(UTF8String utf8)
	{
		if ((object)utf8 != null)
		{
			return utf8.data == null;
		}
		return true;
	}

	public static bool IsNullOrEmpty(UTF8String utf8)
	{
		if ((object)utf8 != null && utf8.data != null)
		{
			return utf8.data.Length == 0;
		}
		return true;
	}

	public static implicit operator string(UTF8String s)
	{
		return ToSystemString(s);
	}

	public static implicit operator UTF8String(string s)
	{
		if (s != null)
		{
			return new UTF8String(s);
		}
		return null;
	}

	public static string ToSystemString(UTF8String utf8)
	{
		if ((object)utf8 == null || utf8.data == null)
		{
			return null;
		}
		if (utf8.data.Length == 0)
		{
			return string.Empty;
		}
		return utf8.String;
	}

	public static string ToSystemStringOrEmpty(UTF8String utf8)
	{
		return ToSystemString(utf8) ?? string.Empty;
	}

	public static int GetHashCode(UTF8String utf8)
	{
		if (IsNullOrEmpty(utf8))
		{
			return 0;
		}
		return Utils.GetHashCode(utf8.data);
	}

	public int CompareTo(UTF8String other)
	{
		return CompareTo(this, other);
	}

	public static int CompareTo(UTF8String a, UTF8String b)
	{
		return Utils.CompareTo(a?.data, b?.data);
	}

	public static int CaseInsensitiveCompareTo(UTF8String a, UTF8String b)
	{
		if ((object)a == b)
		{
			return 0;
		}
		string text = ToSystemString(a);
		string text2 = ToSystemString(b);
		if ((object)text == text2)
		{
			return 0;
		}
		if (text == null)
		{
			return -1;
		}
		if (text2 == null)
		{
			return 1;
		}
		return StringComparer.OrdinalIgnoreCase.Compare(text, text2);
	}

	public static bool CaseInsensitiveEquals(UTF8String a, UTF8String b)
	{
		return CaseInsensitiveCompareTo(a, b) == 0;
	}

	public static bool operator ==(UTF8String left, UTF8String right)
	{
		return CompareTo(left, right) == 0;
	}

	public static bool operator ==(UTF8String left, string right)
	{
		return ToSystemString(left) == right;
	}

	public static bool operator ==(string left, UTF8String right)
	{
		return left == ToSystemString(right);
	}

	public static bool operator !=(UTF8String left, UTF8String right)
	{
		return CompareTo(left, right) != 0;
	}

	public static bool operator !=(UTF8String left, string right)
	{
		return ToSystemString(left) != right;
	}

	public static bool operator !=(string left, UTF8String right)
	{
		return left != ToSystemString(right);
	}

	public static bool operator >(UTF8String left, UTF8String right)
	{
		return CompareTo(left, right) > 0;
	}

	public static bool operator <(UTF8String left, UTF8String right)
	{
		return CompareTo(left, right) < 0;
	}

	public static bool operator >=(UTF8String left, UTF8String right)
	{
		return CompareTo(left, right) >= 0;
	}

	public static bool operator <=(UTF8String left, UTF8String right)
	{
		return CompareTo(left, right) <= 0;
	}

	public UTF8String(byte[] data)
	{
		this.data = data;
	}

	public UTF8String(string s)
		: this((s == null) ? null : Encoding.UTF8.GetBytes(s))
	{
	}

	private static string ConvertFromUTF8(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		try
		{
			return Encoding.UTF8.GetString(data);
		}
		catch
		{
		}
		return null;
	}

	public static bool Equals(UTF8String a, UTF8String b)
	{
		return CompareTo(a, b) == 0;
	}

	public bool Equals(UTF8String other)
	{
		return CompareTo(this, other) == 0;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UTF8String b))
		{
			return false;
		}
		return CompareTo(this, b) == 0;
	}

	public bool Contains(string value)
	{
		return String.Contains(value);
	}

	public bool EndsWith(string value)
	{
		return String.EndsWith(value);
	}

	public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
	{
		return String.EndsWith(value, ignoreCase, culture);
	}

	public bool EndsWith(string value, StringComparison comparisonType)
	{
		return String.EndsWith(value, comparisonType);
	}

	public bool StartsWith(string value)
	{
		return String.StartsWith(value);
	}

	public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
	{
		return String.StartsWith(value, ignoreCase, culture);
	}

	public bool StartsWith(string value, StringComparison comparisonType)
	{
		return String.StartsWith(value, comparisonType);
	}

	public int CompareTo(string strB)
	{
		return String.CompareTo(strB);
	}

	public int IndexOf(char value)
	{
		return String.IndexOf(value);
	}

	public int IndexOf(char value, int startIndex)
	{
		return String.IndexOf(value, startIndex);
	}

	public int IndexOf(char value, int startIndex, int count)
	{
		return String.IndexOf(value, startIndex, count);
	}

	public int IndexOf(string value)
	{
		return String.IndexOf(value);
	}

	public int IndexOf(string value, int startIndex)
	{
		return String.IndexOf(value, startIndex);
	}

	public int IndexOf(string value, int startIndex, int count)
	{
		return String.IndexOf(value, startIndex, count);
	}

	public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		return String.IndexOf(value, startIndex, count, comparisonType);
	}

	public int IndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		return String.IndexOf(value, startIndex, comparisonType);
	}

	public int IndexOf(string value, StringComparison comparisonType)
	{
		return String.IndexOf(value, comparisonType);
	}

	public int LastIndexOf(char value)
	{
		return String.LastIndexOf(value);
	}

	public int LastIndexOf(char value, int startIndex)
	{
		return String.LastIndexOf(value, startIndex);
	}

	public int LastIndexOf(char value, int startIndex, int count)
	{
		return String.LastIndexOf(value, startIndex, count);
	}

	public int LastIndexOf(string value)
	{
		return String.LastIndexOf(value);
	}

	public int LastIndexOf(string value, int startIndex)
	{
		return String.LastIndexOf(value, startIndex);
	}

	public int LastIndexOf(string value, int startIndex, int count)
	{
		return String.LastIndexOf(value, startIndex, count);
	}

	public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		return String.LastIndexOf(value, startIndex, count, comparisonType);
	}

	public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		return String.LastIndexOf(value, startIndex, comparisonType);
	}

	public int LastIndexOf(string value, StringComparison comparisonType)
	{
		return String.LastIndexOf(value, comparisonType);
	}

	public UTF8String Insert(int startIndex, string value)
	{
		return new UTF8String(String.Insert(startIndex, value));
	}

	public UTF8String Remove(int startIndex)
	{
		return new UTF8String(String.Remove(startIndex));
	}

	public UTF8String Remove(int startIndex, int count)
	{
		return new UTF8String(String.Remove(startIndex, count));
	}

	public UTF8String Replace(char oldChar, char newChar)
	{
		return new UTF8String(String.Replace(oldChar, newChar));
	}

	public UTF8String Replace(string oldValue, string newValue)
	{
		return new UTF8String(String.Replace(oldValue, newValue));
	}

	public UTF8String Substring(int startIndex)
	{
		return new UTF8String(String.Substring(startIndex));
	}

	public UTF8String Substring(int startIndex, int length)
	{
		return new UTF8String(String.Substring(startIndex, length));
	}

	public UTF8String ToLower()
	{
		return new UTF8String(String.ToLower());
	}

	public UTF8String ToLower(CultureInfo culture)
	{
		return new UTF8String(String.ToLower(culture));
	}

	public UTF8String ToLowerInvariant()
	{
		return new UTF8String(String.ToLowerInvariant());
	}

	public UTF8String ToUpper()
	{
		return new UTF8String(String.ToUpper());
	}

	public UTF8String ToUpper(CultureInfo culture)
	{
		return new UTF8String(String.ToUpper(culture));
	}

	public UTF8String ToUpperInvariant()
	{
		return new UTF8String(String.ToUpperInvariant());
	}

	public UTF8String Trim()
	{
		return new UTF8String(String.Trim());
	}

	public override int GetHashCode()
	{
		return GetHashCode(this);
	}

	public override string ToString()
	{
		return String;
	}
}
