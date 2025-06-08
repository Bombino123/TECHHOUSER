namespace WinFormAnimation;

public class Path2D
{
	public Path HorizontalPath { get; }

	public Path VerticalPath { get; }

	public Float2D Start => new Float2D(HorizontalPath.Start, VerticalPath.Start);

	public Float2D End => new Float2D(HorizontalPath.End, VerticalPath.End);

	public Path2D(float startX, float endX, float startY, float endY, ulong duration, ulong delay, AnimationFunctions.Function function)
		: this(new Path(startX, endX, duration, delay, function), new Path(startY, endY, duration, delay, function))
	{
	}

	public Path2D(float startX, float endX, float startY, float endY, ulong duration, ulong delay)
		: this(new Path(startX, endX, duration, delay), new Path(startY, endY, duration, delay))
	{
	}

	public Path2D(float startX, float endX, float startY, float endY, ulong duration, AnimationFunctions.Function function)
		: this(new Path(startX, endX, duration, function), new Path(startY, endY, duration, function))
	{
	}

	public Path2D(float startX, float endX, float startY, float endY, ulong duration)
		: this(new Path(startX, endX, duration), new Path(startY, endY, duration))
	{
	}

	public Path2D(Float2D start, Float2D end, ulong duration, ulong delay, AnimationFunctions.Function function)
		: this(new Path(start.X, end.X, duration, delay, function), new Path(start.Y, end.Y, duration, delay, function))
	{
	}

	public Path2D(Float2D start, Float2D end, ulong duration, ulong delay)
		: this(new Path(start.X, end.X, duration, delay), new Path(start.Y, end.Y, duration, delay))
	{
	}

	public Path2D(Float2D start, Float2D end, ulong duration, AnimationFunctions.Function function)
		: this(new Path(start.X, end.X, duration, function), new Path(start.Y, end.Y, duration, function))
	{
	}

	public Path2D(Float2D start, Float2D end, ulong duration)
		: this(new Path(start.X, end.X, duration), new Path(start.Y, end.Y, duration))
	{
	}

	public Path2D(Path x, Path y)
	{
		HorizontalPath = x;
		VerticalPath = y;
	}

	public Path2D Reverse()
	{
		return new Path2D(HorizontalPath.Reverse(), VerticalPath.Reverse());
	}
}
