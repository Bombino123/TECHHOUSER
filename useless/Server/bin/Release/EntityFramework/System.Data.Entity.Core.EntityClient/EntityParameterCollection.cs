using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.EntityClient;

public sealed class EntityParameterCollection : DbParameterCollection
{
	private List<EntityParameter> _items;

	private static readonly Type _itemType = typeof(EntityParameter);

	private bool _isDirty;

	public override int Count
	{
		get
		{
			if (_items == null)
			{
				return 0;
			}
			return _items.Count;
		}
	}

	private List<EntityParameter> InnerList
	{
		get
		{
			List<EntityParameter> list = _items;
			if (list == null)
			{
				list = (_items = new List<EntityParameter>());
			}
			return list;
		}
	}

	public override bool IsFixedSize => ((IList)InnerList).IsFixedSize;

	public override bool IsReadOnly => ((IList)InnerList).IsReadOnly;

	public override bool IsSynchronized => ((ICollection)InnerList).IsSynchronized;

	public override object SyncRoot => ((ICollection)InnerList).SyncRoot;

	public new EntityParameter this[int index]
	{
		get
		{
			return (EntityParameter)GetParameter(index);
		}
		set
		{
			SetParameter(index, value);
		}
	}

	public new EntityParameter this[string parameterName]
	{
		get
		{
			return (EntityParameter)GetParameter(parameterName);
		}
		set
		{
			SetParameter(parameterName, value);
		}
	}

	internal bool IsDirty
	{
		get
		{
			if (_isDirty)
			{
				return true;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (((EntityParameter)enumerator.Current).IsDirty)
					{
						return true;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int Add(object value)
	{
		OnChange();
		Check.NotNull(value, "value");
		ValidateType(value);
		Validate(-1, value);
		InnerList.Add((EntityParameter)value);
		return Count - 1;
	}

	public override void AddRange(Array values)
	{
		OnChange();
		Check.NotNull(values, "values");
		foreach (object value in values)
		{
			ValidateType(value);
		}
		foreach (EntityParameter value2 in values)
		{
			Validate(-1, value2);
			InnerList.Add(value2);
		}
	}

	private int CheckName(string parameterName)
	{
		int num = IndexOf(parameterName);
		if (num < 0)
		{
			throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidParameterName(parameterName));
		}
		return num;
	}

	public override void Clear()
	{
		OnChange();
		List<EntityParameter> innerList = InnerList;
		if (innerList == null)
		{
			return;
		}
		foreach (EntityParameter item in innerList)
		{
			item.ResetParent();
		}
		innerList.Clear();
	}

	public override bool Contains(object value)
	{
		return -1 != IndexOf(value);
	}

	public override void CopyTo(Array array, int index)
	{
		((ICollection)InnerList).CopyTo(array, index);
	}

	public override IEnumerator GetEnumerator()
	{
		return ((IEnumerable)InnerList).GetEnumerator();
	}

	protected override DbParameter GetParameter(int index)
	{
		RangeCheck(index);
		return InnerList[index];
	}

	protected override DbParameter GetParameter(string parameterName)
	{
		int num = IndexOf(parameterName);
		if (num < 0)
		{
			throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidParameterName(parameterName));
		}
		return InnerList[num];
	}

	private static int IndexOf(IEnumerable items, string parameterName)
	{
		if (items != null)
		{
			int num = 0;
			foreach (EntityParameter item in items)
			{
				if (EntityUtil.SrcCompare(parameterName, item.ParameterName) == 0)
				{
					return num;
				}
				num++;
			}
			num = 0;
			foreach (EntityParameter item2 in items)
			{
				if (EntityUtil.DstCompare(parameterName, item2.ParameterName) == 0)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public override int IndexOf(string parameterName)
	{
		return IndexOf(InnerList, parameterName);
	}

	public override int IndexOf(object value)
	{
		if (value != null)
		{
			ValidateType(value);
			List<EntityParameter> innerList = InnerList;
			if (innerList != null)
			{
				int count = innerList.Count;
				for (int i = 0; i < count; i++)
				{
					if (value == innerList[i])
					{
						return i;
					}
				}
			}
		}
		return -1;
	}

	public override void Insert(int index, object value)
	{
		OnChange();
		Check.NotNull(value, "value");
		ValidateType(value);
		Validate(-1, value);
		InnerList.Insert(index, (EntityParameter)value);
	}

	private void RangeCheck(int index)
	{
		if (index < 0 || Count <= index)
		{
			throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidIndex(index.ToString(CultureInfo.InvariantCulture), Count.ToString(CultureInfo.InvariantCulture)));
		}
	}

	public override void Remove(object value)
	{
		OnChange();
		Check.NotNull(value, "value");
		ValidateType(value);
		int num = IndexOf(value);
		if (-1 != num)
		{
			RemoveIndex(num);
		}
		else if (this != ((EntityParameter)value).CompareExchangeParent(null, this))
		{
			throw new ArgumentException(Strings.EntityParameterCollectionRemoveInvalidObject);
		}
	}

	public override void RemoveAt(int index)
	{
		OnChange();
		RangeCheck(index);
		RemoveIndex(index);
	}

	public override void RemoveAt(string parameterName)
	{
		OnChange();
		int index = CheckName(parameterName);
		RemoveIndex(index);
	}

	private void RemoveIndex(int index)
	{
		List<EntityParameter> innerList = InnerList;
		EntityParameter entityParameter = innerList[index];
		innerList.RemoveAt(index);
		entityParameter.ResetParent();
	}

	private void Replace(int index, object newValue)
	{
		List<EntityParameter> innerList = InnerList;
		ValidateType(newValue);
		Validate(index, newValue);
		EntityParameter entityParameter = innerList[index];
		innerList[index] = (EntityParameter)newValue;
		entityParameter.ResetParent();
	}

	protected override void SetParameter(int index, DbParameter value)
	{
		OnChange();
		RangeCheck(index);
		Replace(index, value);
	}

	protected override void SetParameter(string parameterName, DbParameter value)
	{
		OnChange();
		int num = IndexOf(parameterName);
		if (num < 0)
		{
			throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidParameterName(parameterName));
		}
		Replace(num, value);
	}

	private void Validate(int index, object value)
	{
		Check.NotNull(value, "value");
		EntityParameter entityParameter = (EntityParameter)value;
		object obj = entityParameter.CompareExchangeParent(this, null);
		if (obj != null)
		{
			if (this != obj)
			{
				throw new ArgumentException(Strings.EntityParameterContainedByAnotherCollection);
			}
			if (index != IndexOf(value))
			{
				throw new ArgumentException(Strings.EntityParameterContainedByAnotherCollection);
			}
		}
		string parameterName = entityParameter.ParameterName;
		if (parameterName.Length == 0)
		{
			index = 1;
			do
			{
				parameterName = "Parameter" + index.ToString(CultureInfo.CurrentCulture);
				index++;
			}
			while (-1 != IndexOf(parameterName));
			entityParameter.ParameterName = parameterName;
		}
	}

	private static void ValidateType(object value)
	{
		Check.NotNull(value, "value");
		if (!_itemType.IsInstanceOfType(value))
		{
			throw new InvalidCastException(Strings.InvalidEntityParameterType(value.GetType().Name));
		}
	}

	internal EntityParameterCollection()
	{
	}

	public EntityParameter Add(EntityParameter value)
	{
		Add((object)value);
		return value;
	}

	public EntityParameter AddWithValue(string parameterName, object value)
	{
		EntityParameter entityParameter = new EntityParameter();
		entityParameter.ParameterName = parameterName;
		entityParameter.Value = value;
		return Add(entityParameter);
	}

	public EntityParameter Add(string parameterName, DbType dbType)
	{
		return Add(new EntityParameter(parameterName, dbType));
	}

	public EntityParameter Add(string parameterName, DbType dbType, int size)
	{
		return Add(new EntityParameter(parameterName, dbType, size));
	}

	public void AddRange(EntityParameter[] values)
	{
		AddRange((Array)values);
	}

	public override bool Contains(string parameterName)
	{
		return IndexOf(parameterName) != -1;
	}

	public void CopyTo(EntityParameter[] array, int index)
	{
		CopyTo((Array)array, index);
	}

	public int IndexOf(EntityParameter value)
	{
		return IndexOf((object)value);
	}

	public void Insert(int index, EntityParameter value)
	{
		Insert(index, (object)value);
	}

	private void OnChange()
	{
		_isDirty = true;
	}

	public void Remove(EntityParameter value)
	{
		Remove((object)value);
	}

	internal void ResetIsDirty()
	{
		_isDirty = false;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((EntityParameter)enumerator.Current).ResetIsDirty();
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
