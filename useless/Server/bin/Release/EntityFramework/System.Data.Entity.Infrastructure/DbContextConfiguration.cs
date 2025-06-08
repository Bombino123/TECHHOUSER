using System.ComponentModel;
using System.Data.Entity.Internal;

namespace System.Data.Entity.Infrastructure;

public class DbContextConfiguration
{
	private readonly InternalContext _internalContext;

	public bool EnsureTransactionsForFunctionsAndCommands
	{
		get
		{
			return _internalContext.EnsureTransactionsForFunctionsAndCommands;
		}
		set
		{
			_internalContext.EnsureTransactionsForFunctionsAndCommands = value;
		}
	}

	public bool LazyLoadingEnabled
	{
		get
		{
			return _internalContext.LazyLoadingEnabled;
		}
		set
		{
			_internalContext.LazyLoadingEnabled = value;
		}
	}

	public bool ProxyCreationEnabled
	{
		get
		{
			return _internalContext.ProxyCreationEnabled;
		}
		set
		{
			_internalContext.ProxyCreationEnabled = value;
		}
	}

	public bool UseDatabaseNullSemantics
	{
		get
		{
			return _internalContext.UseDatabaseNullSemantics;
		}
		set
		{
			_internalContext.UseDatabaseNullSemantics = value;
		}
	}

	public bool DisableFilterOverProjectionSimplificationForCustomFunctions
	{
		get
		{
			return _internalContext.DisableFilterOverProjectionSimplificationForCustomFunctions;
		}
		set
		{
			_internalContext.DisableFilterOverProjectionSimplificationForCustomFunctions = value;
		}
	}

	public bool AutoDetectChangesEnabled
	{
		get
		{
			return _internalContext.AutoDetectChangesEnabled;
		}
		set
		{
			_internalContext.AutoDetectChangesEnabled = value;
		}
	}

	public bool ValidateOnSaveEnabled
	{
		get
		{
			return _internalContext.ValidateOnSaveEnabled;
		}
		set
		{
			_internalContext.ValidateOnSaveEnabled = value;
		}
	}

	internal DbContextConfiguration(InternalContext internalContext)
	{
		_internalContext = internalContext;
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
