using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal;

internal abstract class InternalNavigationEntry : InternalMemberEntry
{
	private IRelatedEnd _relatedEnd;

	private Func<object, object> _getter;

	private bool _triedToGetGetter;

	private Action<object, object> _setter;

	private bool _triedToGetSetter;

	public virtual bool IsLoaded
	{
		get
		{
			ValidateNotDetached("IsLoaded");
			return _relatedEnd.IsLoaded;
		}
		set
		{
			ValidateNotDetached("IsLoaded");
			_relatedEnd.IsLoaded = value;
		}
	}

	protected IRelatedEnd RelatedEnd
	{
		get
		{
			if (_relatedEnd == null && !InternalEntityEntry.IsDetached)
			{
				_relatedEnd = InternalEntityEntry.GetRelatedEnd(Name);
			}
			return _relatedEnd;
		}
	}

	public override object CurrentValue
	{
		get
		{
			if (Getter == null)
			{
				ValidateNotDetached("CurrentValue");
				return GetNavigationPropertyFromRelatedEnd(InternalEntityEntry.Entity);
			}
			return Getter(InternalEntityEntry.Entity);
		}
	}

	protected Func<object, object> Getter
	{
		get
		{
			if (!_triedToGetGetter)
			{
				DbHelpers.GetPropertyGetters(InternalEntityEntry.EntityType).TryGetValue(Name, out _getter);
				_triedToGetGetter = true;
			}
			return _getter;
		}
	}

	protected Action<object, object> Setter
	{
		get
		{
			if (!_triedToGetSetter)
			{
				DbHelpers.GetPropertySetters(InternalEntityEntry.EntityType).TryGetValue(Name, out _setter);
				_triedToGetSetter = true;
			}
			return _setter;
		}
	}

	protected InternalNavigationEntry(InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
		: base(internalEntityEntry, navigationMetadata)
	{
	}

	public virtual void Load()
	{
		ValidateNotDetached("Load");
		_relatedEnd.Load();
	}

	public virtual Task LoadAsync(CancellationToken cancellationToken)
	{
		ValidateNotDetached("LoadAsync");
		return _relatedEnd.LoadAsync(cancellationToken);
	}

	public virtual IQueryable Query()
	{
		ValidateNotDetached("Query");
		return (IQueryable)_relatedEnd.CreateSourceQuery();
	}

	protected abstract object GetNavigationPropertyFromRelatedEnd(object entity);

	private void ValidateNotDetached(string method)
	{
		if (_relatedEnd == null)
		{
			if (InternalEntityEntry.IsDetached)
			{
				throw Error.DbPropertyEntry_NotSupportedForDetached(method, Name, InternalEntityEntry.EntityType.Name);
			}
			_relatedEnd = InternalEntityEntry.GetRelatedEnd(Name);
		}
	}
}
