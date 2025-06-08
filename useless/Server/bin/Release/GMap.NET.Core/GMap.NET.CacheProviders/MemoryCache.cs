using System;
using GMap.NET.Internals;

namespace GMap.NET.CacheProviders;

public class MemoryCache : IDisposable
{
	private readonly KiberTileCache _tilesInMemory = new KiberTileCache();

	private FastReaderWriterLock _kiberCacheLock = new FastReaderWriterLock();

	public int Capacity
	{
		get
		{
			_kiberCacheLock.AcquireReaderLock();
			try
			{
				return _tilesInMemory.MemoryCacheCapacity;
			}
			finally
			{
				_kiberCacheLock.ReleaseReaderLock();
			}
		}
		set
		{
			_kiberCacheLock.AcquireWriterLock();
			try
			{
				_tilesInMemory.MemoryCacheCapacity = value;
			}
			finally
			{
				_kiberCacheLock.ReleaseWriterLock();
			}
		}
	}

	public double Size
	{
		get
		{
			_kiberCacheLock.AcquireReaderLock();
			try
			{
				return _tilesInMemory.MemoryCacheSize;
			}
			finally
			{
				_kiberCacheLock.ReleaseReaderLock();
			}
		}
	}

	public void Clear()
	{
		_kiberCacheLock.AcquireWriterLock();
		try
		{
			_tilesInMemory.Clear();
		}
		finally
		{
			_kiberCacheLock.ReleaseWriterLock();
		}
	}

	internal byte[] GetTileFromMemoryCache(RawTile tile)
	{
		_kiberCacheLock.AcquireReaderLock();
		try
		{
			if (_tilesInMemory.TryGetValue(tile, out var value))
			{
				return value;
			}
		}
		finally
		{
			_kiberCacheLock.ReleaseReaderLock();
		}
		return null;
	}

	internal void AddTileToMemoryCache(RawTile tile, byte[] data)
	{
		if (data == null)
		{
			return;
		}
		_kiberCacheLock.AcquireWriterLock();
		try
		{
			if (!_tilesInMemory.ContainsKey(tile))
			{
				_tilesInMemory.Add(tile, data);
			}
		}
		finally
		{
			_kiberCacheLock.ReleaseWriterLock();
		}
	}

	internal void RemoveOverload()
	{
		_kiberCacheLock.AcquireWriterLock();
		try
		{
			_tilesInMemory.RemoveMemoryOverload();
		}
		finally
		{
			_kiberCacheLock.ReleaseWriterLock();
		}
	}

	~MemoryCache()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (_kiberCacheLock != null)
		{
			if (disposing)
			{
				Clear();
			}
			_kiberCacheLock.Dispose();
			_kiberCacheLock = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
