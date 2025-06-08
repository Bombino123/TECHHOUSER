using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Utilities;
using System.Reflection;
using System.Xml.Linq;

namespace System.Data.Entity.Internal;

internal class SortableBindingList<T> : BindingList<T>
{
	internal class PropertyComparer : Comparer<T>
	{
		private readonly IComparer _comparer;

		private readonly ListSortDirection _direction;

		private readonly PropertyDescriptor _prop;

		private readonly bool _useToString;

		public PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
		{
			if (!prop.ComponentType.IsAssignableFrom(typeof(T)))
			{
				throw new MissingMemberException(typeof(T).Name, prop.Name);
			}
			_prop = prop;
			_direction = direction;
			if (CanSortWithIComparable(prop.PropertyType))
			{
				PropertyInfo declaredProperty = typeof(Comparer<>).MakeGenericType(prop.PropertyType).GetDeclaredProperty("Default");
				_comparer = (IComparer)declaredProperty.GetValue(null, null);
				_useToString = false;
			}
			else
			{
				_comparer = StringComparer.CurrentCultureIgnoreCase;
				_useToString = true;
			}
		}

		public override int Compare(T left, T right)
		{
			object obj = _prop.GetValue(left);
			object obj2 = _prop.GetValue(right);
			if (_useToString)
			{
				obj = obj?.ToString();
				obj2 = obj2?.ToString();
			}
			if (_direction != 0)
			{
				return _comparer.Compare(obj2, obj);
			}
			return _comparer.Compare(obj, obj2);
		}

		public static bool CanSort(Type type)
		{
			if (!CanSortWithToString(type))
			{
				return CanSortWithIComparable(type);
			}
			return true;
		}

		private static bool CanSortWithIComparable(Type type)
		{
			if (!(type.GetInterface("IComparable") != null))
			{
				if (type.IsGenericType())
				{
					return type.GetGenericTypeDefinition() == typeof(Nullable<>);
				}
				return false;
			}
			return true;
		}

		private static bool CanSortWithToString(Type type)
		{
			if (!type.Equals(typeof(XNode)))
			{
				return type.IsSubclassOf(typeof(XNode));
			}
			return true;
		}
	}

	private bool _isSorted;

	private ListSortDirection _sortDirection;

	private PropertyDescriptor _sortProperty;

	protected override bool IsSortedCore => _isSorted;

	protected override ListSortDirection SortDirectionCore => _sortDirection;

	protected override PropertyDescriptor SortPropertyCore => _sortProperty;

	protected override bool SupportsSortingCore => true;

	public SortableBindingList(List<T> list)
		: base((IList<T>)list)
	{
	}

	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		if (PropertyComparer.CanSort(prop.PropertyType))
		{
			((List<T>)base.Items).Sort(new PropertyComparer(prop, direction));
			_sortDirection = direction;
			_sortProperty = prop;
			_isSorted = true;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}

	protected override void RemoveSortCore()
	{
		_isSorted = false;
		_sortProperty = null;
	}
}
