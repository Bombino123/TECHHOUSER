using System.Data.Entity.Resources;
using System.Threading;

namespace System.Data.Entity.Internal;

internal class ThrowingMonitor
{
	private int _isInCriticalSection;

	public void Enter()
	{
		if (Interlocked.CompareExchange(ref _isInCriticalSection, 1, 0) != 0)
		{
			throw new NotSupportedException(Strings.ConcurrentMethodInvocation);
		}
	}

	public void Exit()
	{
		Interlocked.Exchange(ref _isInCriticalSection, 0);
	}

	public void EnsureNotEntered()
	{
		Thread.MemoryBarrier();
		if (_isInCriticalSection != 0)
		{
			throw new NotSupportedException(Strings.ConcurrentMethodInvocation);
		}
	}
}
