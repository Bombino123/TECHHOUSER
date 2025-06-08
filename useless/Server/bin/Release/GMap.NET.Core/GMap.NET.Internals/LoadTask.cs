using System;

namespace GMap.NET.Internals;

internal struct LoadTask : IEquatable<LoadTask>
{
	public GPoint Pos;

	public int Zoom;

	internal Core Core;

	public LoadTask(GPoint pos, int zoom, Core core = null)
	{
		Pos = pos;
		Zoom = zoom;
		Core = core;
	}

	public override string ToString()
	{
		return Zoom + " - " + Pos.ToString();
	}

	public bool Equals(LoadTask other)
	{
		if (Zoom == other.Zoom)
		{
			return Pos == other.Pos;
		}
		return false;
	}
}
