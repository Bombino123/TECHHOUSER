using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Utilities;

[ComVisible(true)]
public class CountdownLatch
{
	private int m_count;

	private EventWaitHandle m_waitHandle = new EventWaitHandle(initialState: true, EventResetMode.ManualReset);

	public void Increment()
	{
		if (Interlocked.Increment(ref m_count) == 1)
		{
			m_waitHandle.Reset();
		}
	}

	public void Add(int value)
	{
		if (Interlocked.Add(ref m_count, value) == value)
		{
			m_waitHandle.Reset();
		}
	}

	public void Decrement()
	{
		int num = Interlocked.Decrement(ref m_count);
		if (m_count == 0)
		{
			m_waitHandle.Set();
		}
		else if (num < 0)
		{
			throw new InvalidOperationException("Count must be greater than or equal to 0");
		}
	}

	public void WaitUntilZero()
	{
		m_waitHandle.WaitOne();
	}
}
