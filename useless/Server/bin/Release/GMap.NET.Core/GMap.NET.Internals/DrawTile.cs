using System;

namespace GMap.NET.Internals;

internal struct DrawTile : IEquatable<DrawTile>, IComparable<DrawTile>
{
	public GPoint PosXY;

	public GPoint PosPixel;

	public double DistanceSqr;

	public override string ToString()
	{
		GPoint posXY = PosXY;
		string text = posXY.ToString();
		posXY = PosPixel;
		return text + ", px: " + posXY.ToString();
	}

	public bool Equals(DrawTile other)
	{
		return PosXY == other.PosXY;
	}

	public int CompareTo(DrawTile other)
	{
		return other.DistanceSqr.CompareTo(DistanceSqr);
	}
}
