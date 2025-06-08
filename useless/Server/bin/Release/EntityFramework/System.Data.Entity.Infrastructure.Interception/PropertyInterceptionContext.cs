using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

public class PropertyInterceptionContext<TValue> : DbInterceptionContext, IDbMutableInterceptionContext
{
	private readonly InterceptionContextMutableData _mutableData = new InterceptionContextMutableData();

	private TValue _value;

	InterceptionContextMutableData IDbMutableInterceptionContext.MutableData => _mutableData;

	public TValue Value => _value;

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

	public PropertyInterceptionContext()
	{
	}

	public PropertyInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
		if (copyFrom is PropertyInterceptionContext<TValue> propertyInterceptionContext)
		{
			_value = propertyInterceptionContext._value;
		}
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

	public PropertyInterceptionContext<TValue> WithValue(TValue value)
	{
		PropertyInterceptionContext<TValue> propertyInterceptionContext = TypedClone();
		propertyInterceptionContext._value = value;
		return propertyInterceptionContext;
	}

	private PropertyInterceptionContext<TValue> TypedClone()
	{
		return (PropertyInterceptionContext<TValue>)Clone();
	}

	protected override DbInterceptionContext Clone()
	{
		return new PropertyInterceptionContext<TValue>(this);
	}

	public void SuppressExecution()
	{
		_mutableData.SuppressExecution();
	}

	public new PropertyInterceptionContext<TValue> AsAsync()
	{
		return (PropertyInterceptionContext<TValue>)base.AsAsync();
	}

	public new PropertyInterceptionContext<TValue> WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (PropertyInterceptionContext<TValue>)base.WithDbContext(context);
	}

	public new PropertyInterceptionContext<TValue> WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (PropertyInterceptionContext<TValue>)base.WithObjectContext(context);
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
