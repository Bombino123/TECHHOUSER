using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

public abstract class MutableInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext
{
	private readonly InterceptionContextMutableData _mutableData = new InterceptionContextMutableData();

	InterceptionContextMutableData IDbMutableInterceptionContext.MutableData => _mutableData;

	internal InterceptionContextMutableData MutableData => _mutableData;

	public bool IsExecutionSuppressed => _mutableData.IsExecutionSuppressed;

	public Exception OriginalException => _mutableData.OriginalException;

	public Exception Exception
	{
		get
		{
			return _mutableData.Exception;
		}
		set
		{
			_mutableData.Exception = value;
		}
	}

	public TaskStatus TaskStatus => _mutableData.TaskStatus;

	[Obsolete("Not safe when multiple interceptors are in use. Use SetUserState and FindUserState instead.")]
	public object UserState
	{
		get
		{
			return _mutableData.UserState;
		}
		set
		{
			_mutableData.UserState = value;
		}
	}

	protected MutableInterceptionContext()
	{
	}

	protected MutableInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	public void SuppressExecution()
	{
		_mutableData.SuppressExecution();
	}

	public object FindUserState(string key)
	{
		Check.NotNull(key, "key");
		return _mutableData.FindUserState(key);
	}

	public void SetUserState(string key, object value)
	{
		Check.NotNull(key, "key");
		_mutableData.SetUserState(key, value);
	}

	public new MutableInterceptionContext AsAsync()
	{
		return (MutableInterceptionContext)base.AsAsync();
	}

	public new MutableInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (MutableInterceptionContext)base.WithDbContext(context);
	}

	public new MutableInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (MutableInterceptionContext)base.WithObjectContext(context);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
public abstract class MutableInterceptionContext<TResult> : DbInterceptionContext, IDbMutableInterceptionContext<TResult>, IDbMutableInterceptionContext
{
	private readonly InterceptionContextMutableData<TResult> _mutableData = new InterceptionContextMutableData<TResult>();

	InterceptionContextMutableData<TResult> IDbMutableInterceptionContext<TResult>.MutableData => _mutableData;

	InterceptionContextMutableData IDbMutableInterceptionContext.MutableData => _mutableData;

	public TResult OriginalResult => _mutableData.OriginalResult;

	public TResult Result
	{
		get
		{
			return _mutableData.Result;
		}
		set
		{
			_mutableData.Result = value;
		}
	}

	public bool IsExecutionSuppressed => _mutableData.IsExecutionSuppressed;

	[Obsolete("Not safe when multiple interceptors are in use. Use SetUserState and FindUserState instead.")]
	public object UserState
	{
		get
		{
			return _mutableData.UserState;
		}
		set
		{
			_mutableData.UserState = value;
		}
	}

	public Exception OriginalException => _mutableData.OriginalException;

	public Exception Exception
	{
		get
		{
			return _mutableData.Exception;
		}
		set
		{
			_mutableData.Exception = value;
		}
	}

	public TaskStatus TaskStatus => _mutableData.TaskStatus;

	protected MutableInterceptionContext()
	{
	}

	protected MutableInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	public object FindUserState(string key)
	{
		Check.NotNull(key, "key");
		return _mutableData.FindUserState(key);
	}

	public void SetUserState(string key, object value)
	{
		Check.NotNull(key, "key");
		_mutableData.SetUserState(key, value);
	}

	public void SuppressExecution()
	{
		_mutableData.SuppressExecution();
	}

	public new MutableInterceptionContext<TResult> AsAsync()
	{
		return (MutableInterceptionContext<TResult>)base.AsAsync();
	}

	public new MutableInterceptionContext<TResult> WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (MutableInterceptionContext<TResult>)base.WithDbContext(context);
	}

	public new MutableInterceptionContext<TResult> WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (MutableInterceptionContext<TResult>)base.WithObjectContext(context);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
