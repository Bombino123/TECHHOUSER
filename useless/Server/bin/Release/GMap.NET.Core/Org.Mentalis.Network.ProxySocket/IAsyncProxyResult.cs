using System;
using System.Threading;

namespace Org.Mentalis.Network.ProxySocket;

internal class IAsyncProxyResult : IAsyncResult
{
	internal bool Completed = true;

	private object _stateObject;

	private ManualResetEvent _waitHandle;

	public bool IsCompleted => Completed;

	public bool CompletedSynchronously => false;

	public object AsyncState => _stateObject;

	public WaitHandle AsyncWaitHandle
	{
		get
		{
			if (_waitHandle == null)
			{
				_waitHandle = new ManualResetEvent(initialState: false);
			}
			return _waitHandle;
		}
	}

	internal void Init(object stateObject)
	{
		_stateObject = stateObject;
		Completed = false;
		if (_waitHandle != null)
		{
			_waitHandle.Reset();
		}
	}

	internal void Reset()
	{
		_stateObject = null;
		Completed = true;
		if (_waitHandle != null)
		{
			_waitHandle.Set();
		}
	}
}
