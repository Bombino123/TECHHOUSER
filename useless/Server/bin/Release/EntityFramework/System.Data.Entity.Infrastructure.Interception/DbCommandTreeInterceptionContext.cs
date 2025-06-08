using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbCommandTreeInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<DbCommandTree>, IDbMutableInterceptionContext
{
	private readonly InterceptionContextMutableData<DbCommandTree> _mutableData = new InterceptionContextMutableData<DbCommandTree>();

	internal InterceptionContextMutableData<DbCommandTree> MutableData => _mutableData;

	InterceptionContextMutableData<DbCommandTree> IDbMutableInterceptionContext<DbCommandTree>.MutableData => _mutableData;

	InterceptionContextMutableData IDbMutableInterceptionContext.MutableData => _mutableData;

	public DbCommandTree OriginalResult => _mutableData.OriginalResult;

	public DbCommandTree Result
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

	public DbCommandTreeInterceptionContext()
	{
	}

	public DbCommandTreeInterceptionContext(DbInterceptionContext copyFrom)
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

	protected override DbInterceptionContext Clone()
	{
		return new DbCommandTreeInterceptionContext(this);
	}

	public new DbCommandTreeInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbCommandTreeInterceptionContext)base.WithDbContext(context);
	}

	public new DbCommandTreeInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbCommandTreeInterceptionContext)base.WithObjectContext(context);
	}

	public new DbCommandTreeInterceptionContext AsAsync()
	{
		return (DbCommandTreeInterceptionContext)base.AsAsync();
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
