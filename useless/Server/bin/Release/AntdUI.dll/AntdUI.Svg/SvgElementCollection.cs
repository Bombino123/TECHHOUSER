using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AntdUI.Svg;

public sealed class SvgElementCollection : IList<SvgElement>, ICollection<SvgElement>, IEnumerable<SvgElement>, IEnumerable
{
	private List<SvgElement> _elements;

	private SvgElement _owner;

	private bool _mock;

	public SvgElement this[int index]
	{
		get
		{
			return _elements[index];
		}
		set
		{
			_elements[index] = value;
		}
	}

	public int Count => _elements.Count;

	public bool IsReadOnly => false;

	internal SvgElementCollection(SvgElement owner)
		: this(owner, mock: false)
	{
	}

	internal SvgElementCollection(SvgElement owner, bool mock)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_elements = new List<SvgElement>();
		_owner = owner;
		_mock = mock;
	}

	public int IndexOf(SvgElement item)
	{
		return _elements.IndexOf(item);
	}

	public void Insert(int index, SvgElement item)
	{
		InsertAndForceUniqueID(index, item, autoForceUniqueID: true, autoFixChildrenID: true, LogIDChange);
	}

	private void LogIDChange(SvgElement elem, string oldId, string newID)
	{
	}

	public void InsertAndForceUniqueID(int index, SvgElement item, bool autoForceUniqueID = true, bool autoFixChildrenID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		AddToIdManager(item, _elements[index], autoForceUniqueID, autoFixChildrenID, logElementOldIDNewID);
		_elements.Insert(index, item);
		item._parent.OnElementAdded(item, index);
	}

	public void RemoveAt(int index)
	{
		SvgElement svgElement = this[index];
		if (svgElement != null)
		{
			Remove(svgElement);
		}
	}

	public void Add(SvgElement item)
	{
		AddAndForceUniqueID(item, autoForceUniqueID: true, autoFixChildrenID: true, LogIDChange);
	}

	public void AddAndForceUniqueID(SvgElement item, bool autoForceUniqueID = true, bool autoFixChildrenID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		AddToIdManager(item, null, autoForceUniqueID, autoFixChildrenID, logElementOldIDNewID);
		_elements.Add(item);
		item._parent.OnElementAdded(item, Count - 1);
	}

	private void AddToIdManager(SvgElement item, SvgElement sibling, bool autoForceUniqueID = true, bool autoFixChildrenID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		if (!_mock)
		{
			if (_owner.OwnerDocument != null)
			{
				_owner.OwnerDocument.IdManager.AddAndForceUniqueID(item, sibling, autoForceUniqueID, logElementOldIDNewID);
			}
			item._parent = _owner;
		}
	}

	public void Clear()
	{
		while (Count > 0)
		{
			SvgElement item = this[0];
			Remove(item);
		}
	}

	public bool Contains(SvgElement item)
	{
		return _elements.Contains(item);
	}

	public void CopyTo(SvgElement[] array, int arrayIndex)
	{
		_elements.CopyTo(array, arrayIndex);
	}

	public bool Remove(SvgElement item)
	{
		bool num = _elements.Remove(item);
		if (num)
		{
			_owner.OnElementRemoved(item);
			if (!_mock)
			{
				item._parent = null;
			}
		}
		return num;
	}

	public IEnumerable<T> FindSvgElementsOf<T>() where T : SvgElement
	{
		return (from x in _elements
			where x is T
			select x as T).Concat(_elements.SelectMany((SvgElement x) => x.Children.FindSvgElementsOf<T>()));
	}

	public T FindSvgElementOf<T>() where T : SvgElement
	{
		return _elements.OfType<T>().FirstOrDefault() ?? _elements.Select((SvgElement x) => x.Children.FindSvgElementOf<T>()).FirstOrDefault((T x) => x != null);
	}

	public T GetSvgElementOf<T>() where T : SvgElement
	{
		return _elements.FirstOrDefault((SvgElement x) => x is T) as T;
	}

	public IEnumerator<SvgElement> GetEnumerator()
	{
		return _elements.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _elements.GetEnumerator();
	}
}
