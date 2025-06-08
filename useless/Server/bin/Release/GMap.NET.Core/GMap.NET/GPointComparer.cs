using System.Collections.Generic;

namespace GMap.NET;

internal class GPointComparer : IEqualityComparer<GPoint>
{
	public bool Equals(GPoint x, GPoint y)
	{
		if (x.X == y.X)
		{
			return x.Y == y.Y;
		}
		return false;
	}

	public int GetHashCode(GPoint obj)
	{
		return obj.GetHashCode();
	}
}
