using System;
using System.Collections.Generic;

namespace GMap.NET.Internals;

internal class KiberTileCache : Dictionary<RawTile, byte[]>
{
	private readonly Queue<RawTile> _queue = new Queue<RawTile>();

	public int MemoryCacheCapacity = 22;

	private long _memoryCacheSize;

	public double MemoryCacheSize => (double)_memoryCacheSize / 1048576.0;

	public KiberTileCache()
		: base((IEqualityComparer<RawTile>?)new RawTileComparer())
	{
	}

	public new void Add(RawTile key, byte[] value)
	{
		_queue.Enqueue(key);
		base.Add(key, value);
		_memoryCacheSize += value.Length;
	}

	private new void Remove(RawTile key)
	{
	}

	public new void Clear()
	{
		_queue.Clear();
		base.Clear();
		_memoryCacheSize = 0L;
	}

	internal void RemoveMemoryOverload()
	{
		while (MemoryCacheSize > (double)MemoryCacheCapacity && base.Keys.Count > 0 && _queue.Count > 0)
		{
			RawTile key = _queue.Dequeue();
			try
			{
				byte[] array = base[key];
				base.Remove(key);
				_memoryCacheSize -= array.Length;
			}
			catch (Exception)
			{
			}
		}
	}
}
