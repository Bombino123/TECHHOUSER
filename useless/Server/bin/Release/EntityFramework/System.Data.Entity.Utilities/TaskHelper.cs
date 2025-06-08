using System.Threading.Tasks;

namespace System.Data.Entity.Utilities;

internal static class TaskHelper
{
	internal static Task<T> FromException<T>(Exception ex)
	{
		TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
		taskCompletionSource.SetException(ex);
		return taskCompletionSource.Task;
	}

	internal static Task<T> FromCancellation<T>()
	{
		TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
		taskCompletionSource.SetCanceled();
		return taskCompletionSource.Task;
	}
}
