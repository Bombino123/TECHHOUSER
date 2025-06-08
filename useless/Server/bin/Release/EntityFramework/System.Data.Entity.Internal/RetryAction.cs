using System.Diagnostics;

namespace System.Data.Entity.Internal;

internal class RetryAction<TInput>
{
	private readonly object _lock = new object();

	private Action<TInput> _action;

	public RetryAction(Action<TInput> action)
	{
		_action = action;
	}

	[DebuggerStepThrough]
	public void PerformAction(TInput input)
	{
		lock (_lock)
		{
			if (_action != null)
			{
				Action<TInput> action = _action;
				_action = null;
				try
				{
					action(input);
					return;
				}
				catch (Exception)
				{
					_action = action;
					throw;
				}
			}
		}
	}
}
