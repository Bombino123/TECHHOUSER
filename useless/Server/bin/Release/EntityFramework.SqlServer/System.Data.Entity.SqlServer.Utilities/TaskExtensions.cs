using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.SqlServer.Utilities;

public static class TaskExtensions
{
	public struct CultureAwaiter<T> : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly Task<T> _task;

		public bool IsCompleted => _task.IsCompleted;

		public CultureAwaiter(Task<T> task)
		{
			_task = task;
		}

		public CultureAwaiter<T> GetAwaiter()
		{
			return this;
		}

		public T GetResult()
		{
			return _task.GetAwaiter().GetResult();
		}

		public void OnCompleted(Action continuation)
		{
			throw new NotImplementedException();
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			_task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().UnsafeOnCompleted(delegate
			{
				CultureInfo currentCulture2 = Thread.CurrentThread.CurrentCulture;
				CultureInfo currentUICulture2 = Thread.CurrentThread.CurrentUICulture;
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
				try
				{
					continuation();
				}
				finally
				{
					Thread.CurrentThread.CurrentCulture = currentCulture2;
					Thread.CurrentThread.CurrentUICulture = currentUICulture2;
				}
			});
		}
	}

	public struct CultureAwaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly Task _task;

		public bool IsCompleted => _task.IsCompleted;

		public CultureAwaiter(Task task)
		{
			_task = task;
		}

		public CultureAwaiter GetAwaiter()
		{
			return this;
		}

		public void GetResult()
		{
			_task.GetAwaiter().GetResult();
		}

		public void OnCompleted(Action continuation)
		{
			throw new NotImplementedException();
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			_task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().UnsafeOnCompleted(delegate
			{
				CultureInfo currentCulture2 = Thread.CurrentThread.CurrentCulture;
				CultureInfo currentUICulture2 = Thread.CurrentThread.CurrentUICulture;
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
				try
				{
					continuation();
				}
				finally
				{
					Thread.CurrentThread.CurrentCulture = currentCulture2;
					Thread.CurrentThread.CurrentUICulture = currentUICulture2;
				}
			});
		}
	}

	public static CultureAwaiter<T> WithCurrentCulture<T>(this Task<T> task)
	{
		return new CultureAwaiter<T>(task);
	}

	public static CultureAwaiter WithCurrentCulture(this Task task)
	{
		return new CultureAwaiter(task);
	}
}
