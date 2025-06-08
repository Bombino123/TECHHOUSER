using System;
using System.Collections.Generic;

namespace GMap.NET.Internals;

internal class TileMatrix : IDisposable
{
	private List<Dictionary<GPoint, Tile>> _levels = new List<Dictionary<GPoint, Tile>>(33);

	private FastReaderWriterLock _lock = new FastReaderWriterLock();

	private List<KeyValuePair<GPoint, Tile>> _tmp = new List<KeyValuePair<GPoint, Tile>>(44);

	public TileMatrix()
	{
		for (int i = 0; i < _levels.Capacity; i++)
		{
			_levels.Add(new Dictionary<GPoint, Tile>(55, new GPointComparer()));
		}
	}

	public void ClearAllLevels()
	{
		_lock.AcquireWriterLock();
		try
		{
			foreach (Dictionary<GPoint, Tile> level in _levels)
			{
				foreach (KeyValuePair<GPoint, Tile> item in level)
				{
					item.Value.Dispose();
				}
				level.Clear();
			}
		}
		finally
		{
			_lock.ReleaseWriterLock();
		}
	}

	public void ClearLevel(int zoom)
	{
		_lock.AcquireWriterLock();
		try
		{
			if (zoom >= _levels.Count)
			{
				return;
			}
			Dictionary<GPoint, Tile> dictionary = _levels[zoom];
			foreach (KeyValuePair<GPoint, Tile> item in dictionary)
			{
				item.Value.Dispose();
			}
			dictionary.Clear();
		}
		finally
		{
			_lock.ReleaseWriterLock();
		}
	}

	public void ClearLevelAndPointsNotIn(int zoom, List<DrawTile> list)
	{
		_lock.AcquireWriterLock();
		try
		{
			if (zoom >= _levels.Count)
			{
				return;
			}
			Dictionary<GPoint, Tile> dictionary = _levels[zoom];
			_tmp.Clear();
			foreach (KeyValuePair<GPoint, Tile> t in dictionary)
			{
				if (!list.Exists((DrawTile p) => p.PosXY == t.Key))
				{
					_tmp.Add(t);
				}
			}
			foreach (KeyValuePair<GPoint, Tile> item in _tmp)
			{
				dictionary.Remove(item.Key);
				item.Value.Dispose();
			}
			_tmp.Clear();
		}
		finally
		{
			_lock.ReleaseWriterLock();
		}
	}

	public void ClearLevelsBelove(int zoom)
	{
		_lock.AcquireWriterLock();
		try
		{
			if (zoom - 1 >= _levels.Count)
			{
				return;
			}
			for (int num = zoom - 1; num >= 0; num--)
			{
				Dictionary<GPoint, Tile> dictionary = _levels[num];
				foreach (KeyValuePair<GPoint, Tile> item in dictionary)
				{
					item.Value.Dispose();
				}
				dictionary.Clear();
			}
		}
		finally
		{
			_lock.ReleaseWriterLock();
		}
	}

	public void ClearLevelsAbove(int zoom)
	{
		_lock.AcquireWriterLock();
		try
		{
			if (zoom + 1 >= _levels.Count)
			{
				return;
			}
			for (int i = zoom + 1; i < _levels.Count; i++)
			{
				Dictionary<GPoint, Tile> dictionary = _levels[i];
				foreach (KeyValuePair<GPoint, Tile> item in dictionary)
				{
					item.Value.Dispose();
				}
				dictionary.Clear();
			}
		}
		finally
		{
			_lock.ReleaseWriterLock();
		}
	}

	public void EnterReadLock()
	{
		_lock.AcquireReaderLock();
	}

	public void LeaveReadLock()
	{
		_lock.ReleaseReaderLock();
	}

	public Tile GetTileWithNoLock(int zoom, GPoint p)
	{
		Tile value = Tile.Empty;
		_levels[zoom].TryGetValue(p, out value);
		return value;
	}

	public Tile GetTileWithReadLock(int zoom, GPoint p)
	{
		Tile empty = Tile.Empty;
		_lock.AcquireReaderLock();
		try
		{
			return GetTileWithNoLock(zoom, p);
		}
		finally
		{
			_lock.ReleaseReaderLock();
		}
	}

	public void SetTile(Tile t)
	{
		_lock.AcquireWriterLock();
		try
		{
			if (t.Zoom < _levels.Count)
			{
				_levels[t.Zoom][t.Pos] = t;
			}
		}
		finally
		{
			_lock.ReleaseWriterLock();
		}
	}

	~TileMatrix()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (_lock != null)
		{
			if (disposing)
			{
				ClearAllLevels();
			}
			_levels.Clear();
			_levels = null;
			_tmp.Clear();
			_tmp = null;
			_lock.Dispose();
			_lock = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
