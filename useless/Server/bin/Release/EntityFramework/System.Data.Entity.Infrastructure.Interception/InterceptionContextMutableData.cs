using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

internal class InterceptionContextMutableData
{
	private const string LegacyUserState = "__LegacyUserState__";

	private Exception _exception;

	private bool _isSuppressed;

	private IDictionary<string, object> _userStateMap;

	public bool HasExecuted { get; set; }

	public Exception OriginalException { get; set; }

	public TaskStatus TaskStatus { get; set; }

	private IDictionary<string, object> UserStateMap
	{
		get
		{
			if (_userStateMap == null)
			{
				_userStateMap = new Dictionary<string, object>(StringComparer.Ordinal);
			}
			return _userStateMap;
		}
	}

	[Obsolete("Not safe when multiple interceptors are in use. Use SetUserState and FindUserState instead.")]
	public object UserState
	{
		get
		{
			return FindUserState("__LegacyUserState__");
		}
		set
		{
			SetUserState("__LegacyUserState__", value);
		}
	}

	public bool IsExecutionSuppressed => _isSuppressed;

	public Exception Exception
	{
		get
		{
			return _exception;
		}
		set
		{
			if (!HasExecuted)
			{
				SuppressExecution();
			}
			_exception = value;
		}
	}

	public object FindUserState(string key)
	{
		if (_userStateMap == null || !UserStateMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public void SetUserState(string key, object value)
	{
		UserStateMap[key] = value;
	}

	public void SuppressExecution()
	{
		if (!_isSuppressed && HasExecuted)
		{
			throw new InvalidOperationException(Strings.SuppressionAfterExecution);
		}
		_isSuppressed = true;
	}

	public void SetExceptionThrown(Exception exception)
	{
		HasExecuted = true;
		OriginalException = exception;
		Exception = exception;
	}
}
internal class InterceptionContextMutableData<TResult> : InterceptionContextMutableData
{
	private TResult _result;

	public TResult OriginalResult { get; set; }

	public TResult Result
	{
		get
		{
			return _result;
		}
		set
		{
			if (!base.HasExecuted)
			{
				SuppressExecution();
			}
			_result = value;
		}
	}

	public void SetExecuted(TResult result)
	{
		base.HasExecuted = true;
		OriginalResult = result;
		Result = result;
	}
}
