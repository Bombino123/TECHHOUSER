using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class QuotedStringUtils
{
	public static string Quote(string str)
	{
		return $"\"{str}\"";
	}

	public static string Unquote(string str)
	{
		string value = '"'.ToString();
		if (str.Length >= 2 && str.StartsWith(value) && str.EndsWith(value))
		{
			return str.Substring(1, str.Length - 2);
		}
		return str;
	}

	public static bool IsQuoted(string str)
	{
		string value = '"'.ToString();
		if (str.Length >= 2 && str.StartsWith(value) && str.EndsWith(value))
		{
			return true;
		}
		return false;
	}

	public static int IndexOfUnquotedChar(string str, char charToFind)
	{
		return IndexOfUnquotedChar(str, charToFind, 0);
	}

	public static int IndexOfUnquotedChar(string str, char charToFind, int startIndex)
	{
		if (startIndex >= str.Length)
		{
			return -1;
		}
		bool flag = false;
		for (int i = startIndex; i < str.Length; i++)
		{
			if (str[i] == '"')
			{
				flag = !flag;
			}
			else if (!flag && str[i] == charToFind)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOfUnquotedString(string str, string stringToFind)
	{
		return IndexOfUnquotedString(str, stringToFind, 0);
	}

	public static int IndexOfUnquotedString(string str, string stringToFind, int startIndex)
	{
		if (startIndex >= str.Length)
		{
			return -1;
		}
		bool flag = false;
		for (int i = startIndex; i < str.Length; i++)
		{
			if (str[i] == '"')
			{
				flag = !flag;
			}
			else if (!flag && str.Substring(i).StartsWith(stringToFind))
			{
				return i;
			}
		}
		return -1;
	}

	public static List<string> SplitIgnoreQuotedSeparators(string str, char separator)
	{
		return SplitIgnoreQuotedSeparators(str, separator, StringSplitOptions.None);
	}

	public static List<string> SplitIgnoreQuotedSeparators(string str, char separator, StringSplitOptions options)
	{
		List<string> list = new List<string>();
		int num = 0;
		for (int num2 = IndexOfUnquotedChar(str, separator); num2 >= num; num2 = IndexOfUnquotedChar(str, separator, num))
		{
			string text = str.Substring(num, num2 - num);
			if (options != StringSplitOptions.RemoveEmptyEntries || text != string.Empty)
			{
				list.Add(text);
			}
			num = num2 + 1;
		}
		string text2 = str.Substring(num);
		if (options != StringSplitOptions.RemoveEmptyEntries || text2 != string.Empty)
		{
			list.Add(text2);
		}
		return list;
	}
}
