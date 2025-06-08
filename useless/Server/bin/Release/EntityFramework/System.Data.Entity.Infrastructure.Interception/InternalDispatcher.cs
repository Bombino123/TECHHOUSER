using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

internal class InternalDispatcher<TInterceptor> where TInterceptor : class, IDbInterceptor
{
	private volatile List<TInterceptor> _interceptors = new List<TInterceptor>();

	private readonly object _lock = new object();

	public void Add(IDbInterceptor interceptor)
	{
		if (!(interceptor is TInterceptor item))
		{
			return;
		}
		lock (_lock)
		{
			List<TInterceptor> list = _interceptors.ToList();
			list.Add(item);
			_interceptors = list;
		}
	}

	public void Remove(IDbInterceptor interceptor)
	{
		if (!(interceptor is TInterceptor item))
		{
			return;
		}
		lock (_lock)
		{
			List<TInterceptor> list = _interceptors.ToList();
			list.Remove(item);
			_interceptors = list;
		}
	}

	public TResult Dispatch<TResult>(TResult result, Func<TResult, TInterceptor, TResult> accumulator)
	{
		if (_interceptors.Count != 0)
		{
			return _interceptors.Aggregate(result, accumulator);
		}
		return result;
	}

	public void Dispatch(Action<TInterceptor> action)
	{
		if (_interceptors.Count != 0)
		{
			_interceptors.Each(action);
		}
	}

	public TResult Dispatch<TInterceptionContext, TResult>(TResult result, TInterceptionContext interceptionContext, Action<TInterceptor, TInterceptionContext> intercept) where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
	{
		if (_interceptors.Count == 0)
		{
			return result;
		}
		interceptionContext.MutableData.SetExecuted(result);
		foreach (TInterceptor interceptor in _interceptors)
		{
			intercept(interceptor, interceptionContext);
		}
		if (interceptionContext.MutableData.Exception != null)
		{
			throw interceptionContext.MutableData.Exception;
		}
		return interceptionContext.MutableData.Result;
	}

	public void Dispatch<TTarget, TInterceptionContext>(TTarget target, Action<TTarget, TInterceptionContext> operation, TInterceptionContext interceptionContext, Action<TInterceptor, TTarget, TInterceptionContext> executing, Action<TInterceptor, TTarget, TInterceptionContext> executed) where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext
	{
		if (_interceptors.Count == 0)
		{
			operation(target, interceptionContext);
			return;
		}
		foreach (TInterceptor interceptor in _interceptors)
		{
			executing(interceptor, target, interceptionContext);
		}
		if (!interceptionContext.MutableData.IsExecutionSuppressed)
		{
			try
			{
				operation(target, interceptionContext);
				interceptionContext.MutableData.HasExecuted = true;
			}
			catch (Exception ex)
			{
				interceptionContext.MutableData.SetExceptionThrown(ex);
				foreach (TInterceptor interceptor2 in _interceptors)
				{
					executed(interceptor2, target, interceptionContext);
				}
				if (interceptionContext.MutableData.Exception == ex)
				{
					throw;
				}
			}
		}
		if (interceptionContext.MutableData.OriginalException == null)
		{
			foreach (TInterceptor interceptor3 in _interceptors)
			{
				executed(interceptor3, target, interceptionContext);
			}
		}
		if (interceptionContext.MutableData.Exception == null)
		{
			return;
		}
		throw interceptionContext.MutableData.Exception;
	}

	public TResult Dispatch<TTarget, TInterceptionContext, TResult>(TTarget target, Func<TTarget, TInterceptionContext, TResult> operation, TInterceptionContext interceptionContext, Action<TInterceptor, TTarget, TInterceptionContext> executing, Action<TInterceptor, TTarget, TInterceptionContext> executed) where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
	{
		if (_interceptors.Count == 0)
		{
			return operation(target, interceptionContext);
		}
		foreach (TInterceptor interceptor in _interceptors)
		{
			executing(interceptor, target, interceptionContext);
		}
		if (!interceptionContext.MutableData.IsExecutionSuppressed)
		{
			try
			{
				interceptionContext.MutableData.SetExecuted(operation(target, interceptionContext));
			}
			catch (Exception ex)
			{
				interceptionContext.MutableData.SetExceptionThrown(ex);
				foreach (TInterceptor interceptor2 in _interceptors)
				{
					executed(interceptor2, target, interceptionContext);
				}
				if (interceptionContext.MutableData.Exception == ex)
				{
					throw;
				}
			}
		}
		if (interceptionContext.MutableData.OriginalException == null)
		{
			foreach (TInterceptor interceptor3 in _interceptors)
			{
				executed(interceptor3, target, interceptionContext);
			}
		}
		if (interceptionContext.MutableData.Exception != null)
		{
			throw interceptionContext.MutableData.Exception;
		}
		return interceptionContext.MutableData.Result;
	}

	public Task DispatchAsync<TTarget, TInterceptionContext>(TTarget target, Func<TTarget, TInterceptionContext, CancellationToken, Task> operation, TInterceptionContext interceptionContext, Action<TInterceptor, TTarget, TInterceptionContext> executing, Action<TInterceptor, TTarget, TInterceptionContext> executed, CancellationToken cancellationToken) where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext
	{
		if (_interceptors.Count == 0)
		{
			return operation(target, interceptionContext, cancellationToken);
		}
		foreach (TInterceptor interceptor in _interceptors)
		{
			executing(interceptor, target, interceptionContext);
		}
		Task obj = (interceptionContext.MutableData.IsExecutionSuppressed ? Task.FromResult<object>(null) : operation(target, interceptionContext, cancellationToken));
		TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
		obj.ContinueWith(delegate(Task t)
		{
			interceptionContext.MutableData.TaskStatus = t.Status;
			if (t.IsFaulted)
			{
				interceptionContext.MutableData.SetExceptionThrown(t.Exception.InnerException);
			}
			else if (!interceptionContext.MutableData.IsExecutionSuppressed)
			{
				interceptionContext.MutableData.HasExecuted = true;
			}
			try
			{
				foreach (TInterceptor interceptor2 in _interceptors)
				{
					executed(interceptor2, target, interceptionContext);
				}
			}
			catch (Exception exception)
			{
				interceptionContext.MutableData.Exception = exception;
			}
			if (interceptionContext.MutableData.Exception != null)
			{
				tcs.SetException(interceptionContext.MutableData.Exception);
			}
			else if (t.IsCanceled)
			{
				tcs.SetCanceled();
			}
			else
			{
				tcs.SetResult(null);
			}
		}, TaskContinuationOptions.ExecuteSynchronously);
		return tcs.Task;
	}

	public Task<TResult> DispatchAsync<TTarget, TInterceptionContext, TResult>(TTarget target, Func<TTarget, TInterceptionContext, CancellationToken, Task<TResult>> operation, TInterceptionContext interceptionContext, Action<TInterceptor, TTarget, TInterceptionContext> executing, Action<TInterceptor, TTarget, TInterceptionContext> executed, CancellationToken cancellationToken) where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_interceptors.Count == 0)
		{
			return operation(target, interceptionContext, cancellationToken);
		}
		foreach (TInterceptor interceptor in _interceptors)
		{
			executing(interceptor, target, interceptionContext);
		}
		Task<TResult> obj = (interceptionContext.MutableData.IsExecutionSuppressed ? Task.FromResult(interceptionContext.MutableData.Result) : operation(target, interceptionContext, cancellationToken));
		TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
		obj.ContinueWith(delegate(Task<TResult> t)
		{
			interceptionContext.MutableData.TaskStatus = t.Status;
			if (t.IsFaulted)
			{
				interceptionContext.MutableData.SetExceptionThrown(t.Exception.InnerException);
			}
			else if (!interceptionContext.MutableData.IsExecutionSuppressed)
			{
				interceptionContext.MutableData.SetExecuted((t.IsCanceled || t.IsFaulted) ? default(TResult) : t.Result);
			}
			try
			{
				foreach (TInterceptor interceptor2 in _interceptors)
				{
					executed(interceptor2, target, interceptionContext);
				}
			}
			catch (Exception exception)
			{
				interceptionContext.MutableData.Exception = exception;
			}
			if (interceptionContext.MutableData.Exception != null)
			{
				tcs.SetException(interceptionContext.MutableData.Exception);
			}
			else if (t.IsCanceled)
			{
				tcs.SetCanceled();
			}
			else
			{
				tcs.SetResult(interceptionContext.MutableData.Result);
			}
		}, TaskContinuationOptions.ExecuteSynchronously);
		return tcs.Task;
	}
}
