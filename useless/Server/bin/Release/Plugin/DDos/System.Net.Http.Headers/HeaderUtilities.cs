using System.Collections.Generic;

namespace System.Net.Http.Headers;

internal static class HeaderUtilities
{
	internal const string ConnectionClose = "close";

	internal static readonly TransferCodingHeaderValue TransferEncodingChunked;

	internal static readonly NameValueWithParametersHeaderValue ExpectContinue;

	internal const string BytesUnit = "bytes";

	internal static readonly Action<HttpHeaderValueCollection<string>, string> TokenValidator;

	internal static extern void SetQuality(ICollection<NameValueHeaderValue> parameters, double? value);

	internal static extern double? GetQuality(ICollection<NameValueHeaderValue> parameters);

	internal static extern void CheckValidToken(string value, string parameterName);

	internal static extern void CheckValidComment(string value, string parameterName);

	internal static extern void CheckValidQuotedString(string value, string parameterName);

	internal static extern bool AreEqualCollections<T>(ICollection<T> x, ICollection<T> y);

	internal static extern bool AreEqualCollections<T>(ICollection<T> x, ICollection<T> y, IEqualityComparer<T> comparer);

	internal static extern int GetNextNonEmptyOrWhitespaceIndex(string input, int startIndex, bool skipEmptyValues, out bool separatorFound);

	internal static extern DateTimeOffset? GetDateTimeOffsetValue(string headerName, HttpHeaders store);

	internal static extern TimeSpan? GetTimeSpanValue(string headerName, HttpHeaders store);

	internal static extern bool TryParseInt32(string value, out int result);

	internal static extern bool TryParseInt64(string value, out long result);

	internal static extern string DumpHeaders(params HttpHeaders[] headers);

	internal static extern bool IsValidEmailAddress(string value);
}
