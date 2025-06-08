namespace System.Net.Http;

internal static class RtcRequestMessageExtensions
{
	internal static extern void SetRtcOptions(this HttpRequestMessage request, HttpWebRequest webRequest);

	internal static extern void MarkRtcFlushComplete(this HttpRequestMessage request);

	internal static extern void AbortRtcRequest(this HttpRequestMessage request);
}
