using System.Collections.Generic;

namespace GMap.NET.Internals;

internal class LoadTaskComparer : IEqualityComparer<LoadTask>
{
	public bool Equals(LoadTask x, LoadTask y)
	{
		if (x.Zoom == y.Zoom)
		{
			return x.Pos == y.Pos;
		}
		return false;
	}

	public int GetHashCode(LoadTask obj)
	{
		return obj.Zoom ^ obj.Pos.GetHashCode();
	}
}
