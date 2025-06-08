using System;

namespace dnlib.DotNet;

internal static class Utils
{
	internal static string ToHex(byte[] bytes, bool upper)
	{
		if (bytes == null)
		{
			return "";
		}
		char[] array = new char[bytes.Length * 2];
		int i = 0;
		int num = 0;
		for (; i < bytes.Length; i++)
		{
			byte b = bytes[i];
			array[num++] = ToHexChar(b >> 4, upper);
			array[num++] = ToHexChar(b & 0xF, upper);
		}
		return new string(array);
	}

	private static char ToHexChar(int val, bool upper)
	{
		if (0 <= val && val <= 9)
		{
			return (char)(val + 48);
		}
		return (char)(val - 10 + (upper ? 65 : 97));
	}

	internal static byte[] ParseBytes(string hexString)
	{
		try
		{
			if (hexString.Length % 2 != 0)
			{
				return null;
			}
			byte[] array = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
			{
				int num = TryParseHexChar(hexString[i]);
				int num2 = TryParseHexChar(hexString[i + 1]);
				if (num < 0 || num2 < 0)
				{
					return null;
				}
				array[i / 2] = (byte)((num << 4) | num2);
			}
			return array;
		}
		catch
		{
			return null;
		}
	}

	private static int TryParseHexChar(char c)
	{
		if ('0' <= c && c <= '9')
		{
			return c - 48;
		}
		if ('a' <= c && c <= 'f')
		{
			return 10 + c - 97;
		}
		if ('A' <= c && c <= 'F')
		{
			return 10 + c - 65;
		}
		return -1;
	}

	internal static int CompareTo(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return 0;
		}
		if (a == null)
		{
			return -1;
		}
		if (b == null)
		{
			return 1;
		}
		int num = Math.Min(a.Length, b.Length);
		for (int i = 0; i < num; i++)
		{
			byte b2 = a[i];
			byte b3 = b[i];
			if (b2 < b3)
			{
				return -1;
			}
			if (b2 > b3)
			{
				return 1;
			}
		}
		return a.Length.CompareTo(b.Length);
	}

	internal static bool Equals(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static int GetHashCode(byte[] a)
	{
		if (a == null || a.Length == 0)
		{
			return 0;
		}
		int num = Math.Min(a.Length / 2, 20);
		if (num == 0)
		{
			num = 1;
		}
		uint num2 = 0u;
		int num3 = 0;
		int num4 = a.Length - 1;
		while (num3 < num)
		{
			num2 ^= (uint)(a[num3] | (a[num4] << 8));
			num2 = (num2 << 13) | (num2 >> 19);
			num3++;
			num4--;
		}
		return (int)num2;
	}

	internal static int CompareTo(Version a, Version b)
	{
		if ((object)a == null)
		{
			a = new Version();
		}
		if ((object)b == null)
		{
			b = new Version();
		}
		if (a.Major != b.Major)
		{
			return a.Major.CompareTo(b.Major);
		}
		if (a.Minor != b.Minor)
		{
			return a.Minor.CompareTo(b.Minor);
		}
		if (GetDefaultVersionValue(a.Build) != GetDefaultVersionValue(b.Build))
		{
			return GetDefaultVersionValue(a.Build).CompareTo(GetDefaultVersionValue(b.Build));
		}
		return GetDefaultVersionValue(a.Revision).CompareTo(GetDefaultVersionValue(b.Revision));
	}

	internal static bool Equals(Version a, Version b)
	{
		return CompareTo(a, b) == 0;
	}

	internal static Version CreateVersionWithNoUndefinedValues(Version a)
	{
		if ((object)a == null)
		{
			return new Version(0, 0, 0, 0);
		}
		return new Version(a.Major, a.Minor, GetDefaultVersionValue(a.Build), GetDefaultVersionValue(a.Revision));
	}

	private static int GetDefaultVersionValue(int val)
	{
		if (val != -1)
		{
			return val;
		}
		return 0;
	}

	internal static Version ParseVersion(string versionString)
	{
		try
		{
			return CreateVersionWithNoUndefinedValues(new Version(versionString));
		}
		catch
		{
			return null;
		}
	}

	internal static int LocaleCompareTo(UTF8String a, UTF8String b)
	{
		return GetCanonicalLocale(a).CompareTo(GetCanonicalLocale(b));
	}

	internal static bool LocaleEquals(UTF8String a, UTF8String b)
	{
		return LocaleCompareTo(a, b) == 0;
	}

	internal static int LocaleCompareTo(UTF8String a, string b)
	{
		return GetCanonicalLocale(a).CompareTo(GetCanonicalLocale(b));
	}

	internal static bool LocaleEquals(UTF8String a, string b)
	{
		return LocaleCompareTo(a, b) == 0;
	}

	internal static int GetHashCodeLocale(UTF8String a)
	{
		return GetCanonicalLocale(a).GetHashCode();
	}

	private static string GetCanonicalLocale(UTF8String locale)
	{
		return GetCanonicalLocale(UTF8String.ToSystemStringOrEmpty(locale));
	}

	private static string GetCanonicalLocale(string locale)
	{
		string text = locale.ToUpperInvariant();
		if (text == "NEUTRAL")
		{
			text = string.Empty;
		}
		return text;
	}

	public static uint AlignUp(uint v, uint alignment)
	{
		return (v + alignment - 1) & ~(alignment - 1);
	}

	public static int AlignUp(int v, uint alignment)
	{
		return (int)AlignUp((uint)v, alignment);
	}

	public static uint RoundToNextPowerOfTwo(uint num)
	{
		num--;
		num |= num >> 1;
		num |= num >> 2;
		num |= num >> 4;
		num |= num >> 8;
		num |= num >> 16;
		return num + 1;
	}
}
