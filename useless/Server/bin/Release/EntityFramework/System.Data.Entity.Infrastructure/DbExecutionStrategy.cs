using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace System.Data.Entity.Infrastructure;

public abstract class DbExecutionStrategy : IDbExecutionStrategy
{
	private readonly List<Exception> _exceptionsEncountered = new List<Exception>();

	private readonly Random _random = new Random();

	private readonly int _maxRetryCount;

	private readonly TimeSpan _maxDelay;

	private const string ContextName = "ExecutionStrategySuspended";

	private const int DefaultMaxRetryCount = 5;

	private const double DefaultRandomFactor = 1.1;

	private const double DefaultExponentialBase = 2.0;

	private static readonly TimeSpan DefaultCoefficient = TimeSpan.FromSeconds(1.0);

	private static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(30.0);

	public bool RetriesOnFailure => !Suspended;

	protected internal static bool Suspended
	{
		get
		{
			return ((bool?)CallContext.LogicalGetData("ExecutionStrategySuspended")).GetValueOrDefault();
		}
		set
		{
			CallContext.LogicalSetData("ExecutionStrategySuspended", (object)value);
		}
	}

	protected DbExecutionStrategy()
		: this(5, DefaultMaxDelay)
	{
	}

	protected DbExecutionStrategy(int maxRetryCount, TimeSpan maxDelay)
	{
		if (maxRetryCount < 0)
		{
			throw new ArgumentOutOfRangeException("maxRetryCount");
		}
		if (maxDelay.TotalMilliseconds < 0.0)
		{
			throw new ArgumentOutOfRangeException("maxDelay");
		}
		_maxRetryCount = maxRetryCount;
		_maxDelay = maxDelay;
	}

	public void Execute(Action operation)
	{
		Check.NotNull(operation, "operation");
		Execute(delegate
		{
			operation();
			return (object)null;
		});
	}

	public TResult Execute<TResult>(Func<TResult> operation)
	{
		Check.NotNull(operation, "operation");
		if (RetriesOnFailure)
		{
			EnsurePreexecutionState();
			TimeSpan? nextDelay;
			while (true)
			{
				try
				{
					Suspended = true;
					return operation();
				}
				catch (Exception ex)
				{
					if (!UnwrapAndHandleException(ex, ShouldRetryOn))
					{
						throw;
					}
					nextDelay = GetNextDelay(ex);
					if (!nextDelay.HasValue)
					{
						throw new RetryLimitExceededException(Strings.ExecutionStrategy_RetryLimitExceeded(_maxRetryCount, GetType().Name), ex);
					}
				}
				finally
				{
					Suspended = false;
				}
				if (nextDelay < TimeSpan.Zero)
				{
					break;
				}
				Thread.Sleep(nextDelay.Value);
			}
			throw new InvalidOperationException(Strings.ExecutionStrategy_NegativeDelay(nextDelay));
		}
		return operation();
	}

	public Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
	{
		Check.NotNull(operation, "operation");
		if (RetriesOnFailure)
		{
			EnsurePreexecutionState();
		}
		cancellationToken.ThrowIfCancellationRequested();
		return ProtectedExecuteAsync(async delegate
		{
			await operation().WithCurrentCulture();
			return true;
		}, cancellationToken);
	}

	public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
	{
		Check.NotNull(operation, "operation");
		if (RetriesOnFailure)
		{
			EnsurePreexecutionState();
		}
		cancellationToken.ThrowIfCancellationRequested();
		return ProtectedExecuteAsync(operation, cancellationToken);
	}

	private async Task<TResult> ProtectedExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
	{
		if (!RetriesOnFailure)
		{
			return await operation().WithCurrentCulture();
		}
		TimeSpan? nextDelay;
		while (true)
		{
			try
			{
				Suspended = true;
				return await operation().WithCurrentCulture();
			}
			catch (Exception ex)
			{
				if (!UnwrapAndHandleException(ex, ShouldRetryOn))
				{
					throw;
				}
				nextDelay = GetNextDelay(ex);
				if (!nextDelay.HasValue)
				{
					throw new RetryLimitExceededException(Strings.ExecutionStrategy_RetryLimitExceeded(_maxRetryCount, GetType().Name), ex);
				}
			}
			finally
			{
				Suspended = false;
			}
			if (nextDelay < TimeSpan.Zero)
			{
				break;
			}
			await Task.Delay(nextDelay.Value, cancellationToken).WithCurrentCulture();
		}
		throw new InvalidOperationException(Strings.ExecutionStrategy_NegativeDelay(nextDelay));
	}

	private void EnsurePreexecutionState()
	{
		if (Transaction.Current != null)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_ExistingTransaction(GetType().Name));
		}
		_exceptionsEncountered.Clear();
	}

	protected internal virtual TimeSpan? GetNextDelay(Exception lastException)
	{
		_exceptionsEncountered.Add(lastException);
		int num = _exceptionsEncountered.Count - 1;
		if (num < _maxRetryCount)
		{
			double num2 = (Math.Pow(2.0, num) - 1.0) * (1.0 + _random.NextDouble() * 0.10000000000000009);
			TimeSpan defaultCoefficient = DefaultCoefficient;
			double val = defaultCoefficient.TotalMilliseconds * num2;
			defaultCoefficient = _maxDelay;
			return TimeSpan.FromMilliseconds(Math.Min(val, defaultCoefficient.TotalMilliseconds));
		}
		return null;
	}

	public static T UnwrapAndHandleException<T>(Exception exception, Func<Exception, T> exceptionHandler)
	{
		if (exception is EntityException ex)
		{
			return UnwrapAndHandleException(ex.InnerException, exceptionHandler);
		}
		if (exception is DbUpdateException ex2)
		{
			return UnwrapAndHandleException(ex2.InnerException, exceptionHandler);
		}
		if (exception is UpdateException ex3)
		{
			return UnwrapAndHandleException(ex3.InnerException, exceptionHandler);
		}
		return exceptionHandler(exception);
	}

	protected internal abstract bool ShouldRetryOn(Exception exception);
}
