using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Text;

namespace System.Data.Entity.Core.Objects;

public class ObjectParameterCollection : ICollection<ObjectParameter>, IEnumerable<ObjectParameter>, IEnumerable
{
	private bool _locked;

	private readonly List<ObjectParameter> _parameters;

	private readonly ClrPerspective _perspective;

	private string _cacheKey;

	public int Count => _parameters.Count;

	bool ICollection<ObjectParameter>.IsReadOnly => _locked;

	public ObjectParameter this[string name]
	{
		get
		{
			int num = IndexOf(name);
			if (num == -1)
			{
				throw new ArgumentOutOfRangeException("name", Strings.ObjectParameterCollection_ParameterNameNotFound(name));
			}
			return _parameters[num];
		}
	}

	internal ObjectParameterCollection(ClrPerspective perspective)
	{
		_perspective = perspective;
		_parameters = new List<ObjectParameter>();
	}

	public void Add(ObjectParameter item)
	{
		Check.NotNull(item, "item");
		CheckUnlocked();
		if (Contains(item))
		{
			throw new ArgumentException(Strings.ObjectParameterCollection_ParameterAlreadyExists(item.Name), "item");
		}
		if (Contains(item.Name))
		{
			throw new ArgumentException(Strings.ObjectParameterCollection_DuplicateParameterName(item.Name), "item");
		}
		if (!item.ValidateParameterType(_perspective))
		{
			throw new ArgumentOutOfRangeException("item", Strings.ObjectParameter_InvalidParameterType(item.ParameterType.FullName));
		}
		_parameters.Add(item);
		_cacheKey = null;
	}

	public void Clear()
	{
		CheckUnlocked();
		_parameters.Clear();
		_cacheKey = null;
	}

	public bool Contains(ObjectParameter item)
	{
		Check.NotNull(item, "item");
		return _parameters.Contains(item);
	}

	public bool Contains(string name)
	{
		Check.NotNull(name, "name");
		if (IndexOf(name) != -1)
		{
			return true;
		}
		return false;
	}

	public void CopyTo(ObjectParameter[] array, int arrayIndex)
	{
		_parameters.CopyTo(array, arrayIndex);
	}

	public bool Remove(ObjectParameter item)
	{
		Check.NotNull(item, "item");
		CheckUnlocked();
		bool num = _parameters.Remove(item);
		if (num)
		{
			_cacheKey = null;
		}
		return num;
	}

	public virtual IEnumerator<ObjectParameter> GetEnumerator()
	{
		return ((IEnumerable<ObjectParameter>)_parameters).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_parameters).GetEnumerator();
	}

	internal string GetCacheKey()
	{
		if (_cacheKey == null && _parameters.Count > 0)
		{
			if (1 == _parameters.Count)
			{
				ObjectParameter objectParameter = _parameters[0];
				_cacheKey = "@@1" + objectParameter.Name + ":" + objectParameter.ParameterType.FullName;
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder(_parameters.Count * 20);
				stringBuilder.Append("@@");
				stringBuilder.Append(_parameters.Count);
				for (int i = 0; i < _parameters.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(";");
					}
					ObjectParameter objectParameter2 = _parameters[i];
					stringBuilder.Append(objectParameter2.Name);
					stringBuilder.Append(":");
					stringBuilder.Append(objectParameter2.ParameterType.FullName);
				}
				_cacheKey = stringBuilder.ToString();
			}
		}
		return _cacheKey;
	}

	internal void SetReadOnly(bool isReadOnly)
	{
		_locked = isReadOnly;
	}

	internal static ObjectParameterCollection DeepCopy(ObjectParameterCollection copyParams)
	{
		if (copyParams == null)
		{
			return null;
		}
		ObjectParameterCollection objectParameterCollection = new ObjectParameterCollection(copyParams._perspective);
		foreach (ObjectParameter copyParam in copyParams)
		{
			objectParameterCollection.Add(copyParam.ShallowCopy());
		}
		return objectParameterCollection;
	}

	private int IndexOf(string name)
	{
		int num = 0;
		foreach (ObjectParameter parameter in _parameters)
		{
			if (string.Compare(name, parameter.Name, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private void CheckUnlocked()
	{
		if (_locked)
		{
			throw new InvalidOperationException(Strings.ObjectParameterCollection_ParametersLocked);
		}
	}
}
