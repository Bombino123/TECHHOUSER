using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbInterceptionContext
{
	private readonly IList<DbContext> _dbContexts;

	private readonly IList<ObjectContext> _objectContexts;

	private bool _isAsync;

	public IEnumerable<DbContext> DbContexts => _dbContexts;

	public IEnumerable<ObjectContext> ObjectContexts => _objectContexts;

	public bool IsAsync => _isAsync;

	public DbInterceptionContext()
	{
		_dbContexts = new List<DbContext>();
		_objectContexts = new List<ObjectContext>();
	}

	protected DbInterceptionContext(DbInterceptionContext copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
		_dbContexts = copyFrom.DbContexts.Where((DbContext c) => c.InternalContext == null || !c.InternalContext.IsDisposed).ToList();
		_objectContexts = copyFrom.ObjectContexts.Where((ObjectContext c) => !c.IsDisposed).ToList();
		_isAsync = copyFrom._isAsync;
	}

	private DbInterceptionContext(IEnumerable<DbInterceptionContext> copyFrom)
	{
		_dbContexts = (from c in copyFrom.SelectMany((DbInterceptionContext c) => c.DbContexts).Distinct()
			where !c.InternalContext.IsDisposed
			select c).ToList();
		_objectContexts = (from c in copyFrom.SelectMany((DbInterceptionContext c) => c.ObjectContexts).Distinct()
			where !c.IsDisposed
			select c).ToList();
		_isAsync = copyFrom.Any((DbInterceptionContext c) => c.IsAsync);
	}

	public DbInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		DbInterceptionContext dbInterceptionContext = Clone();
		if (!dbInterceptionContext._dbContexts.Contains(context, ObjectReferenceEqualityComparer.Default))
		{
			dbInterceptionContext._dbContexts.Add(context);
		}
		return dbInterceptionContext;
	}

	public DbInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		DbInterceptionContext dbInterceptionContext = Clone();
		if (!dbInterceptionContext._objectContexts.Contains(context, ObjectReferenceEqualityComparer.Default))
		{
			dbInterceptionContext._objectContexts.Add(context);
		}
		return dbInterceptionContext;
	}

	public DbInterceptionContext AsAsync()
	{
		DbInterceptionContext dbInterceptionContext = Clone();
		dbInterceptionContext._isAsync = true;
		return dbInterceptionContext;
	}

	protected virtual DbInterceptionContext Clone()
	{
		return new DbInterceptionContext(this);
	}

	internal static DbInterceptionContext Combine(IEnumerable<DbInterceptionContext> interceptionContexts)
	{
		return new DbInterceptionContext(interceptionContexts);
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
