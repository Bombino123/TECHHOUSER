using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal static class StringUtil
{
	internal delegate string ToStringConverter<T>(T value);

	private const string s_defaultDelimiter = ", ";

	internal static string BuildDelimitedList<T>(IEnumerable<T> values, ToStringConverter<T> converter, string delimiter)
	{
		if (values == null)
		{
			return string.Empty;
		}
		if (converter == null)
		{
			converter = InvariantConvertToString;
		}
		if (delimiter == null)
		{
			delimiter = ", ";
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (T value in values)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(delimiter);
			}
			stringBuilder.Append(converter(value));
		}
		return stringBuilder.ToString();
	}

	internal static string ToCommaSeparatedString(IEnumerable list)
	{
		return ToSeparatedString(list, ", ", string.Empty);
	}

	internal static string ToSeparatedString(IEnumerable list, string separator, string nullValue)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToSeparatedString(stringBuilder, list, separator, nullValue);
		return stringBuilder.ToString();
	}

	internal static string ToCommaSeparatedStringSorted(IEnumerable list)
	{
		return ToSeparatedStringSorted(list, ", ", string.Empty);
	}

	internal static string ToSeparatedStringSorted(IEnumerable list, string separator, string nullValue)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToSeparatedStringPrivate(stringBuilder, list, separator, nullValue, toSort: true);
		return stringBuilder.ToString();
	}

	internal static string MembersToCommaSeparatedString(IEnumerable members)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		ToCommaSeparatedString(stringBuilder, members);
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	internal static void ToCommaSeparatedString(StringBuilder builder, IEnumerable list)
	{
		ToSeparatedStringPrivate(builder, list, ", ", string.Empty, toSort: false);
	}

	internal static void ToCommaSeparatedStringSorted(StringBuilder builder, IEnumerable list)
	{
		ToSeparatedStringPrivate(builder, list, ", ", string.Empty, toSort: true);
	}

	internal static void ToSeparatedString(StringBuilder builder, IEnumerable list, string separator)
	{
		ToSeparatedStringPrivate(builder, list, separator, string.Empty, toSort: false);
	}

	internal static void ToSeparatedStringSorted(StringBuilder builder, IEnumerable list, string separator)
	{
		ToSeparatedStringPrivate(builder, list, separator, string.Empty, toSort: true);
	}

	internal static void ToSeparatedString(StringBuilder stringBuilder, IEnumerable list, string separator, string nullValue)
	{
		ToSeparatedStringPrivate(stringBuilder, list, separator, nullValue, toSort: false);
	}

	private static void ToSeparatedStringPrivate(StringBuilder stringBuilder, IEnumerable list, string separator, string nullValue, bool toSort)
	{
		if (list == null)
		{
			return;
		}
		bool flag = true;
		List<string> list2 = new List<string>();
		foreach (object item2 in list)
		{
			string item = ((item2 != null) ? FormatInvariant("{0}", item2) : nullValue);
			list2.Add(item);
		}
		if (toSort)
		{
			list2.Sort(StringComparer.Ordinal);
		}
		foreach (string item3 in list2)
		{
			if (!flag)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(item3);
			flag = false;
		}
	}

	internal static string FormatInvariant(string format, params object[] args)
	{
		return string.Format(CultureInfo.InvariantCulture, format, args);
	}

	internal static StringBuilder FormatStringBuilder(StringBuilder builder, string format, params object[] args)
	{
		builder.AppendFormat(CultureInfo.InvariantCulture, format, args);
		return builder;
	}

	internal static StringBuilder IndentNewLine(StringBuilder builder, int indent)
	{
		builder.AppendLine();
		for (int i = 0; i < indent; i++)
		{
			builder.Append("    ");
		}
		return builder;
	}

	internal static string FormatIndex(string arrayVarName, int index)
	{
		return new StringBuilder(arrayVarName.Length + 10 + 2).Append(arrayVarName).Append('[').Append(index)
			.Append(']')
			.ToString();
	}

	private static string InvariantConvertToString<T>(T value)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { value });
	}
}
