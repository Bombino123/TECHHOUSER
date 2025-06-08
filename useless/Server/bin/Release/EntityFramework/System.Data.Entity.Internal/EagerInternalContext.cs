using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal;

internal class EagerInternalContext : InternalContext
{
	private readonly ObjectContext _objectContext;

	private readonly bool _objectContextOwned;

	private readonly string _originalConnectionString;

	public override ObjectContext ObjectContext
	{
		get
		{
			Initialize();
			return ObjectContextInUse;
		}
	}

	private ObjectContext ObjectContextInUse => base.TempObjectContext ?? _objectContext;

	public override IDatabaseInitializer<DbContext> DefaultInitializer => null;

	public override DbConnection Connection
	{
		get
		{
			CheckContextNotDisposed();
			return ((EntityConnection)_objectContext.Connection).StoreConnection;
		}
	}

	public override string OriginalConnectionString => _originalConnectionString;

	public override DbConnectionStringOrigin ConnectionStringOrigin => DbConnectionStringOrigin.UserCode;

	public override bool EnsureTransactionsForFunctionsAndCommands
	{
		get
		{
			return ObjectContextInUse.ContextOptions.EnsureTransactionsForFunctionsAndCommands;
		}
		set
		{
			ObjectContextInUse.ContextOptions.EnsureTransactionsForFunctionsAndCommands = value;
		}
	}

	public override bool LazyLoadingEnabled
	{
		get
		{
			return ObjectContextInUse.ContextOptions.LazyLoadingEnabled;
		}
		set
		{
			ObjectContextInUse.ContextOptions.LazyLoadingEnabled = value;
		}
	}

	public override bool ProxyCreationEnabled
	{
		get
		{
			return ObjectContextInUse.ContextOptions.ProxyCreationEnabled;
		}
		set
		{
			ObjectContextInUse.ContextOptions.ProxyCreationEnabled = value;
		}
	}

	public override bool UseDatabaseNullSemantics
	{
		get
		{
			return !ObjectContextInUse.ContextOptions.UseCSharpNullComparisonBehavior;
		}
		set
		{
			ObjectContextInUse.ContextOptions.UseCSharpNullComparisonBehavior = !value;
		}
	}

	public override bool DisableFilterOverProjectionSimplificationForCustomFunctions
	{
		get
		{
			return !ObjectContextInUse.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions;
		}
		set
		{
			ObjectContextInUse.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions = !value;
		}
	}

	public override int? CommandTimeout
	{
		get
		{
			return ObjectContextInUse.CommandTimeout;
		}
		set
		{
			ObjectContextInUse.CommandTimeout = value;
		}
	}

	public EagerInternalContext(DbContext owner)
		: base(owner)
	{
	}

	public EagerInternalContext(DbContext owner, ObjectContext objectContext, bool objectContextOwned)
		: base(owner)
	{
		_objectContext = objectContext;
		_objectContextOwned = objectContextOwned;
		_originalConnectionString = InternalConnection.GetStoreConnectionString(_objectContext.Connection);
		_objectContext.InterceptionContext = _objectContext.InterceptionContext.WithDbContext(owner);
		LoadContextConfigs();
		ResetDbSets();
		_objectContext.InitializeMappingViewCacheFactory(base.Owner);
	}

	public override ObjectContext GetObjectContextWithoutDatabaseInitialization()
	{
		InitializeContext();
		return ObjectContextInUse;
	}

	protected override void InitializeContext()
	{
		CheckContextNotDisposed();
	}

	public override void MarkDatabaseNotInitialized()
	{
	}

	public override void MarkDatabaseInitialized()
	{
	}

	protected override void InitializeDatabase()
	{
	}

	public override void DisposeContext(bool disposing)
	{
		if (!base.IsDisposed)
		{
			base.DisposeContext(disposing);
			if (disposing && _objectContextOwned)
			{
				_objectContext.Dispose();
			}
		}
	}

	public override void OverrideConnection(IInternalConnection connection)
	{
		throw Error.EagerInternalContext_CannotSetConnectionInfo();
	}
}
