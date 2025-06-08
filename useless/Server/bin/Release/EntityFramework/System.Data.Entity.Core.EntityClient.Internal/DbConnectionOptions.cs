using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.EntityClient.Internal;

internal class DbConnectionOptions
{
	private enum ParserState
	{
		NothingYet = 1,
		Key,
		KeyEqual,
		KeyEnd,
		UnquotedValue,
		DoubleQuoteValue,
		DoubleQuoteValueQuote,
		SingleQuoteValue,
		SingleQuoteValueQuote,
		QuotedValueEnd,
		NullTermination
	}

	internal const string DataDirectory = "|datadirectory|";

	private readonly string _usersConnectionString;

	private readonly Dictionary<string, string> _parsetable = new Dictionary<string, string>();

	internal readonly NameValuePair KeyChain;

	internal string UsersConnectionString => _usersConnectionString ?? string.Empty;

	internal bool IsEmpty => KeyChain == null;

	internal Dictionary<string, string> Parsetable => _parsetable;

	internal virtual string this[string keyword]
	{
		get
		{
			_parsetable.TryGetValue(keyword, out var value);
			return value;
		}
	}

	internal DbConnectionOptions()
	{
	}

	internal DbConnectionOptions(string connectionString, IList<string> validKeywords)
	{
		_usersConnectionString = connectionString ?? "";
		if (0 < _usersConnectionString.Length)
		{
			KeyChain = ParseInternal(_parsetable, _usersConnectionString, validKeywords);
		}
	}

	private static string GetKeyName(StringBuilder buffer)
	{
		int num = buffer.Length;
		while (0 < num && char.IsWhiteSpace(buffer[num - 1]))
		{
			num--;
		}
		return buffer.ToString(0, num).ToLowerInvariant();
	}

	private static string GetKeyValue(StringBuilder buffer, bool trimWhitespace)
	{
		int num = buffer.Length;
		int i = 0;
		if (trimWhitespace)
		{
			for (; i < num && char.IsWhiteSpace(buffer[i]); i++)
			{
			}
			while (0 < num && char.IsWhiteSpace(buffer[num - 1]))
			{
				num--;
			}
		}
		return buffer.ToString(i, num - i);
	}

	private static int GetKeyValuePair(string connectionString, int currentPosition, StringBuilder buffer, out string keyname, out string keyvalue)
	{
		int num = currentPosition;
		buffer.Length = 0;
		keyname = null;
		keyvalue = null;
		char c = '\0';
		ParserState parserState = ParserState.NothingYet;
		for (int length = connectionString.Length; currentPosition < length; currentPosition++)
		{
			c = connectionString[currentPosition];
			switch (parserState)
			{
			case ParserState.NothingYet:
				if (';' == c || char.IsWhiteSpace(c))
				{
					continue;
				}
				if (c == '\0')
				{
					parserState = ParserState.NullTermination;
					continue;
				}
				if (char.IsControl(c))
				{
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				num = currentPosition;
				if ('=' != c)
				{
					parserState = ParserState.Key;
					goto IL_0257;
				}
				parserState = ParserState.KeyEqual;
				continue;
			case ParserState.Key:
				if ('=' == c)
				{
					parserState = ParserState.KeyEqual;
					continue;
				}
				if (!char.IsWhiteSpace(c) && char.IsControl(c))
				{
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				goto IL_0257;
			case ParserState.KeyEqual:
				if ('=' == c)
				{
					parserState = ParserState.Key;
					goto IL_0257;
				}
				keyname = GetKeyName(buffer);
				if (string.IsNullOrEmpty(keyname))
				{
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				buffer.Length = 0;
				parserState = ParserState.KeyEnd;
				goto case ParserState.KeyEnd;
			case ParserState.KeyEnd:
				if (char.IsWhiteSpace(c))
				{
					continue;
				}
				if ('\'' == c)
				{
					parserState = ParserState.SingleQuoteValue;
					continue;
				}
				if ('"' == c)
				{
					parserState = ParserState.DoubleQuoteValue;
					continue;
				}
				if (';' == c || c == '\0')
				{
					break;
				}
				if (char.IsControl(c))
				{
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				parserState = ParserState.UnquotedValue;
				goto IL_0257;
			case ParserState.UnquotedValue:
				if (!char.IsWhiteSpace(c) && (char.IsControl(c) || ';' == c))
				{
					break;
				}
				goto IL_0257;
			case ParserState.DoubleQuoteValue:
				if ('"' == c)
				{
					parserState = ParserState.DoubleQuoteValueQuote;
					continue;
				}
				if (c == '\0')
				{
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				goto IL_0257;
			case ParserState.DoubleQuoteValueQuote:
				if ('"' == c)
				{
					parserState = ParserState.DoubleQuoteValue;
					goto IL_0257;
				}
				keyvalue = GetKeyValue(buffer, trimWhitespace: false);
				parserState = ParserState.QuotedValueEnd;
				goto case ParserState.QuotedValueEnd;
			case ParserState.SingleQuoteValue:
				if ('\'' == c)
				{
					parserState = ParserState.SingleQuoteValueQuote;
					continue;
				}
				if (c == '\0')
				{
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				goto IL_0257;
			case ParserState.SingleQuoteValueQuote:
				if ('\'' == c)
				{
					parserState = ParserState.SingleQuoteValue;
					goto IL_0257;
				}
				keyvalue = GetKeyValue(buffer, trimWhitespace: false);
				parserState = ParserState.QuotedValueEnd;
				goto case ParserState.QuotedValueEnd;
			case ParserState.QuotedValueEnd:
				if (char.IsWhiteSpace(c))
				{
					continue;
				}
				if (';' != c)
				{
					if (c == '\0')
					{
						parserState = ParserState.NullTermination;
						continue;
					}
					throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
				}
				break;
			case ParserState.NullTermination:
				if (c == '\0' || char.IsWhiteSpace(c))
				{
					continue;
				}
				throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(currentPosition));
			default:
				{
					throw new InvalidOperationException(Strings.ADP_InternalProviderError(1015));
				}
				IL_0257:
				buffer.Append(c);
				continue;
			}
			break;
		}
		switch (parserState)
		{
		case ParserState.Key:
		case ParserState.DoubleQuoteValue:
		case ParserState.SingleQuoteValue:
			throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
		case ParserState.KeyEqual:
			keyname = GetKeyName(buffer);
			if (string.IsNullOrEmpty(keyname))
			{
				throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
			}
			break;
		case ParserState.UnquotedValue:
		{
			keyvalue = GetKeyValue(buffer, trimWhitespace: true);
			char c2 = keyvalue[keyvalue.Length - 1];
			if ('\'' == c2 || '"' == c2)
			{
				throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(num));
			}
			break;
		}
		case ParserState.DoubleQuoteValueQuote:
		case ParserState.SingleQuoteValueQuote:
		case ParserState.QuotedValueEnd:
			keyvalue = GetKeyValue(buffer, trimWhitespace: false);
			break;
		default:
			throw new InvalidOperationException(Strings.ADP_InternalProviderError(1016));
		case ParserState.NothingYet:
		case ParserState.KeyEnd:
		case ParserState.NullTermination:
			break;
		}
		if (';' == c && currentPosition < connectionString.Length)
		{
			currentPosition++;
		}
		return currentPosition;
	}

	private static NameValuePair ParseInternal(IDictionary<string, string> parsetable, string connectionString, IList<string> validKeywords)
	{
		StringBuilder buffer = new StringBuilder();
		NameValuePair nameValuePair = null;
		NameValuePair result = null;
		int num = 0;
		int length = connectionString.Length;
		while (num < length)
		{
			int currentPosition = num;
			num = GetKeyValuePair(connectionString, currentPosition, buffer, out var keyname, out var keyvalue);
			if (string.IsNullOrEmpty(keyname))
			{
				break;
			}
			if (!validKeywords.Contains(keyname))
			{
				throw new ArgumentException(Strings.ADP_KeywordNotSupported(keyname));
			}
			parsetable[keyname] = keyvalue;
			if (nameValuePair != null)
			{
				NameValuePair nameValuePair3 = (nameValuePair.Next = new NameValuePair());
				nameValuePair = nameValuePair3;
			}
			else
			{
				result = (nameValuePair = new NameValuePair());
			}
		}
		return result;
	}
}
