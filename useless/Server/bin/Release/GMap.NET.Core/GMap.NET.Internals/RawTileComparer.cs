using System.Collections.Generic;

namespace GMap.NET.Internals;

internal class RawTileComparer : IEqualityComparer<RawTile>
{
	public bool Equals(RawTile x, RawTile y)
	{
		if (x.Type == y.Type && x.Zoom == y.Zoom)
		{
			return x.Pos == y.Pos;
		}
		return false;
	}

	public int GetHashCode(RawTile obj)
	{
		return obj.Type ^ obj.Zoom ^ obj.Pos.GetHashCode();
	}
}
