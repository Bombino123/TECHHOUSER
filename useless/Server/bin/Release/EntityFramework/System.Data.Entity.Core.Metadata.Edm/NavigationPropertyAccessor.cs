using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class NavigationPropertyAccessor
{
	private Func<object, object> _memberGetter;

	private Action<object, object> _memberSetter;

	private Action<object, object> _collectionAdd;

	private Func<object, object, bool> _collectionRemove;

	private Func<object> _collectionCreate;

	private readonly string _propertyName;

	public bool HasProperty => _propertyName != null;

	public string PropertyName => _propertyName;

	public Func<object, object> ValueGetter
	{
		get
		{
			return _memberGetter;
		}
		set
		{
			Interlocked.CompareExchange(ref _memberGetter, value, null);
		}
	}

	public Action<object, object> ValueSetter
	{
		get
		{
			return _memberSetter;
		}
		set
		{
			Interlocked.CompareExchange(ref _memberSetter, value, null);
		}
	}

	public Action<object, object> CollectionAdd
	{
		get
		{
			return _collectionAdd;
		}
		set
		{
			Interlocked.CompareExchange(ref _collectionAdd, value, null);
		}
	}

	public Func<object, object, bool> CollectionRemove
	{
		get
		{
			return _collectionRemove;
		}
		set
		{
			Interlocked.CompareExchange(ref _collectionRemove, value, null);
		}
	}

	public Func<object> CollectionCreate
	{
		get
		{
			return _collectionCreate;
		}
		set
		{
			Interlocked.CompareExchange(ref _collectionCreate, value, null);
		}
	}

	public static NavigationPropertyAccessor NoNavigationProperty => new NavigationPropertyAccessor(null);

	public NavigationPropertyAccessor(string propertyName)
	{
		_propertyName = propertyName;
	}
}
