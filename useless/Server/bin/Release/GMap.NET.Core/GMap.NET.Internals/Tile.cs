using System;
using System.Collections.Generic;
using System.Threading;

namespace GMap.NET.Internals;

public struct Tile : IDisposable
{
	public static readonly Tile Empty;

	private GPoint _pos;

	private PureImage[] _overlays;

	private long _overlaysCount;

	public readonly bool NotEmpty;

	public IEnumerable<PureImage> Overlays
	{
		get
		{
			long i = 0L;
			for (long size = Interlocked.Read(ref _overlaysCount); i < size; i++)
			{
				yield return _overlays[i];
			}
		}
	}

	internal bool HasAnyOverlays => Interlocked.Read(ref _overlaysCount) > 0;

	public int Zoom { get; private set; }

	public GPoint Pos
	{
		get
		{
			return _pos;
		}
		private set
		{
			_pos = value;
		}
	}

	public Tile(int zoom, GPoint pos)
	{
		NotEmpty = true;
		Zoom = zoom;
		_pos = pos;
		_overlays = null;
		_overlaysCount = 0L;
	}

	internal void AddOverlay(PureImage i)
	{
		if (_overlays == null)
		{
			_overlays = new PureImage[4];
		}
		_overlays[Interlocked.Increment(ref _overlaysCount) - 1] = i;
	}

	public void Dispose()
	{
		if (_overlays != null)
		{
			for (long num = Interlocked.Read(ref _overlaysCount) - 1; num >= 0; num--)
			{
				Interlocked.Decrement(ref _overlaysCount);
				_overlays[num].Dispose();
				_overlays[num] = null;
			}
			_overlays = null;
		}
	}

	public static bool operator ==(Tile m1, Tile m2)
	{
		if (m1._pos == m2._pos)
		{
			return m1.Zoom == m2.Zoom;
		}
		return false;
	}

	public static bool operator !=(Tile m1, Tile m2)
	{
		return !(m1 == m2);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Tile tile))
		{
			return false;
		}
		if (tile.Zoom == Zoom)
		{
			return tile.Pos == Pos;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Zoom ^ _pos.GetHashCode();
	}
}
