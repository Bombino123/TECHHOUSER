using System.Threading.Tasks;

namespace System.Net.Http;

internal static class HttpUtilities
{
	internal static readonly Version DefaultVersion;

	internal static readonly byte[] EmptyByteArray;

	internal static extern bool IsHttpUri(Uri uri);

	internal static extern bool HandleFaultsAndCancelation<T>(Task task, TaskCompletionSource<T> tcs);

	internal static extern Task ContinueWithStandard(this Task task, Action<Task> continuation);

	internal static extern Task ContinueWithStandard<T>(this Task<T> task, Action<Task<T>> continuation);
}
