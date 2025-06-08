using System;
using System.Diagnostics;
using System.Threading;

namespace GMap.NET.Internals;

internal sealed class FastResourceLock : IDisposable
{
	public struct Statistics
	{
		public int AcqExcl;

		public int AcqShrd;

		public int AcqExclCont;

		public int AcqShrdCont;

		public int AcqExclSlp;

		public int AcqShrdSlp;

		public int PeakExclWtrsCount;

		public int PeakShrdWtrsCount;
	}

	private const int LockOwned = 1;

	private const int LockExclusiveWaking = 2;

	private const int LockSharedOwnersShift = 2;

	private const int LockSharedOwnersMask = 1023;

	private const int LockSharedOwnersIncrement = 4;

	private const int LockSharedWaitersShift = 12;

	private const int LockSharedWaitersMask = 1023;

	private const int LockSharedWaitersIncrement = 4096;

	private const int LockExclusiveWaitersShift = 22;

	private const int LockExclusiveWaitersMask = 1023;

	private const int LockExclusiveWaitersIncrement = 4194304;

	private const int ExclusiveMask = -4194302;

	private static readonly int SpinCount = NativeMethods.SpinCount;

	private int _value;

	private IntPtr _sharedWakeEvent;

	private IntPtr _exclusiveWakeEvent;

	public int ExclusiveWaiters => (_value >> 22) & 0x3FF;

	public bool Owned => (_value & 1) != 0;

	public int SharedOwners => (_value >> 2) & 0x3FF;

	public int SharedWaiters => (_value >> 12) & 0x3FF;

	public FastResourceLock()
	{
		_value = 0;
		_sharedWakeEvent = NativeMethods.CreateSemaphore(IntPtr.Zero, 0, int.MaxValue);
		_exclusiveWakeEvent = NativeMethods.CreateSemaphore(IntPtr.Zero, 0, int.MaxValue);
	}

	~FastResourceLock()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (_sharedWakeEvent != IntPtr.Zero)
		{
			NativeMethods.CloseHandle(_sharedWakeEvent);
			_sharedWakeEvent = IntPtr.Zero;
		}
		if (_exclusiveWakeEvent != IntPtr.Zero)
		{
			NativeMethods.CloseHandle(_exclusiveWakeEvent);
			_exclusiveWakeEvent = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void AcquireExclusive()
	{
		int num = 0;
		int value;
		while (true)
		{
			value = _value;
			if ((value & 3) == 0)
			{
				if (Interlocked.CompareExchange(ref _value, value + 1, value) == value)
				{
					return;
				}
			}
			else if (num >= SpinCount && Interlocked.CompareExchange(ref _value, value + 4194304, value) == value)
			{
				break;
			}
			num++;
		}
		if (NativeMethods.WaitForSingleObject(_exclusiveWakeEvent, -1) != 0)
		{
			UtilsBreak("Utils.MsgFailedToWaitIndefinitely");
		}
		do
		{
			value = _value;
		}
		while (Interlocked.CompareExchange(ref _value, value + 1 - 2, value) != value);
	}

	public void AcquireShared()
	{
		int num = 0;
		while (true)
		{
			int value = _value;
			if ((value & -4190209) == 0)
			{
				if (Interlocked.CompareExchange(ref _value, value + 1 + 4, value) == value)
				{
					break;
				}
			}
			else if (((uint)value & (true ? 1u : 0u)) != 0 && ((uint)(value >> 2) & 0x3FFu) != 0 && (value & -4194302) == 0)
			{
				if (Interlocked.CompareExchange(ref _value, value + 4, value) == value)
				{
					break;
				}
			}
			else if (num >= SpinCount && Interlocked.CompareExchange(ref _value, value + 4096, value) == value)
			{
				if (NativeMethods.WaitForSingleObject(_sharedWakeEvent, -1) != 0)
				{
					UtilsBreak("Utils.MsgFailedToWaitIndefinitely");
				}
				continue;
			}
			num++;
		}
	}

	public static void UtilsBreak(string logMessage)
	{
		Debugger.Log(0, "Error", logMessage);
		Debugger.Break();
	}

	public void ConvertExclusiveToShared()
	{
		int value;
		int num;
		do
		{
			value = _value;
			num = (value >> 12) & 0x3FF;
		}
		while (Interlocked.CompareExchange(ref _value, (value + 4) & -4190209, value) != value);
		if (num != 0)
		{
			NativeMethods.ReleaseSemaphore(_sharedWakeEvent, num, IntPtr.Zero);
		}
	}

	public Statistics GetStatistics()
	{
		return default(Statistics);
	}

	public void ReleaseExclusive()
	{
		int num;
		while (true)
		{
			int value = _value;
			if (((uint)(value >> 22) & 0x3FFu) != 0)
			{
				if (Interlocked.CompareExchange(ref _value, value - 1 + 2 - 4194304, value) == value)
				{
					NativeMethods.ReleaseSemaphore(_exclusiveWakeEvent, 1, IntPtr.Zero);
					return;
				}
			}
			else
			{
				num = (value >> 12) & 0x3FF;
				if (Interlocked.CompareExchange(ref _value, value & -4190210, value) == value)
				{
					break;
				}
			}
		}
		if (num != 0)
		{
			NativeMethods.ReleaseSemaphore(_sharedWakeEvent, num, IntPtr.Zero);
		}
	}

	public void ReleaseShared()
	{
		while (true)
		{
			int value = _value;
			if (((value >> 2) & 0x3FF) > 1)
			{
				if (Interlocked.CompareExchange(ref _value, value - 4, value) == value)
				{
					break;
				}
			}
			else if (((uint)(value >> 22) & 0x3FFu) != 0)
			{
				if (Interlocked.CompareExchange(ref _value, value - 1 + 2 - 4 - 4194304, value) == value)
				{
					NativeMethods.ReleaseSemaphore(_exclusiveWakeEvent, 1, IntPtr.Zero);
					break;
				}
			}
			else if (Interlocked.CompareExchange(ref _value, value - 1 - 4, value) == value)
			{
				break;
			}
		}
	}

	public void SpinAcquireExclusive()
	{
		while (true)
		{
			int value = _value;
			if (((uint)value & 3u) != 0 || Interlocked.CompareExchange(ref _value, value + 1, value) != value)
			{
				if (NativeMethods.SpinEnabled)
				{
					Thread.SpinWait(8);
				}
				else
				{
					Thread.Sleep(0);
				}
				continue;
			}
			break;
		}
	}

	public void SpinAcquireShared()
	{
		while (true)
		{
			int value = _value;
			if ((value & -4194302) == 0)
			{
				if ((value & 1) == 0)
				{
					if (Interlocked.CompareExchange(ref _value, value + 1 + 4, value) == value)
					{
						break;
					}
				}
				else if (((uint)(value >> 2) & 0x3FFu) != 0 && Interlocked.CompareExchange(ref _value, value + 4, value) == value)
				{
					break;
				}
			}
			if (NativeMethods.SpinEnabled)
			{
				Thread.SpinWait(8);
			}
			else
			{
				Thread.Sleep(0);
			}
		}
	}

	public void SpinConvertSharedToExclusive()
	{
		while (true)
		{
			int value = _value;
			if (((value >> 2) & 0x3FF) != 1 || Interlocked.CompareExchange(ref _value, value - 4, value) != value)
			{
				if (NativeMethods.SpinEnabled)
				{
					Thread.SpinWait(8);
				}
				else
				{
					Thread.Sleep(0);
				}
				continue;
			}
			break;
		}
	}

	public bool TryAcquireExclusive()
	{
		int value = _value;
		if (((uint)value & 3u) != 0)
		{
			return false;
		}
		return Interlocked.CompareExchange(ref _value, value + 1, value) == value;
	}

	public bool TryAcquireShared()
	{
		int value = _value;
		if (((uint)value & 0xFFC00002u) != 0)
		{
			return false;
		}
		if ((value & 1) == 0)
		{
			return Interlocked.CompareExchange(ref _value, value + 1 + 4, value) == value;
		}
		if (((uint)(value >> 2) & 0x3FFu) != 0)
		{
			return Interlocked.CompareExchange(ref _value, value + 4, value) == value;
		}
		return false;
	}

	public bool TryConvertSharedToExclusive()
	{
		int value;
		do
		{
			value = _value;
			if (((value >> 2) & 0x3FF) != 1)
			{
				return false;
			}
		}
		while (Interlocked.CompareExchange(ref _value, value - 4, value) != value);
		return true;
	}
}
