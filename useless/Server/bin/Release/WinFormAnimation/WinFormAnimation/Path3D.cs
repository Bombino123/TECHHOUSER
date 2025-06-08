namespace WinFormAnimation;

public class Path3D
{
	public Path HorizontalPath { get; }

	public Path VerticalPath { get; }

	public Path DepthPath { get; }

	public Float3D Start => new Float3D(HorizontalPath.Start, VerticalPath.Start, DepthPath.Start);

	public Float3D End => new Float3D(HorizontalPath.End, VerticalPath.End, DepthPath.End);

	public Path3D(float startX, float endX, float startY, float endY, float startZ, float endZ, ulong duration, ulong delay, AnimationFunctions.Function function)
		: this(new Path(startX, endX, duration, delay, function), new Path(startY, endY, duration, delay, function), new Path(startZ, endZ, duration, delay, function))
	{
	}

	public Path3D(float startX, float endX, float startY, float endY, float startZ, float endZ, ulong duration, ulong delay)
		: this(new Path(startX, endX, duration, delay), new Path(startY, endY, duration, delay), new Path(startZ, endZ, duration, delay))
	{
	}

	public Path3D(float startX, float endX, float startY, float endY, float startZ, float endZ, ulong duration, AnimationFunctions.Function function)
		: this(new Path(startX, endX, duration, function), new Path(startY, endY, duration, function), new Path(startZ, endZ, duration, function))
	{
	}

	public Path3D(float startX, float endX, float startY, float endY, float startZ, float endZ, ulong duration)
		: this(new Path(startX, endX, duration), new Path(startY, endY, duration), new Path(startZ, endZ, duration))
	{
	}

	public Path3D(Float3D start, Float3D end, ulong duration, ulong delay, AnimationFunctions.Function function)
		: this(new Path(start.X, end.X, duration, delay, function), new Path(start.Y, end.Y, duration, delay, function), new Path(start.Z, end.Z, duration, delay, function))
	{
	}

	public Path3D(Float3D start, Float3D end, ulong duration, ulong delay)
		: this(new Path(start.X, end.X, duration, delay), new Path(start.Y, end.Y, duration, delay), new Path(start.Z, end.Z, duration, delay))
	{
	}

	public Path3D(Float3D start, Float3D end, ulong duration, AnimationFunctions.Function function)
		: this(new Path(start.X, end.X, duration, function), new Path(start.Y, end.Y, duration, function), new Path(start.Z, end.Z, duration, function))
	{
	}

	public Path3D(Float3D start, Float3D end, ulong duration)
		: this(new Path(start.X, end.X, duration), new Path(start.Y, end.Y, duration), new Path(start.Z, end.Z, duration))
	{
	}

	public Path3D(Path x, Path y, Path z)
	{
		HorizontalPath = x;
		VerticalPath = y;
		DepthPath = z;
	}

	public Path3D Reverse()
	{
		return new Path3D(HorizontalPath.Reverse(), VerticalPath.Reverse(), DepthPath.Reverse());
	}
}
