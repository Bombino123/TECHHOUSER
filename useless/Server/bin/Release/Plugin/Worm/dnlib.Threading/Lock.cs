using System.Threading;

namespace dnlib.Threading;

internal class Lock
{
	private readonly object lockObj;

	private int recurseCount;

	public static Lock Create()
	{
		return new Lock();
	}

	private Lock()
	{
		lockObj = new object();
		recurseCount = 0;
	}

	public void EnterReadLock()
	{
		Monitor.Enter(lockObj);
		if (recurseCount != 0)
		{
			Monitor.Exit(lockObj);
			throw new LockException("Recursive locks aren't supported");
		}
		recurseCount++;
	}

	public void ExitReadLock()
	{
		if (recurseCount <= 0)
		{
			throw new LockException("Too many exit lock method calls");
		}
		recurseCount--;
		Monitor.Exit(lockObj);
	}

	public void EnterWriteLock()
	{
		Monitor.Enter(lockObj);
		if (recurseCount != 0)
		{
			Monitor.Exit(lockObj);
			throw new LockException("Recursive locks aren't supported");
		}
		recurseCount--;
	}

	public void ExitWriteLock()
	{
		if (recurseCount >= 0)
		{
			throw new LockException("Too many exit lock method calls");
		}
		recurseCount++;
		Monitor.Exit(lockObj);
	}
}
