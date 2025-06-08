using System.Diagnostics;

namespace System.Data.Entity.Internal;

internal class RetryLazy<TInput, TResult> where TResult : class
{
	private readonly object _lock = new object();

	private Func<TInput, TResult> _valueFactory;

	private TResult _value;

	public RetryLazy(Func<TInput, TResult> valueFactory)
	{
		_valueFactory = valueFactory;
	}

	[DebuggerStepThrough]
	public TResult GetValue(TInput input)
	{
		lock (_lock)
		{
			if (_value == null)
			{
				Func<TInput, TResult> valueFactory = _valueFactory;
				try
				{
					_valueFactory = null;
					_value = valueFactory(input);
				}
				catch (Exception)
				{
					_valueFactory = valueFactory;
					throw;
				}
			}
			return _value;
		}
	}
}
