using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbCommandInterceptionContext : DbInterceptionContext
{
	private CommandBehavior _commandBehavior;

	public CommandBehavior CommandBehavior => _commandBehavior;

	public DbCommandInterceptionContext()
	{
	}

	public DbCommandInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
		if (copyFrom is DbCommandInterceptionContext dbCommandInterceptionContext)
		{
			_commandBehavior = dbCommandInterceptionContext._commandBehavior;
		}
	}

	public DbCommandInterceptionContext WithCommandBehavior(CommandBehavior commandBehavior)
	{
		DbCommandInterceptionContext dbCommandInterceptionContext = TypedClone();
		dbCommandInterceptionContext._commandBehavior = commandBehavior;
		return dbCommandInterceptionContext;
	}

	private DbCommandInterceptionContext TypedClone()
	{
		return (DbCommandInterceptionContext)Clone();
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbCommandInterceptionContext(this);
	}

	public new DbCommandInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbCommandInterceptionContext)base.WithDbContext(context);
	}

	public new DbCommandInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbCommandInterceptionContext)base.WithObjectContext(context);
	}

	public new DbCommandInterceptionContext AsAsync()
	{
		return (DbCommandInterceptionContext)base.AsAsync();
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
public class DbCommandInterceptionContext<TResult> : DbCommandInterceptionContext, IDbMutableInterceptionContext<TResult>, IDbMutableInterceptionContext
{
	private readonly InterceptionContextMutableData<TResult> _mutableData = new InterceptionContextMutableData<TResult>();

	InterceptionContextMutableData IDbMutableInterceptionContext.MutableData => _mutableData;

	InterceptionContextMutableData<TResult> IDbMutableInterceptionContext<TResult>.MutableData => _mutableData;

	internal InterceptionContextMutableData<TResult> MutableData => _mutableData;

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

	public DbCommandInterceptionContext()
	{
	}

	public DbCommandInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
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

	public new DbCommandInterceptionContext<TResult> AsAsync()
	{
		return (DbCommandInterceptionContext<TResult>)base.AsAsync();
	}

	public new DbCommandInterceptionContext<TResult> WithCommandBehavior(CommandBehavior commandBehavior)
	{
		return (DbCommandInterceptionContext<TResult>)base.WithCommandBehavior(commandBehavior);
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbCommandInterceptionContext<TResult>(this);
	}

	public new DbCommandInterceptionContext<TResult> WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbCommandInterceptionContext<TResult>)base.WithDbContext(context);
	}

	public new DbCommandInterceptionContext<TResult> WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbCommandInterceptionContext<TResult>)base.WithObjectContext(context);
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
