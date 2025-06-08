using System.Collections;
using System.Collections.Generic;

namespace AntdUI.Svg.Pathing;

public sealed class SvgPathSegmentList : IList<SvgPathSegment>, ICollection<SvgPathSegment>, IEnumerable<SvgPathSegment>, IEnumerable
{
	internal SvgPath _owner;

	private List<SvgPathSegment> _segments;

	public SvgPathSegment Last => _segments[_segments.Count - 1];

	public SvgPathSegment this[int index]
	{
		get
		{
			return _segments[index];
		}
		set
		{
			_segments[index] = value;
			_owner.OnPathUpdated();
		}
	}

	public int Count => _segments.Count;

	public bool IsReadOnly => false;

	public SvgPathSegmentList()
	{
		_segments = new List<SvgPathSegment>();
	}

	public int IndexOf(SvgPathSegment item)
	{
		return _segments.IndexOf(item);
	}

	public void Insert(int index, SvgPathSegment item)
	{
		_segments.Insert(index, item);
		if (_owner != null)
		{
			_owner.OnPathUpdated();
		}
	}

	public void RemoveAt(int index)
	{
		_segments.RemoveAt(index);
		if (_owner != null)
		{
			_owner.OnPathUpdated();
		}
	}

	public void Add(SvgPathSegment item)
	{
		_segments.Add(item);
		if (_owner != null)
		{
			_owner.OnPathUpdated();
		}
	}

	public void Clear()
	{
		_segments.Clear();
	}

	public bool Contains(SvgPathSegment item)
	{
		return _segments.Contains(item);
	}

	public void CopyTo(SvgPathSegment[] array, int arrayIndex)
	{
		_segments.CopyTo(array, arrayIndex);
	}

	public bool Remove(SvgPathSegment item)
	{
		bool num = _segments.Remove(item);
		if (num && _owner != null)
		{
			_owner.OnPathUpdated();
		}
		return num;
	}

	public IEnumerator<SvgPathSegment> GetEnumerator()
	{
		return _segments.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _segments.GetEnumerator();
	}
}
