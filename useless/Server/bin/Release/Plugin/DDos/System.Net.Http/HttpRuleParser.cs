using System.Text;

namespace System.Net.Http;

internal static class HttpRuleParser
{
	internal const char CR = '\r';

	internal const char LF = '\n';

	internal const int MaxInt64Digits = 19;

	internal const int MaxInt32Digits = 10;

	internal static readonly Encoding DefaultHttpEncoding;

	static extern HttpRuleParser();

	internal static extern bool IsTokenChar(char character);

	internal static extern int GetTokenLength(string input, int startIndex);

	internal static extern int GetWhitespaceLength(string input, int startIndex);

	internal static extern bool ContainsInvalidNewLine(string value);

	internal static extern bool ContainsInvalidNewLine(string value, int startIndex);

	internal static extern int GetNumberLength(string input, int startIndex, bool allowDecimal);

	internal static extern int GetHostLength(string input, int startIndex, bool allowToken, out string host);

	internal static extern HttpParseResult GetCommentLength(string input, int startIndex, out int length);

	internal static extern HttpParseResult GetQuotedStringLength(string input, int startIndex, out int length);

	internal static extern HttpParseResult GetQuotedPairLength(string input, int startIndex, out int length);

	internal static extern string DateToString(DateTimeOffset dateTime);

	internal static extern bool TryStringToDate(string input, out DateTimeOffset result);
}
