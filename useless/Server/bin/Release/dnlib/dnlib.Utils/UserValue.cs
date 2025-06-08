using System;
using System.Diagnostics;
using dnlib.Threading;

namespace dnlib.Utils;

[DebuggerDisplay("{value}")]
internal struct UserValue<TValue>
{
	private Lock theLock;

	private Func<TValue> readOriginalValue;

	private TValue value;

	private bool isUserValue;

	private bool isValueInitialized;

	public Lock Lock
	{
		set
		{
			theLock = value;
		}
	}

	public Func<TValue> ReadOriginalValue
	{
		set
		{
			readOriginalValue = value;
		}
	}

	public TValue Value
	{
		get
		{
			theLock?.EnterWriteLock();
			try
			{
				if (!isValueInitialized)
				{
					value = readOriginalValue();
					readOriginalValue = null;
					isValueInitialized = true;
				}
				return value;
			}
			finally
			{
				theLock?.ExitWriteLock();
			}
		}
		set
		{
			theLock?.EnterWriteLock();
			try
			{
				this.value = value;
				readOriginalValue = null;
				isUserValue = true;
				isValueInitialized = true;
			}
			finally
			{
				theLock?.ExitWriteLock();
			}
		}
	}

	public bool IsValueInitialized
	{
		get
		{
			theLock?.EnterReadLock();
			try
			{
				return isValueInitialized;
			}
			finally
			{
				theLock?.ExitReadLock();
			}
		}
	}

	public bool IsUserValue
	{
		get
		{
			theLock?.EnterReadLock();
			try
			{
				return isUserValue;
			}
			finally
			{
				theLock?.ExitReadLock();
			}
		}
	}
}
